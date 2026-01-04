using BlazorAiAgentTodo.Models;
using BlazorAiAgentTodo.Services.Interfaces;

namespace BlazorAiAgentTodo.Services;

/// <summary>
/// In-memory implementation of IChatService.
/// Thread-safe for concurrent operations.
/// </summary>
public sealed class ChatService : IChatService
{
    private readonly List<ChatMessage> _messages = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            _messages.Add(message);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return new List<ChatMessage>(_messages);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            _messages.Clear();
        }
        finally
        {
            _lock.Release();
        }
    }
}
