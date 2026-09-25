using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nova.Client.Core.Networking;

public sealed class NovaApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private string? _accessToken;

    public NovaApiClient(string baseUrl, HttpClient? httpClient = null)
    {
        _http = httpClient ?? new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(20);
        BaseUrl = NormalizeBaseUrl(baseUrl);
    }

    public string BaseUrl { get; }
    public void SetAccessToken(string? token) => _accessToken = token;

    public async Task<bool> HealthAsync(CancellationToken ct = default)
    {
        using var response = await SendAsync(HttpMethod.Get, "health", null, ct, authenticated: false);
        return response.IsSuccessStatusCode;
    }

    public Task<JsonObject> RegisterAsync(string username, string displayName, string password, CancellationToken ct = default) =>
        PostJsonAsync("api/v1/auth/register", new { username, displayName, password }, ct, false);

    public Task<JsonObject> LoginAsync(string username, string password, CancellationToken ct = default) =>
        PostJsonAsync("api/v1/auth/login", new { username, password }, ct, false);

    public Task<JsonObject> MeAsync(CancellationToken ct = default) =>
        GetJsonAsync("api/v1/me", ct);

    public Task<JsonObject> ConversationsAsync(CancellationToken ct = default) =>
        GetJsonAsync("api/v1/conversations", ct);

    public Task<JsonObject> MessagesAsync(string conversationId, CancellationToken ct = default) =>
        GetJsonAsync($"api/v1/conversations/{Uri.EscapeDataString(conversationId)}/messages", ct);

    public Task<JsonObject> SendMessageAsync(object payload, CancellationToken ct = default) =>
        PostJsonAsync("api/v1/messages", payload, ct, true);

    private async Task<JsonObject> GetJsonAsync(string path, CancellationToken ct)
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, ct, true);
        return await ReadObjectAsync(response, ct);
    }

    private async Task<JsonObject> PostJsonAsync(string path, object payload, CancellationToken ct, bool authenticated)
    {
        var json = JsonSerializer.Serialize(payload, _json);
        using var response = await SendAsync(HttpMethod.Post, path, new StringContent(json, Encoding.UTF8, "application/json"), ct, authenticated);
        return await ReadObjectAsync(response, ct);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content, CancellationToken ct, bool authenticated)
    {
        using var request = new HttpRequestMessage(method, new Uri(new Uri(BaseUrl), path));
        request.Content = content;
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (authenticated && !string.IsNullOrWhiteSpace(_accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return response;
    }

    private async Task<JsonObject> ReadObjectAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new NovaApiException(response.StatusCode, ExtractError(body));

        if (string.IsNullOrWhiteSpace(body)) return new JsonObject();
        return JsonNode.Parse(body)?.AsObject() ?? new JsonObject();
    }

    private static string ExtractError(string body)
    {
        try
        {
            var node = JsonNode.Parse(body);
            return node?["message"]?.GetValue<string>()
                ?? node?["error"]?.GetValue<string>()
                ?? node?["title"]?.GetValue<string>()
                ?? "The Nova server rejected the request.";
        }
        catch { return "The Nova server rejected the request."; }
    }

    private static string NormalizeBaseUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new ArgumentException("Nova backend URL must be an absolute URL.", nameof(value));
        return uri.ToString().TrimEnd('/') + "/";
    }
}

public sealed class NovaApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}