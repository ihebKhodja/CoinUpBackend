using CoinUp.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace CoinUpWorkerService.Services
{
    public class CoinCapService : IDataCollectorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<CoinCapService> _logger;

        public CoinCapService(HttpClient httpClient, IConfiguration configuration, ILogger<CoinCapService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiKey = configuration["CoinGecko:ApiKey"]
                ?? throw new InvalidOperationException("API Key manquante dans appsettings.json");

            var baseUrl = configuration["CoinGecko:BaseUrl"]
                ?? throw new InvalidOperationException("Base Url manquante dans appsettings.json");

            _httpClient.BaseAddress = new Uri(baseUrl);

            // REQUIRED: CoinGecko blocks requests without User-Agent
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CoinUpWorker/1.0");

            //_httpClient.DefaultRequestHeaders.Add("X-CG-API-KEY", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("x-cg-demo-api-key", _apiKey);
        }


        public async Task<List<CoinsMarket>> FetchCoinsMarketAsync()
        {

            var items = await _httpClient.GetFromJsonAsync<List<JsonElement>>(
                "coins/markets?vs_currency=usd&per_page=100"
            );

            var result = new List<CoinsMarket>();


            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                result.Add(new CoinsMarket
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Symbol = item.GetProperty("symbol").GetString() ?? "",
                    Name = item.GetProperty("name").GetString() ?? "",
                    Image = item.GetProperty("image").GetString() ?? "",

                    Current_Price = GetDecimalSafe(item, "current_price"),
                    Market_Cap = GetDecimalSafe(item, "market_cap"),
                    Market_Cap_Rank = GetIntSafe(item, "market_cap_rank"),

                    Fully_Diluted_Valuation = GetDecimalSafe(item, "fully_diluted_valuation"),
                    Total_Volume = GetDecimalSafe(item, "total_volume"),

                    High_24h = GetDecimalSafe(item, "high_24h"),
                    Low_24h = GetDecimalSafe(item, "low_24h"),

                    Price_Change_24h = GetDecimalSafe(item, "price_change_24h"),
                    Price_Change_Percentage_24h = GetDecimalSafe(item, "price_change_percentage_24h"),

                    Market_Cap_Change_24h = GetDecimalSafe(item, "market_cap_change_24h"),
                    Market_Cap_Change_Percentage_24h = GetDecimalSafe(item, "market_cap_change_percentage_24h"),

                    Circulating_Supply = GetDecimalSafe(item, "circulating_supply"),
                    Total_Supply = GetDecimalSafe(item, "total_supply"),
                    Max_Supply = GetDecimalSafe(item, "max_supply"),

                    Ath = GetDecimalSafe(item, "ath"),
                    Ath_Change_Percentage = GetDecimalSafe(item, "ath_change_percentage"),
                    Ath_Date = GetDateSafe(item, "ath_date"),

                    Atl = GetDecimalSafe(item, "atl"),
                    Atl_Change_Percentage = GetDecimalSafe(item, "atl_change_percentage"),
                    Atl_Date = GetDateSafe(item, "atl_date"),

                    Roi = item.TryGetProperty("roi", out var roi)
                        ? roi.ValueKind == JsonValueKind.Null ? null : roi
                        : null,

                    Last_Updated = item.GetProperty("last_updated").GetDateTime(),
                    Rank = i + 1,

                });
            }

            return result;
        }

        public async Task<List<CoinsMarketCategory>> FetchMarketCategoriesAsync()
        {
            var items = await _httpClient.GetFromJsonAsync<List<JsonElement>>("coins/categories");

            var result = new List<CoinsMarketCategory>();

            foreach (var item in items)
            {
                result.Add(new CoinsMarketCategory
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Name = item.GetProperty("name").GetString() ?? "",
                    MarketCap = item.TryGetProperty("market_cap", out var marketCap) && marketCap.ValueKind == JsonValueKind.Number
                        ? marketCap.GetDouble()
                        : 0.0,

                    MarketCapChange24h = item.TryGetProperty("market_cap_change_24h", out var marketCapChange) && marketCapChange.ValueKind == JsonValueKind.Number
                        ? marketCapChange.GetDouble()
                        : 0.0,
                    Content = item.GetProperty("content").GetString() ?? "",

                    Top3CoinsId = item.TryGetProperty("top_3_coins_id", out var top3Ids) && top3Ids.ValueKind == JsonValueKind.Array
                        ? top3Ids.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                        : new List<string>(),

                    Top3Coins = item.TryGetProperty("top_3_coins", out var top3Images) && top3Images.ValueKind == JsonValueKind.Array
                        ? top3Images.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                        : new List<string>(),

                    Volume24h = item.TryGetProperty("volume_24h", out var volume) && volume.ValueKind == JsonValueKind.Number
                        ? volume.GetDouble()
                        : 0.0,  // fallback

                    UpdatedAt = item.TryGetProperty("updated_at", out var updatedAt) && updatedAt.ValueKind == JsonValueKind.String
                        ? updatedAt.GetDateTime()
                        : DateTime.UtcNow
                });
            }

            return result;
        }

        public async Task<MarketChartDetails?> FetchMarketChartAsync(string id, int rank)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id is required", nameof(id));

            try
            {
                string endpoint =
                    $"coins/{id}/market_chart?vs_currency=usd&days=365";

                var json = await _httpClient.GetFromJsonAsync<JsonElement>(endpoint);

                return new MarketChartDetails
                {
                    Id = id,
                    Rank = rank,
                    Prices = ConvertToDecimalList(json.GetProperty("prices")),
                    MarketCaps = ConvertToDecimalList(json.GetProperty("market_caps")),
                    TotalVolumes = ConvertToDecimalList(json.GetProperty("total_volumes"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching market chart for id {Id}", id);
                return null;
            }
        }

        // Converts JSON arrays like [[timestamp, value], [...]] → List<List<decimal>>
        private List<List<decimal>> ConvertToDecimalList(JsonElement arrayNode)
        {
            var result = new List<List<decimal>>();

            foreach (var arr in arrayNode.EnumerateArray())
            {
                var inner = new List<decimal>();

                foreach (var val in arr.EnumerateArray())
                {
                    if (val.ValueKind == JsonValueKind.Number)
                        inner.Add(val.GetDecimal());
                }

                result.Add(inner);
            }

            return result;
        }

        private decimal GetDecimalSafe(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
                return 0;

            if (prop.ValueKind == JsonValueKind.Null)
                return 0;

            if (prop.ValueKind == JsonValueKind.String &&
                decimal.TryParse(prop.GetString(), out var num))
                return num;

            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetDecimal();

            return 0;
        }

        private int GetIntSafe(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
                return 0;

            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var num))
                return num;

            if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out num))
                return num;

            return 0;
        }

        private DateTime GetDateSafe(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
                return DateTime.MinValue;

            if (prop.ValueKind == JsonValueKind.String && DateTime.TryParse(prop.GetString(), out var dt))
                return dt;

            return DateTime.MinValue;
        }

    }
}