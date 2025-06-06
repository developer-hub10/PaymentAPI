using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.IO;
using System.Text.Json;
using PaymentAPI.Models; // Assuming you have a Models folder with UPIPaymentRequest model

namespace PaymebtAPI.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        [HttpPost("upi-transaction")]
        public IActionResult ProcessUPIPayment([FromBody] UPIPaymentRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Amount) || string.IsNullOrEmpty(request.Vpa))
            {
                return BadRequest("Invalid payment request.");
            }

            try
            {
                RazorpayClient client = new RazorpayClient("rzp_test_6lLBu8V20Mr6NY", "Qo1s8dFsKmUe8sUHoyyzPRqe");

                var options = new Dictionary<string, object>
                {
                    { "amount", Convert.ToInt32(request.Amount) * 100 }, // Amount in paise
                    { "currency", "INR" },
                    { "payment_capture", 1 },
                    { "method", "upi" },
                    { "vpa", request.Vpa }
                };

                Order order = client.Order.Create(options);

                return Ok(new
                {
                    Message = "UPI payment initiated successfully.",
                    OrderId = order["id"],
                    Amount = request.Amount,
                    Currency = "INR"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error processing payment.", Error = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public IActionResult RazorpayWebhook()
        {
            try
            {
                // Read the webhook payload
                using var reader = new StreamReader(Request.Body);
                var payload = reader.ReadToEnd();

                // Verify the webhook signature (optional but recommended)
                var signature = Request.Headers["X-Razorpay-Signature"];
                var secret = "moharoon11"; // Set this in Razorpay Dashboard
                bool isValid = VerifyWebhookSignature(payload, signature, secret);

                if (!isValid)
                {
                    return BadRequest("Invalid webhook signature.");
                }

                // Parse the payload
                var webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(payload);

                // Handle payment events
                if (webhookEvent.Event == "payment.captured")
                {
                    Console.WriteLine("Payment captured successfully (webhook).");
                    // Handle successful payment logic
                    // You can access webhookEvent.Payload to get payment details
                }
                else if (webhookEvent.Event == "payment.failed")
                {
                    // Payment failed
                    // Handle failure logic
                    Console.WriteLine("Payment failed. (webhook).");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error processing webhook.", Error = ex.Message });
            }
        }

        private bool VerifyWebhookSignature(string payload, string signature, string secret)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return computedSignature == signature;
        }
    }

 


  
}