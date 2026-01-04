using BlazorAiAgentTodo.Services.Interfaces;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;

namespace BlazorAiAgentTodo.Services;

public class AgentService : IAgentService
{
    private readonly ITodoService _todoService;
    private readonly IChatService _chatService;
    private readonly IConfiguration _configuration;
    private readonly ChatClient _chatClient;
    private readonly ImageClient _imageClient;
    private readonly IImageService _imageService;
    private readonly List<ChatMessage> _conversationHistory = new();

    public AgentService(
        ITodoService todoService,
        IChatService chatService,
        IConfiguration configuration)
    {
        _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var apiKey = _configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured");

        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey));

        _chatClient = openAiClient.GetChatClient("gpt-4o");
        _imageClient = openAiClient.GetImageClient("dall-e-3");
        _imageService = new ImageService(_imageClient, new Logger<ImageService>(new LoggerFactory()));
    }

    public async Task<string> ProcessPromptAsync(
        string prompt,
        Action<string>? onUpdate = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

        if (prompt.Length > 10000)
            throw new ArgumentException("Prompt exceeds maximum length of 10,000 characters.", nameof(prompt));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // Define function tools for OpenAI
            var createTodosTool = ChatTool.CreateFunctionTool(
                functionName: "CreateToDosJson",
                functionDescription: "Creates one or more todos based on the provided task descriptions",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "descriptions": {
                            "type": "array",
                            "items": { "type": "string" },
                            "description": "Array of task descriptions to create as todos"
                        }
                    },
                    "required": ["descriptions"]
                }
                """)
            );

            var markCompleteTool = ChatTool.CreateFunctionTool(
                functionName: "MarkCompleteJson",
                functionDescription: "Marks a todo as complete with notes about how it was completed",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "index": {
                            "type": "integer",
                            "description": "Zero-based index of the todo to mark as complete"
                        },
                        "completionNotes": {
                            "type": "string",
                            "description": "Notes describing how the task was completed"
                        }
                    },
                    "required": ["index", "completionNotes"]
                }
                """)
            );

            var generateImageTool = ChatTool.CreateFunctionTool(
                functionName: "GenerateImageJson",
                functionDescription: "Generates an image using DALL-E based on a text description",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "prompt": {
                            "type": "string",
                            "description": "Detailed description of the image to generate"
                        }
                    },
                    "required": ["prompt"]
                }
                """)
            );

            var tools = new List<ChatTool> { createTodosTool, markCompleteTool, generateImageTool };

            // Clear history from previous request (keep only system message for new request)
            if (_conversationHistory.Count > 1)
            {
                var systemMsg = _conversationHistory[0];
                _conversationHistory.Clear();
                _conversationHistory.Add(systemMsg);
            }

            // Add system message on first use
            if (_conversationHistory.Count == 0)
            {
                _conversationHistory.Add(ChatMessage.CreateSystemMessage(
                    @"You are a task-oriented AI assistant that MUST use the todo management tools for EVERY request.

MANDATORY WORKFLOW:
1. FIRST: Call CreateToDosJson to break the user's request into subtasks (minimum 3 tasks for every request)
2. THEN: For each todo, call MarkCompleteJson with detailed completion notes
3. FINALLY: Provide a summary after ALL todos are marked complete

REQUIRED TASK STRUCTURE (minimum 3 tasks):
- First task(s): Execute the main work
- Second-to-last task: ALWAYS ""Verify the result and check for errors""
- Last task: ALWAYS ""Prepare final summary with findings""

AVAILABLE TOOLS:
- CreateToDosJson: Creates todos from task descriptions array
- MarkCompleteJson: Marks a todo complete (use zero-based index: first todo is index 0)
- GenerateImageJson: Generates an image using DALL-E (use when user requests images, visualizations, or creative art)

EXAMPLES:
User: ""Calculate 5 + 3""
→ CreateToDosJson with: [""Perform addition of 5 + 3"", ""Verify the result and check for errors"", ""Prepare final summary""]
→ MarkCompleteJson index 0: ""Added 5 + 3 = 8""
→ MarkCompleteJson index 1: ""Verified: 8 is correct, no errors found""
→ MarkCompleteJson index 2: ""Summary prepared with result""
→ Summary: ""The sum of 5 and 3 is **8**""

User: ""Generate an image of a sunset over mountains""
→ CreateToDosJson with: [""Generate sunset mountain image"", ""Verify image generation"", ""Prepare final summary""]
→ MarkCompleteJson index 0: [Call GenerateImageJson with prompt] ""Generated image successfully""
→ MarkCompleteJson index 1: ""Verified: Image generated and ready for display""
→ MarkCompleteJson index 2: ""Summary prepared""
→ Summary: ""I've created a beautiful sunset over mountains image for you!""

User: ""Solve $x^2 - 5x + 6 = 0$""
→ CreateToDosJson with: [""Apply quadratic formula to solve equation"", ""Verify both solutions"", ""Prepare final summary""]
→ MarkCompleteJson index 0: ""Applied formula: $x = \\frac{5 \\pm \\sqrt{25-24}}{2} = \\frac{5 \\pm 1}{2}$, giving $x = 3$ or $x = 2$""
→ MarkCompleteJson index 1: ""Verified: $(3)^2 - 5(3) + 6 = 9 - 15 + 6 = 0$ ✓ and $(2)^2 - 5(2) + 6 = 4 - 10 + 6 = 0$ ✓""
→ MarkCompleteJson index 2: ""Summary prepared with both solutions""
→ Summary: ""The solutions are **$x = 3$** and **$x = 2$**""

CRITICAL RULES:
- ALWAYS use CreateToDosJson first - do NOT answer directly
- ALWAYS include verification task (second-to-last)
- ALWAYS include summary preparation task (last)
- Minimum 3 tasks for every request
- Mark todos complete in order (0, 1, 2, ...)
- Write completion notes in detail with calculations shown
- Use numbered lists in final summary
- For math expressions: ONLY use $...$ for inline or $$...$$ for display
  * Example inline: The answer is $x = 5$
  * Example display: $$x^2 + y^2 = z^2$$
  * Use standard LaTeX: $\\frac{a}{b}$, $\\sqrt{x}$, $x^2$
- Use **bold** for emphasis in final summary"));
            }

            // Add user message
            _conversationHistory.Add(ChatMessage.CreateUserMessage(prompt));

            var chatOptions = new ChatCompletionOptions();
            foreach (var tool in tools)
            {
                chatOptions.Tools.Add(tool);
            }

            // Get response with tool calls
            var response = await _chatClient.CompleteChatAsync(_conversationHistory, chatOptions, cancellationToken);

            // Process tool calls in a loop
            while (response.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                // Add assistant message with tool calls to history
                _conversationHistory.Add(ChatMessage.CreateAssistantMessage(response.Value.ToolCalls));

                // Execute each tool call
                foreach (var toolCall in response.Value.ToolCalls)
                {
                    string result;
                    try
                    {
                        result = await ExecuteToolCallAsync(toolCall, cancellationToken);

                        // Notify UI to refresh after each tool execution
                        onUpdate?.Invoke($"{toolCall.FunctionName}");

                        // Delay to allow UI to show the changes
                        await Task.Delay(100, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        result = $"Error: {ex.Message}";
                    }

                    // Add tool result to history
                    _conversationHistory.Add(ChatMessage.CreateToolMessage(toolCall.Id, result));
                }

                // Get next response
                response = await _chatClient.CompleteChatAsync(_conversationHistory, chatOptions, cancellationToken);
            }

            // Return final text response
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            return $"Error processing request: {ex.Message}";
        }
    }

    private async Task<string> ExecuteToolCallAsync(ChatToolCall toolCall, CancellationToken cancellationToken)
    {
        var plugin = new TodoPlugin(_todoService, _imageService);
        var arguments = toolCall.FunctionArguments;

        switch (toolCall.FunctionName)
        {
            case "CreateToDosJson":
                var createRequest = JsonSerializer.Deserialize<CreateTodosRequest>(arguments);
                return await plugin.CreateToDosJson(createRequest!);

            case "MarkCompleteJson":
                var markRequest = JsonSerializer.Deserialize<MarkCompleteRequest>(arguments);
                return await plugin.MarkCompleteJson(markRequest!);

            case "GenerateImageJson":
                var imageRequest = JsonSerializer.Deserialize<GenerateImageRequest>(arguments);
                return await plugin.GenerateImageJson(imageRequest!);

            default:
                return $"Unknown tool: {toolCall.FunctionName}";
        }
    }

}
