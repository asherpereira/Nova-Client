# Nova Client

Nova is a modern, privacy-conscious communications client. Windows 11 is the first platform, with shared protocol/core architecture reserved for macOS, Android, iOS and web.

## Current stack

- .NET 10 LTS
- WinUI 3 through Windows App SDK 2.5.1
- Windows 11 baseline
- C#
- x64 + ARM64 targets

## Architecture

- `Nova.Client.Protocol` — versioned REST and WebSocket contracts
- `Nova.Client.Core` — authentication, API transport, realtime, storage, transfers and security abstractions
- `Nova.Client.Windows` — WinUI 3 application shell and Windows integration
- Future platform projects — native UI/OS integrations over shared contracts/core where practical

The architecture already reserves seams for DMs, group DMs, servers, channels, presence, notifications, attachments, WebRTC calling, E2EE, themes, accessibility, search, bots/integrations, discovery and multi-device sessions.

## Development

Open `Nova.Client.sln` in Visual Studio with Windows App SDK tooling installed. The Windows app uses Windows App SDK 2.5.1 and self-contained deployment so it is not coupled to an older separately installed Windows App Runtime.

Backend URLs are configuration concerns and are not hardcoded into production builds.

See `docs/` for architecture, protocol, security, UI and contribution guidance.
