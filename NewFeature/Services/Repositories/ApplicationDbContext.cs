using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;

namespace NewFeature.Services.Repositories
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<ClientContact> ClientContacts { get; set; } = null!;
        public DbSet<Contract> Contracts { get; set; } = null!;
        public DbSet<ContractItem> ContractItems { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<ProjectMilestone> ProjectMilestones { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Models.Route> Routes { get; set; } = null!;
        public DbSet<Trip> Trips { get; set; } = null!;
        public DbSet<Models.Task> Tasks { get; set; } = null!;
        public DbSet<TaskDependency> TaskDependencies { get; set; } = null!;
        public DbSet<TimeLog> TimeLogs { get; set; } = null!;

        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<ViolationClassification> ViolationClassifications { get; set; } = null!;
        public DbSet<Violation> Violations { get; set; } = null!;
        public DbSet<InternalAudit> InternalAudits { get; set; } = null!;
        public DbSet<ImprovementAction> ImprovementActions { get; set; } = null!;
        public DbSet<OperationalAudit> OperationalAudits { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<EmployeeEvaluation> EmployeeEvaluations { get; set; } = null!;
        public DbSet<ItTicket> ItTickets { get; set; } = null!;
        public DbSet<ItSystem> ItSystems { get; set; } = null!;
        public DbSet<HseIncident> HseIncidents { get; set; } = null!;
        public DbSet<HseInspection> HseInspections { get; set; } = null!;
        public DbSet<ProcurementRequest> ProcurementRequests { get; set; } = null!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public DbSet<StrategicGoal> StrategicGoals { get; set; } = null!;
        public DbSet<PmoInitiative> PmoInitiatives { get; set; } = null!;
        public DbSet<FinanceTransaction> FinanceTransactions { get; set; } = null!;
        public DbSet<FinanceBudget> FinanceBudgets { get; set; } = null!;
        public DbSet<CommercialContract> CommercialContracts { get; set; } = null!;
        public DbSet<CommercialLead> CommercialLeads { get; set; } = null!;
        public DbSet<TourismHotelBooking> TourismHotelBookings { get; set; } = null!;
        public DbSet<TourismTour> TourismTours { get; set; } = null!;
        public DbSet<OperationsDailyPlan> OperationsDailyPlans { get; set; } = null!;
        public DbSet<OperationsIncident> OperationsIncidents { get; set; } = null!;
        public DbSet<MohuStandardKpi> MohuStandardKpis { get; set; } = null!;
        public DbSet<MohuPilgrimGroup> MohuPilgrimGroups { get; set; } = null!;
        public DbSet<MohuFeedback> MohuFeedbacks { get; set; } = null!;
        public DbSet<MohuViolationRecord> MohuViolationRecords { get; set; } = null!;
        public DbSet<MohuPermitLog> MohuPermitLogs { get; set; } = null!;
        public DbSet<MaintenanceWorkOrder> MaintenanceWorkOrders { get; set; } = null!;
        public DbSet<SparePartConsumption> SparePartConsumptions { get; set; } = null!;

        // Sales Department
        public DbSet<SalesImportBatch> SalesImportBatches { get; set; } = null!;
        public DbSet<SalesCustomerRecord> SalesCustomerRecords { get; set; } = null!;
        public DbSet<SalesFleetCapacity> SalesFleetCapacities { get; set; } = null!;
        public DbSet<SalesDailyOperation> SalesDailyOperations { get; set; } = null!;

        // Operations Department - roster/scheduling snapshots
        public DbSet<OfficialDriver> OfficialDrivers { get; set; } = null!;
        public DbSet<RouteScheduleRequest> RouteScheduleRequests { get; set; } = null!;

        // Operations Department - the approved single dispatch-log template
        public DbSet<OperationsDispatchRecord> OperationsDispatchRecords { get; set; } = null!;

        // Finance Department - the approved single template's two data sheets
        public DbSet<ChartOfAccount> ChartOfAccounts { get; set; } = null!;
        public DbSet<FinanceAccountBalance> FinanceAccountBalances { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Decimal Precision
            modelBuilder.Entity<Contract>()
                .Property(c => c.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ContractItem>()
                .Property(ci => ci.Quantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ContractItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ContractItem>()
                .Property(ci => ci.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Capacity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Mileage)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.MaxKilometers)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Project>()
                .Property(p => p.ContractValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Models.Route>()
                .Property(r => r.DistanceKm)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Models.Route>()
                .HasIndex(r => r.Code)
                .IsUnique()
                .HasFilter("[Code] IS NOT NULL");

            modelBuilder.Entity<Models.Task>()
                .Property(t => t.EstimatedHours)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TimeLog>()
                .Property(tl => tl.HoursLogged)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProcurementRequest>()
                .Property(pr => pr.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InventoryItem>()
                .Property(ii => ii.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PmoInitiative>()
                .Property(pi => pi.Budget)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinanceTransaction>()
                .Property(ft => ft.Amount)
                .HasPrecision(18, 2);

            // ─── The Finance department's two approved templates ───
            modelBuilder.Entity<FinanceAccountBalance>()
                .Property(fab => fab.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinanceAccountBalance>()
                .Property(fab => fab.OpeningBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinanceAccountBalance>()
                .Property(fab => fab.Debit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinanceAccountBalance>()
                .Property(fab => fab.Credit)
                .HasPrecision(18, 2);

            // A balance is one account as of one date in one branch, and the chart of accounts
            // holds each account once. Both are enforced in the database so a re-uploaded month can
            // only ever update the rows it already owns, never silently append a second copy of
            // them - and so the two branches' figures can't overwrite each other.
            modelBuilder.Entity<FinanceAccountBalance>()
                .HasIndex(fab => new { fab.Date, fab.Branch, fab.AccountNumber })
                .IsUnique();

            modelBuilder.Entity<ChartOfAccount>()
                .HasIndex(coa => coa.AccountNumber)
                .IsUnique();

            // The Maintenance page and the dashboard both list work orders newest-first, one page at a
            // time. Without this index SQL Server sorts the whole table (wide text columns included)
            // on every page request, which measured ~21s for a 20-row page on ~2,800 work orders.
            modelBuilder.Entity<MaintenanceWorkOrder>()
                .HasIndex(o => new { o.Date, o.Id })
                .IsDescending(true, true);

            // Work-order number is how the Excel import and the add/edit form find an existing order.
            modelBuilder.Entity<MaintenanceWorkOrder>()
                .HasIndex(o => o.WorkOrderNumber);

            // One dispatch line per direction + rental order + bus + execution date.
            modelBuilder.Entity<OperationsDispatchRecord>()
                .HasIndex(r => new { r.DeliveryDate, r.RentOrder, r.BusNumber, r.Direction })
                .IsUnique();

            modelBuilder.Entity<FinanceBudget>()
                .Property(fb => fb.AllocatedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinanceBudget>()
                .Property(fb => fb.SpentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CommercialContract>()
                .Property(cc => cc.Value)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CommercialLead>()
                .Property(cl => cl.EstimatedValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CommercialLead>()
                .Property(cl => cl.AcquisitionCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TourismHotelBooking>()
                .Property(thb => thb.Value)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SparePartConsumption>()
                .Property(spc => spc.UnitPrice)
                .HasPrecision(18, 2);

            // Cascade Deletes Mitigation (Preventing cycles in SQL Server)
            
            // Client -> ClientContacts (Cascade delete is fine)
            modelBuilder.Entity<ClientContact>()
                .HasOne(cc => cc.Client)
                .WithMany(c => c.Contacts)
                .HasForeignKey(cc => cc.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client -> Contracts (Cascade delete is fine)
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contract -> ContractItems (Cascade delete is fine)
            modelBuilder.Entity<ContractItem>()
                .HasOne(ci => ci.Contract)
                .WithMany(c => c.ContractItems)
                .HasForeignKey(ci => ci.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client -> Projects (SetNull or Restrict to avoid multiple cascade paths)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project -> ProjectMembers
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMembers)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent user delete from cascade deleting members if it conflicts

            // Project -> ProjectMilestones
            modelBuilder.Entity<ProjectMilestone>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> Tasks
            modelBuilder.Entity<Models.Task>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Task>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Task -> TimeLogs
            modelBuilder.Entity<TimeLog>()
                .HasOne(tl => tl.Task)
                .WithMany(t => t.TimeLogs)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeLog>()
                .HasOne(tl => tl.User)
                .WithMany(u => u.TimeLogs)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Task -> TaskDependencies (Multiple relationships between Task and TaskDependency)
            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.PredecessorTask)
                .WithMany(t => t.SuccessorDependencies)
                .HasForeignKey(td => td.PredecessorTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.SuccessorTask)
                .WithMany(t => t.PredecessorDependencies)
                .HasForeignKey(td => td.SuccessorTaskId)
                .OnDelete(DeleteBehavior.Cascade); // One can be Cascade, the other must be Restrict/NoAction

            // Vehicle -> Trips
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Route -> Trips
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany(r => r.Trips)
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Driver (User) -> Trips
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany(u => u.Trips)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project -> Trips
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Trips)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Compliance Department Precision
            modelBuilder.Entity<Violation>()
                .Property(v => v.FineAmount)
                .HasPrecision(18, 2);

            // Compliance Department Relationships
            modelBuilder.Entity<Violation>()
                .HasOne(v => v.Department)
                .WithMany(d => d.Violations)
                .HasForeignKey(v => v.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Violation>()
                .HasOne(v => v.Classification)
                .WithMany(vc => vc.Violations)
                .HasForeignKey(v => v.ClassificationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InternalAudit>()
                .HasOne(ia => ia.Department)
                .WithMany(d => d.Audits)
                .HasForeignKey(ia => ia.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ImprovementAction>()
                .HasOne(ia => ia.Department)
                .WithMany(d => d.ImprovementActions)
                .HasForeignKey(ia => ia.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperationalAudit>()
                .HasOne(ia => ia.Department)
                .WithMany()
                .HasForeignKey(ia => ia.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeEvaluation>()
                .HasOne(ee => ee.Employee)
                .WithMany(e => e.Evaluations)
                .HasForeignKey(ee => ee.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vehicle -> MaintenanceWorkOrders (Restrict delete)
            modelBuilder.Entity<MaintenanceWorkOrder>()
                .HasOne(m => m.Vehicle)
                .WithMany()
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // MaintenanceWorkOrder -> SparePartConsumptions (Cascade delete)
            modelBuilder.Entity<SparePartConsumption>()
                .HasOne(s => s.MaintenanceWorkOrder)
                .WithMany(m => m.ConsumedParts)
                .HasForeignKey(s => s.MaintenanceWorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // SparePartConsumption -> InventoryItem (Restrict delete)
            modelBuilder.Entity<SparePartConsumption>()
                .HasOne(s => s.InventoryItem)
                .WithMany()
                .HasForeignKey(s => s.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sales Department: import batches own their child rows (customer roster /
            // fleet capacity / daily operations), so a batch delete cascades to its rows,
            // mirroring the MaintenanceWorkOrder -> SparePartConsumption cascade above.
            modelBuilder.Entity<SalesCustomerRecord>()
                .HasOne(r => r.ImportBatch)
                .WithMany()
                .HasForeignKey(r => r.SalesImportBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesFleetCapacity>()
                .HasOne(r => r.ImportBatch)
                .WithMany()
                .HasForeignKey(r => r.SalesImportBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesDailyOperation>()
                .HasOne(r => r.ImportBatch)
                .WithMany()
                .HasForeignKey(r => r.SalesImportBatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
