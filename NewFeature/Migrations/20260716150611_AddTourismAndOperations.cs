using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    public partial class AddTourismAndOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationsDailyPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledTripsCount = table.Column<int>(type: "int", nullable: false),
                    CompletedTripsCount = table.Column<int>(type: "int", nullable: false),
                    FuelEfficiencyIndex = table.Column<double>(type: "float", nullable: false),
                    PassengerSatisfactionRate = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationsDailyPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationsIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescriptionAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResponseTimeMinutes = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationsIncidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TourismHotelBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HotelNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HotelNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoomCount = table.Column<int>(type: "int", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GuestRating = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourismHotelBookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TourismTours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TourNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuideNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuideNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PassengerCount = table.Column<int>(type: "int", nullable: false),
                    BookingLeadTimeHours = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourismTours", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationsDailyPlans");

            migrationBuilder.DropTable(
                name: "OperationsIncidents");

            migrationBuilder.DropTable(
                name: "TourismHotelBookings");

            migrationBuilder.DropTable(
                name: "TourismTours");
        }
    }
}
