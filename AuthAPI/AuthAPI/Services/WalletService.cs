using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace AuthAPI.Services
{
    public class WalletService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WalletService> _logger;

        public WalletService(HttpClient httpClient, IConfiguration configuration, ILogger<WalletService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> CreateCheckoutSession(string token, decimal amount)
        {
            try
            {
                // Clear and set headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration["Authentication:ApiKey"]);

                // Create request payload
                var requestData = new { Amount = amount };
                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending checkout session request to WalletAPI for amount: {Amount}", amount);

                // Make the request
                var response = await _httpClient.PostAsync(
                    "http://walletapi:80/api/wallet/create-checkout-session",
                    content);

                // Ensure successful response
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("WalletAPI returned error: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    throw new Exception($"WalletAPI error: {errorContent}");
                }

                // Parse response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Received response from WalletAPI: {Response}", responseContent);

                var result = JsonSerializer.Deserialize<CheckoutSessionResponse>(responseContent);

                if (string.IsNullOrEmpty(result?.url))
                {
                    _logger.LogError("Received null or empty URL from WalletAPI");
                    throw new Exception("Failed to get checkout URL from payment service");
                }

                return result.url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout session");
                throw;
            }
        }

        public async Task<decimal> GetBalance(string token)
        {
            try
            {
                // Clear and set headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration["Authentication:ApiKey"]);

                // Send request to WalletAPI to fetch balance
                var response = await _httpClient.GetAsync("http://walletapi:80/api/wallet/balance");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Balance request failed: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    throw new Exception($"Balance request failed: {errorContent}");
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BalanceResponse>(responseData);

                if (result == null)
                {
                    throw new Exception("Invalid balance response");
                }

                return result.balance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance");
                throw;
            }
        }

        public class BalanceResponse
        {
            public decimal balance { get; set; }
        }

    }

    public class CheckoutSessionResponse
    {
        public string url { get; set; }
    }
}