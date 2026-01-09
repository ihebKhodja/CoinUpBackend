using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinUpWorkerService.Migrations
{
    /// <inheritdoc />
    public partial class MarketChartDetails_MultiRangeCharts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarketCapsJson",
                table: "MarketChartDetails");

            migrationBuilder.DropColumn(
                name: "PricesJson",
                table: "MarketChartDetails");

            migrationBuilder.RenameColumn(
                name: "TotalVolumesJson",
                table: "MarketChartDetails",
                newName: "ChartsJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChartsJson",
                table: "MarketChartDetails",
                newName: "TotalVolumesJson");

            migrationBuilder.AddColumn<string>(
                name: "MarketCapsJson",
                table: "MarketChartDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PricesJson",
                table: "MarketChartDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
