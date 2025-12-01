using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinUpWorkerService.Migrations
{
    /// <inheritdoc />
    public partial class MarketChartDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketChartDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PricesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarketCapsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalVolumesJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketChartDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketChartDetails");
        }
    }
}
