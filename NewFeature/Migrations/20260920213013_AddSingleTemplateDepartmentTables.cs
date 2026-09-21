using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    public partial class AddSingleTemplateDepartmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TechnicianName2",
                table: "MaintenanceWorkOrders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianName3",
                table: "MaintenanceWorkOrders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianName4",
                table: "MaintenanceWorkOrders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkOrderNumber",
                table: "MaintenanceWorkOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChartOfAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Mapping = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BsClassification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsClassification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RsmClassification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ManagementClassification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RevenueMainClassification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RevenueSubClassification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartOfAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceAccountBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceAccountBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationsDispatchRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Direction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DirectionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RentOrder = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerAccount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    BusType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BusNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DriverNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    AdditionalDriverNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdditionalDriverName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FromLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ToLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActualKm = table.Column<double>(type: "float", nullable: true),
                    PlannedKm = table.Column<double>(type: "float", nullable: true),
                    DieselLiters = table.Column<double>(type: "float", nullable: true),
                    Completion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationsDispatchRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_AccountNumber",
                table: "ChartOfAccounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccountBalances_Date_AccountNumber",
                table: "FinanceAccountBalances",
                columns: new[] { "Date", "AccountNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationsDispatchRecords_DeliveryDate_RentOrder_BusNumber_Direction",
                table: "OperationsDispatchRecords",
                columns: new[] { "DeliveryDate", "RentOrder", "BusNumber", "Direction" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartOfAccounts");

            migrationBuilder.DropTable(
                name: "FinanceAccountBalances");

            migrationBuilder.DropTable(
                name: "OperationsDispatchRecords");

            migrationBuilder.DropColumn(
                name: "TechnicianName2",
                table: "MaintenanceWorkOrders");

            migrationBuilder.DropColumn(
                name: "TechnicianName3",
                table: "MaintenanceWorkOrders");

            migrationBuilder.DropColumn(
                name: "TechnicianName4",
                table: "MaintenanceWorkOrders");

            migrationBuilder.DropColumn(
                name: "WorkOrderNumber",
                table: "MaintenanceWorkOrders");
        }
    }
}
