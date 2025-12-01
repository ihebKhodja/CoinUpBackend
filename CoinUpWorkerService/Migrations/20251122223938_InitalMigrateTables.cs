using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinUpWorkerService.Migrations
{
    /// <inheritdoc />
    public partial class InitalMigrateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoinsMarket",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Current_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Market_Cap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Market_Cap_Rank = table.Column<int>(type: "int", nullable: false),
                    Fully_Diluted_Valuation = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Total_Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High_24h = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low_24h = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price_Change_24h = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price_Change_Percentage_24h = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Market_Cap_Change_24h = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Market_Cap_Change_Percentage_24h = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Circulating_Supply = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_Supply = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Max_Supply = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ath = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ath_Change_Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ath_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Atl = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Atl_Change_Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Atl_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Last_Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinsMarket", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoinsMarketCategory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarketCap = table.Column<double>(type: "float", nullable: false),
                    MarketCapChange24h = table.Column<double>(type: "float", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Top3CoinsId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Top3Coins = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Volume24h = table.Column<double>(type: "float", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinsMarketCategory", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoinsMarket");

            migrationBuilder.DropTable(
                name: "CoinsMarketCategory");
        }
    }
}
