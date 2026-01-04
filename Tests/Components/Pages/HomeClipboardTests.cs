using Bunit;
using BlazorAiAgentTodo.Components.Pages;
using BlazorAiAgentTodo.Models;
using BlazorAiAgentTodo.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using Xunit;
using Microsoft.JSInterop;

namespace BlazorAiAgentTodo.Tests.Components.Pages;

public class HomeClipboardTests : TestContext
{
    private readonly Mock<IAgentService> _mockAgentService;
    private readonly Mock<ITodoService> _mockTodoService;
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IJSRuntime> _mockJSRuntime;

    public HomeClipboardTests()
    {
        _mockAgentService = new Mock<IAgentService>();
        _mockTodoService = new Mock<ITodoService>();
        _mockChatService = new Mock<IChatService>();
        _mockJSRuntime = new Mock<IJSRuntime>();
        
        Services.AddSingleton(_mockAgentService.Object);
        Services.AddSingleton(_mockTodoService.Object);
        Services.AddSingleton(_mockChatService.Object);
        Services.AddSingleton(_mockJSRuntime.Object);
    }

    [Fact]
    public void Should_Render_Copy_Button_For_Each_Todo()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            new() { Id = 1, Description = "Task 1", IsActive = false, IsCompleted = true },
            new() { Id = 2, Description = "Task 2", IsActive = true, IsCompleted = false }
        };

        _mockTodoService.Setup(x => x.GetAllAsync(default)).ReturnsAsync(todos);
        _mockChatService.Setup(x => x.GetMessagesAsync(default)).ReturnsAsync(new List<ChatMessage>());

        // Act
        var cut = RenderComponent<Home>();
        cut.WaitForAssertion(() => cut.FindAll(".todo-item").Count.Should().Be(2), timeout: TimeSpan.FromSeconds(2));

        // Assert
        var copyButtons = cut.FindAll(".copy-button");
        copyButtons.Should().HaveCountGreaterOrEqualTo(2, "each todo should have a copy button");
    }


    [Fact]
    public async Task Should_Copy_Todo_Description_To_Clipboard_When_Copy_Button_Clicked()
    {
        // Arrange
        var todo = new TodoItem
        {
            Id = 1,
            Description = "Test **bold** and `code`",
            IsActive = false,
            IsCompleted = true
        };

        var todos = new List<TodoItem> { todo };
        _mockTodoService.Setup(x => x.GetAllAsync(default)).ReturnsAsync(todos);
        _mockChatService.Setup(x => x.GetMessagesAsync(default)).ReturnsAsync(new List<ChatMessage>());

        string? copiedText = null;
        _mockJSRuntime.Setup(x => x.InvokeAsync<object>(
            "clipboardCopy.copyToClipboard",
            It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) => copiedText = args[0]?.ToString())
            .ReturnsAsync(new ValueTask<object>());

        var cut = RenderComponent<Home>();

        // Act
        var copyButton = cut.Find(".copy-button");
        await copyButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        copiedText.Should().Be("Test **bold** and `code`");
    }

    [Fact]
    public async Task Should_Copy_Todo_With_Completion_Notes_To_Clipboard()
    {
        // Arrange
        var todo = new TodoItem
        {
            Id = 1,
            Description = "Task description",
            CompletionNotes = "Completed with $x^2 + y^2$",
            IsCompleted = true
        };

        var todos = new List<TodoItem> { todo };
        _mockTodoService.Setup(x => x.GetAllAsync(default)).ReturnsAsync(todos);
        _mockChatService.Setup(x => x.GetMessagesAsync(default)).ReturnsAsync(new List<ChatMessage>());

        string? copiedText = null;
        _mockJSRuntime.Setup(x => x.InvokeAsync<object>(
            "clipboardCopy.copyToClipboard",
            It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) => copiedText = args[0]?.ToString())
            .ReturnsAsync(new ValueTask<object>());

        var cut = RenderComponent<Home>();

        // Act
        var copyButton = cut.Find(".copy-button");
        await copyButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        copiedText.Should().Contain("Task description");
        copiedText.Should().Contain("Completed with $x^2 + y^2$");
    }
}
