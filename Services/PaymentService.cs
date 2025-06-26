using PaymentAPI.interfaces;
using PaymentAPI.Models;
using PaymentAPI.Utils;

using Razorpay.Api;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Dapper;
using MySql.Data.MySqlClient;


namespace PaymentAPI.Services
{


    public class PaymentService : IPaymentService
    {


        private static IConfiguration _config;


        public PaymentService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateOrder(PaymentRequest request)
        {
            try
            {


                Dictionary<string, object> input = new Dictionary<string, object>();
                input.Add("amount", request.Amount);
                input.Add("currency", "INR");
                input.Add("receipt", "order_rcptid_" + Guid.NewGuid().ToString("N").Substring(0, 8));


                string key = _config.GetValue<string>("RazorpayConfigs:Key");
                string secret = _config.GetValue<string>("RazorpayConfigs:Secret");

                var client = new RazorpayClient(key, secret);
                var order = client.Order.Create(input);

                var orderId = order["id"].ToString();

                return orderId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<int> ProcessWebhookAsync(string body, string signature, string secret)
        {
            try
            {
                var paymentHelper = new PaymentHelper();

                if (!paymentHelper.VerifySignature(body, signature, secret))
                    throw new Exception("Invalid Signature");

                var webhook = JsonConvert.DeserializeObject<RazorpayWebhookPayload>(body);
                string eventType = webhook.Event;

                if (eventType == "payment.captured" || eventType == "payment.failed")
                {
                    JObject payload = webhook.Payload;
                    var payment = payload["payment"] as JObject;
                    var entity = payment?["entity"] as JObject;

                    if (entity == null)
                        throw new Exception("Invalid payload structure.");

                    var paymentDetail = new
                    {
                        payment_id = (string)entity["id"],
                        amount = (double)entity["amount"] / 100,
                        currency = (string)entity["currency"],
                        payment_status = ((string)entity["status"] == "captured") ? "success" : "failed",
                        payment_method = (string)entity["method"],
                        email = (string)entity["email"],
                        contact = (string)entity["contact"],
                        created_at = DateTime.Now
                    };

                    const string insertQuery = @"
                INSERT INTO payment_details
                (payment_id, amount, currency, payment_status, payment_method, email, contact, created_at)
                VALUES
                (@payment_id, @amount, @currency, @payment_status, @payment_method, @email, @contact, @created_at);";

                    using var connection = new MySqlConnection("Server=localhost;Database=payment_database;User Id=root;Password=root;");
                    await connection.OpenAsync();

                    using var transaction = await connection.BeginTransactionAsync();

                    try
                    {
                        int rowsAffected = await connection.ExecuteAsync(insertQuery, paymentDetail, transaction);
                        await transaction.CommitAsync();
                        return rowsAffected;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }



    }
}