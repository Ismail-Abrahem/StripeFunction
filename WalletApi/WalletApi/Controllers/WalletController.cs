using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using WalletApi.Models;
using WalletApi.Services;

namespace WalletApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly StripeService _stripeService;
        private readonly ILogger<WalletController> _logger;
        private readonly WalletService _walletService;

        public WalletController(StripeService stripeService, ILogger<WalletController> logger, WalletService walletService)
        {
            _stripeService = stripeService;
            _logger = logger;
            _walletService = walletService;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] DepositRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("Invalid user token");
                }

                if (request.Amount <= 0)
                {
                    return BadRequest("Amount must be greater than zero");
                }

                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var sessionUrl = await _stripeService.CreateCheckoutSession(userId, request.Amount, token);

                return Ok(new { Url = sessionUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout session");
                return StatusCode(500, $"Error creating payment session: {ex.Message}");
            }
        }



        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized();
                }

                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var balance = await _walletService.GetBalance(userId);
                return Ok(new { Balance = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance");
                return StatusCode(500, "Error retrieving balance");
            }
        }

    }

    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly WalletService _walletService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            WalletService walletService,
            IConfiguration configuration,
            ILogger<WebhookController> logger)
        {
            _walletService = walletService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("stripe")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            _logger.LogInformation("Raw webhook received: {Json}", json); // Add this

            try
            {
                var signature = Request.Headers["Stripe-Signature"];
                var secret = _configuration["Stripe:WebhookSecret"];

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    secret,
                    throwOnApiVersionMismatch: false); // Important for testing

                _logger.LogInformation("Processing event: {Type}", stripeEvent.Type);

                await _walletService.HandleStripeWebhook(stripeEvent, Request);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing failed");
                return BadRequest();
            }
        }
    }

}