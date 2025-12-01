using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinUpWorkerService.Migrations
{
    /// <inheritdoc />
    public partial class RankInMaketChart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "MarketChartDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "MarketChartDetails");
        }
    }
}
