// Models/RazorpayWebhookPayload.cs
namespace PaymentAPI.Models
{
    public class RazorpayWebhookPayload
    {
        public string Event { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}
