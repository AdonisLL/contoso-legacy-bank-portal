# Screenshot capture guidance

Only commit screenshots captured from a running, validated portal. Do not use mockups as runtime evidence.

1. Start the WCF accounts service and statements Web API.
2. Build and launch the Release executable.
3. Set Windows display scaling to 100% and resize the portal to 1280×820.
4. Use seeded demo data that contains no real customer information.
5. Capture:
   - login/search landing state,
   - populated customer and accounts tab,
   - transaction history with date filters,
   - statement ready state (without exposing a local filesystem username).
6. Save PNG files here with descriptive kebab-case names.
7. Redact secrets, tokens, machine paths, and any non-demo personal data.

Suggested names: `portal-search.png`, `portal-transactions.png`, and `portal-statement-ready.png`.
