using Common.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentProcessController : ControllerBase
    {
        private readonly ILogger<PaymentProcessController> _logger;
        public PaymentProcessController(ILogger<PaymentProcessController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Create(PaymentCreateRequestDto request)
        {
            const decimal balance = 1000;
            if (request.TotalPrice > balance)
                return BadRequest(ResponseDto<PaymentCreateResponseDto>.Fail(400, " Bakiye yetersiz"));

            _logger.LogInformation("Kart işlemi başarılı. {@orderCode}", request.OrderCode);

            return Ok(ResponseDto<PaymentCreateResponseDto>.Success(200, new PaymentCreateResponseDto
            {
                Description = "İşlem başarılı"
            }));
        }
    }
}
