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
        result.Should().Contain("<p style='margin-bottom: 0.75rem;'>");
        result.Should().Contain("</p>");
        result.Should().Contain("First paragraph");
        result.Should().Contain("Second paragraph");
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
        result.Should().Contain("<ol style='margin-left: 1.5rem; margin-bottom: 1rem;'>");
        result.Should().Contain("<li style='margin-bottom: 0.5rem;'>");
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
        result.Should().Contain("<ol style='margin-left: 1.5rem; margin-bottom: 1rem;'>");
        result.Should().Contain("$E=mc^2$");
        result.Should().Contain("$F=ma$");
    }

    [Fact(Skip = "Bullet lists not yet implemented in FormatReport")]
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
        result.Should().Contain("<ol style='margin-left: 1.5rem; margin-bottom: 1rem;'>");
        result.Should().Contain("$$E = mc^2\nF = ma$$");
    }

    [Fact]
    public void FormatReport_Should_Not_Treat_Currency_As_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "The laptop costs $1,299 and the tablet costs $799.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("$1,299");
        result.Should().Contain("$799");
        result.Should().NotContain("___MATH_"); // Should not extract currency as math
        result.Should().Contain("<p"); // Should still create paragraphs
    }

    [Fact]
    public void FormatReport_Should_Handle_Currency_In_Numbered_Lists()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "1. Subtotal: $12,990\n2. Tax: $974.25\n3. Total: $13,964.25";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("$12,990");
        result.Should().Contain("$974.25");
        result.Should().Contain("$13,964.25");
        result.Should().Contain("<ol");
        result.Should().Contain("<li");
    }

    [Fact]
    public void FormatReport_Should_Handle_Bold_With_Currency()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "**Total Cost**: $1,500 for 10 items at $150 each.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("<strong>Total Cost</strong>");
        result.Should().Contain("$1,500");
        result.Should().Contain("$150");
        // Currency amounts should stay as plain text, not be extracted as math
    }

    [Fact]
    public void FormatReport_Should_Handle_Currency_And_Math_Together()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "Cost is $200. Formula: $x^2 + 5$ equals $25$ when $x=2$.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("$200"); // Currency should stay as-is
        result.Should().Contain("$x^2 + 5$"); // Math should be preserved
        result.Should().Contain("$25$"); // This is ambiguous but should be treated as math due to pattern
        result.Should().Contain("$x=2$");
    }

    [Fact]
    public void FormatInlineMarkdown_Should_Not_Treat_Currency_As_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "I paid $50 for the book and $30 for shipping.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatInlineMarkdown", input);

        // Assert
        result.Should().Contain("$50");
        result.Should().Contain("$30");
        result.Should().NotContain("___MATH_");
    }

    [Fact]
    public void FormatInlineMarkdown_Should_Handle_LaTeX_Inline_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "The area is \\(A = \\pi r^2\\) for a circle.";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatInlineMarkdown", input);

        // Assert
        result.Should().Contain("$A = \\pi r^2$"); // Should convert to $ syntax
        result.Should().NotContain("\\("); // LaTeX syntax should be converted
    }

    [Fact]
    public void FormatReport_Should_Handle_LaTeX_Display_Math()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var instance = cut.Instance;
        var input = "The quadratic formula:\n\\[x = \\frac{-b \\pm \\sqrt{b^2-4ac}}{2a}\\]";

        // Act
        var result = CallPrivateMethod<string>(instance, "FormatReport", input);

        // Assert
        result.Should().Contain("$$x = \\frac{-b \\pm \\sqrt{b^2-4ac}}{2a}$$"); // Should convert to $$ syntax
        result.Should().NotContain("\\["); // LaTeX syntax should be converted
    }

    private T CallPrivateMethod<T>(object obj, string methodName, params object[] args)
    {
        var method = obj.GetType().GetMethod(methodName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)method!.Invoke(obj, args)!;
    }
}
