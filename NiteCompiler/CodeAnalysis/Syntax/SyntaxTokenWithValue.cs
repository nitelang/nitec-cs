using System.Collections.Immutable;
using System.ComponentModel;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal sealed class SyntaxTokenWithValue<T> : SyntaxToken
{
	public T Value { get; }

	public override SpecialType ExplicitType { get; }

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override object? LiteralValue => Value;

	public SyntaxTokenWithValue(SyntaxKind kind, TextSpan span,
		ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia,
		SyntaxTree syntaxTree, T value, SpecialType explicitType = SpecialType.None, bool isMissing = false) : base(kind, span, leadingTrivia, trailingTrivia, syntaxTree, isMissing)
	{
		Value = value;
		ExplicitType = explicitType;
	}

	public override SyntaxTokenWithValue<T> WithKind(SyntaxKind kind, bool? isMissing = false)
	{
		return new SyntaxTokenWithValue<T>(kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree, Value, ExplicitType, isMissing ?? IsMissing);
	}

	public override SyntaxTokenWithValue<T> WithMissing(bool isMissing = true)
	{
		return new SyntaxTokenWithValue<T>(Kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree, Value, ExplicitType, isMissing);
	}
}