using BlazorAiAgentTodo.Services;
using BlazorAiAgentTodo.Services.Interfaces;
using BlazorAiAgentTodo.Models;
using Moq;
using FluentAssertions;
using Xunit;

namespace BlazorAiAgentTodo.Tests.Services;

public class TodoPluginTests
{
    private readonly Mock<ITodoService> _mockTodoService;
    private readonly Mock<IImageService> _mockImageService;
    private readonly TodoPlugin _plugin;

    public TodoPluginTests()
    {
        _mockTodoService = new Mock<ITodoService>();
        _mockImageService = new Mock<IImageService>();
        _plugin = new TodoPlugin(_mockTodoService.Object, _mockImageService.Object);
    }

    [Fact]
    public async Task CreateToDosJson_ShouldCreateTodosAndReturnConfirmation()
    {
        // Arrange
        var descriptions = new[] { "Task 1", "Task 2", "Task 3" };
        var request = new CreateTodosRequest { Descriptions = descriptions };
        
        var expectedTodos = new List<TodoItem>
        {
            TodoItemFactory.Create(1, "Task 1"),
            TodoItemFactory.Create(2, "Task 2"),
            TodoItemFactory.Create(3, "Task 3")
        };

        _mockTodoService
            .Setup(x => x.CreateTodosAsync(descriptions, default))
            .ReturnsAsync(expectedTodos);

        // Act
        var result = await _plugin.CreateToDosJson(request);

        // Assert
        result.Should().Contain("Created 3 todo(s)");
        result.Should().Contain("- Task 1");
        result.Should().Contain("- Task 2");
        result.Should().Contain("- Task 3");
        
        _mockTodoService.Verify(x => x.CreateTodosAsync(descriptions, default), Times.Once);
    }

    [Fact]
    public async Task MarkCompleteJson_ShouldMarkTodoAsActiveAndThenComplete()
    {
        // Arrange
        var request = new MarkCompleteRequest 
        { 
            Index = 0, 
            CompletionNotes = "Task completed successfully" 
        };

        var activeTodo = TodoItemFactory.Create(1, "Test Task");
        activeTodo = TodoItemFactory.MarkAsActive(activeTodo, "AI Agent");
        
        var completedTodo = TodoItemFactory.MarkAsCompleted(activeTodo, "Task completed successfully");

        _mockTodoService
            .Setup(x => x.MarkActiveAsync(1, "AI Agent", default))
            .ReturnsAsync(activeTodo);

        _mockTodoService
            .Setup(x => x.MarkCompleteAsync(1, "Task completed successfully", default))
            .ReturnsAsync(completedTodo);

        // Act
        var result = await _plugin.MarkCompleteJson(request);

        // Assert
        result.Should().Contain("Marked todo 'Test Task' as complete");
        result.Should().Contain("Notes: Task completed successfully");
        
        _mockTodoService.Verify(x => x.MarkActiveAsync(1, "AI Agent", default), Times.Once);
        _mockTodoService.Verify(x => x.MarkCompleteAsync(1, "Task completed successfully", default), Times.Once);
    }

    [Fact]
    public async Task MarkCompleteJson_ShouldHandleErrorsGracefully()
    {
        // Arrange
        var request = new MarkCompleteRequest 
        { 
            Index = 0, 
            CompletionNotes = "Test notes" 
        };

        _mockTodoService
            .Setup(x => x.MarkActiveAsync(1, "AI Agent", default))
            .ThrowsAsync(new InvalidOperationException("Todo not found"));

        // Act
        var result = await _plugin.MarkCompleteJson(request);

        // Assert
        result.Should().Contain("Error:");
        result.Should().Contain("Todo not found");
    }

    [Fact]
    public async Task CreateToDosJson_ShouldHandleEmptyDescriptionsArray()
    {
        // Arrange
        var request = new CreateTodosRequest { Descriptions = Array.Empty<string>() };
        
        _mockTodoService
            .Setup(x => x.CreateTodosAsync(It.IsAny<string[]>(), default))
            .ReturnsAsync(new List<TodoItem>());

        // Act
        var result = await _plugin.CreateToDosJson(request);

        // Assert
        result.Should().Contain("Created 0 todo(s)");
    }

    [Fact]
    public async Task MarkCompleteJson_ShouldUseZeroBasedIndexing()
    {
        // Arrange - Request index 0 should map to todo index 1 (1-based in service)
        var request = new MarkCompleteRequest 
        { 
            Index = 0, 
            CompletionNotes = "First task done" 
        };

        var todo = TodoItemFactory.Create(1, "First Task");
        todo = TodoItemFactory.MarkAsActive(todo, "AI Agent");
        todo = TodoItemFactory.MarkAsCompleted(todo, "First task done");

        _mockTodoService
            .Setup(x => x.MarkActiveAsync(1, "AI Agent", default))
            .ReturnsAsync(todo);

        _mockTodoService
            .Setup(x => x.MarkCompleteAsync(1, "First task done", default))
            .ReturnsAsync(todo);

        // Act
        await _plugin.MarkCompleteJson(request);

        // Assert
        _mockTodoService.Verify(x => x.MarkActiveAsync(1, "AI Agent", default), Times.Once);
        _mockTodoService.Verify(x => x.MarkCompleteAsync(1, "First task done", default), Times.Once);
    }
}
