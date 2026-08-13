# Contributing to FocusPocus

Thanks for helping improve FocusPocus.

## Workflow

1. Fork the repository and create a focused branch.
2. Keep changes small and avoid committing generated output from `bin`, `obj`, `publish-*`, or `dist`.
3. Build both projects before opening a pull request:

```powershell
dotnet build .\src\FocusPocus.UI\FocusPocus.UI.csproj -c Release
dotnet build .\src\SpotDot\SpotDot.csproj -c Release
```

4. Test the WinUI interface, tray behavior, global shortcuts, overlay behavior, and Spanish/English text when relevant.
5. Explain the user-facing impact and verification performed in the pull request.

## Style

- Follow the existing C# and XAML conventions.
- Preserve the two-process architecture.
- Keep UI text available in Spanish and English.
- Avoid adding telemetry or network requests unrelated to explicit user actions.
