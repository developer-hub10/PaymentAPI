using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.IO;
using System.Text.Json;
using PaymentAPI.Models; 

namespace PaymebtAPI.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {

        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] UPIPaymentRequest request)
        {

            Console.WriteLine("end point called");
            Dictionary<string, object> input = new Dictionary<string, object>();
            input.Add("amount", request.Amount * 100);
            input.Add("currency", "INR");
            input.Add("receipt", "12121");

            string key = "rzp_test_gCwU8DoXGtNICx";
            string secret = "QisqqM9TwNdRM0hcob82rQFy";

            RazorpayClient client = new RazorpayClient(key, secret);

            Order order = client.Order.Create(input);
            string orderId = order["id"].ToString();
            Console.WriteLine($"{orderId} order retruning");
            return Ok(order);
        }

    }





}