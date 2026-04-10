using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.User;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User,Premium")]
public class PaymentsController : ControllerBase
{
    private readonly IPremiumPaymentService _premiumPaymentService;

    public PaymentsController(IPremiumPaymentService premiumPaymentService)
    {
        _premiumPaymentService = premiumPaymentService;
    }

    [HttpPost("google-pay")]
    public async Task<ActionResult> UpgradeWithGooglePay([FromBody] GooglePayUpgradeDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _premiumPaymentService.UpgradeToPremiumnAsync(userId, dto.PaymentToken);
            return Ok(new { upgraded = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }
}
