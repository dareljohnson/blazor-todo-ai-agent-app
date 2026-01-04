using BlazorAiAgentTodo.Models;

namespace BlazorAiAgentTodo.Services.Interfaces;

/// <summary>
/// Service for managing chat messages.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Adds a message to the chat history.
    /// </summary>
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all messages in the current chat session.
    /// </summary>
    Task<List<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all messages (for new chat).
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}

