using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinUpAPI.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurableAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CoinId",
                table: "PriceAlerts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<decimal>(
                name: "AbovePercentFromBuy",
                table: "PriceAlerts",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AbovePrice",
                table: "PriceAlerts",
                type: "decimal(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceBelow",
                table: "PriceAlerts",
                type: "decimal(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BelowPercentFromBuy",
                table: "PriceAlerts",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BelowPrice",
                table: "PriceAlerts",
                type: "decimal(18,8)",
                nullable: true);

            // Migrate legacy alerts into the new schema:
            // Old types were:
            //  1 = PriceAbove (ThresholdPrice)
            //  2 = PriceBelow (ThresholdPrice)
            //  3/4 = percent change (legacy, disable)
            // New types are:
            //  1 = WatchlistPrice
            //  2 = WalletPriceVsBuy
            //  3 = WalletBalanceBelow
            migrationBuilder.Sql(@"
UPDATE PriceAlerts
SET AbovePrice = COALESCE(AbovePrice, ThresholdPrice),
    Type = 1
WHERE Type = 1;

UPDATE PriceAlerts
SET BelowPrice = COALESCE(BelowPrice, ThresholdPrice),
    Type = 1
WHERE Type = 2;

UPDATE PriceAlerts
SET IsActive = 0
WHERE Type IN (3,4);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbovePercentFromBuy",
                table: "PriceAlerts");

            migrationBuilder.DropColumn(
                name: "AbovePrice",
                table: "PriceAlerts");

            migrationBuilder.DropColumn(
                name: "BalanceBelow",
                table: "PriceAlerts");

            migrationBuilder.DropColumn(
                name: "BelowPercentFromBuy",
                table: "PriceAlerts");

            migrationBuilder.DropColumn(
                name: "BelowPrice",
                table: "PriceAlerts");

            migrationBuilder.AlterColumn<string>(
                name: "CoinId",
                table: "PriceAlerts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
