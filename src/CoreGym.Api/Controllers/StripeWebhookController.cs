using CoreGym.Api.Contracts;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeWebhookService _webhooks;
    private readonly StripeOptions _options;

    public StripeWebhookController(IStripeWebhookService webhooks, IOptions<StripeOptions> options)
    {
        _webhooks = webhooks;
        _options = options.Value;
    }

    /// <summary>
    /// Stripe webhook receiver. Signature-verified against Stripe-Signature
    /// (t/v1 scheme, HMAC-SHA256 of "{t}.{raw body}" with the webhook secret);
    /// unauthenticated by design.
    /// </summary>
    [HttpPost("stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> Stripe(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Not configured",
                detail: "Stripe:WebhookSecret is not configured.");
        }

        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync(cancellationToken);
        }

        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (signature is null
            || !StripeSignatureVerifier.Verify(
                body, signature, _options.WebhookSecret,
                TimeSpan.FromSeconds(_options.SignatureToleranceSeconds)))
        {
            return BadRequest(new { title = "Invalid signature" });
        }

        await _webhooks.HandleEventAsync(body, cancellationToken);
        return Ok(new { received = true });
    }
}
