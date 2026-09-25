# Security baseline

Never store passwords. Never put access tokens in URLs or logs. Use OS secure storage for secrets. Treat server data and WebSocket events as untrusted. Do not invent cryptography; keep E2EE behind an abstraction until a reviewed protocol is selected. Fail closed on authentication or cryptographic verification errors. Never log plaintext private messages or private keys. Validate file names, paths, sizes and content types.
