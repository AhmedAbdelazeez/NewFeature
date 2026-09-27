using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services;
using NewFeature.Services.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Every new or changed password must be at least 12 characters and mix upper case, lower case,
    // digits and symbols. Existing passwords keep working until they are next changed.
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 6;

    // Repeated wrong passwords lock the account for 15 minutes.
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageSettings", policy =>
        policy.RequireRole("Admin"));

    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IFleetService, FleetService>();
builder.Services.AddScoped<IRouteOperationsService, RouteOperationsService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<IOperationalAuditService, OperationalAuditService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IItService, ItService>();
builder.Services.AddScoped<IHseService, HseService>();
builder.Services.AddScoped<IProcurementService, ProcurementService>();
builder.Services.AddScoped<IStrategyService, StrategyService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<ICommercialService, CommercialService>();
builder.Services.AddScoped<ITourismService, TourismService>();
builder.Services.AddScoped<IOperationsService, OperationsService>();
builder.Services.AddScoped<IMohuStandardsService, MohuStandardsService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Department Excel uploads. A month of Operations dispatch lines exported from the ERP runs past
// 30 MB (Excel keeps hundreds of thousands of formatted-but-empty rows below the data), and the
// framework's default 30,000,000-byte request cap rejected those files before the upload endpoint
// ever ran. Raised for every host (Kestrel, IIS, multipart form reader) so they all agree.
// Keep in step with UPLOAD_LIMIT_MB in wwwroot/js/excel-upload.js.
const long MaxUploadBytes = 200L * 1024 * 1024;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = MaxUploadBytes);
builder.Services.Configure<Microsoft.AspNetCore.Builder.IISServerOptions>(options => options.MaxRequestBodySize = MaxUploadBytes);
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options => options.MultipartBodyLengthLimit = MaxUploadBytes);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DashboardPortal", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("DashboardPortal");

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logPath = Path.Combine(builder.Environment.ContentRootPath, "db_seed_log.txt");
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        DbInitializer.SeedAsync(services).Wait();

        // The dashboard's summary query joins Trips against Routes/AspNetUsers, and on this dev SQL
        // Server instance that specific join has repeatedly picked a catastrophically bad execution
        // plan (a few hundred ms should-be query timing out at 30s+) whenever statistics drift even
        // slightly out of date - independent of whether new data was actually bulk-imported. Refresh
        // on every startup so this can't silently resurface after a restart.
        var startupLogger = services.GetRequiredService<ILogger<Program>>();
        NewFeature.Services.Repositories.DbMaintenanceHelper
            .RefreshStatisticsAsync(context, startupLogger, "Trips", "Vehicles", "MaintenanceWorkOrders", "AspNetUsers", "Routes", "SparePartConsumptions")
            .Wait();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = userManager.FindByEmailAsync("admin@rawahil.com.sa").Result;
        if (user != null)
        {
            user.UserName = "admin@rawahil.com.sa";
            user.NormalizedUserName = "ADMIN@RAWAHIL.COM.SA";
            user.EmailConfirmed = true;
            user.IsActive = true;
            userManager.UpdateAsync(user).Wait();

            userManager.SetLockoutEndDateAsync(user, null).Wait();
            userManager.ResetAccessFailedCountAsync(user).Wait();

            // The password is deliberately NOT touched here. This block used to reset it to a
            // hard-coded value on every start, which silently undid any password change.
            var roles = userManager.GetRolesAsync(user).Result;
            Console.WriteLine($"User admin@rawahil.com.sa exists. Email confirmed: {user.EmailConfirmed}, Lockout cleared. Username: {user.UserName}, Active: {user.IsActive}, Roles: {string.Join(", ", roles)}");
        }
        else
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@rawahil.com.sa",
                Email = "admin@rawahil.com.sa",
                FullNameEn = "System Admin",
                FullNameAr = "مدير النظام",
                IsActive = true,
                EmailConfirmed = true
            };
            var initialPassword = NewFeature.Services.Repositories.SeedPassword.Generate();
            var result = userManager.CreateAsync(adminUser, initialPassword).Result;
            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                Console.WriteLine($"Forced seed Succeeded! User admin@rawahil.com.sa created with initial password: {initialPassword}");
            }
            else
            {
                Console.WriteLine($"User does not exist and forced seeding failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception occurred during seeding check: {ex.Message}\n{ex.StackTrace}");
    }
}

app.Run();
