namespace BlazorAiAgentTodo.Models;

/// <summary>
/// Represents a todo item in the agent's workflow.
/// </summary>
public sealed record TodoItem
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public bool IsActive { get; init; }
    public bool IsCompleted { get; init; }
    public string? ToolUsed { get; init; }
    public string? CompletionNotes { get; init; }
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    
    public TimeSpan? Duration => EndTime.HasValue && StartTime.HasValue 
        ? EndTime.Value - StartTime.Value 
        : null;
}

/// <summary>
/// Factory for creating TodoItem instances with validation.
/// </summary>
public static class TodoItemFactory
{
    public static TodoItem Create(int id, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        return new TodoItem
        {
            Id = id,
            Description = description,
            IsActive = false,
            IsCompleted = false,
            ToolUsed = null,
            CompletionNotes = null,
            StartTime = null,
            EndTime = null
        };
    }

    public static TodoItem MarkAsActive(TodoItem item, string toolName)
    {
        if (item.IsCompleted)
            throw new InvalidOperationException("Cannot mark a completed todo as active.");

        return item with
        {
            IsActive = true,
            ToolUsed = toolName,
            StartTime = item.StartTime ?? DateTime.UtcNow
        };
    }

    public static TodoItem MarkAsCompleted(TodoItem item, string completionNotes)
    {
        if (string.IsNullOrWhiteSpace(completionNotes))
            throw new ArgumentException("Completion notes cannot be empty.", nameof(completionNotes));

        return item with
        {
            IsActive = false,
            IsCompleted = true,
            CompletionNotes = completionNotes,
            EndTime = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Extension methods for TodoItem display and formatting.
/// </summary>
public static class TodoItemExtensions
{
    public static string GetStatusBadge(this TodoItem item)
    {
        return item switch
        {
            { IsCompleted: true } => "✓ Completed",
            { IsActive: true } => "⚙️ In Progress",
            _ => "⏳ Pending"
        };
    }

    public static string GetDurationDisplay(this TodoItem item)
    {
        if (!item.Duration.HasValue)
            return item.IsActive ? "In progress..." : "--";

        var duration = item.Duration.Value;
        return duration.TotalSeconds < 60
            ? $"{duration.TotalSeconds:F1}s"
            : $"{duration.TotalMinutes:F1}m";
    }
}
