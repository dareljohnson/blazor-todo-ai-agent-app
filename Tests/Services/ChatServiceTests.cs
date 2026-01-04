using BlazorAiAgentTodo.Models;
using BlazorAiAgentTodo.Services;

namespace BlazorAiAgentTodo.Tests.Services;

public class ChatServiceTests
{
    private readonly ChatService _sut;

    public ChatServiceTests()
    {
        _sut = new ChatService();
    }

    [Fact]
    public async Task AddMessageAsync_AddsToList()
    {
        // Arrange
        var message = ChatMessageFactory.CreateUser("Hello");

        // Act
        await _sut.AddMessageAsync(message);
        var result = await _sut.GetMessagesAsync();

        // Assert
        result.Should().ContainSingle();
        result[0].Content.Should().Be("Hello");
    }

    [Fact]
    public async Task AddMessageAsync_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        ChatMessage? message = null;

        // Act
        var act = async () => await _sut.AddMessageAsync(message!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task GetMessagesAsync_ReturnsAllMessages()
    {
        // Arrange
        var message1 = ChatMessageFactory.CreateUser("Message 1");
        var message2 = ChatMessageFactory.CreateAssistant("Message 2");
        await _sut.AddMessageAsync(message1);
        await _sut.AddMessageAsync(message2);

        // Act
        var result = await _sut.GetMessagesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllMessages()
    {
        // Arrange
        await _sut.AddMessageAsync(ChatMessageFactory.CreateUser("Test"));

        // Act
        await _sut.ClearAsync();
        var result = await _sut.GetMessagesAsync();

        // Assert
        result.Should().BeEmpty();
    }

}
