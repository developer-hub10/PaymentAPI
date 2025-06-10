using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using PaymentAPI.Models;
using Razorpay.Api;

namespace PaymebtAPI.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {

        private readonly ILogger<PaymentController> _logger;

        public PaymentController(ILogger<PaymentController> logger)
        {
            _logger = logger;
        }

       

        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] UPIPaymentRequest request)
        {
            var response = new Dictionary<string, object>();

            try
            {
                if (request == null || request.Amount <= 0)
                {
                    _logger.LogWarning("Invalid payment request received.");
                    return BadRequest(new { message = "Invalid amount." });
                }

                _logger.LogInformation("Creating Razorpay order for amount: ₹{Amount}", request.Amount);

                Dictionary<string, object> input = new Dictionary<string, object>();
                input.Add("amount", request.Amount);
                input.Add("currency", "INR");
                input.Add("receipt", "order_rcptid_" + Guid.NewGuid().ToString("N").Substring(0, 8));



                string key = "rzp_test_gCwU8DoXGtNICx";
                string secret = "QisqqM9TwNdRM0hcob82rQFy";

                var client = new RazorpayClient(key, secret);
                var order = client.Order.Create(input);

                var orderId = order["id"].ToString();

                _logger.LogInformation($"Creating Razorpay order for amount: {request.Amount} with Order Id {orderId}"); ;
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating Razorpay order.");
                return StatusCode(500, new { message = "Something went wrong while creating the order." });
            }
        }


        [HttpPost("webhook")]
        public async Task<IActionResult> RazorpayWebhook()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            string RazorpaySecret = "QisqqM9TwNdRM0hcob82rQFy";

            var receivedSignature = Request.Headers["X-Razorpay-Signature"].ToString();
            var isValid = VerifySignature(body, receivedSignature, RazorpaySecret);

            if (!isValid)
            {
                _logger.LogWarning("Invalid Razorpay webhook signature.");
                return BadRequest("Invalid signature");
            }

            var webhook = JsonConvert.DeserializeObject<RazorpayWebhookPayload>(body);

            _logger.LogInformation("Valid Razorpay webhook received. Event: {EventType}", webhook?.Event);

            switch (webhook?.Event)
            {
                case "payment.captured":
                    _logger.LogInformation("Payment Captured: {Payload}", JsonConvert.SerializeObject(webhook.Payload));
                    // TODO: update database, send email, etc.
                    break;

                case "payment.failed":
                    _logger.LogError("Payment Failed: {Payload}", JsonConvert.SerializeObject(webhook.Payload));
                    // TODO: mark as failed in DB or notify user
                    break;

                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", webhook?.Event);
                    break;
            }

            return Ok();
        }

        private bool VerifySignature(string payload, string receivedSignature, string secret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(payload);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(bodyBytes);
            var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return generatedSignature == receivedSignature.ToLower();
        }

    }





}