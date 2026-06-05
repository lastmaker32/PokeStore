namespace PokeStore.Api.Infrastructure.External;

using System.Net.Http.Json;

/// <summary>
/// Interface for payment gateway operations
/// </summary>
public interface IPaymentGatewayClient
{
    Task<bool> VerifyWebhookSignatureAsync(string signature, Dictionary<string, string> postData);
}

/// <summary>
/// PayPal payment gateway client for integration
/// </summary>
public class PayPalPaymentGatewayClient : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PayPalPaymentGatewayClient> _logger;

    public PayPalPaymentGatewayClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PayPalPaymentGatewayClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Verify webhook signature with PayPal servers (for additional security)
    /// </summary>
    /// <remarks>
    /// PayPal provides a verification endpoint to confirm webhook authenticity
    /// In production, this would call: https://ipnpb.paypal.com/cgi-bin/webscr
    /// </remarks>
    public async Task<bool> VerifyWebhookSignatureAsync(string signature, Dictionary<string, string> postData)
    {
        try
        {
            var paypalWebhookUrl = _configuration["PayPal:WebhookVerificationUrl"] 
                ?? "https://ipnpb.paypal.com/cgi-bin/webscr";

            // In sandbox mode, use:
            // https://ipnpb.sandbox.paypal.com/cgi-bin/webscr

            // Create verification request
            var verificationData = new Dictionary<string, string>
            {
                { "cmd", "_notify-validate" }
            };

            // Add all original POST data
            foreach (var kvp in postData)
            {
                verificationData[kvp.Key] = kvp.Value;
            }

            var content = new FormUrlEncodedContent(verificationData);
            
            var response = await _httpClient.PostAsync(paypalWebhookUrl, content);
            var responseText = await response.Content.ReadAsStringAsync();

            // PayPal returns "VERIFIED" if valid, "INVALID" if not
            if (responseText.Equals("VERIFIED", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("PayPal webhook signature verified successfully");
                return true;
            }
            else
            {
                _logger.LogWarning("PayPal webhook signature verification failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying PayPal webhook signature");
            return false;
        }
    }

    /// <summary>
    /// Get PayPal access token for API calls
    /// </summary>
    private async Task<string> GetAccessTokenAsync()
    {
        var clientId = _configuration["PayPal:ClientId"];
        var clientSecret = _configuration["PayPal:ClientSecret"];
        var tokenUrl = _configuration["PayPal:TokenUrl"] ?? "https://api.sandbox.paypal.com/v1/oauth2/token";

        var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") })
        };

        try
        {
            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            
            // Simple JSON parsing for access token (production would use System.Text.Json)
            var accessTokenStart = jsonString.IndexOf("\"access_token\":\"") + 16;
            var accessTokenEnd = jsonString.IndexOf("\"", accessTokenStart);
            var accessToken = jsonString.Substring(accessTokenStart, accessTokenEnd - accessTokenStart);
            
            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PayPal access token");
            throw;
        }
    }
}
