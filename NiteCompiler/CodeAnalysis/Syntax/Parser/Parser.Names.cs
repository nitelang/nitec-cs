using System.Diagnostics;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Parser
{
	private NameSyntax ParseName(NameOptions options = NameOptions.None)
	{
		return ParsePlainName(options);
	}

	private PlainNameSyntax ParsePlainName(NameOptions options = NameOptions.None)
	{
		SyntaxToken current = PeekAndAdvance();
		Debug.Assert(current.Kind is SyntaxKind.Identifier or SyntaxKind.EscapedIdentifier || current.Kind.IsContextual);

		if (current.Kind.IsContextual)
		{
			current = current.WithKind(SyntaxKind.Identifier);
		}

		if (options.HasFlag(NameOptions.InExpression))
		{
			// TODO: template arguments
		}

		return new IdentifierNameSyntax(_tree, current);
	}
}