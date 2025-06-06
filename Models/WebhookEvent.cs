

namespace PaymentAPI.Models
{
    public class WebhookEvent
    {
        public string Event { get; set; }
        public dynamic Payload { get; set; }
    }

    
}
