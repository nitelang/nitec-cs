using System.Text.Json.Serialization;
using PolyType;
using StreamJsonRpc;

namespace NiteCompiler.LanguageServer;

/// <summary>
/// The LSP methods the server currently handles. Method aliases match the LSP wire names.
/// </summary>
[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
internal partial interface INiteLanguageServer
{
	[JsonRpcMethod("initialize")]
	Task<InitializeResult> InitializeAsync(InitializeParams? parameters);

	[JsonRpcMethod("initialized")]
	Task InitializedAsync(InitializedParams? parameters);

	[JsonRpcMethod("shutdown")]
	Task ShutdownAsync();

	[JsonRpcMethod("exit")]
	Task ExitAsync();
}

internal sealed class InitializeParams;

internal sealed class InitializedParams;

internal sealed class InitializeResult
{
	[JsonPropertyName("capabilities")]
	public ServerCapabilities Capabilities { get; init; } = new();

	[JsonPropertyName("serverInfo")]
	public ServerInfo ServerInfo { get; init; } = new();
}

internal sealed class ServerCapabilities
{
	// TODO: advertise textDocumentSync and other capabilities as features are implemented.
}

internal sealed class ServerInfo
{
	[JsonPropertyName("name")]
	public string Name { get; init; } = "nite-language-server";

	[JsonPropertyName("version")]
	public string? Version { get; init; }
}

[JsonSerializable(typeof(InitializeParams))]
[JsonSerializable(typeof(InitializedParams))]
[JsonSerializable(typeof(InitializeResult))]
internal partial class NiteJsonSerializerContext : JsonSerializerContext;