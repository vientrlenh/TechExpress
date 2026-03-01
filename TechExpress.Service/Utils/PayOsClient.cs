using System;
using System.Collections.Generic;
using System.Text;

namespace TechExpress.Service.Utils
{
    using Microsoft.Extensions.Configuration;
    using Net.payOS;
    using Net.payOS.Types;

    namespace TechExpress.Service.Utils
    {
        public class PayOsClient
        {
            private readonly IConfiguration _config;

            public PayOsClient(IConfiguration config)
            {
                _config = config;
            }

            private PayOS Create()
            {
                var clientId = _config["PayOS:ClientId"];
                var apiKey = _config["PayOS:ApiKey"];
                var checksumKey = _config["PayOS:ChecksumKey"];

                if (string.IsNullOrWhiteSpace(clientId) ||
                    string.IsNullOrWhiteSpace(apiKey) ||
                    string.IsNullOrWhiteSpace(checksumKey))
                {
                    throw new InvalidOperationException("Thiếu cấu hình PayOS (PayOS:ClientId/ApiKey/ChecksumKey).");
                }

                return new PayOS(clientId, apiKey, checksumKey);
            }

            public string ReturnUrl => _config["PayOS:ReturnUrl"] ?? "";
            public string CancelUrl => _config["PayOS:CancelUrl"] ?? "";

            public int ExpirationSeconds
            {
                get
                {
                    var raw = _config["PayOS:ExpirationSeconds"];
                    return int.TryParse(raw, out var n) && n > 0 ? n : 900;
                }
            }

            public int SessionTtlMinutes
            {
                get
                {
                    var raw = _config["PayOS:SessionTtlMinutes"];
                    return int.TryParse(raw, out var n) && n > 0 ? n : 20;
                }
            }

            public string RedisKeyPrefix => _config["PayOS:RedisKeyPrefix"] ?? "payos:sess:";

            public Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData data)
            {
                var payos = Create();
                return payos.createPaymentLink(data);
            }

            public WebhookData VerifyWebhook(WebhookType body)
            {
                var payos = Create();
                return payos.verifyPaymentWebhookData(body);
            }
        }
    }

}
