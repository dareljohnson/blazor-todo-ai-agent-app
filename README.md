# AI Agent Todo - Blazor

A beautiful Blazor Server application featuring an AI-powered agent that solves problems by breaking them down into actionable todo steps with real-time progress tracking and rich content rendering.

## ✨ Features

- 🤖 **OpenAI GPT-4o Integration**: Intelligent problem-solving with structured task breakdown
- ✅ **Dynamic Todo Management**: Agent creates and completes todos in real-time
- 💬 **Interactive Chat Interface**: Clean, modern chat UI with persistent message history
- 📊 **Live Progress Tracking**: See exactly what the agent is working on with accurate timing
- ⏱️ **Performance Metrics**: Real elapsed time tracking from creation to completion
- 📝 **Enhanced Markdown Rendering**: Full support for bold, italic, lists, and code blocks
- 🧮 **KaTeX Math Rendering**: Beautiful LaTeX and inline math expressions ($x^2$, $$E=mc^2$$)
- 💰 **Smart Currency Handling**: Distinguishes between currency ($1,299) and math ($x^2$)
- 🎨 **DALL-E 3 Image Generation**: AI-powered image creation embedded in responses
- 📋 **Copy Functionality**: One-click copy for tasks and chat messages
- 🎨 **Beautiful UI**: Gradient headers, smooth animations, responsive design

## 🚀 Technology Stack

- **.NET 8.0** with C# 12 (file-scoped namespaces, records, nullable reference types)
- **Blazor Server** for real-time SignalR updates
- **OpenAI .NET SDK 2.1.0** with GPT-4o and DALL-E 3
- **KaTeX 0.16.9** for mathematical expression rendering
- **Radzen Blazor** for beautiful UI components
- **xUnit 2.9.2 + FluentAssertions 6.12.2 + Moq 4.20.72 + bUnit 1.28.9** for comprehensive testing

## ⚡ Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- Any modern IDE (Visual Studio 2022, VS Code, Rider)

### 1. Clone & Navigate

```powershell
cd c:\development\dotnet_apps\blazor-ai-agent-todo
```

### 2. Configure OpenAI API Key

**Option 1: User Secrets (Recommended for Development)**
```powershell
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-your-api-key-here"
dotnet user-secrets set "OpenAI:Model" "gpt-4o"
```

**Option 2: Environment Variables**
```powershell
# Windows PowerShell
$env:OpenAI__ApiKey = "sk-proj-your-api-key-here"
$env:OpenAI__Model = "gpt-4o"

# Linux/macOS
export OpenAI__ApiKey="sk-proj-your-api-key-here"
export OpenAI__Model="gpt-4o"
```

⚠️ **Security Note**: Never commit API keys to Git! The `.gitignore` is configured to exclude `appsettings.Development.json` and other secret files.

### 3. Restore & Build

```powershell
dotnet restore
dotnet build -c Release
```

### 4. Run Tests (103 out of 104 pass, 1 skipped!)

```powershell
dotnet test --verbosity minimal
```

Expected output:
```
Test summary: total: 104, failed: 0, succeeded: 103, skipped: 1
Build succeeded in X.Xs
```

### 5. Run the Application

```powershell
dotnet run --urls "http://localhost:5000"
```

Navigate to: **http://localhost:5000**

## 📖 Usage Guide

### Example Prompts

Try these to see the agent in action:

**Mathematical (with LaTeX rendering):**
```
Calculate the limit as x approaches 9 for (x-9)/(x-3)
```

**Financial (with currency handling):**
```
Calculate the total cost of buying 10 laptops at $1,299 each with 7.5% sales tax
```

**Creative (with image generation):**
```
Generate an image of a futuristic AI assistant robot
```

**Planning:**
```
Create a weekly meal plan for a family of 4 with a $200 budget
```

### Advanced Features

**Math Rendering:**
- Inline math: `$x^2 + 5$`
- Display math: `$$E = mc^2$$`
- LaTeX syntax: `\( x^2 \)` and `\[ E=mc^2 \]` (auto-converted)

**Currency vs Math:**
- The app intelligently distinguishes `$1,299` (currency) from `$x^2$` (math)
- Commas in inline math are excluded to prevent false matches

**Image Generation:**
- Ask the agent to generate images with DALL-E 3
- Images appear directly in the chat response
- High-quality 1024x1024 HD images

### How It Works

1. **User enters prompt** → Agent analyzes using GPT-4o
2. **Agent creates todos** → Breaks down into actionable steps with StartTime set
3. **Real-time execution** → Each todo shows elapsed time from creation
4. **Function calling** → Agent uses `CreateToDosJson`, `MarkCompleteJson`, `GenerateImageJson`
5. **Final report generated** → Markdown with KaTeX math, embedded images, styled HTML

## 🏗️ Architecture

