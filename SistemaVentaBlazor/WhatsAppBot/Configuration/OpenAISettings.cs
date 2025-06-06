
namespace SistemaVentaBlazor.WhatsAppBot.Configuration
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
    }
}