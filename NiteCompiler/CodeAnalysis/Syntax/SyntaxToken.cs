using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.Text;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

// SyntaxToken doesn't have auto-generated brother-of-file, it's simpler to work out this by hand.

public sealed class SyntaxToken : SyntaxNode
{
	public override SyntaxKind Kind { get; }
	public override TextSpan Span { get; }

	/// <summary>
	/// Immutable array of leading trivia attached to <see langword="this"/> token.
	/// </summary>
	public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

	/// <summary>
	/// Immutable array of leading trivia attached to <see langword="this"/> token.
	/// </summary>
	public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

	public override TextSpan FullSpan
	{
		get
		{
			if (field == default)
			{
				TextSpan fullSpan = CalculateFullSpan();
				Debug.Assert(fullSpan.Contains(Span)); // Full span should always be able to contain the token span

				Interlocked.CompareExchange(ref field, fullSpan, default);
			}

			return field;

			TextSpan CalculateFullSpan()
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
		}
	}

	public SyntaxToken(SyntaxKind kind, TextSpan span,
		ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia,
		SyntaxTree syntaxTree) : base(syntaxTree)
	{
		Debug.Assert(kind is { IsToken: true } or SyntaxKind.BadToken && kind.IsValid());

		Kind = kind;
		Span = span;
		LeadingTrivia = leadingTrivia;
		TrailingTrivia = trailingTrivia;
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

	public string GetText()
	{
		return SyntaxTree.Text.GetText(Span);
	}

	public ReadOnlySpan<char> GetTextAsSpan()
	{
		return SyntaxTree.Text.AsSpan(Span);
	}

	/// <summary>
	/// Creates a copy of <see langword="this"/> token with other kind.
	/// </summary>
	/// <param name="kind">Kind of new token.</param>
	/// <returns>New instance of <see cref="SyntaxToken"/>, with different syntax <paramref name="kind"/>.</returns>
	public SyntaxToken WithKind(SyntaxKind kind)
	{
		return new SyntaxToken(kind, Span, LeadingTrivia, TrailingTrivia, SyntaxTree);
	}
}