# Protocol

Current backend routes: `/health`, `/api/v1/auth/register`, `/api/v1/auth/login`, `/api/v1/me`, `/api/v1/messages`, `/ws`.

WebSocket authentication is sent as an application message: `{"type":"auth","accessToken":"TOKEN"}`. Successful authentication currently returns a `ready` event with a user ID.

The client must tolerate unknown events and optional fields because the backend is evolving.
