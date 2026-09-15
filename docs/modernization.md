# Modernization notes

This repository is the legacy baseline, not the target architecture.

## Recommended sequence

1. Add contract-level integration tests against both local services.
2. Introduce asynchronous gateway methods and remove `Application.DoEvents`.
3. Move orchestration into a presenter/view-model layer with cancellation and retry policies.
4. Replace the generated-style WCF contract only after the account service migration contract is approved.
5. Move configuration to environment-specific, secret-safe settings.
6. Add structured telemetry and correlation IDs across account and statement calls.
7. Incrementally move presentation to a supported desktop or web client while retaining gateway interfaces.

## Known legacy constraints

- Synchronous network calls can temporarily block the UI thread.
- Demo login is intentionally not authentication or authorization.
- SOAP contract namespace and operation shapes must stay aligned with the running WCF host.
- Statement polling is manual; there is no push notification.
- The desktop process relies on a local PDF/browser file association.

## Compatibility boundary

Keep `IAccountGateway` and `IStatementGateway` stable while replacing transports. Pure rules in `PortalRules` should remain framework-independent and can move to a modern shared library first.
