# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-01-04

### Added
- Accurate task timing from creation to completion (StartTime set at todo creation)
- KaTeX 0.16.9 math rendering with LaTeX syntax support
- Smart currency vs math detection (excludes commas in inline math)
- LaTeX syntax auto-conversion: `\(...\)` → `$...$`, `\[...\]` → `$$...$$`
- DALL-E 3 image generation embedded in chat responses
- Enhanced markdown formatting with styled HTML output
- Message persistence across browser sessions
- Copy functionality for tasks and chat messages
- "New Chat" button to clear session and start fresh
- 104 comprehensive unit tests (103 passing, 1 skipped)
- HomeFormattingTests with currency and LaTeX test cases

### Changed
- Removed artificial 200ms delay from task completion (Task.Delay)
- Enhanced math regex to prevent matching currency as math expressions
- Updated TodoPlugin with separate MarkActiveJson and MarkCompleteJson functions
- Improved FormatReport to handle both Markdown and LaTeX syntax
- Test assertions updated to expect styled HTML (`<ol style='...'>`)

### Fixed
- Currency amounts ($1,299) no longer treated as math expressions
- Duration display now shows actual elapsed time instead of fake values
- Math expressions properly preserved during markdown processing
- Unicode asterisk issue resolved (was appearing as ∗∗ instead of **)
- Character splitting issue in formatted output
- Git secret leak documentation added (FIX_SECRET_LEAK.md)

### Technical Details
- Math rendering engine with placeholder system
- Enhanced regex: `\$\$([^\$]+?)\$\$|\$([a-zA-Z0-9+\-*/=^(){}\[\]\\\s\.]+?)\$`
- OpenAI .NET SDK 2.1.0 with streaming disabled for function calling
- Duration calculation: EndTime - StartTime with proper null handling
- Test coverage: 103/104 tests passing (99.04%)

## [1.0.0] - 2026-01-03

### Added
- Initial release of AI Agent Todo application
- OpenAI GPT-4o integration with function calling
- Real-time todo management with AI agent
- Beautiful Radzen Blazor UI components
- Interactive chat interface
- Live progress tracking for agent tasks
- Performance metrics (task duration tracking)
- Comprehensive solution reports
- Thread-safe in-memory services
- Full unit test coverage (xUnit + FluentAssertions + Moq + bUnit)
- Security: gitignored API keys and secrets
- Modern C# 12 features (records, file-scoped namespaces, nullable reference types)
- Responsive design with custom CSS

### Technical Details
- .NET 8.0 target framework
- Blazor Server architecture with SignalR
- SOLID principles and clean architecture
- Factory pattern for record creation
- Dependency injection throughout
- Async/await with CancellationToken support
- Thread-safe service implementations with SemaphoreSlim

