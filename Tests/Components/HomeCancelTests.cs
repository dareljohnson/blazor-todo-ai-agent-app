using BlazorAiAgentTodo.Components.Pages;
using BlazorAiAgentTodo.Models;
using BlazorAiAgentTodo.Services.Interfaces;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlazorAiAgentTodo.Tests.Components;

public class HomeCancelTests : TestContext
{
    private readonly Mock<IAgentService> _mockAgentService;
    private readonly Mock<ITodoService> _mockTodoService;
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IJSRuntime> _mockJSRuntime;

    public HomeCancelTests()
    {
        _mockAgentService = new Mock<IAgentService>();
        _mockTodoService = new Mock<ITodoService>();
        _mockChatService = new Mock<IChatService>();
        _mockJSRuntime = new Mock<IJSRuntime>();

        // Setup default returns
        _mockTodoService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TodoItem>());

        _mockChatService.Setup(x => x.GetMessagesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessage>());

        // Register services
        Services.AddSingleton(_mockAgentService.Object);
        Services.AddSingleton(_mockTodoService.Object);
        Services.AddSingleton(_mockChatService.Object);
        Services.AddSingleton(_mockJSRuntime.Object);
    }

    [Fact]
    public void CancelButton_NotVisible_WhenNotProcessing()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert - Cancel button should not be visible
        var cancelButtons = cut.FindAll(".btn-cancel");
        cancelButtons.Should().BeEmpty();
    }

    [Fact]
    public async Task CancelButton_Visible_WhenProcessing()
    {
        // Arrange
        var tcs = new TaskCompletionSource<string>();
        _mockAgentService.Setup(x => x.ProcessPromptAsync(It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cut = RenderComponent<Home>();

        // Act - Trigger processing by entering text and clicking send
        var textarea = cut.Find(".chat-input");
        textarea.Input("Test prompt");
        
        var sendButton = cut.Find(".btn-primary");
        await cut.InvokeAsync(() => sendButton.Click());

        // Assert - Cancel button should be visible
        var cancelButton = cut.Find(".btn-cancel");
        cancelButton.Should().NotBeNull();
        cancelButton.TextContent.Should().Contain("Cancel");

        // Cleanup
        tcs.SetResult("Test response");
    }

    [Fact]
    public async Task CancelButton_Click_StopsProcessing()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var processingStarted = false;
        var processingCancelled = false;

        _mockAgentService.Setup(x => x.ProcessPromptAsync(It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
            .Returns(async (string prompt, Action<string>? onUpdate, CancellationToken ct) =>
            {
                processingStarted = true;
                try
                {
                    await Task.Delay(10000, ct); // Long delay to allow cancellation
                    return "Should not reach here";
                }
                catch (OperationCanceledException)
                {
                    processingCancelled = true;
                    throw;
                }
            });

        var cut = RenderComponent<Home>();

        // Act - Start processing
        var textarea = cut.Find(".chat-input");
        textarea.Input("Test prompt");
        
        var sendButton = cut.Find(".btn-primary");
        var processingTask = cut.InvokeAsync(() => sendButton.Click());

        // Wait a bit for processing to start
        await Task.Delay(100);

        // Click cancel button
        var cancelButton = cut.Find(".btn-cancel");
        await cut.InvokeAsync(() => cancelButton.Click());

        // Wait for processing to complete
        await Task.Delay(200);

        // Assert
        processingStarted.Should().BeTrue();
        processingCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task CancelButton_Click_ReEnablesSendButton()
    {
        // Arrange
        var tcs = new TaskCompletionSource<string>();
        _mockAgentService.Setup(x => x.ProcessPromptAsync(It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cut = RenderComponent<Home>();

        // Act - Start processing
        var textarea = cut.Find(".chat-input");
        textarea.Input("Test prompt");
        
        var sendButton = cut.Find(".btn-primary");
        await cut.InvokeAsync(() => sendButton.Click());

        // Verify send button is disabled during processing
        sendButton.GetAttribute("disabled").Should().NotBeNull();

        // Click cancel button
        var cancelButton = cut.Find(".btn-cancel");
        await cut.InvokeAsync(() => cancelButton.Click());

        // Complete the task
        tcs.SetCanceled();
        await Task.Delay(100);

        // Assert - Send button should be enabled again (with text)
        textarea.Input("New prompt");
        sendButton = cut.Find(".btn-primary");
        sendButton.GetAttribute("disabled").Should().BeNull();
    }

    [Fact]
    public async Task CancelButton_Click_AddsErrorMessage()
    {
        // Arrange
        var tcs = new TaskCompletionSource<string>();
        _mockAgentService.Setup(x => x.ProcessPromptAsync(It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
            .Returns((string prompt, Action<string>? onUpdate, CancellationToken ct) =>
            {
                ct.Register(() => tcs.SetCanceled());
                return tcs.Task;
            });

        var cut = RenderComponent<Home>();

        // Act - Start processing
        var textarea = cut.Find(".chat-input");
        textarea.Input("Test prompt");
        
        var sendButton = cut.Find(".btn-primary");
        await cut.InvokeAsync(() => sendButton.Click());

        // Click cancel button
        var cancelButton = cut.Find(".btn-cancel");
        await cut.InvokeAsync(() => cancelButton.Click());

        await Task.Delay(100);

        // Assert - Should show cancellation message
        cut.Markup.Should().Contain("cancelled");
    }

    [Fact]
    public void SendButton_Disabled_WhenProcessing()
    {
        // Arrange
        var tcs = new TaskCompletionSource<string>();
        _mockAgentService.Setup(x => x.ProcessPromptAsync(It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cut = RenderComponent<Home>();

        // Act - Start processing
        var textarea = cut.Find(".chat-input");
        textarea.Input("Test prompt");
        
        var sendButton = cut.Find(".btn-primary");
        cut.InvokeAsync(() => sendButton.Click());

        // Assert - Send button should be disabled
        sendButton.GetAttribute("disabled").Should().NotBeNull();

        // Cleanup
        tcs.SetResult("Test response");
    }
}
