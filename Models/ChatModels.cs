namespace ResumeChat.RagApi.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<DocumentSource>? Sources { get; set; }
}

public class DocumentSource
{
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public List<DocumentSource> Sources { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;
}

public class ChatSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public List<ChatMessage> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}
