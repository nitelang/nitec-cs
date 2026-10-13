using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.Text;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

// SyntaxToken doesn't have auto-generated brother-of-file, it's simpler to work out this by hand.

public class SyntaxToken : SyntaxNode
{
	public sealed override SyntaxKind Kind { get; }
	public sealed override TextSpan Span { get; }
	public sealed override TextSpan FullSpan { get; }
	public bool IsMissing { get; }

	/// <summary>
	/// Immutable array of leading trivia attached to <see langword="this"/> token.
	/// </summary>
	public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

	/// <summary>
	/// Immutable array of leading trivia attached to <see langword="this"/> token.
	/// </summary>
	public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

	/// <summary>
	/// Returns identifier value, if token is an identifier; otherwise returns <see langword="null"/>.
	/// </summary>
	public virtual string? IdentifierValue => null;

	/// <summary>
	/// Returns token literal value, if token is a literal; otherwise returns <see langword="null"/>.
	/// </summary>
	public virtual object? LiteralValue => null;

	/// <summary>
	/// Explicit type token declared with.
	/// </summary>
	public virtual SpecialType ExplicitType => SpecialType.None;

	public SyntaxToken(SyntaxKind kind, TextSpan span,
		ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia,
		SyntaxTree syntaxTree, bool isMissing = false) : base(syntaxTree)
	{
		Debug.Assert(kind is { IsToken: true } or SyntaxKind.BadToken);
		Debug.Assert(kind.IsValid());

		Kind = kind;
		Span = span;
		LeadingTrivia = leadingTrivia;
		TrailingTrivia = trailingTrivia;
		IsMissing = isMissing;

		FullSpan = CalculateFullSpan();
		Debug.Assert(FullSpan.Contains(Span));
	}

	private TextSpan CalculateFullSpan()
	{
		int start = Span.Start;

		if (!LeadingTrivia.IsDefaultOrEmpty)
		{
			start = LeadingTrivia.First().Span.Start;
		}

		int end = Span.End;

		if (!TrailingTrivia.IsDefaultOrEmpty)
		{
			end = TrailingTrivia.Last().Span.End;
		}

		return TextSpan.FromBounds(start, end);
	}

	public bool IsBefore(SyntaxNode afterNode)
	{
		return Span.End == afterNode.Span.Start;
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.Visit(this);
	}

	public override TResult Accept<TResult, TArgument>(SyntaxVisitor<TResult, TArgument> visitor, TArgument argument)
	{
		return visitor.Visit(this, argument);
	}

	public override IEnumerable<SyntaxNode> GetChildNodes() => [];
	public override IEnumerable<SyntaxNode> GetChildNodesAndTokens() => [];
	public override IEnumerable<SyntaxToken> GetChildTokens() => [];

	public string Text => SyntaxTree.Text.GetText(Span);

	public ReadOnlySpan<char> GetText()
	{
		return SyntaxTree.Text.AsSpan(Span);
	}

	/// <summary>
	/// Creates a copy of <see langword="this"/> token with other kind.
	/// </summary>
	/// <param name="kind">Kind of new token.</param>
	/// <param name="isMissing">Is token missing.</param>
	/// <returns>New instance of <see cref="SyntaxToken"/>, with different syntax <paramref name="kind"/>.</returns>
	public virtual SyntaxToken WithKind(SyntaxKind kind, bool? isMissing = false)
	{
		return new SyntaxToken(kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree, isMissing ?? IsMissing);
	}

	public virtual SyntaxToken WithMissing(bool isMissing = true)
	{
		return new SyntaxToken(Kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree, isMissing);
	}

	internal static SyntaxToken Merge(SyntaxToken left, SyntaxToken right)
	{
		return new SyntaxToken(
			left.Kind,
			TextSpan.FromBounds(left.Span, right.Span),
			left.LeadingTrivia,
			right.TrailingTrivia,
			left.SyntaxTree
		);
	}

	internal static SyntaxToken Merge(SyntaxToken left, SyntaxToken right, SyntaxKind newKind)
	{
		return new SyntaxToken(
			newKind,
			TextSpan.FromBounds(left.Span, right.Span),
			left.LeadingTrivia,
			right.TrailingTrivia,
			left.SyntaxTree
		);
	}

	internal static SyntaxToken Merge(params ReadOnlySpan<SyntaxToken> tokens)
	{
		switch (tokens.Length)
		{
			case 0:
				throw new ArgumentException("Cannot merge zero tokens.");
			case 1:
				return tokens[0];
			case 2:
				return Merge(tokens[0], tokens[1]);
			default:
				SyntaxToken left = tokens[0], right = tokens[^1];
				return new SyntaxToken(
					left.Kind,
					TextSpan.FromBounds(left.Span, right.Span),
					left.LeadingTrivia,
					right.TrailingTrivia,
					left.SyntaxTree
				);
		}
	}
}