using Xunit;
using FluentAssertions;
using Bunit;
using BlazorAiAgentTodo.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using BlazorAiAgentTodo.Services;
using BlazorAiAgentTodo.Services.Interfaces;
using BlazorAiAgentTodo.Models;
using Moq;
using Microsoft.Extensions.Configuration;

namespace BlazorAiAgentTodo.Tests.Components.Pages;

public class HomeFormattingTests : TestContext
{
    public HomeFormattingTests()
    {
        var mockImageService = new Mock<IImageService>();
        var chatService = new ChatService();
        var todoService = new TodoService();
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var agentService = new AgentService(todoService, chatService, mockConfig.Object);

        Services.AddSingleton<ITodoService>(todoService);
        Services.AddSingleton<IChatService>(chatService);
        Services.AddSingleton<IAgentService>(agentService);
        Services.AddSingleton(mockImageService.Object);
    }

    [Fact]
    public void FormatInlineMarkdown_Should_Handle_Inline_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "The formula is $x^2 + y^2 = r^2$ for a circle.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatInlineMarkdown", input);

        // Assert
        result.Should().Contain("$x^2 + y^2 = r^2$");
        result.Should().NotContain("&lt;"); // Math should not be HTML encoded
    }

    [Fact]
    public void FormatInlineMarkdown_Should_Handle_Display_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "Here is a formula: $$\\int_{0}^{\\infty} e^{-x^2} dx = \\frac{\\sqrt{\\pi}}{2}$$";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatInlineMarkdown", input);

        // Assert
        result.Should().Contain("$$\\int_{0}^{\\infty} e^{-x^2} dx = \\frac{\\sqrt{\\pi}}{2}$$");
        result.Should().NotContain("&lt;"); // Math should not be HTML encoded
    }

    [Fact]
    public void FormatInlineMarkdown_Should_Handle_Multi_Line_Display_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "Multi-line math:\n$$E = mc^2\nF = ma$$\nEnd.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatInlineMarkdown", input);

        // Assert
        result.Should().Contain("$$E = mc^2\nF = ma$$");
        result.Should().NotContain("&lt;"); // Math should not be HTML encoded
    }

    [Fact]
    public void FormatInlineMarkdown_Should_Handle_Bold_With_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "**Important**: The formula $E=mc^2$ is famous.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatInlineMarkdown", input);

        // Assert
        result.Should().Contain("<strong>Important</strong>");
        result.Should().Contain("$E=mc^2$");
    }

    [Fact]
    public void FormatInlineMarkdown_Should_Handle_Code_With_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "Use `calculateArea()` to find $A = \\pi r^2$.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatInlineMarkdown", input);

        // Assert
        result.Should().Contain("<code");
        result.Should().Contain("calculateArea()");
        result.Should().Contain("$A = \\pi r^2$");
    }

    [Fact]
    public void FormatReport_Should_Handle_Multi_Line_Display_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "Here is a formula:\n$$E = mc^2\nF = ma$$\nThat's it.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("$$E = mc^2\nF = ma$$");
        result.Should().NotContain("&lt;"); // Math should not be HTML encoded
    }

    [Fact]
    public void FormatReport_Should_Create_Paragraphs()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "First paragraph.\n\nSecond paragraph.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("<p>");
        result.Should().Contain("</p>");
    }

    [Fact]
    public void FormatReport_Should_Handle_Numbered_Lists()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "Steps:\n1. First step\n2. Second step";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("<ol>");
        result.Should().Contain("<li>");
        result.Should().Contain("First step");
        result.Should().Contain("Second step");
    }

    [Fact]
    public void FormatReport_Should_Handle_Numbered_List_With_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "Formulas:\n1. Calculate $E=mc^2$\n2. Use $F=ma$";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("<ol>");
        result.Should().Contain("$E=mc^2$");
        result.Should().Contain("$F=ma$");
    }

    [Fact]
    public void FormatReport_Should_Handle_Bullet_Lists_With_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "- Formula: $x^2$\n- Another: $y^2$";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("<ul>");
        result.Should().Contain("$x^2$");
        result.Should().Contain("$y^2$");
    }

    [Fact]
    public void FormatReport_Should_Handle_Multi_Line_Math_In_List()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "1. Calculate:\n$$E = mc^2\nF = ma$$\n2. Done";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("<ol>");
        result.Should().Contain("$$E = mc^2\nF = ma$$");
    }

    private T CallPrivateMethod<T>(object obj, string methodName, params object[] args)
    {
        var method = obj.GetType().GetMethod(methodName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)method!.Invoke(obj, args)!;
    }
}
