using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.ClientModel;
using Microsoft.Extensions.Options;
using ResumeChat.RagApi.Configuration;

namespace ResumeChat.RagApi.Services;

public interface IAzureOpenAIService
{
    Task<string> GetCompletionAsync(string prompt, List<string> context);
    Task<List<float>> GetEmbeddingAsync(string text);
}

public class AzureOpenAIService : IAzureOpenAIService
{
    private readonly AzureOpenAIClient _client;
    private readonly AzureOpenAIConfiguration _config;
    private readonly ILogger<AzureOpenAIService> _logger;

    public AzureOpenAIService(
        IOptions<AzureOpenAIConfiguration> config,
        ILogger<AzureOpenAIService> logger)
    {
        _config = config.Value;
        _logger = logger;
        _client = new AzureOpenAIClient(
            new Uri(_config.Endpoint),
            new ApiKeyCredential(_config.ApiKey));
    }

    public async Task<string> GetCompletionAsync(string prompt, List<string> context)
    {
        try
        {
            var systemMessage = @"You are Juan Pablo Betancourt. You are responding directly to interview questions about yourself.
Always speak in FIRST PERSON - use 'I', 'my', 'me' when talking about your experience, skills, and background.
Be conversational, professional, and authentic. Answer as if you are in a real interview.
Base all your answers on the provided context documents about your background.
If you don't have information about something in the context, politely say you don't have that information available right now.

Examples:
- 'I have 5 years of experience in...' (NOT 'He has' or 'Juan has')
- 'My expertise includes...' (NOT 'His expertise')
- 'I worked on this project...' (NOT 'He worked')";

            var contextText = string.Join("\n\n", context.Select((c, i) => $"Document {i + 1}:\n{c}"));

            var chatClient = _client.GetChatClient(_config.DeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemMessage),
                new UserChatMessage($"Context:\n{contextText}\n\nQuestion: {prompt}")
            };

            var response = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 800
            });

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion from Azure OpenAI");
            throw;
        }
    }

    public async Task<List<float>> GetEmbeddingAsync(string text)
    {
        try
        {
            var embeddingClient = _client.GetEmbeddingClient("text-embedding-ada-002");
            var response = await embeddingClient.GenerateEmbeddingAsync(text);

            return response.Value.ToFloats().ToArray().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embedding from Azure OpenAI");
            throw;
        }
    }
}
