using BlazorAiAgentTodo.Models;

namespace BlazorAiAgentTodo.Tests.Models;

public class TodoItemTests
{
    [Fact]
    public void TodoItemFactory_Create_ReturnsValidTodoItem()
    {
        // Act
        var result = TodoItemFactory.Create(1, "Test task");

        // Assert
        result.Id.Should().Be(1);
        result.Description.Should().Be("Test task");
        result.IsActive.Should().BeFalse();
        result.IsCompleted.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TodoItemFactory_Create_WithInvalidDescription_ThrowsArgumentException(string? description)
    {
        // Act
        var act = () => TodoItemFactory.Create(1, description!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    [Fact]
    public void TodoItemFactory_MarkAsActive_SetsActiveState()
    {
        // Arrange
        var todo = TodoItemFactory.Create(1, "Test");

        // Act
        var result = TodoItemFactory.MarkAsActive(todo, "test_tool");

        // Assert
        result.IsActive.Should().BeTrue();
        result.ToolUsed.Should().Be("test_tool");
        result.StartTime.Should().NotBeNull();
    }

    [Fact]
    public void TodoItemFactory_MarkAsCompleted_SetsCompletedState()
    {
        // Arrange
        var todo = TodoItemFactory.Create(1, "Test");

        // Act
        var result = TodoItemFactory.MarkAsCompleted(todo, "Done successfully");

        // Assert
        result.IsCompleted.Should().BeTrue();
        result.IsActive.Should().BeFalse();
        result.CompletionNotes.Should().Be("Done successfully");
        result.EndTime.Should().NotBeNull();
    }

    [Fact]
    public void TodoItemFactory_MarkAsCompleted_OnCompletedTodo_ThrowsInvalidOperationException()
    {
        // Arrange
        var todo = TodoItemFactory.Create(1, "Test");
        var completedTodo = TodoItemFactory.MarkAsCompleted(todo, "Done");

        // Act
        var act = () => TodoItemFactory.MarkAsActive(completedTodo, "tool");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TodoItem_Duration_CalculatesCorrectly()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddSeconds(5);
        var todo = new TodoItem
        {
            Id = 1,
            Description = "Test",
            StartTime = startTime,
            EndTime = endTime
        };

        // Act
        var duration = todo.Duration;

        // Assert
        duration.Should().NotBeNull();
        duration!.Value.TotalSeconds.Should().BeApproximately(5, 0.1);
    }

    [Fact]
    public void TodoItemExtensions_GetStatusBadge_ReturnsCorrectStatus()
    {
        // Arrange
        var pending = TodoItemFactory.Create(1, "Test");
        var active = TodoItemFactory.MarkAsActive(pending, "tool");
        var completed = TodoItemFactory.MarkAsCompleted(pending, "Done");

        // Act & Assert
        pending.GetStatusBadge().Should().Contain("Pending");
        active.GetStatusBadge().Should().Contain("Progress");
        completed.GetStatusBadge().Should().Contain("Completed");
    }

}
