using System.Security.Cryptography;
using System.Text;
using CoreGym.Infrastructure.Integrations;

namespace CoreGym.Infrastructure.Tests;

public class StripeSignatureVerifierTests
{
    private const string Secret = "whsec_test_secret";
    private const string Payload = """{"id":"evt_1","type":"checkout.session.completed"}""";

    [Fact]
    public void Valid_signature_passes()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = Sign(Payload, timestamp);

        Assert.True(StripeSignatureVerifier.Verify(Payload, $"t={timestamp},v1={signature}", Secret));
    }

    [Fact]
    public void Multiple_v1_signatures_pass_when_one_matches()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = Sign(Payload, timestamp);

        Assert.True(StripeSignatureVerifier.Verify(
            Payload, $"t={timestamp},v1={Sign("older payload", timestamp)},v1={signature}", Secret));
    }

    [Fact]
    public void Tampered_payload_fails()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = Sign(Payload, timestamp);

        Assert.False(StripeSignatureVerifier.Verify(Payload + " ", $"t={timestamp},v1={signature}", Secret));
    }

    [Fact]
    public void Wrong_secret_fails()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = Sign(Payload, timestamp);

        Assert.False(StripeSignatureVerifier.Verify(Payload, $"t={timestamp},v1={signature}", "whsec_other"));
    }

    [Fact]
    public void Stale_timestamp_fails_replay()
    {
        var oldTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3600; // 1h old, tolerance 5m
        var signature = Sign(Payload, oldTimestamp);

        Assert.False(StripeSignatureVerifier.Verify(Payload, $"t={oldTimestamp},v1={signature}", Secret));
    }

    [Fact]
    public void Malformed_headers_fail()
    {
        Assert.False(StripeSignatureVerifier.Verify(Payload, "v1=deadbeef", Secret));                       // no t
        Assert.False(StripeSignatureVerifier.Verify(Payload, "t=123", Secret));                             // no v1
        Assert.False(StripeSignatureVerifier.Verify(Payload, "not a signature header", Secret));            // garbage
        Assert.False(StripeSignatureVerifier.Verify(Payload, "", Secret));                                  // empty
    }

    private static string Sign(string payload, long timestamp)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{payload}")))
            .ToLowerInvariant();
    }
}
