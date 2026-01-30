using Microsoft.AspNetCore.SignalR;
using ResumeChat.RagApi.Models;

namespace ResumeChat.RagApi.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public async Task SendMessage(string message, string sessionId)
    {
        _logger.LogInformation("Received message from client: {Message}", message);
        
        // This would typically be processed through your RAG service
        // For now, we'll just echo back
        var response = new ChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            Content = message,
            IsUser = false,
            Timestamp = DateTime.UtcNow
        };
        
        await Clients.Caller.SendAsync("ReceiveMessage", response);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
