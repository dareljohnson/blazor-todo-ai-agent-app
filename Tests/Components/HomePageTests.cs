using BlazorAiAgentTodo.Components.Pages;
using BlazorAiAgentTodo.Services.Interfaces;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using FluentAssertions;
using Xunit;
using BlazorAiAgentTodo.Models;

namespace BlazorAiAgentTodo.Tests.Components;

public class HomePageTests : TestContext
{
    private readonly Mock<IAgentService> _mockAgentService;
    private readonly Mock<ITodoService> _mockTodoService;
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IJSRuntime> _mockJSRuntime;

    public HomePageTests()
    {
        _mockAgentService = new Mock<IAgentService>();
        _mockTodoService = new Mock<ITodoService>();
        _mockChatService = new Mock<IChatService>();
        _mockJSRuntime = new Mock<IJSRuntime>();

        // Setup default returns
        _mockTodoService.Setup(x => x.GetAllAsync(default))
            .ReturnsAsync(new List<TodoItem>());
        
        _mockChatService.Setup(x => x.GetMessagesAsync(default))
            .ReturnsAsync(new List<ChatMessage>());

        // Register services
        Services.AddSingleton(_mockAgentService.Object);
        Services.AddSingleton(_mockTodoService.Object);
        Services.AddSingleton(_mockChatService.Object);
        Services.AddSingleton(_mockJSRuntime.Object);
    }

    [Fact]
    public void Home_RendersWithoutErrors()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Markup.Should().Contain("AI Agent Todo App");
        cut.Markup.Should().Contain("Agent Tasks");
        cut.Markup.Should().Contain("Conversation");
    }

    [Fact]
    public void Home_InitialState_ShowsEmptyStates()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Markup.Should().Contain("No tasks yet");
        cut.Markup.Should().Contain("Send a message to start chatting");
    }

    [Fact]
    public void Home_WithTodos_DisplaysTodoList()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            TodoItemFactory.Create(1, "Test task 1"),
            TodoItemFactory.Create(2, "Test task 2")
        };
        
        _mockTodoService.Setup(x => x.GetAllAsync(default))
            .ReturnsAsync(todos);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Markup.Should().Contain("Test task 1");
        cut.Markup.Should().Contain("Test task 2");
    }

    [Fact]
    public void Home_ShowsAppropriateEmptyStateMessages()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Markup.Should().Contain("No tasks yet");
        cut.Markup.Should().Contain("Start a conversation");
    }

    [Fact]
    public void Home_HasChatInputTextarea()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var textarea = cut.Find(".chat-input");
        textarea.Should().NotBeNull();
        textarea.GetAttribute("placeholder").Should().Contain("calculations, planning");
    }

    [Fact]
    public void Home_EmptyPrompt_DisablesSendButton()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert - Button should be disabled with empty prompt
        var sendButton = cut.Find(".btn-primary");
        sendButton.GetAttribute("disabled").Should().NotBeNull();
    }

    [Fact]
    public void Home_NewChatButton_ClearsMessages()
    {
        // Arrange
        _mockTodoService.Setup(x => x.ClearAsync(default))
            .Returns(Task.CompletedTask);

        var cut = RenderComponent<Home>();

        // Act - Click new chat button
        var newChatButton = cut.Find(".btn-secondary");
        newChatButton.Click();

        // Assert - Verify ClearAsync was called
        _mockTodoService.Verify(x => x.ClearAsync(default), Times.Once);
    }

    [Fact]
    public void Home_DisplaysTodoStatusBadge()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            TodoItemFactory.Create(1, "Test 1"),
            TodoItemFactory.MarkAsCompleted(TodoItemFactory.Create(2, "Test 2"), "Done")
        };
        
        _mockTodoService.Setup(x => x.GetAllAsync(default))
            .ReturnsAsync(todos);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Markup.Should().Contain("1/2"); // 1 completed out of 2 total
    }

    [Fact]
    public void Home_DisplaysCorrectTitle()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Markup.Should().Contain("🤖 AI Agent Todo App");
        cut.Markup.Should().Contain("Powered by OpenAI GPT-5.2");
    }

    [Fact]
    public void Home_HasNewChatButton()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var newChatButton = cut.Find(".btn-secondary");
        newChatButton.TextContent.Should().Contain("New Chat");
    }
}
