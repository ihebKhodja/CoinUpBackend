using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinUpAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Current_Price = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Market_Cap = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "MarketChartDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    ChartsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketChartDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWallets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioSnapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioSnapshots_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchlistItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchlistItems_CoinsMarket_CoinId",
                        column: x => x.CoinId,
                        principalTable: "CoinsMarket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchlistItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoinHoldings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WalletId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    AverageBuyPrice = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinHoldings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinHoldings_CoinsMarket_CoinId",
                        column: x => x.CoinId,
                        principalTable: "CoinsMarket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoinHoldings_EWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "EWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WalletId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    PriceAtOperation = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_CoinsMarket_CoinId",
                        column: x => x.CoinId,
                        principalTable: "CoinsMarket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_EWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "EWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoinHoldings_CoinId",
                table: "CoinHoldings",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinHoldings_WalletId",
                table: "CoinHoldings",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_EWallets_UserId",
                table: "EWallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSnapshots_UserId",
                table: "PortfolioSnapshots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CoinId",
                table: "Transactions",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_CoinId",
                table: "WatchlistItems",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_UserId",
                table: "WatchlistItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoinHoldings");

            migrationBuilder.DropTable(
                name: "CoinsMarketCategory");

            migrationBuilder.DropTable(
                name: "MarketChartDetails");

            migrationBuilder.DropTable(
                name: "PortfolioSnapshots");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "WatchlistItems");

            migrationBuilder.DropTable(
                name: "EWallets");

            migrationBuilder.DropTable(
                name: "CoinsMarket");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
