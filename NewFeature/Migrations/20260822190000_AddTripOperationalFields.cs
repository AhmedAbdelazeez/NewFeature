using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    public partial class AddTripOperationalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingReference",
                table: "Trips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Trips",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FuelConsumedLiters",
                table: "Trips",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OdometerKm",
                table: "Trips",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PassengerCount",
                table: "Trips",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingReference",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "FuelConsumedLiters",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "OdometerKm",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PassengerCount",
                table: "Trips");
        }
    }
}
