using System.Collections.Immutable;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal sealed class SyntaxTokenIdentifier : SyntaxToken
{
	public override string IdentifierValue { get; }

	public SyntaxTokenIdentifier(SyntaxKind kind, TextSpan span,
		ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia,
		SyntaxTree syntaxTree, string identifier, bool isMissing = false) : base(kind, span, leadingTrivia, trailingTrivia, syntaxTree, isMissing)
	{
		IdentifierValue = identifier;
	}

	public override SyntaxTokenIdentifier WithKind(SyntaxKind kind, bool? isMissing = false)
	{
		return new SyntaxTokenIdentifier(kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree, IdentifierValue, isMissing ?? IsMissing);
	}

	public override SyntaxTokenIdentifier WithMissing(bool isMissing = true)
	{
		return new SyntaxTokenIdentifier(Kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree, IdentifierValue, isMissing);
	}
}