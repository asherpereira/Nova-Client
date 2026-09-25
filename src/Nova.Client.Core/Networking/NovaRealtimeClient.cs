using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nova.Client.Core.Networking;

public enum RealtimeState { Closed, Connecting, Authenticating, Ready, Reconnecting, Offline, AuthenticationFailed }

public sealed class NovaRealtimeClient : IAsyncDisposable
{
    private readonly Uri _endpoint;
    private readonly Func<string?> _tokenProvider;
    private readonly SemaphoreSlim _lifecycle = new(1, 1);
    private ClientWebSocket? _socket;
    private CancellationTokenSource? _cts;
    private Task? _receiveLoop;
    private int _reconnectAttempt;

    public NovaRealtimeClient(string baseUrl, Func<string?> tokenProvider)
    {
        var baseUri = new Uri(baseUrl.TrimEnd('/') + "/");
        var scheme = baseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
        var builder = new UriBuilder(baseUri) { Scheme = scheme, Path = baseUri.AbsolutePath.TrimEnd('/') + "/ws" };
        _endpoint = builder.Uri;
        _tokenProvider = tokenProvider;
    }

    public RealtimeState State { get; private set; } = RealtimeState.Closed;
    public event EventHandler<RealtimeState>? StateChanged;
    public event EventHandler<JsonObject>? EventReceived;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycle.WaitAsync(cancellationToken);
        try
        {
            if (_socket?.State is WebSocketState.Open or WebSocketState.Connecting)
                return;

            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ConnectOnceAsync(_cts.Token);
        }
        finally { _lifecycle.Release(); }
    }

    public async Task DisconnectAsync()
    {
        await _lifecycle.WaitAsync();
        try
        {
            _cts?.Cancel();
            if (_socket is { State: WebSocketState.Open })
            {
                try { await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client shutdown", CancellationToken.None); }
                catch (WebSocketException) { }
            }
            _socket?.Dispose();
            _socket = null;
            SetState(RealtimeState.Closed);
        }
        finally { _lifecycle.Release(); }
    }

    private async Task ConnectOnceAsync(CancellationToken ct)
    {
        SetState(_reconnectAttempt == 0 ? RealtimeState.Connecting : RealtimeState.Reconnecting);
        var socket = new ClientWebSocket();
        _socket = socket;
        await socket.ConnectAsync(_endpoint, ct);

        SetState(RealtimeState.Authenticating);
        var token = _tokenProvider();
        if (string.IsNullOrWhiteSpace(token))
        {
            SetState(RealtimeState.AuthenticationFailed);
            return;
        }

        await SendAsync(new JsonObject { ["type"] = "auth", ["accessToken"] = token }, ct);
        _receiveLoop = ReceiveLoopAsync(socket, ct);
        await _receiveLoop;
    }

    private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[64 * 1024];
        try
        {
            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(buffer, ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        SetState(RealtimeState.Reconnecting);
                        await ReconnectAsync(ct);
                        return;
                    }
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                var text = Encoding.UTF8.GetString(ms.ToArray());
                if (JsonNode.Parse(text) is JsonObject obj)
                {
                    if (string.Equals(obj["type"]?.GetValue<string>(), "ready", StringComparison.OrdinalIgnoreCase))
                    {
                        _reconnectAttempt = 0;
                        SetState(RealtimeState.Ready);
                    }
                    EventReceived?.Invoke(this, obj);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (WebSocketException) { await ReconnectAsync(ct); }
    }

    private async Task ReconnectAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        _reconnectAttempt++;
        var delay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, Math.Min(_reconnectAttempt, 5))));
        await Task.Delay(delay, ct);
        try
        {
            _socket?.Dispose();
            await ConnectOnceAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch
        {
            SetState(RealtimeState.Offline);
            await ReconnectAsync(ct);
        }
    }

    public async Task SendAsync(JsonObject payload, CancellationToken ct = default)
    {
        var socket = _socket;
        if (socket?.State != WebSocketState.Open)
            throw new InvalidOperationException("Nova realtime connection is not ready.");

        var bytes = Encoding.UTF8.GetBytes(payload.ToJsonString());
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    private void SetState(RealtimeState state)
    {
        State = state;
        StateChanged?.Invoke(this, state);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _cts?.Dispose();
        _lifecycle.Dispose();
    }
}