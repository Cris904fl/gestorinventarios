using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SistemaVentaBlazor.WhatsAppBot.Configuration;
using SistemaVentaBlazor.WhatsAppBot.Models.WhatsApp;

namespace SistemaVentaBlazor.WhatsAppBot.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            HttpClient httpClient,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
        }

        public async Task<bool> SendTextMessageAsync(string to, string message)
        {
            try
            {
                var response = new WhatsAppResponse
                {
                    To = to,
                    Text = new TextContent { Body = message }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(response),
                    Encoding.UTF8,
                    "application/json");

                var url = $"/{_settings.PhoneNumberId}/messages";
                var result = await _httpClient.PostAsync(url, content);

                if (!result.IsSuccessStatusCode)
                {
                    var error = await result.Content.ReadAsStringAsync();
                    _logger.LogError("Error sending WhatsApp message: {Error}", error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending WhatsApp message");
                return false;
            }
        }

        public bool VerifyWebhook(string token)
        {
            return token == _settings.WebhookVerifyToken;
        }
    }
}