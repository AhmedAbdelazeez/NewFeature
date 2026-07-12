using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalAudits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationalAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    AuditedProcessCount = table.Column<int>(type: "int", nullable: false),
                    PassedProcessCount = table.Column<int>(type: "int", nullable: false),
                    CriticalFindingsCount = table.Column<int>(type: "int", nullable: false),
                    RecommendationsCount = table.Column<int>(type: "int", nullable: false),
                    RiskMitigationRate = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalAudits_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationalAudits_DepartmentId",
                table: "OperationalAudits",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationalAudits");
        }
    }
}
