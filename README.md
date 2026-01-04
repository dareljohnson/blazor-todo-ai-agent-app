# AI Agent Todo - Blazor

A beautiful Blazor Server application featuring an AI-powered agent that solves problems by breaking them down into actionable todo steps.

## ✨ Features

- 🤖 **AI Agent Integration**: Intelligent problem-solving with structured task breakdown
- ✅ **Dynamic Todo Management**: Agent creates and completes todos in real-time
- 💬 **Interactive Chat Interface**: Clean, modern chat UI powered by Radzen Blazor
- 📊 **Live Progress Tracking**: See exactly what the agent is working on
- ⏱️ **Performance Metrics**: Track time taken for each task
- 📝 **Detailed Reports**: Get comprehensive Markdown-formatted solutions
- 🎨 **Beautiful UI**: Gradient headers, smooth animations, responsive design

## 🚀 Technology Stack

- **.NET 8.0** with C# 10+ (file-scoped namespaces, records, nullable reference types)
- **Blazor Server** for real-time SignalR updates
- **Microsoft AutoGen** (framework included, demonstration mode active)
- **Radzen Blazor** for beautiful UI components
- **xUnit + FluentAssertions + Moq** for comprehensive testing

## ⚡ Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- Any modern IDE (Visual Studio 2022, VS Code, Rider)

### 1. Clone & Navigate

```powershell
cd c:\development\dotnet_apps\blazor-ai-agent-todo
```

### 2. Configure (Optional)

The app works out of the box in demonstration mode. To enable real AI:

Edit `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-4o"
  }
}
```

⚠️ **Never commit API keys!** (Already gitignored)

### 3. Restore & Build

```powershell
dotnet restore BlazorAiAgentTodo.sln
dotnet build BlazorAiAgentTodo.sln -c Release
```

### 4. Run Tests (All 31 pass!)

```powershell
dotnet test BlazorAiAgentTodo.sln -c Release --no-build
```

Expected output:
```
Test Run Successful.
Total tests: 31
     Passed: 31
```

### 5. Run the Application

```powershell
dotnet run --project BlazorAiAgentTodo.csproj
```

Navigate to: **https://localhost:5001**

## 📖 Usage Guide

### Example Prompts

Try these to see the agent in action:

**Mathematical:**
```
Calculate the total cost of buying 10 laptops at $1,299 each with 7.5% sales tax
```

**Planning:**
```
Plan a 3-day conference for 50 software developers
```

**Creative:**
```
Create a weekly meal plan for a family of 4 with a $200 budget
```

### How It Works

1. **User enters prompt** → Agent analyzes the problem
2. **Agent creates todos** → Breaks down into 4-6 steps
3. **Real-time execution** → Each todo marked active → completed
4. **Final report generated** → Markdown-formatted solution

## 🏗️ Architecture

```
BlazorAiAgentTodo/
├── Models/              # Domain models (immutable records)
│   ├── TodoItem.cs      # Task with timing & status
│   ├── ChatMessage.cs   # Chat conversation
│   └── AgentSession.cs  # Complete session state
├── Services/            # Business logic (thread-safe)
│   ├── TodoService      # In-memory todo management
│   ├── ChatService      # Message history
│   └── AgentService     # AI orchestration
├── Components/          # Blazor UI components
│   ├── Pages/Home       # Main application page
│   ├── TodoList         # Live todo display
│   ├── ChatInterface    # Chat UI
│   └── ReportView       # Solution reports
└── Tests/               # 31 passing unit tests
```

### Design Patterns Used

- **Factory Pattern**: `TodoItemFactory`, `ChatMessageFactory` for record creation
- **Repository Pattern**: `ITodoService` abstracts storage
- **Dependency Injection**: All services via built-in DI container
- **Observer Pattern**: Blazor's StateHasChanged for real-time UI updates
- **Immutability**: Records with `with` expressions for state changes

## 🧪 Testing

Comprehensive test coverage across all layers:

```powershell
# Run all tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~TodoServiceTests"

# Generate coverage report (requires coverlet)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Breakdown
- **11 Model tests**: TodoItem, ChatMessage factories & extensions
- **14 Service tests**: TodoService and ChatService
- **6 Integration tests**: End-to-end workflows

## 🔒 Security

- ✅ API keys gitignored (`.gitignore` includes `appsettings.Development.json`)
- ✅ Input validation on all public APIs
- ✅ Thread-safe services with `SemaphoreSlim`
- ✅ Argument validation with exceptions
- ✅ Structured logging (no PII exposure)

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

- [ ] Real OpenAI GPT-4o integration (AutoGen fully wired)
- [ ] Persistent storage (Entity Framework Core + SQL Server)
- [ ] User authentication (Azure AD B2C)
- [ ] Multi-session support (per-user isolation)
- [ ] Export reports as PDF
- [ ] Dark mode toggle
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

- **Microsoft**: .NET 8, Blazor, AutoGen framework
- **Radzen**: Beautiful Blazor component library
- **OpenAI**: GPT-4o model architecture inspiration

## 📧 Support

For issues, questions, or feedback:
- Open an issue on GitHub
- Check existing issues first
- Provide detailed reproduction steps

---

**Built with ❤️ using .NET 8 and Blazor Server**
