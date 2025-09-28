# Scopestify

A Windows Command Palette extension for Spotify integration.

## Development

### Code Quality & Linting

This project enforces code quality through multiple automated checks:

**Code Formatting:**
- Uses [CSharpier](https://csharpier.com/) for consistent code formatting
- Configuration in `.csharpierrc.json`

**Static Analysis:**
- .NET analyzers with enhanced rules (see `analyzers.ruleset`)
- EditorConfig for consistent coding styles (`.editorconfig`)
- Custom build targets for additional analysis (`Directory.Build.targets`)

**Local Development:**
```bash
# Restore tools (including CSharpier)
dotnet tool restore

# Check if code is properly formatted
dotnet csharpier check .

# Format all C# files
dotnet csharpier format .

# Build with analysis (Debug configuration enables more analyzers)
dotnet build --configuration Debug
```

**Automated CI:**
- **Code Quality & Linting**: Runs on every push and pull request
  - Code formatting validation
  - Static code analysis with .NET analyzers
  - Package vulnerability scanning
  - Deprecated package detection
- **Build**: Full Windows build and MSIX packaging
- **Formatting**: Automatic code formatting on main branch

All linting rules and configurations can be found in:
- `.editorconfig` - Editor and style rules
- `analyzers.ruleset` - Code analysis rules
- `Directory.Build.targets` - Enhanced analysis configuration