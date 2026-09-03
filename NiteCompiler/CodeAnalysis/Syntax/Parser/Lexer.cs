using NiteCompiler.Compilation;
using NiteCompiler.Diagnostics;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Parser
{
	private sealed partial class Lexer
	{
		public SyntaxTree SyntaxTree { get; }
		public SourceText Source { get; }
		public NiteParseOptions Options { get; }
		public DiagnosticBag Diagnostics { get; }

		public Lexer(SyntaxTree syntaxTree, SourceText source, NiteParseOptions options, DiagnosticBag diagnostics)
		{
			SyntaxTree = syntaxTree;
			Source = source;
			Options = options;
			Diagnostics = diagnostics;
		}

		public SyntaxToken Lex()
		{
			throw new NotImplementedException();
		}
	}
}