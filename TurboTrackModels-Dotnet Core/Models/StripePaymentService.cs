using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TurboTrackModels_Dotnet_Core.Models;

public sealed class StripePaymentService(
    IHttpClientFactory httpClientFactory,
    IOptions<StripeSettings> stripeOptions)
{
    private readonly StripeSettings _settings = stripeOptions.Value;

    public async Task<StripeRefundResult> RefundAsync(
        string chargeId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/refunds");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["charge"] = chargeId
        });

        using var response = await httpClientFactory.CreateClient()
            .SendAsync(request, cancellationToken);
        using var refundResponse = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = refundResponse.RootElement.TryGetProperty("error", out var error) &&
                          error.TryGetProperty("message", out var errorMessage)
                ? errorMessage.GetString()
                : null;
            return new StripeRefundResult(false, null, message ?? "Stripe could not issue the refund.");
        }

        if (!refundResponse.RootElement.TryGetProperty("id", out var refundId) ||
            string.IsNullOrWhiteSpace(refundId.GetString()))
        {
            return new StripeRefundResult(false, null, "Stripe did not return a refund reference.");
        }

        if (!refundResponse.RootElement.TryGetProperty("status", out var status) ||
            status.GetString() != "succeeded")
        {
            return new StripeRefundResult(false, null,
                "Stripe has not confirmed the refund as successful yet.");
        }

        return new StripeRefundResult(true, refundId.GetString(), null);
    }
}

public sealed record StripeRefundResult(bool Succeeded, string? RefundId, string? ErrorMessage);
