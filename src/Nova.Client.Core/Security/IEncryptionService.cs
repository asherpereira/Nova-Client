namespace Nova.Client.Core.Security;
public interface IEncryptionService { Task InitializeIdentityAsync(CancellationToken cancellationToken=default); Task<EncryptedPayload> EncryptAsync(ReadOnlyMemory<byte> plaintext,EncryptionContext context,CancellationToken cancellationToken=default); Task<byte[]> DecryptAsync(EncryptedPayload payload,EncryptionContext context,CancellationToken cancellationToken=default); }
public sealed record EncryptionContext(string ConversationId,int ProtocolVersion);
public sealed record EncryptedPayload(byte[] Ciphertext,byte[] Nonce,int ProtocolVersion);
