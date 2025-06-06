using System.Text.Json.Serialization;

namespace SistemaVentaBlazor.WhatsAppBot.Models.WhatsApp
{
    public class WhatsAppResponse
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonPropertyName("recipient_type")]
        public string RecipientType { get; set; } = "individual";

        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public TextContent Text { get; set; } = new TextContent();
    }

    public class TextContent
    {
        [JsonPropertyName("preview_url")]
        public bool PreviewUrl { get; set; } = false;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}