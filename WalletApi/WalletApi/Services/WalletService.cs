using Stripe.Checkout;
using Stripe;
using WalletApi.Repository;
using Microsoft.Extensions.Logging;

namespace WalletApi.Services
{
    public class WalletService
    {
        private readonly WalletRepository _walletRepository;
        private readonly ILogger<WalletService> _logger;

        public WalletService(WalletRepository walletRepository, ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _logger = logger;
        }

        public async Task AddBalanceToWallet(string userId, decimal amount, string bearerToken)
        {
            await _walletRepository.UpdateWalletBalance(userId, amount, bearerToken);
        }


        public async Task<decimal> GetBalance(string userId)
        {
            return await _walletRepository.GetWalletBalance(userId);
        }



        public async Task HandleStripeWebhook(Event stripeEvent, HttpRequest request)
        {
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session == null || session.Metadata == null)
                {
                    _logger.LogWarning("Session metadata is missing");
                    return;
                }


                if (session.AmountTotal == null)
                {
                    _logger.LogWarning("Stripe session is missing AmountTotal");
                    return;
                }


                var userId = session.Metadata["userId"];
                var bearerToken = session.Metadata["token"];
                var amountTotal = session.AmountTotal.Value / 100m;

                _logger.LogInformation("Stripe webhook: completed session for user {UserId} amount {Amount}", userId, amountTotal);

                await _walletRepository.UpdateWalletBalance(userId, amountTotal, bearerToken);
            }
        }


    }
}