```
BlazorAiAgentTodo/
├── Models/                  # Domain models (immutable records)
│   ├── TodoItem.cs          # Task with StartTime/EndTime tracking
│   ├── ChatMessage.cs       # User/Assistant messages with images
│   └── OpenAIConfig.cs      # Configuration for API key/model
├── Services/                # Business logic (thread-safe)
│   ├── TodoService.cs       # In-memory todo management with SemaphoreSlim
│   ├── ChatService.cs       # Message history persistence
│   ├── AgentService.cs      # OpenAI chat completion orchestration
│   ├── TodoPlugin.cs        # Function calling: CreateToDos, MarkActive, MarkComplete
│   └── ImageService.cs      # DALL-E 3 image generation
├── Components/              # Blazor UI components
│   └── Pages/
│       └── Home.razor       # Main page with chat, todos, math rendering
├── wwwroot/                 # Static files
│   ├── css/app.css          # Custom styles with gradients
│   └── js/                  # KaTeX and rendering scripts
└── Tests/                   # 104 comprehensive unit tests
    ├── Models/              # TodoItem, ChatMessage factory tests
    ├── Services/            # TodoService, ChatService, AgentService tests
    └── Components/Pages/    # Home.razor formatting tests (103 passing, 1 skipped)
```

### Design Patterns Used

- **Factory Pattern**: `TodoItemFactory`, `ChatMessageFactory` for record creation with validation
- **Repository Pattern**: `ITodoService`, `IChatService` abstract storage
- **Dependency Injection**: All services via built-in DI container (Singleton lifestyle)
- **Observer Pattern**: Blazor's `StateHasChanged` for real-time UI updates
- **Immutability**: Records with `with` expressions for thread-safe state changes
- **Function Calling**: OpenAI tools pattern with JSON schema attributes

### Key Technical Features

**Math Rendering Engine:**
- Enhanced regex pattern: `\$\$([^\$]+?)\$\$|\$([a-zA-Z0-9+\-*/=^(){}\[\]\\\s\.]+?)\$`
- Excludes commas to prevent matching currency as math
- LaTeX syntax auto-conversion: `\(...\)` → `$...$`, `\[...\]` → `$$...$$`
- Placeholder system prevents markdown/HTML escaping issues

**Timing Architecture:**
- `StartTime` set at todo creation (`DateTime.UtcNow`)
- `EndTime` set when marked complete
- `Duration` computed property: `EndTime - StartTime`
- Display format: `0.1s` for sub-minute, `1.5m` for minutes

**OpenAI Integration:**
- Streaming disabled for reliable function calling
- Temperature: 0.7 for balanced creativity/accuracy
- Function tools: `CreateToDosJson`, `MarkActiveJson`, `MarkCompleteJson`, `GenerateImageJson`
- Automatic retry with exponential backoff

## 🧪 Testing

Comprehensive test coverage with 104 tests (103 passing, 1 skipped):

```powershell
# Run all tests
dotnet test --verbosity minimal

# Run specific test class
dotnet test --filter "FullyQualifiedName~HomeFormattingTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Test Breakdown
- **18 Model tests**: TodoItem, ChatMessage factories, extensions, validation
- **23 Service tests**: TodoService, ChatService with thread safety
- **11 Formatting tests**: Math rendering, currency handling, LaTeX conversion
- **52 Additional tests**: Integration, edge cases, error handling

### Notable Test Cases
- ✅ Currency not treated as math (`$1,299` vs `$x^2$`)
- ✅ LaTeX syntax conversion (`\(...\)` → `$...$`)
- ✅ Mixed currency and math in same text
- ✅ Styled HTML output (`<ol style='...'>`)
- ⏭️ Bullet lists (skipped - not yet implemented)

## 🔒 Security

- ✅ OpenAI API keys via user secrets (never in source control)
- ✅ `.gitignore` excludes `appsettings.Development.json`, `*.user`, secrets
- ✅ Input validation on all public APIs (ArgumentException for invalid data)
- ✅ Thread-safe services with `SemaphoreSlim` locks
- ✅ Nullable reference types enabled (C# 12)
- ✅ No PII logging (structured logging only)

⚠️ **If you accidentally committed secrets, see [FIX_SECRET_LEAK.md](FIX_SECRET_LEAK.md)**

## 🚀 Deployment

```powershell
# Publish for production
dotnet publish -c Release -o ./publish

# The published output is in ./publish/
```

Deploy to:
- **Azure App Service** (Blazor Server)
- **Docker** (add Dockerfile)
- **IIS** (Windows Server)
- **Linux** (with Kestrel)

## 📝 Roadmap

- [x] OpenAI GPT-4o integration with function calling
- [x] DALL-E 3 image generation
- [x] KaTeX math rendering (LaTeX + Markdown syntax)
- [x] Smart currency vs math detection
- [x] Accurate task timing from creation to completion
- [x] Message persistence across sessions
- [x] Copy functionality for tasks and messages
- [x] 103 passing unit tests
- [ ] Persistent storage (Entity Framework Core + SQL Server)
- [ ] User authentication (Azure AD B2C)
- [ ] Multi-user session isolation
- [ ] Export reports as PDF
- [ ] Dark mode toggle
- [ ] Bullet list formatting support
- [ ] WebAssembly hosting option

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Write tests for new features (TDD approach)
4. Ensure all tests pass (`dotnet test`)
5. Commit changes (`git commit -m 'Add amazing feature'`)
6. Push to branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details

## 🙏 Acknowledgments

- **OpenAI**: GPT-4o and DALL-E 3 models
- **Microsoft**: .NET 8, Blazor Server, C# 12
- **KaTeX**: Beautiful math rendering library
- **Radzen**: Blazor component library
- **xUnit, FluentAssertions, Moq, bUnit**: Testing frameworks

---

**Built with ❤️ using .NET 8, Blazor Server, and OpenAI GPT-4o**

