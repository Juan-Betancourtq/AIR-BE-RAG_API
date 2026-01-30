using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResumeChat.RagApi.Models;

public class ResumeDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("uploadDate")]
    public DateTime UploadDate { get; set; }

    [JsonPropertyName("contentVector")]
    public List<float>? ContentVector { get; set; }

    // JPB: Changed from Dictionary<string, object> to JsonElement to handle complex nested JSON from Azure Search
    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; set; }
}
