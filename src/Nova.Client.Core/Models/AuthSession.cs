namespace Nova.Client.Core.Models;

public sealed record AuthSession(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset? AccessTokenExpiresAt,
    string? DeviceId
);