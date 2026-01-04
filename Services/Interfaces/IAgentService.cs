namespace BlazorAiAgentTodo.Services.Interfaces;

/// <summary>
/// Service for AI agent orchestration using AutoGen.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Processes a user prompt through the AI agent with tool calling.
    /// </summary>
    /// <param name="prompt">The user's problem to solve.</param>
    /// <param name="onUpdate">Callback for real-time updates during processing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The final report/solution.</returns>
    Task<string> ProcessPromptAsync(
        string prompt,
        Action<string>? onUpdate = null,
        CancellationToken cancellationToken = default);
}

