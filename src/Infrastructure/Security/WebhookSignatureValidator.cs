namespace PokeStore.Api.Infrastructure.Security;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Service for validating PayPal webhook signatures using HMAC-SHA256
/// </summary>
public interface IWebhookSignatureValidator
{
    bool ValidateSignature(string signature, Dictionary<string, string> postData, string webhookSigningKey);
}

public class WebhookSignatureValidator : IWebhookSignatureValidator
{
    /// <summary>
    /// Validates PayPal IPN signature using HMAC-SHA256
    /// </summary>
    /// <remarks>
    /// PayPal IPN signature verification:
    /// 1. Get all POST parameters and signing key from PayPal
    /// 2. Recreate the signed string in the exact order received
    /// 3. Hash with HMAC-SHA256 using signing key
    /// 4. Compare with signature provided in X-PAYPAL-TRANSMISSION-SIG header
    /// </remarks>
    public bool ValidateSignature(string signature, Dictionary<string, string> postData, string webhookSigningKey)
    {
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(webhookSigningKey))
            return false;

        try
        {
            // Recreate the signed string by concatenating all POST parameters
            var signedString = new StringBuilder();
            foreach (var kvp in postData.OrderBy(x => x.Key))
            {
                signedString.Append(kvp.Key);
                signedString.Append("=");
                signedString.Append(kvp.Value);
                signedString.Append("&");
            }

            // Remove trailing &
            if (signedString.Length > 0)
                signedString.Length--;

            // Hash with HMAC-SHA256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSigningKey)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedString.ToString()));
                var calculatedSignature = Convert.ToBase64String(hashBytes);

                // Compare with provided signature
                return calculatedSignature.Equals(signature, StringComparison.Ordinal);
            }
        }
        catch
        {
            return false;
        }
    }
}
