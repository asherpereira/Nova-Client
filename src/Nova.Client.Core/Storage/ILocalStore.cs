namespace Nova.Client.Core.Storage;
public interface ILocalStore { Task SaveDraftAsync(string conversationId,string text,CancellationToken cancellationToken=default); Task<string?> GetDraftAsync(string conversationId,CancellationToken cancellationToken=default); Task RemoveDraftAsync(string conversationId,CancellationToken cancellationToken=default); }
