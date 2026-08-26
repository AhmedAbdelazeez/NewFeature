using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NewFeature.Services.Repositories;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260824130000_ExtendVehicleFleetData")]
    public partial class ExtendVehicleFleetData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The official Vehicle Management register this table is being seeded from does not
            // carry seat counts, so Capacity can no longer be required.
            migrationBuilder.AlterColumn<decimal>(
                name: "Capacity",
                table: "Vehicles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "BusNumber",
                table: "Vehicles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusTypeCode",
                table: "Vehicles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChassisNumber",
                table: "Vehicles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAirConditioning",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInStorage",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxKilometers",
                table: "Vehicles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibilityCode",
                table: "Vehicles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleEmployeeName",
                table: "Vehicles",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "BusTypeCode",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ChassisNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasAirConditioning",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsInStorage",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "MaxKilometers",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ResponsibilityCode",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ResponsibleEmployeeName",
                table: "Vehicles");

            migrationBuilder.AlterColumn<decimal>(
                name: "Capacity",
                table: "Vehicles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
    }
}
