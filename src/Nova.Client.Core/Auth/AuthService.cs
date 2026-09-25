using System.Text.Json.Nodes;
using Nova.Client.Core.Models;
using Nova.Client.Core.Networking;

namespace Nova.Client.Core.Auth;

public sealed class AuthService
{
    private readonly NovaApiClient _api;
    private readonly SessionStore _store;

    public AuthService(NovaApiClient api, SessionStore store)
    {
        _api = api;
        _store = store;
    }

    public AuthSession? CurrentSession { get; private set; }
    public UserProfile? CurrentUser { get; private set; }

    public async Task<bool> RestoreAsync(CancellationToken ct = default)
    {
        var session = await _store.LoadAsync(ct);
        if (session is null) return false;

        _api.SetAccessToken(session.AccessToken);
        try
        {
            var me = await _api.MeAsync(ct);
            CurrentSession = session;
            CurrentUser = ParseUser(me);
            return true;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
    }

    public async Task<UserProfile> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var response = await _api.LoginAsync(username, password, ct);
        var session = ParseSession(response);
        _api.SetAccessToken(session.AccessToken);
        var me = await _api.MeAsync(ct);
        CurrentSession = session;
        CurrentUser = ParseUser(me);
        await _store.SaveAsync(session, ct);
        return CurrentUser;
    }

    public async Task<UserProfile> RegisterAsync(string username, string displayName, string password, CancellationToken ct = default)
    {
        var response = await _api.RegisterAsync(username, displayName, password, ct);
        if (TryGetToken(response, out _))
        {
            var session = ParseSession(response);
            _api.SetAccessToken(session.AccessToken);
            var me = await _api.MeAsync(ct);
            CurrentSession = session;
            CurrentUser = ParseUser(me);
            await _store.SaveAsync(session, ct);
            return CurrentUser;
        }

        return await LoginAsync(username, password, ct);
    }

    public Task LogoutAsync()
    {
        CurrentSession = null;
        CurrentUser = null;
        _api.SetAccessToken(null);
        _store.Clear();
        return Task.CompletedTask;
    }

    private static AuthSession ParseSession(JsonNode response)
    {
        if (response is not JsonObject json)
            throw new InvalidOperationException("The Nova server returned an invalid authentication response.");

        if (!TryGetToken(json, out var token))
            throw new InvalidOperationException("The server did not return an access token.");

        var refresh = json["refreshToken"]?.GetValue<string>();
        var deviceId = json["deviceId"]?.GetValue<string>();
        DateTimeOffset? expiry = null;
        if (json["expiresAt"]?.GetValue<string>() is string expiresAt && DateTimeOffset.TryParse(expiresAt, out var parsed))
            expiry = parsed;

        return new AuthSession(token, refresh, expiry, deviceId);
    }

    private static bool TryGetToken(JsonObject json, out string token)
    {
        token = json["accessToken"]?.GetValue<string>()
            ?? json["token"]?.GetValue<string>()
            ?? string.Empty;
        return !string.IsNullOrWhiteSpace(token);
    }

    private static UserProfile ParseUser(JsonNode response)
    {
        if (response is not JsonObject json)
            throw new InvalidOperationException("The Nova server returned an invalid user response.");

        var id = json["id"]?.GetValue<string>()
            ?? json["userId"]?.GetValue<string>()
            ?? string.Empty;
        var username = json["username"]?.GetValue<string>() ?? "user";
        var displayName = json["displayName"]?.GetValue<string>();
        var avatar = json["avatarUrl"]?.GetValue<string>();
        var status = json["status"]?.GetValue<string>() ?? "Online";
        return new UserProfile(id, username, displayName, avatar, status);
    }
}