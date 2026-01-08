using CoinUp.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;

namespace CoinUpWorkerService.Services
{
    public class CoinCapService : IDataCollectorService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoinCapService> _logger;

        private readonly string? _apiKey;
        private readonly string _apiKeyHeader;
        private const string DemoApiKeyHeader = "x_cg_demo_api_key";
        private const string ProApiKeyHeader = "x-cg-pro-api-key";
        private const string DefaultDemoApiKey = "CG-gLneL1GMruEEaWknWz6LtGi9";

        public CoinCapService(HttpClient httpClient, IConfiguration configuration, ILogger<CoinCapService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var baseUrl = configuration["CoinGecko:BaseUrl"]
                ?? throw new InvalidOperationException("Base Url manquante dans appsettings.json (CoinGecko:BaseUrl)");

            // Ensure trailing slash so relative URLs behave predictably.
            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            _httpClient.BaseAddress = new Uri(baseUrl);

            // CoinGecko may reject requests without a User-Agent.
            if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CoinUpWorker/1.0");
            }

            // API key is optional (some CoinGecko endpoints work without it, but are rate-limited).
            // If you have a Pro key, you typically need:
            // - CoinGecko:BaseUrl = https://pro-api.coingecko.com/api/v3/
            // - CoinGecko:ApiKeyHeader = x-cg-pro-api-key
            var configuredHeader = configuration["CoinGecko:ApiKeyHeader"];
            _apiKeyHeader = !string.IsNullOrWhiteSpace(configuredHeader)
                ? configuredHeader.Trim()
                : (baseUrl.Contains("pro-api.coingecko.com", StringComparison.OrdinalIgnoreCase)
                    ? ProApiKeyHeader
                    : DemoApiKeyHeader);

            var apiKey = configuration["CoinGecko:ApiKey"]; // can come from env vars / user-secrets too

            // Normalize common copy/paste issues:
            // - extra whitespace/newlines
            // - key accidentally pasted twice concatenated (A + A)
            apiKey = NormalizeApiKey(apiKey);

            // For public CoinGecko v3 usage, keep a demo key fallback.
            // For Pro, never inject a demo key.
            if (string.IsNullOrWhiteSpace(apiKey) && string.Equals(_apiKeyHeader, DemoApiKeyHeader, StringComparison.Ordinal))
            {
                apiKey = DefaultDemoApiKey;
            }

            _apiKey = apiKey;

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                // Attach the configured header; we'll also re-attach per request.
                if (_httpClient.DefaultRequestHeaders.Contains(_apiKeyHeader))
                {
                    _httpClient.DefaultRequestHeaders.Remove(_apiKeyHeader);
                }

                _httpClient.DefaultRequestHeaders.Add(_apiKeyHeader, apiKey);

                _logger.LogInformation("CoinGecko API key header configured: {Header}", _apiKeyHeader);
            }
            else
            {
                _logger.LogWarning("CoinGecko:ApiKey is missing/empty. Requests may fail with 401.");
            }
        }

        private static string? NormalizeApiKey(string? apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            apiKey = apiKey.Trim().TrimEnd(',');

            // If multiple tokens were pasted, take the first non-empty token.
            // (This covers newline/space-separated copies.)
            var firstToken = apiKey
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(firstToken))
                return null;

            apiKey = firstToken;

            apiKey = apiKey.TrimEnd(',');

            // If the key appears duplicated with no separator, keep only one half.
            // Example: "ABCABC" => "ABC"
            if (apiKey.Length % 2 == 0)
            {
                var half = apiKey.Length / 2;
                var left = apiKey.Substring(0, half);
                var right = apiKey.Substring(half, half);
                if (string.Equals(left, right, StringComparison.Ordinal))
                {
                    apiKey = left;
                }
            }

            return apiKey;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private async Task<T> GetFromJsonWithErrorsAsync<T>(string relativeUrl, CancellationToken cancellationToken = default)
        {
            var absoluteUrl = new Uri(_httpClient.BaseAddress!, relativeUrl).ToString();
            var hasApiKeyHeader = _httpClient.DefaultRequestHeaders.Contains(_apiKeyHeader);

            _logger.LogInformation(
                "CoinGecko request: {Url} (apiKeyHeaderPresent={ApiKeyPresent})",
                absoluteUrl,
                hasApiKeyHeader
            );

            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                request.Headers.Remove(_apiKeyHeader);
                request.Headers.Add(_apiKeyHeader, _apiKey);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
                if (value is null)
                {
                    throw new InvalidOperationException($"Empty/invalid JSON from CoinGecko for '{relativeUrl}'.");
                }
                return value;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "CoinGecko request failed: {StatusCode} {ReasonPhrase}. Url: {Url}. Body: {Body}",
                (int)response.StatusCode,
                response.ReasonPhrase,
                new Uri(_httpClient.BaseAddress!, relativeUrl).ToString(),
                body
            );

            throw new HttpRequestException(
                $"CoinGecko request failed with {(int)response.StatusCode} ({response.StatusCode}).",
                inner: null,
                statusCode: response.StatusCode
            );
        }


        public async Task<List<CoinsMarket>> FetchCoinsMarketAsync()
        {
            var items = await GetFromJsonWithErrorsAsync<List<JsonElement>>(
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
            var items = await GetFromJsonWithErrorsAsync<List<JsonElement>>("coins/categories");

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

        public async Task<MarketChartWindow?> FetchMarketChartAsync(string id, int rank, int days = 90)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id is required", nameof(id));

            try
            {
                days = days <= 0 ? 90 : days;
                string endpoint =
                    $"coins/{id}/market_chart?vs_currency=usd&days={90}";

                var json = await GetFromJsonWithErrorsAsync<JsonElement>(endpoint);

                return new MarketChartWindow
                {
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