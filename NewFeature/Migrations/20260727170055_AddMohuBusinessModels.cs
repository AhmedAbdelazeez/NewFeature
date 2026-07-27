using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    public partial class AddMohuBusinessModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MohuFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MohuPilgrimGroupId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HasComplaint = table.Column<bool>(type: "bit", nullable: false),
                    ComplaintDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WaitingTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    FeedbackDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MohuFeedbacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MohuPermitLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermitNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsEntryPortMatched = table.Column<bool>(type: "bit", nullable: false),
                    IsEntryDateMatched = table.Column<bool>(type: "bit", nullable: false),
                    IsHousingMatched = table.Column<bool>(type: "bit", nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MohuPermitLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MohuPilgrimGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgeGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PackageCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PackagePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsNewPilgrim = table.Column<bool>(type: "bit", nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalPort = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PilgrimCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MohuPilgrimGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MohuViolationRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViolationType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    AffectedPilgrimsCount = table.Column<int>(type: "int", nullable: false),
                    CommitteeEvaluationScore = table.Column<int>(type: "int", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MohuViolationRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MohuFeedbacks");

            migrationBuilder.DropTable(
                name: "MohuPermitLogs");

            migrationBuilder.DropTable(
                name: "MohuPilgrimGroups");

            migrationBuilder.DropTable(
                name: "MohuViolationRecords");
        }
    }
}
