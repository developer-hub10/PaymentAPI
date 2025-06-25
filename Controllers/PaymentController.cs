using Microsoft.AspNetCore.Mvc;


using PaymentAPI.Models;
using PaymentAPI.interfaces;




namespace PaymebtAPI.Controllers
{

    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {

        private static IPaymentService? _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
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
            string secret = "QisqqM9TwNdRM0hcob82rQFy"; // ✅ Move to appsettings.json in production

            int result = await _paymentService.ProcessWebhookAsync(body, signature, secret);


            return Ok(result + " rows has been affected");
        }




    }





}