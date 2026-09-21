using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NewFeature.Services.Repositories;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260921120000_AddMaintenanceWorkOrderListIndexes")]
    public partial class AddMaintenanceWorkOrderListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Newest-first paging (Maintenance page + dashboard work-orders table) reads straight
            // off this index instead of sorting the whole table on every request.
            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_Date_Id",
                table: "MaintenanceWorkOrders",
                columns: new[] { "Date", "Id" },
                descending: new[] { true, true });

            // Lookup by work-order number (Excel import + add/edit duplicate check).
            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_WorkOrderNumber",
                table: "MaintenanceWorkOrders",
                column: "WorkOrderNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaintenanceWorkOrders_Date_Id",
                table: "MaintenanceWorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceWorkOrders_WorkOrderNumber",
                table: "MaintenanceWorkOrders");
        }
    }
}
