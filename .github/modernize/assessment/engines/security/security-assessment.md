# Security Assessment Report

**Generated:** 2026-09-16T00:50:06.0000000Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 2 |
| CVE Vulnerabilities | 0 |
| CWE Vulnerabilities | 2 |
| Total Rules Assessed | 59 |
| Rules Passed | 57 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 0 |
| optional | 0 |
| potential | 2 |

## CVE Findings (Dependency Vulnerabilities)

No CVE vulnerabilities meeting the configured severity threshold were found.


## CWE Findings (Code-Level Vulnerabilities)

### CWE-778: Insufficient Logging
- **Category:** Credentials & Secrets
- **Severity:** potential
- **Story Points:** 3
- **Files:** src/Contoso.LegacyBank.Portal/UI/MainForm.cs:306, src/Contoso.LegacyBank.Portal/UI/MainForm.cs:477

MainForm.ConfigureEvents treats selecting a demo login as the sign-in transition and enables access to customer search/actions without writing any audit record. RunServiceCall handles rejected account service requests and statement failures by updating UI labels or message boxes, but the production source contains no logging sink or audit event for these security-relevant access and failure events.

### CWE-99: Improper Control of Resource Identifiers ('Resource Injection')
- **Category:** Injection Attacks
- **Severity:** potential
- **Story Points:** 3
- **Files:** src/Contoso.LegacyBank.Portal/Presentation/PortalRules.cs:77, src/Contoso.LegacyBank.Portal/UI/MainForm.cs:450

StatementResult.PdfUrl is received from the statement service and passed to PortalRules.TryGetDocumentUri, which accepts any rooted local file path or absolute HTTP/HTTPS URI. MainForm.OpenStatementPdf then passes the resulting URI to Process.Start with shell execution, so an upstream service response controls the local or remote document resource opened by the client.
