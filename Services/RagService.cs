using ResumeChat.RagApi.Models;

namespace ResumeChat.RagApi.Services;

public interface IRagService
{
    Task<ChatResponse> ProcessQueryAsync(ChatRequest request);
}

public class RagService : IRagService
{
    private readonly IAzureOpenAIService _openAIService;
    private readonly IAzureSearchService _searchService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IAzureOpenAIService openAIService,
        IAzureSearchService searchService,
        ISessionService sessionService,
        ILogger<RagService> logger)
    {
        _openAIService = openAIService;
        _searchService = searchService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessQueryAsync(ChatRequest request)
    {
        try
        {
            // Get or create session
            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
            var session = _sessionService.GetSession(sessionId);
            
            // Search for relevant documents
            var searchResults = await _searchService.SearchAsync(request.Message, maxResults: 5);
            
            // Extract context from search results
            var context = searchResults.Select(doc => doc.Content).ToList();
            
            // Get AI completion
            var aiResponse = await _openAIService.GetCompletionAsync(request.Message, context);
            
            // Prepare document sources
            var sources = searchResults.Select(doc => new DocumentSource
            {
                FileName = doc.FileName,
                Content = TruncateContent(doc.Content, 200),
                RelevanceScore = 0.85 // Simplified - would come from search score
            }).ToList();
            
            // Update session
            session.Messages.Add(new ChatMessage
            {
                Content = request.Message,
                IsUser = true,
                Timestamp = DateTime.UtcNow
            });
            
            session.Messages.Add(new ChatMessage
            {
                Content = aiResponse,
                IsUser = false,
                Timestamp = DateTime.UtcNow,
                Sources = sources
            });
            
            session.LastActivity = DateTime.UtcNow;
            _sessionService.UpdateSession(session);
            
            return new ChatResponse
            {
                Message = aiResponse,
                Sources = sources,
                SessionId = sessionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RAG query");
            throw;
        }
    }

    private string TruncateContent(string content, int maxLength)
    {
        if (content.Length <= maxLength)
            return content;
        
        return content.Substring(0, maxLength) + "...";
    }
}
