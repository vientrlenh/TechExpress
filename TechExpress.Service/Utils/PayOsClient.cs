using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V1.Payouts.Batch;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TechExpress.Service.Utils
{
    public class PayOsClient
    {
        private readonly IConfiguration _config;

        public PayOsClient(IConfiguration config)
        {
            _config = config;
        }

        private PayOSClient Create()
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

            return new PayOSClient(clientId, apiKey, checksumKey);
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

        // ===== 2.0.1: Create Payment Link =====
        public Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
            CreatePaymentLinkRequest request,
            CancellationToken ct = default)
        {
            var client = Create();
            return client.PaymentRequests.CreateAsync(request);
        }

        // ===== 2.0.1: Verify Webhook (async) =====
        public Task<WebhookData> VerifyWebhookAsync(
            Webhook webhook,
            CancellationToken ct = default)
        {
            var client = Create();
            return client.Webhooks.VerifyAsync(webhook);
        }

        // ===== 2.0.1: Payout batch =====
        public Task<Payout> CreatePayoutBatchAsync(
            PayoutBatchRequest request,
            CancellationToken ct = default)
        {
            var client = Create();
            return client.Payouts.Batch.CreateAsync(request);
        }
    }
}