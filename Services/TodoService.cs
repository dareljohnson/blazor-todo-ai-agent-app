using BlazorAiAgentTodo.Models;
using BlazorAiAgentTodo.Services.Interfaces;

namespace BlazorAiAgentTodo.Services;

/// <summary>
/// In-memory implementation of ITodoService.
/// Thread-safe for concurrent operations.
/// </summary>
public sealed class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private int _nextId = 1;

    public async Task<List<TodoItem>> CreateTodosAsync(string[] descriptions, CancellationToken cancellationToken = default)
    {
        if (descriptions == null || descriptions.Length == 0)
            throw new ArgumentException("Descriptions cannot be null or empty.", nameof(descriptions));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            foreach (var description in descriptions)
            {
                if (string.IsNullOrWhiteSpace(description))
                    continue;

                var todo = TodoItemFactory.Create(_nextId++, description.Trim());
                _todos.Add(todo);
            }

            return new List<TodoItem>(_todos);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<TodoItem> MarkCompleteAsync(int index, string completionNotes, CancellationToken cancellationToken = default)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be 1 or greater.");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var arrayIndex = index - 1;
            if (arrayIndex >= _todos.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Only {_todos.Count} todos exist.");

            var todo = _todos[arrayIndex];
            var completedTodo = TodoItemFactory.MarkAsCompleted(todo, completionNotes);
            _todos[arrayIndex] = completedTodo;

            return completedTodo;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<TodoItem> MarkActiveAsync(int index, string toolName, CancellationToken cancellationToken = default)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be 1 or greater.");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var arrayIndex = index - 1;
            if (arrayIndex >= _todos.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Only {_todos.Count} todos exist.");

            var todo = _todos[arrayIndex];
            var activeTodo = TodoItemFactory.MarkAsActive(todo, toolName);
            _todos[arrayIndex] = activeTodo;

            return activeTodo;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return new List<TodoItem>(_todos);
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
            _todos.Clear();
            _nextId = 1;
        }
        finally
        {
            _lock.Release();
        }
    }
}
