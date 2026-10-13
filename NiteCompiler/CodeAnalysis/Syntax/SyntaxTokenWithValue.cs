using System.Collections.Immutable;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal sealed class SyntaxTokenWithValue<T> : SyntaxToken
{
	public T Value { get; }

	public SyntaxTokenWithValue(SyntaxKind kind, TextSpan span,
		ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia,
		SyntaxTree syntaxTree, T value, bool isMissing = false) : base(kind, span, leadingTrivia, trailingTrivia, syntaxTree, isMissing)
	{
		Value = value;
	}

	public override SyntaxTokenWithValue<T> WithKind(SyntaxKind kind, bool? isMissing = false)
	{
		return new SyntaxTokenWithValue<T>(kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree, Value, isMissing ?? IsMissing);
	}

	public override SyntaxTokenWithValue<T> WithMissing(bool isMissing = true)
	{
		return new SyntaxTokenWithValue<T>(Kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree, Value, isMissing);
	}
}