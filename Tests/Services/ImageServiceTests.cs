using BlazorAiAgentTodo.Services;
using Microsoft.Extensions.Logging;
using OpenAI.Images;

namespace BlazorAiAgentTodo.Tests.Services;

public class ImageServiceTests
{
    private readonly Mock<ImageClient> _mockImageClient;
    private readonly Mock<ILogger<ImageService>> _mockLogger;

    public ImageServiceTests()
    {
        _mockImageClient = new Mock<ImageClient>();
        _mockLogger = new Mock<ILogger<ImageService>>();
    }

    [Fact]
    public void Constructor_WithNullImageClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ImageService(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("imageClient");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ImageService(_mockImageClient.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task GenerateImageAsync_WithNullPrompt_ThrowsArgumentException()
    {
        // Arrange
        var service = new ImageService(_mockImageClient.Object, _mockLogger.Object);

        // Act & Assert
        var act = async () => await service.GenerateImageAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Prompt cannot be null or whitespace.*");
    }

    [Fact]
    public async Task GenerateImageAsync_WithEmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var service = new ImageService(_mockImageClient.Object, _mockLogger.Object);

        // Act & Assert
        var act = async () => await service.GenerateImageAsync("");
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Prompt cannot be null or whitespace.*");
    }

    [Fact]
    public async Task GenerateImageAsync_WithWhitespacePrompt_ThrowsArgumentException()
    {
        // Arrange
        var service = new ImageService(_mockImageClient.Object, _mockLogger.Object);

        // Act & Assert
        var act = async () => await service.GenerateImageAsync("   ");
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Prompt cannot be null or whitespace.*");
    }

    [Fact]
    public async Task GenerateImageAsync_WithTooLongPrompt_ThrowsArgumentException()
    {
        // Arrange
        var service = new ImageService(_mockImageClient.Object, _mockLogger.Object);
        var longPrompt = new string('a', 1001);

        // Act & Assert
        var act = async () => await service.GenerateImageAsync(longPrompt);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Prompt cannot exceed 1000 characters.*");
    }

    [Fact]
    public async Task GenerateImageAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var service = new ImageService(_mockImageClient.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var act = async () => await service.GenerateImageAsync("test prompt", cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
