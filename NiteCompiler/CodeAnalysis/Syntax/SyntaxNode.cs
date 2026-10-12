using NiteCompiler.Diagnostics;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

public partial class SyntaxNode
{
	/// <summary>
	/// Kind of this syntax node.
	/// </summary>
	public abstract SyntaxKind Kind { get; }

	/// <summary>
	/// Span of meaningful part in syntax node.
	/// </summary>
	public abstract TextSpan Span { get; }

	/// <summary>
	/// Span of all text in syntax node, including trivia.
	/// </summary>
	public abstract TextSpan FullSpan { get; }

	/// <summary>
	/// The syntax node that holds <see langword="this"/> node as one of its children.
	/// </summary>
	public virtual SyntaxNode? Parent { get; internal set; }

	public abstract void Accept(SyntaxVisitor visitor);
	public abstract TResult Accept<TResult>(SyntaxVisitor<TResult> visitor);
	public abstract TResult Accept<TResult, TArgument>(SyntaxVisitor<TResult, TArgument> visitor, TArgument argument);

	public SourceLocation GetLocation() => new(this);

	public abstract IEnumerable<SyntaxNode> GetChildNodes();
	public abstract IEnumerable<SyntaxNode> GetChildNodesAndTokens();
	public abstract IEnumerable<SyntaxToken> GetChildTokens();
	/// <summary>
	/// Returns the direct child whose <see cref="FullSpan"/> contains <paramref name="position"/>,
	/// or <see langword="null"/> when no child does.
	/// </summary>
	public virtual SyntaxNode? GetChildWithinPosition(int position) => null;
	// TODO[later]:
	// public abstract bool Contains(SyntaxNode);
}