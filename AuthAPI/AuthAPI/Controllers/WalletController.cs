using AuthAPI.Models;
using AuthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AuthAPI.Controllers { 
[Authorize]
[ApiController]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly WalletService _walletService;

    public WalletController(WalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var checkoutUrl = await _walletService.CreateCheckoutSession(token, request.Amount);
        return Ok(new { Url = checkoutUrl });
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var balance = await _walletService.GetBalance(token);
        return Ok(new { Balance = balance });
    }
}
}