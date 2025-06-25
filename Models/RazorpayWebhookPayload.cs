// Models/RazorpayWebhookPayload.cs
using Newtonsoft.Json.Linq;

namespace PaymentAPI.Models
{
    public class RazorpayWebhookPayload
    {
        public string Event { get; set; }
        public JObject Payload { get; set; }
    }
}
