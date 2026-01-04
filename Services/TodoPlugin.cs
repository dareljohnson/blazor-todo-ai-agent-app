using BlazorAiAgentTodo.Models;
using BlazorAiAgentTodo.Services.Interfaces;
using System.ComponentModel;

namespace BlazorAiAgentTodo.Services;

/// <summary>
/// Plugin that provides AI agent with tools to manage todos
/// </summary>
public class TodoPlugin
{
    private readonly ITodoService _todoService;
    private readonly IImageService _imageService;

    public TodoPlugin(ITodoService todoService, IImageService imageService)
    {
        _todoService = todoService;
        _imageService = imageService;
    }

    /// <summary>
    /// Creates one or more todos based on the provided descriptions.
    /// </summary>
    /// <param name="request">The request containing todo descriptions</param>
    /// <returns>Confirmation message</returns>
    [Description("Creates one or more todos based on the provided task descriptions")]
    public async Task<string> CreateToDosJson(
        [Description("Request object containing array of todo descriptions")] CreateTodosRequest request)
    {
        var todos = await _todoService.CreateTodosAsync(request.Descriptions);
        var createdTodos = todos.Select(t => $"- {t.Description}");

        return $"Created {todos.Count} todo(s):\n{string.Join("\n", createdTodos)}";
    }

    /// <summary>
    /// Marks a specific todo as active when beginning work on it.
    /// </summary>
    /// <param name="request">The request containing the todo index</param>
    /// <returns>Confirmation message</returns>
    [Description("Marks a todo as active/in-progress when you begin working on it")]
    public async Task<string> MarkActiveJson(
        [Description("Request object containing todo index")] MarkActiveRequest request)
    {
        try
        {
            var updatedTodo = await _todoService.MarkActiveAsync(request.Index + 1, "AI Agent");
            return $"Started working on: {updatedTodo.Description}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Marks a specific todo as complete with completion notes and timing information.
    /// </summary>
    /// <param name="request">The request containing index and completion notes</param>
    /// <returns>Confirmation message with completion details</returns>
    [Description("Marks a todo as complete with notes about how it was completed")]
    public async Task<string> MarkCompleteJson(
        [Description("Request object containing todo index and completion notes")] MarkCompleteRequest request)
    {
        try
        {
            var updatedTodo = await _todoService.MarkCompleteAsync(request.Index + 1, request.CompletionNotes);
            return $"Marked todo '{updatedTodo.Description}' as complete.\nNotes: {updatedTodo.CompletionNotes}\nDuration: {updatedTodo.GetDurationDisplay()}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Generates an image using DALL-E based on a text description.
    /// </summary>
    /// <param name="request">The request containing the image prompt</param>
    /// <returns>Confirmation message with image data URL</returns>
    [Description("Generates an image using DALL-E based on a text description")]
    public async Task<string> GenerateImageJson(
        [Description("Request object containing the image prompt")] GenerateImageRequest request)
    {
        try
        {
            var imageDataUrl = await _imageService.GenerateImageAsync(request.Prompt);
            return $"🎨 Generated image: {request.Prompt}\n[IMAGE:{imageDataUrl}]";
        }
        catch (Exception ex)
        {
            return $"Error generating image: {ex.Message}";
        }
    }
}

/// <summary>
/// Request model for creating todos
/// </summary>
public record CreateTodosRequest
{
    [Description("Array of task descriptions to create as todos")]
    [JsonPropertyName("descriptions")]
    public required string[] Descriptions { get; init; }
}

/// <summary>
/// Request model for marking todo as active
/// </summary>
public record MarkActiveRequest
{
    [Description("Zero-based index of the todo to mark as active")]
    [JsonPropertyName("index")]
    public required int Index { get; init; }
}

/// <summary>
/// Request model for marking todo as complete
/// </summary>
public record MarkCompleteRequest
{
    [Description("Zero-based index of the todo to mark as complete")]
    [JsonPropertyName("index")]
    public required int Index { get; init; }

    [Description("Notes describing how the task was completed")]
    [JsonPropertyName("completionNotes")]
    public required string CompletionNotes { get; init; }
}

/// <summary>
/// Request model for generating images
/// </summary>
public record GenerateImageRequest
{
    [Description("Detailed description of the image to generate")]
    [JsonPropertyName("prompt")]
    public required string Prompt { get; init; }
}
