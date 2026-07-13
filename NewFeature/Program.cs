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
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
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
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IFleetService, FleetService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<IOperationalAuditService, OperationalAuditService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IItService, ItService>();
builder.Services.AddScoped<IHseService, HseService>();


builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DashboardPortal", policy =>
    {
        policy.WithOrigins("http://localhost:5118", "https://localhost:7290")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = userManager.FindByEmailAsync("admin@company.com").Result;
        if (user != null)
        {
            user.UserName = "admin@company.com";
            user.NormalizedUserName = "ADMIN@COMPANY.COM";
            user.EmailConfirmed = true;
            user.IsActive = true;
            userManager.UpdateAsync(user).Wait();

            userManager.SetLockoutEndDateAsync(user, null).Wait();
            userManager.ResetAccessFailedCountAsync(user).Wait();

            var token = userManager.GeneratePasswordResetTokenAsync(user).Result;
            var resetResult = userManager.ResetPasswordAsync(user, token, "Admin@123").Result;
            var roles = userManager.GetRolesAsync(user).Result;
            File.WriteAllText(logPath, $"User admin@company.com exists! Password reset: {resetResult.Succeeded}. Email confirmed: {user.EmailConfirmed}, Lockout cleared. Username: {user.UserName}, Active: {user.IsActive}, Roles: {string.Join(", ", roles)}");
        }
        else
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@company.com",
                Email = "admin@company.com",
                FullNameEn = "System Admin",
                FullNameAr = "مدير النظام",
                IsActive = true,
                EmailConfirmed = true
            };
            var result = userManager.CreateAsync(adminUser, "Admin@123").Result;
            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                File.WriteAllText(logPath, "Forced seed Succeeded! User admin@company.com created.");
            }
            else
            {
                File.WriteAllText(logPath, $"User does not exist and forced seeding failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
    catch (Exception ex)
    {
        File.WriteAllText(logPath, $"Exception occurred during seeding check: {ex.Message}\n{ex.StackTrace}");
    }
}

app.Run();
