using Microsoft.AspNetCore.Mvc;


using PaymentAPI.Models;
using PaymentAPI.interfaces;


namespace PaymentAPI.Controllers
{

    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {

        private static IPaymentService _paymentService;

        private static IConfiguration _config;

        public PaymentController(IPaymentService paymentService, IConfiguration config)
        {
            _paymentService = paymentService;
            _config = config;
        }

        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] PaymentRequest request)
        {
            if (request == null || request.Amount <= 0)
            {
                return BadRequest(new { message = "Invalid amount." });
            }

            string orderId = _paymentService.CreateOrder(request);

            return Ok(new { orderId = orderId });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> RazorpayWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            string signature = Request.Headers["X-Razorpay-Signature"];
            string secret = _config.GetValue<string>("RazorpayConfigs:Secret"); 

            int result = await _paymentService.ProcessWebhookAsync(body, signature, secret);

            return Ok(result + " rows has been affected");
        }


    }





}