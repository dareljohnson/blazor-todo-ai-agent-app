using BlazorAiAgentTodo.Models;

namespace BlazorAiAgentTodo.Services.Interfaces;

/// <summary>
/// Service for managing todo items.
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// Creates new todos from descriptions and returns the full list.
    /// </summary>
    Task<List<TodoItem>> CreateTodosAsync(string[] descriptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a todo as complete at the given 1-based index.
    /// </summary>
    Task<TodoItem> MarkCompleteAsync(int index, string completionNotes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a todo as active (being worked on).
    /// </summary>
    Task<TodoItem> MarkActiveAsync(int index, string toolName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all todos in the current session.
    /// </summary>
    Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all todos (for new session).
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
