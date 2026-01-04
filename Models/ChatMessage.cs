namespace BlazorAiAgentTodo.Models;

/// <summary>
/// Represents a chat message in the conversation.
/// </summary>
public sealed record ChatMessage
{
    public required Guid Id { get; init; }
    public required MessageRole Role { get; init; }
    public required string Content { get; init; }
    public required DateTime Timestamp { get; init; }
    public bool IsStreaming { get; init; }
}

/// <summary>
/// The role of a chat message sender.
/// </summary>
public enum MessageRole
{
    User,
    Assistant,
    System
}

/// <summary>
/// Factory for creating ChatMessage instances.
/// </summary>
public static class ChatMessageFactory
{
    public static ChatMessage CreateUser(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            Role = MessageRole.User,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsStreaming = false
        };
    }

    public static ChatMessage CreateAssistant(string content, bool isStreaming = false)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            Role = MessageRole.Assistant,
            Content = content ?? string.Empty,
            Timestamp = DateTime.UtcNow,
            IsStreaming = isStreaming
        };
    }

    public static ChatMessage CreateSystem(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            Role = MessageRole.System,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsStreaming = false
        };
    }

}
