namespace Nova.Client.Core.Configuration;
public enum ClientEnvironment { Development, Test, Production }
public sealed record NovaClientOptions(ClientEnvironment Environment,Uri ApiBaseUri) { public Uri WebSocketUri=>new(ApiBaseUri,"ws"); }
