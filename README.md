# Scopestify

A Windows Command Palette extension for Spotify integration.

## Development

### Code Formatting

This project uses [CSharpier](https://csharpier.com/) for code formatting. 

**Local Development:**
```bash
# Restore tools (including CSharpier)
dotnet tool restore

# Check if code is properly formatted
dotnet csharpier check .

# Format all C# files
dotnet csharpier format .
```

**Automated Formatting:**
- Pull requests are automatically checked for proper formatting
- Code pushed to the main branch is automatically formatted if needed
- **Note**: The formatting workflow uses a deploy key to push changes even when branch protection rules are active. See `.github/DEPLOY_KEY_SETUP.md` for setup instructions.

The formatting configuration is in `.csharpierrc.json`.