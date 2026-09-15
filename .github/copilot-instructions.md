# Copilot instructions

- This is a classic WinForms application targeting .NET Framework 4.8.
- Keep project files non-SDK-style and dependencies in `packages.config`.
- Do not replace WCF `BasicHttpBinding` or Web API 2 integration without an explicit modernization task.
- Preserve the service gateway interfaces and keep formatting/calculation logic testable outside WinForms.
- Use synchronous orchestration only where needed for legacy fidelity; always use bounded timeouts and clear failure UX.
- Never add real customer data, credentials, tokens, generated statements, or fabricated screenshots.
- Build with Visual Studio MSBuild and run MSTest through `vstest.console.exe`.
