using BlazorAiAgentTodo.Services;

namespace BlazorAiAgentTodo.Tests.Services;

public class TodoServiceTests
{
    private readonly TodoService _sut;

    public TodoServiceTests()
    {
        _sut = new TodoService();
    }

    [Fact]
    public async Task CreateTodosAsync_WithValidDescriptions_ReturnsTodoList()
    {
        // Arrange
        var descriptions = new[] { "Task 1", "Task 2", "Task 3" };

        // Act
        var result = await _sut.CreateTodosAsync(descriptions);

        // Assert
        result.Should().HaveCount(3);
        result[0].Description.Should().Be("Task 1");
        result[0].Id.Should().Be(1);
        result[1].Description.Should().Be("Task 2");
        result[1].Id.Should().Be(2);
        result[2].Description.Should().Be("Task 3");
        result[2].Id.Should().Be(3);
    }

    [Fact]
    public async Task CreateTodosAsync_WithEmptyDescriptions_ThrowsArgumentException()
    {
        // Arrange
        var descriptions = Array.Empty<string>();

        // Act
        var act = async () => await _sut.CreateTodosAsync(descriptions);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("descriptions");
    }

    [Fact]
    public async Task CreateTodosAsync_WithNullDescriptions_ThrowsArgumentException()
    {
        // Arrange
        string[]? descriptions = null;

        // Act
        var act = async () => await _sut.CreateTodosAsync(descriptions!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("descriptions");
    }

    [Fact]
    public async Task CreateTodosAsync_SkipsEmptyStrings()
    {
        // Arrange
        var descriptions = new[] { "Task 1", "", "Task 2", "   ", "Task 3" };

        // Act
        var result = await _sut.CreateTodosAsync(descriptions);

        // Assert
        result.Should().HaveCount(3);
        result.Select(t => t.Description).Should().Equal("Task 1", "Task 2", "Task 3");
    }

    [Fact]
    public async Task MarkCompleteAsync_WithValidIndex_MarksComplete()
    {
        // Arrange
        await _sut.CreateTodosAsync(new[] { "Task 1", "Task 2" });
        var completionNotes = "Completed successfully";

        // Act
        var result = await _sut.MarkCompleteAsync(1, completionNotes);

        // Assert
        result.IsCompleted.Should().BeTrue();
        result.CompletionNotes.Should().Be(completionNotes);
        result.EndTime.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkCompleteAsync_WithInvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        await _sut.CreateTodosAsync(new[] { "Task 1" });

        // Act
        var act = async () => await _sut.MarkCompleteAsync(5, "Notes");

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("index");
    }

    [Fact]
    public async Task MarkCompleteAsync_WithZeroIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        await _sut.CreateTodosAsync(new[] { "Task 1" });

        // Act
        var act = async () => await _sut.MarkCompleteAsync(0, "Notes");

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("index");
    }

    [Fact]
    public async Task MarkCompleteAsync_RecordsCompletionTime()
    {
        // Arrange
        await _sut.CreateTodosAsync(new[] { "Task 1" });
        var beforeCompletion = DateTime.UtcNow;

        // Act
        var result = await _sut.MarkCompleteAsync(1, "Done");

        // Assert
        var afterCompletion = DateTime.UtcNow;
        result.EndTime.Should().NotBeNull();
        result.EndTime!.Value.Should().BeOnOrAfter(beforeCompletion);
        result.EndTime!.Value.Should().BeOnOrBefore(afterCompletion);
    }

    [Fact]
    public async Task MarkActiveAsync_WithValidIndex_MarksActive()
    {
        // Arrange
        await _sut.CreateTodosAsync(new[] { "Task 1" });

        // Act
        var result = await _sut.MarkActiveAsync(1, "create_todos");

        // Assert
        result.IsActive.Should().BeTrue();
        result.ToolUsed.Should().Be("create_todos");
        result.StartTime.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTodos()
    {
        // Arrange
        await _sut.CreateTodosAsync(new[] { "Task 1", "Task 2" });

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllTodos()
    {
        // Arrange
        await _sut.CreateTodosAsync(new[] { "Task 1", "Task 2" });

        // Act
        await _sut.ClearAsync();
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearAsync_ResetsIdCounter()
    {
        // Arrange
        await _sut.CreateTodosAsync(new[] { "Task 1", "Task 2" });
        await _sut.ClearAsync();

        // Act
        var result = await _sut.CreateTodosAsync(new[] { "New Task" });

        // Assert
        result[0].Id.Should().Be(1);
    }
}
