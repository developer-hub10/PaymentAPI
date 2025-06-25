using System.Security.Cryptography;
using System.Text;


namespace PaymentAPI.Utils
{


    public class PaymentHelper
    {

        public bool VerifySignature(string payload, string receivedSignature, string secret)
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