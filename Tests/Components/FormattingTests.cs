using FluentAssertions;
using Xunit;

namespace BlazorAiAgentTodo.Tests.Components;

public class FormattingTests
{
    [Theory]
    [InlineData("The answer is $x = 5$", "The answer is $x = 5$")]
    [InlineData("Formula: $x^2 + y^2 = z^2$", "Formula: $x^2 + y^2 = z^2$")]
    [InlineData("Fraction: $\\frac{a}{b}$", "Fraction: $\\frac{a}{b}$")]
    [InlineData("Square root: $\\sqrt{x}$", "Square root: $\\sqrt{x}$")]
    public void FormatInlineMarkdown_ShouldPreserveMathDelimiters(string input, string expected)
    {
        // Arrange & Act
        var result = FormatInlineMarkdown(input);
        
        // Assert
        result.Should().Contain("$");
        result.Should().Contain(expected.Split('$')[1]); // Contains the math content
    }

    [Theory]
    [InlineData("This is **bold** text", "<strong>bold</strong>")]
    [InlineData("Use `code` here", "<code style='background: #f3f4f6; padding: 0.125rem 0.375rem; border-radius: 0.25rem; font-family: monospace;'>code</code>")]
    public void FormatInlineMarkdown_ShouldFormatMarkdown(string input, string expectedContent)
    {
        // Arrange & Act
        var result = FormatInlineMarkdown(input);
        
        // Assert
        result.Should().Contain(expectedContent);
    }

    [Theory]
    [InlineData("**Bold** with $x = 5$ math", "<strong>Bold</strong>", "$x = 5$")]
    [InlineData("Code `test` and $\\frac{1}{2}$ fraction", ">test</code>", "$\\frac{1}{2}$")]
    public void FormatInlineMarkdown_ShouldHandleMixedContent(string input, string expectedMarkdown, string expectedMath)
    {
        // Arrange & Act
        var result = FormatInlineMarkdown(input);
        
        // Assert
        result.Should().Contain(expectedMarkdown);
        result.Should().Contain(expectedMath);
    }

    [Fact]
    public void FormatInlineMarkdown_ShouldEscapeHtmlButPreserveSpecialChars()
    {
        // Arrange
        var input = "Test <script>alert('xss')</script> with $x < y$";
        
        // Act
        var result = FormatInlineMarkdown(input);
        
        // Assert
        result.Should().Contain("&lt;script&gt;"); // Script tags should be HTML encoded
        result.Should().Contain("$x < y$"); // Math should be preserved with < and >
    }

    [Theory]
    [InlineData("$$x^2 + y^2 = z^2$$", "$$x^2 + y^2 = z^2$$")]
    [InlineData("$$\\frac{a + b}{c}$$", "$$\\frac{a + b}{c}$$")]
    public void FormatInlineMarkdown_ShouldPreserveDisplayMath(string input, string expected)
    {
        // Arrange & Act
        var result = FormatInlineMarkdown(input);
        
        // Assert
        result.Should().Contain("$$");
        result.Should().Contain(expected);
    }

    [Fact]
    public void FormatReport_ShouldHandleNumberedLists()
    {
        // Arrange
        var input = @"Here are the steps:
1. First step
2. Second step
3. Third step";
        
        // Act
        var result = FormatReport(input);
        
        // Assert
        result.Should().Contain("<ol");
        result.Should().Contain("<li");
        result.Should().Contain("First step");
        result.Should().Contain("Second step");
        result.Should().Contain("Third step");
        result.Should().Contain("</ol>");
    }

    [Fact]
    public void FormatReport_ShouldHandleMathInLists()
    {
        // Arrange
        var input = @"Solutions:
1. The answer is $x = 5$
2. Or $x = -2$";
        
        // Act
        var result = FormatReport(input);
        
        // Assert
        result.Should().Contain("$x = 5$");
        result.Should().Contain("$x = -2$");
        result.Should().Contain("<li");
    }

    [Fact]
    public void FormatReport_ShouldHandleParagraphsWithMath()
    {
        // Arrange
        var input = @"The quadratic formula is $x = \frac{-b \pm \sqrt{b^2 - 4ac}}{2a}$.

This solves equations of the form $ax^2 + bx + c = 0$.";
        
        // Act
        var result = FormatReport(input);
        
        // Assert
        result.Should().Contain("$x = \\frac{-b \\pm \\sqrt{b^2 - 4ac}}{2a}$");
        result.Should().Contain("$ax^2 + bx + c = 0$");
        result.Should().Contain("<p");
    }

