using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Nova.Client.Core.Models;

namespace Nova.Client.Core.Auth;

public sealed class SessionStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("Nova.Client.Session.v1");
    private readonly string _path;

    public SessionStore()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nova");
        Directory.CreateDirectory(root);
        _path = Path.Combine(root, "session.bin");
    }

    public async Task SaveAsync(AuthSession session, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(session);
        var protectedBytes = ProtectedData.Protect(json, Entropy, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_path, protectedBytes, cancellationToken);
    }

    public async Task<AuthSession?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path)) return null;
        try
        {
            var protectedBytes = await File.ReadAllBytesAsync(_path, cancellationToken);
            var json = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
            return JsonSerializer.Deserialize<AuthSession>(json);
        }
        catch (CryptographicException) { return null; }
        catch (JsonException) { return null; }
    }

    public void Clear()
    {
        try { if (File.Exists(_path)) File.Delete(_path); }
        catch (IOException) { }
    }
}