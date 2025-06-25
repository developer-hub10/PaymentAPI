using Microsoft.VisualBasic;
using PaymentAPI.Models;


namespace PaymentAPI.interfaces
{

    public interface IPaymentService
    {
        string CreateOrder(PaymentRequest request);

        Task<int> ProcessWebhookAsync(string body, string signature, string secret);

    
    }

}