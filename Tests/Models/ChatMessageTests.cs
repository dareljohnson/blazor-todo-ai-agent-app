using BlazorAiAgentTodo.Models;

namespace BlazorAiAgentTodo.Tests.Models;

public class ChatMessageTests
{
    [Fact]
    public void ChatMessageFactory_CreateUser_ReturnsValidMessage()
    {
        // Act
        var result = ChatMessageFactory.CreateUser("Hello");

        // Assert
        result.Role.Should().Be(MessageRole.User);
        result.Content.Should().Be("Hello");
        result.IsStreaming.Should().BeFalse();
        result.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ChatMessageFactory_CreateUser_WithInvalidContent_ThrowsArgumentException(string? content)
    {
        // Act
        var act = () => ChatMessageFactory.CreateUser(content!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("content");
    }

    [Fact]
    public void ChatMessageFactory_CreateAssistant_ReturnsValidMessage()
    {
        // Act
        var result = ChatMessageFactory.CreateAssistant("Response", isStreaming: true);

        // Assert
        result.Role.Should().Be(MessageRole.Assistant);
        result.Content.Should().Be("Response");
        result.IsStreaming.Should().BeTrue();
    }

    [Fact]
    public void ChatMessageFactory_CreateSystem_ReturnsValidMessage()
    {
        // Act
        var result = ChatMessageFactory.CreateSystem("System message");

        // Assert
        result.Role.Should().Be(MessageRole.System);
        result.Content.Should().Be("System message");
    }

}
