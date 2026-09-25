namespace Nova.Client.Core.Authentication;
public interface ISecureTokenStore { Task StoreAsync(string accessToken,CancellationToken cancellationToken=default); Task<string?> GetAsync(CancellationToken cancellationToken=default); Task ClearAsync(CancellationToken cancellationToken=default); }
