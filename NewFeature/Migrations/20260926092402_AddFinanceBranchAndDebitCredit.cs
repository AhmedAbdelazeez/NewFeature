using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFeature.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceBranchAndDebitCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinanceAccountBalances_Date_AccountNumber",
                table: "FinanceAccountBalances");

            // Rows uploaded before the branch column existed belong to the single branch the
            // company reported as one, which is the same default the template ships with - an
            // empty branch would make the same month appear twice under two names.
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "FinanceAccountBalances",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "فرع 1");

            migrationBuilder.Sql(
                "UPDATE [FinanceAccountBalances] SET [Branch] = N'فرع 1' WHERE [Branch] IS NULL OR LTRIM(RTRIM([Branch])) = N''");

            migrationBuilder.AddColumn<decimal>(
                name: "Credit",
                table: "FinanceAccountBalances",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Debit",
                table: "FinanceAccountBalances",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalance",
                table: "FinanceAccountBalances",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccountBalances_Date_Branch_AccountNumber",
                table: "FinanceAccountBalances",
                columns: new[] { "Date", "Branch", "AccountNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinanceAccountBalances_Date_Branch_AccountNumber",
                table: "FinanceAccountBalances");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "FinanceAccountBalances");

            migrationBuilder.DropColumn(
                name: "Credit",
                table: "FinanceAccountBalances");

            migrationBuilder.DropColumn(
                name: "Debit",
                table: "FinanceAccountBalances");

            migrationBuilder.DropColumn(
                name: "OpeningBalance",
                table: "FinanceAccountBalances");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccountBalances_Date_AccountNumber",
                table: "FinanceAccountBalances",
                columns: new[] { "Date", "AccountNumber" },
                unique: true);
        }
    }
}
