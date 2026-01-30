using Microsoft.AspNetCore.Mvc;
using ResumeChat.RagApi.Models;
using ResumeChat.RagApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace ResumeChat.RagApi.Controllers;

/// <summary>
/// Chat controller for Resume RAG chatbot interactions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IRagService ragService,
        ISessionService sessionService,
        ILogger<ChatController> logger)
    {
        _ragService = ragService;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to the RAG chatbot
    /// </summary>
    /// <param name="request">The chat request containing the user message and session ID</param>
    /// <returns>AI-generated response based on resume context</returns>
    /// <response code="200">Returns the chat response</response>
    /// <response code="400">If the message is empty</response>
    /// <response code="500">If an error occurs processing the message</response>
    [HttpPost("message")]
    [SwaggerOperation(Summary = "Send a chat message", Description = "Sends a message to the AI chatbot and receives a context-aware response")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var response = await _ragService.ProcessQueryAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, "An error occurred processing your message");
        }
    }

    /// <summary>
    /// Create a new chat session
    /// </summary>
    /// <returns>A new session ID</returns>
    /// <response code="200">Returns the new session ID</response>
    /// <response code="500">If an error occurs creating the session</response>
    [HttpPost("session")]
    [SwaggerOperation(Summary = "Create a new session", Description = "Creates a new chat session and returns a unique session ID")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public ActionResult<object> CreateSession()
    {
        try
        {
            var sessionId = _sessionService.CreateSession();
            return Ok(new { sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, "An error occurred creating the session");
        }
    }

    /// <summary>
    /// Get chat history for a session
    /// </summary>
    /// <param name="sessionId">The session ID to retrieve history for</param>
    /// <returns>List of chat messages in the session</returns>
    /// <response code="200">Returns the chat history</response>
    /// <response code="500">If an error occurs retrieving the history</response>
    [HttpGet("history/{sessionId}")]
    [SwaggerOperation(Summary = "Get chat history", Description = "Retrieves the complete message history for a specific chat session")]
    [ProducesResponseType(typeof(List<ChatMessage>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public ActionResult<List<ChatMessage>> GetHistory(string sessionId)
    {
        try
        {
            var session = _sessionService.GetSession(sessionId);
            return Ok(session.Messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session history");
            return StatusCode(500, "An error occurred retrieving the history");
        }
    }
}
