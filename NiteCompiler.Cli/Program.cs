using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;
using NiteCompiler.Diagnostics.Rendering;

namespace NiteCompiler.Cli;

internal class Program
{
	private static void Main(string[] args)
	{
		/*SyntaxTree tree = SyntaxTree.FromText("3 + 2 /* never closed", filePath: args.Length > 0 ? args[0] : "main.nite");

		bool useColor = !Console.IsOutputRedirected && Environment.GetEnvironmentVariable("NO_COLOR") is null;
		var renderer = new ConsoleDiagnosticRenderer(Console.Out, useColor);
		var formatter = new CodeFrameFormatter(renderer);

		foreach (Diagnostic diagnostic in tree.GetDiagnostics())
		{
			formatter.Render(diagnostic);
		}*/
	}
}