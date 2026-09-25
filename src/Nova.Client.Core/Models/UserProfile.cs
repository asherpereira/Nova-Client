namespace Nova.Client.Core.Models;

public sealed record UserProfile(
    string Id,
    string Username,
    string? DisplayName,
    string? AvatarUrl,
    string? Status
);