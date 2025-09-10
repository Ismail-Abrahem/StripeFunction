using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Logging;
using Stripe.Issuing;

namespace WalletApi.Services
{
    public class StripeService
    {
        private readonly string _secretKey;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            _secretKey = configuration["Stripe:SecretKey"];
            _logger = logger;
            StripeConfiguration.ApiKey = _secretKey;

            // Verify Stripe key is loaded
            if (string.IsNullOrEmpty(_secretKey))
            {
                _logger.LogError("Stripe SecretKey is not configured!");
                throw new ArgumentNullException("Stripe:SecretKey");
            }
        }

        public async Task<string> CreateCheckoutSession(string userId, decimal amount, string token)
        {
            try
            {
                _logger.LogInformation("Creating Stripe session for user {UserId}, amount {Amount}",
                    userId, amount);

                // Verify amount is valid
                if (amount <= 0)
                {
                    throw new ArgumentException("Amount must be positive");
                }

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmountDecimal = amount * 100, // Convert to cents
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Wallet Deposit",
                                    Description = $"Top-up for user {userId}"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = "http://localhost:7001/api/wallet/success?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = "http://localhost:7001/api/wallet/cancel",
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", userId },
                        { "token", token }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                if (string.IsNullOrEmpty(session.Url))
                {
                    _logger.LogError("Stripe returned null URL for session {SessionId}", session.Id);
                    throw new Exception("Stripe did not return a checkout URL");
                }

                _logger.LogInformation("Successfully created Stripe session with URL: {Url}", session.Url);
                return session.Url;
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe API error");
                throw new Exception($"Payment service error: {e.StripeError?.Message ?? e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating checkout session");
                throw;
            }
        }
    }
}