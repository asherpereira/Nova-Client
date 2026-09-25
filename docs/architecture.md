# Architecture

Nova Client separates protocol, core services and platform UI. The Windows UI is not the source of truth for application state or networking.

## Baseline
- .NET 10 LTS
- WinUI 3 through Windows App SDK 2.5.1
- Windows 11
- x64 first, ARM64 alongside it

## Boundaries
Protocol owns versioned REST/WebSocket contracts. Core owns authentication, transport, realtime, local state, transfer and security abstractions. Platforms own UI and OS integration.

Future seams include conversations, servers, channels, presence, notifications, WebRTC, files, search, device/session management, integrations and reviewed E2EE.
