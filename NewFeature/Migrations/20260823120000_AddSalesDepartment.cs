using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NewFeature.Services.Repositories;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260823120000_AddSalesDepartment")]
    public partial class AddSalesDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesImportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<int>(type: "int", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceSheet = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReportingPeriod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImportedRowCount = table.Column<int>(type: "int", nullable: false),
                    RejectedRowCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorDetails = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesCustomerRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CustomerGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    SalesImportBatchId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesCustomerRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesCustomerRecords_SalesImportBatches_SalesImportBatchId",
                        column: x => x.SalesImportBatchId,
                        principalTable: "SalesImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesFleetCapacities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BusType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModelYear = table.Column<int>(type: "int", nullable: true),
                    NumberOfBuses = table.Column<int>(type: "int", nullable: false),
                    SeatsPerBus = table.Column<int>(type: "int", nullable: false),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesImportBatchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesFleetCapacities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesFleetCapacities_SalesImportBatches_SalesImportBatchId",
                        column: x => x.SalesImportBatchId,
                        principalTable: "SalesImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesDailyOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RentalOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ExecutionPoint = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Direction = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BusTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OperationalCount = table.Column<int>(type: "int", nullable: false),
                    ScheduledBuses = table.Column<int>(type: "int", nullable: false),
                    ExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    SalesImportBatchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDailyOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDailyOperations_SalesImportBatches_SalesImportBatchId",
                        column: x => x.SalesImportBatchId,
                        principalTable: "SalesImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesCustomerRecords_FiscalYear_CustomerCode",
                table: "SalesCustomerRecords",
                columns: new[] { "FiscalYear", "CustomerCode" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesCustomerRecords_SalesImportBatchId",
                table: "SalesCustomerRecords",
                column: "SalesImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesFleetCapacities_SalesImportBatchId",
                table: "SalesFleetCapacities",
                column: "SalesImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDailyOperations_ExecutionDate",
                table: "SalesDailyOperations",
                column: "ExecutionDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDailyOperations_SalesImportBatchId",
                table: "SalesDailyOperations",
                column: "SalesImportBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesCustomerRecords");

            migrationBuilder.DropTable(
                name: "SalesFleetCapacities");

            migrationBuilder.DropTable(
                name: "SalesDailyOperations");

            migrationBuilder.DropTable(
                name: "SalesImportBatches");
        }
    }
}
