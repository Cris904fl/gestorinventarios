using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SistemaVentaBlazor.WhatsAppBot.Configuration;
using SistemaVentaBlazor.WhatsAppBot.Models.OpenAI;

namespace SistemaVentaBlazor.WhatsAppBot.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _settings;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(
            HttpClient httpClient,
            IOptions<OpenAISettings> settings,
            ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        public async Task<string> GetCompletionAsync(string prompt, string systemMessage = "")
        {
            try
            {
                var messages = new List<ChatMessage>();

                if (!string.IsNullOrEmpty(systemMessage))
                {
                    messages.Add(new ChatMessage
                    {
                        Role = "system",
                        Content = systemMessage
                    });
                }

                messages.Add(new ChatMessage
                {
                    Role = "user",
                    Content = prompt
                });

                var request = new ChatCompletionRequest
                {
                    Model = _settings.ModelName,
                    Messages = messages,
                    Temperature = _settings.Temperature,
                    MaxTokens = _settings.MaxTokens
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error from OpenAI API: {Error}", error);
                    return "Lo siento, no pude procesar tu solicitud en este momento.";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseString);

                if (completionResponse?.Choices == null || !completionResponse.Choices.Any())
                {
                    return "No se pudo generar una respuesta.";
                }

                return completionResponse.Choices[0].Message.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception calling OpenAI API");
                return "Ocurrió un error al procesar tu solicitud.";
            }
        }
    }
}