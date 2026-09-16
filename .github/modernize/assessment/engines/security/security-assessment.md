# Security Assessment Report

**Generated:** 2026-09-16T04:27:05.038280Z

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

No CVE vulnerabilities at or above the high severity threshold were found.

## CWE Findings (Code-Level Vulnerabilities)

### CWE-778: Insufficient Logging
- **Category:** Credentials & Secrets
- **Severity:** potential
- **Story Points:** 3
- **Files:** src/Contoso.LegacyBank.Portal/UI/MainForm.cs

MainForm performs customer searches, transaction retrieval, statement requests, status refreshes, and statement opening at lines 332-470, but records outcomes only in transient UI labels and message boxes; no durable audit log captures these security-sensitive banking operations.

### CWE-99: Improper Control of Resource Identifiers ('Resource Injection')
- **Category:** Injection Attacks
- **Severity:** potential
- **Story Points:** 3
- **Files:** src/Contoso.LegacyBank.Portal/Presentation/PortalRules.cs, src/Contoso.LegacyBank.Portal/UI/MainForm.cs

PortalRules.TryGetDocumentUri accepts a statement-service-controlled rooted file path or arbitrary HTTP(S) URL at lines 77-99, and MainForm.OpenStatement launches that resource through Process.Start with shell execution at lines 458-468.
