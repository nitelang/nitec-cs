using System.Diagnostics;

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
	public virtual SyntaxNode? Parent
	{
		get
		{
			if (field == null)
			{
				SyntaxNode? parent = SyntaxTree.LookupForParent(this);
				Debug.Assert(parent != null);

				Interlocked.CompareExchange(ref field, parent, null);
			}

			return field;
		}
	}

	public abstract void Accept(SyntaxVisitor visitor);
	public abstract TResult Accept<TResult>(SyntaxVisitor<TResult> visitor);
	public abstract TResult Accept<TResult, TArgument>(SyntaxVisitor<TResult, TArgument> visitor, TArgument argument);

	public abstract SyntaxNode? GetSlot(int index);
	public abstract IEnumerable<SyntaxNode> GetChildNodes();
	public abstract IEnumerable<SyntaxNode> GetChildNodesAndTokens();
	public abstract IEnumerable<SyntaxToken> GetChildTokens();
	// TODO[later]:
	// public abstract SyntaxNode? GetChildWithinPosition(int position);
	// public abstract bool Contains(SyntaxNode);
}