using BlazorAiAgentTodo.Services;
using BlazorAiAgentTodo.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlazorAiAgentTodo.Tests.Services;

public class AgentServiceTests
{
    private readonly Mock<ITodoService> _mockTodoService;
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public AgentServiceTests()
    {
        _mockTodoService = new Mock<ITodoService>();
        _mockChatService = new Mock<IChatService>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup default configuration
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("sk-test-key-12345");
    }

    [Fact]
    public void Constructor_WithValidConfiguration_Succeeds()
    {
        // Act
        var act = () => new AgentService(_mockTodoService.Object, _mockChatService.Object, _mockConfiguration.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullTodoService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AgentService(null!, _mockChatService.Object, _mockConfiguration.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("todoService");
    }

    [Fact]
    public void Constructor_WithNullChatService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AgentService(_mockTodoService.Object, null!, _mockConfiguration.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("chatService");
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AgentService(_mockTodoService.Object, _mockChatService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_WithMissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var emptyConfig = new Mock<IConfiguration>();
        emptyConfig.Setup(c => c["OpenAI:ApiKey"]).Returns((string?)null);

        // Act
        var act = () => new AgentService(_mockTodoService.Object, _mockChatService.Object, emptyConfig.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*API key*");
    }

    [Fact]
    public async Task ProcessPromptAsync_WithNullPrompt_ThrowsArgumentException()
    {
        // Arrange
        var sut = new AgentService(_mockTodoService.Object, _mockChatService.Object, _mockConfiguration.Object);

        // Act
        var act = async () => await sut.ProcessPromptAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Prompt*");
    }

    [Fact]
    public async Task ProcessPromptAsync_WithEmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var sut = new AgentService(_mockTodoService.Object, _mockChatService.Object, _mockConfiguration.Object);

        // Act
        var act = async () => await sut.ProcessPromptAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Prompt*");
    }

    [Fact]
    public async Task ProcessPromptAsync_WithTooLongPrompt_ThrowsArgumentException()
    {
        // Arrange
        var sut = new AgentService(_mockTodoService.Object, _mockChatService.Object, _mockConfiguration.Object);
        var longPrompt = new string('x', 10001);

        // Act
        var act = async () => await sut.ProcessPromptAsync(longPrompt);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*maximum length*");
    }

    [Fact]
    public async Task ProcessPromptAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var sut = new AgentService(_mockTodoService.Object, _mockChatService.Object, _mockConfiguration.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await sut.ProcessPromptAsync("test", cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
