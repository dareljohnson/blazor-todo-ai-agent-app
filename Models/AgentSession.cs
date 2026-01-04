namespace BlazorAiAgentTodo.Models;

/// <summary>
/// Represents a complete agent session with chat and todos.
/// </summary>
public sealed record AgentSession
{
    public required Guid SessionId { get; init; }
    public required ImmutableList<ChatMessage> Messages { get; init; }
    public required ImmutableList<TodoItem> Todos { get; init; }
    public string? CurrentReport { get; init; }
    public bool IsProcessing { get; init; }
}

/// <summary>
/// Factory for creating AgentSession instances.
/// </summary>
public static class AgentSessionFactory
{
    public static AgentSession CreateNew()
    {
        return new AgentSession
        {
            SessionId = Guid.NewGuid(),
            Messages = ImmutableList<ChatMessage>.Empty,
            Todos = ImmutableList<TodoItem>.Empty,
            CurrentReport = null,
            IsProcessing = false
        };
    }
}
