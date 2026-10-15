using NiteCompiler.Diagnostics;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Parser
{
	private NameSyntax ParseName(NameOptions options = NameOptions.None)
	{
		NameSyntax left = ParsePlainName(options);

		while (Current.Kind is SyntaxKind.DoubleColon)
		{
			SyntaxToken doubleColon = PeekThenAdvance();
			PlainNameSyntax right = ParsePlainName(options);

			left = new PathNameSyntax(_tree, left, doubleColon, right);
		}

		return left;
	}

	private PlainNameSyntax ParsePlainName(NameOptions options = NameOptions.None)
	{
		SyntaxToken current = PeekThenAdvance();

		if (current.Kind is not (SyntaxKind.Identifier or SyntaxKind.EscapedIdentifier) && !current.Kind.IsContextual)
		{
			_diagnostics.ReportExpectedIdentifier(current.GetLocation());
			current = current.WithKind(SyntaxKind.Identifier, isMissing: true);
		}
		else if (current.Kind.IsContextual)
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