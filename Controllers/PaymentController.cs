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
        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] UPIPaymentRequest request)
        {
            Dictionary<string, object> input = new Dictionary<string, object>();
            input.Add("amount", request.Amount * 100); // Amount is in currency subunits. Default currency is INR. Hence, 50000 refers to 50000 paise
            input.Add("currency", "INR");
            input.Add("receipt", "12121");

            string key = "rzp_test_gCwU8DoXGtNICx";
            string secret = "QisqqM9TwNdRM0hcob82rQFy";

            RazorpayClient client = new RazorpayClient(key, secret);

            Razorpay.Api.Order order = client.Order.Create(input);
            string orderId = order["id"].ToString();
            return Ok(orderId);
        }

      
     
    }





}