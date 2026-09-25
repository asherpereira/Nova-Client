namespace Nova.Client.Protocol;
public static class ApiRoutes { public const string Health="/health"; public const string Register="/api/v1/auth/register"; public const string Login="/api/v1/auth/login"; public const string Me="/api/v1/me"; public const string Messages="/api/v1/messages"; public const string WebSocket="/ws"; }
public sealed record RegisterRequest(string Username,string DisplayName,string Password);
public sealed record LoginRequest(string Identifier,string Password);
public sealed record AuthResponse(string AccessToken,string? RefreshToken=null);
public sealed record CurrentUser(Guid Id,string Username,string DisplayName,string? AvatarUrl,string? Status);
public sealed record EncryptedMessage(Guid Id,Guid ConversationId,Guid SenderId,string Ciphertext,string Nonce,int EncryptionVersion,DateTimeOffset CreatedAt,string? ClientMessageId=null);
public sealed record SendMessageRequest(Guid ConversationId,string Ciphertext,string Nonce,int EncryptionVersion,string ClientMessageId);
public sealed record WebSocketEnvelope(string Type,string? AccessToken=null,string? UserId=null,string? RequestId=null,object? Payload=null);
