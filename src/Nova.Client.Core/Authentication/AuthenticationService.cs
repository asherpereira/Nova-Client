using System.Net.Http.Json;
using Nova.Client.Protocol;
namespace Nova.Client.Core.Authentication;
public sealed class AuthenticationService(HttpClient httpClient,ISecureTokenStore tokenStore) { public async Task<AuthResponse> LoginAsync(LoginRequest request,CancellationToken ct=default){using var r=await httpClient.PostAsJsonAsync(ApiRoutes.Login,request,ct);r.EnsureSuccessStatusCode();return await r.Content.ReadFromJsonAsync<AuthResponse>(ct)??throw new InvalidOperationException("Authentication response was empty.");} public Task SignOutAsync(CancellationToken ct=default)=>tokenStore.ClearAsync(ct); }
