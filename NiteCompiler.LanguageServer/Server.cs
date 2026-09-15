using System.Reflection;
using NiteCompiler.CodeAnalysis.Syntax;
using Serilog;

namespace NiteCompiler.LanguageServer;

public static class Server
{
	private static void Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.MinimumLevel.Debug()
			.CreateLogger();

		Log.Information("Startup Nite Language Server");
		Log.Information("Language Server Version: {Version}", typeof(Server).Assembly.InformationalVersion);
		Log.Information("Nite Compiler Version: {Version}", typeof(SyntaxNode).Assembly.InformationalVersion); // TODO: replace with compiler class

		while (true)
		{

		}

		Log.Information("Terminate Nite Language Server...");
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