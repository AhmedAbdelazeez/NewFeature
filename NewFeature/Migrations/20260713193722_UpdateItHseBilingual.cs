using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItHseBilingual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "ItTickets",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "ItTickets",
                newName: "DescriptionEn");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "HseInspections",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "HseIncidents",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "HseIncidents",
                newName: "DescriptionEn");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "ItTickets",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "ItTickets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "HseInspections",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "HseIncidents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "HseIncidents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "ItTickets");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "ItTickets");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "HseInspections");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "HseIncidents");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "HseIncidents");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "ItTickets",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "DescriptionEn",
                table: "ItTickets",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "HseInspections",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "HseIncidents",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "DescriptionEn",
                table: "HseIncidents",
                newName: "Description");
        }
    }
}
