using System.Security.Cryptography;
using System.Text;

namespace CoreGym.Infrastructure.Integrations;

/// <summary>
/// Verifies Stripe's webhook signature scheme:
///   header: t=&lt;unix seconds&gt;,v1=&lt;hex hmac&gt; (v1 may repeat)
///   signed payload: "{t}.{raw body}"
///   expected: HMAC-SHA256(webhook secret, signed payload)
/// Rejects stale timestamps (replay) and compares in constant time.
/// </summary>
public static class StripeSignatureVerifier
{
    public static bool Verify(string payload, string signatureHeader, string secret, TimeSpan? tolerance = null)
    {
        if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        long? timestamp = null;
        var signatures = new List<string>();
        foreach (var part in signatureHeader.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length != 2)
            {
                continue;
            }

            if (keyValue[0] == "t" && long.TryParse(keyValue[1], out var parsed))
            {
                timestamp = parsed;
            }
            else if (keyValue[0] == "v1")
            {
                signatures.Add(keyValue[1]);
            }
        }

        if (timestamp is null || signatures.Count == 0)
        {
            return false;
        }

        var toleranceSeconds = (int)(tolerance ?? TimeSpan.FromMinutes(5)).TotalSeconds;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - timestamp.Value) > toleranceSeconds)
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{payload}")))
            .ToLowerInvariant();

        return signatures.Any(s =>
            s.Length == expected.Length &&
            CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(s), Encoding.UTF8.GetBytes(expected)));
    }
}
