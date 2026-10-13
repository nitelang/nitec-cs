using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using StreamJsonRpc;

namespace NiteCompiler.LanguageServer;

public static class Server
{
	private static int Main() => Run(Console.OpenStandardInput(), Console.OpenStandardOutput());

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Uses the JSON source generator.")]
	[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Uses the JSON source generator.")]
	private static int Run(Stream input, Stream output)
	{
		NiteLanguageServer server = new();
		using JsonRpc rpc = new(new HeaderDelimitedMessageHandler(output, input, new SystemTextJsonFormatter
		{
			JsonSerializerOptions = { TypeInfoResolver = NiteJsonSerializerContext.Default },
		}));

		// LSP sends method arguments as one object (e.g. InitializeParams); tell StreamJsonRpc to
		// deserialize that object into a single complex parameter instead of matching named args.
		rpc.AddLocalRpcTarget(RpcTargetMetadata.FromShape<INiteLanguageServer>(), server,
			new JsonRpcTargetOptions { UseSingleObjectParameterDeserialization = true });

		rpc.StartListening();

		Task.WaitAny(server.Exited, rpc.Completion);

		return server.ShutdownRequested ? 0 : -1;
	}

	extension(Assembly assembly)
	{
		public string? InformationalVersion
		{
			get
			{
				var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
				return attribute?.InformationalVersion;
			}
		}
	}
}