    // Helper methods that replicate the logic from Home.razor
    private static string FormatInlineMarkdown(string text)
    {
        // Temporarily replace math delimiters to protect them
        var mathPlaceholders = new System.Collections.Generic.List<string>();
        var mathPattern = @"\$(.*?)\$|\$\$(.*?)\$\$";
        var mathIndex = 0;
        
        text = System.Text.RegularExpressions.Regex.Replace(text, mathPattern, match => {
            var placeholder = $"___MATH_{mathIndex}___";
            mathPlaceholders.Add(match.Value);
            mathIndex++;
            return placeholder;
        });
        
        // Now safely HTML encode everything else
        text = System.Net.WebUtility.HtmlEncode(text);
        
        // Handle bold **text**
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        
        // Handle inline code `code`
        text = System.Text.RegularExpressions.Regex.Replace(text, @"`(.+?)`", "<code style='background: #f3f4f6; padding: 0.125rem 0.375rem; border-radius: 0.25rem; font-family: monospace;'>$1</code>");
        
        // Restore math expressions
        for (int i = 0; i < mathPlaceholders.Count; i++)
        {
            text = text.Replace($"___MATH_{i}___", mathPlaceholders[i]);
        }
        
        return text;
    }

    private static string FormatReport(string report)
    {
        if (string.IsNullOrEmpty(report))
            return string.Empty;

        var lines = report.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var formatted = new System.Text.StringBuilder();
        var inList = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Handle numbered lists (e.g., "1. ", "2. ")
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d+\.\s"))
            {
                if (!inList)
                {
                    formatted.Append("<ol style='margin-left: 1.5rem; margin-bottom: 1rem;'>");
                    inList = true;
                }
                var content = System.Text.RegularExpressions.Regex.Replace(trimmed, @"^\d+\.\s", "");
                formatted.Append($"<li style='margin-bottom: 0.5rem;'>{FormatInlineMarkdown(content)}</li>");
            }
            else
            {
                if (inList)
                {
                    formatted.Append("</ol>");
                    inList = false;
                }
                formatted.Append($"<p style='margin-bottom: 0.75rem;'>{FormatInlineMarkdown(trimmed)}</p>");
            }
        }

        if (inList)
        {
            formatted.Append("</ol>");
        }

        return formatted.ToString();
    }

    [Theory]
    [InlineData("Nested **bold `code`** text", "<strong>", "<code")]
    [InlineData("Multiple $x = 5$ and $y = 10$ in one line", "$x = 5$", "$y = 10$")]
    public void FormatInlineMarkdown_ShouldHandleMultipleExpressionsInOneLine(string input, params string[] expectedParts)
    {
        // Act
        var result = FormatInlineMarkdown(input);
        
        // Assert
        foreach (var part in expectedParts)
        {
            result.Should().Contain(part);
        }
    }

    [Fact]
    public void FormatReport_ShouldHandleEmptyInput()
    {
        // Act
        var result = FormatReport(string.Empty);
        
        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatReport_ShouldHandleNullInput()
    {
        // Act
        var result = FormatReport(null!);
        
        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatInlineMarkdown_ShouldHandleOnlyWhitespace()
    {
        // Act
        var result = FormatInlineMarkdown("   ");
        
        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FormatInlineMarkdown_ShouldPreserveMultipleConsecutiveMath()
    {
        // Arrange
        var input = "First $a = 1$ then $b = 2$ and finally $c = 3$";
        
        // Act
        var result = FormatInlineMarkdown(input);
        
        // Assert
        result.Should().Contain("$a = 1$");
        result.Should().Contain("$b = 2$");
        result.Should().Contain("$c = 3$");
    }

    [Fact]
    public void FormatReport_ShouldHandleComplexNestedMarkdown()
    {
        // Arrange
        var input = @"Results:
1. First: **bold** with $x = 5$
2. Second: `code` with $y = 10$
3. Third: Mixed **bold `code`** and $z = 15$";
        
        // Act
        var result = FormatReport(input);
        
        // Assert
        result.Should().Contain("<ol");
        result.Should().Contain("<li");
        result.Should().Contain("<strong>");
        result.Should().Contain("<code");
        result.Should().Contain("$x = 5$");
        result.Should().Contain("$y = 10$");
        result.Should().Contain("$z = 15$");
    }
}
