namespace NiteCompiler.CodeAnalysis.Syntax;

public abstract partial class SyntaxVisitor
{
	public virtual void Visit(SyntaxNode? node)
	{
		node?.Accept(this);
	}

	protected virtual void DefaultVisit(SyntaxNode node)
	{
	}

	// only SyntaxToken here, other syntax nodes is auto-generated.
	public virtual void Visit(SyntaxToken token) => DefaultVisit(token);
}

public abstract partial class SyntaxVisitor<TResult>
{
	public virtual TResult? Visit(SyntaxNode? node)
	{
		return node != null ? node.Accept(this) : default;
	}

	protected virtual TResult? DefaultVisit(SyntaxNode node)
	{
		return default;
	}

	public virtual TResult Visit(SyntaxToken token) => DefaultVisit(token)!;
}

public abstract partial class SyntaxVisitor<TResult, TArgument>
{
	public virtual TResult? Visit(SyntaxNode? node, TArgument argument)
	{
		return node != null ? node.Accept(this, argument) : default;
	}

	protected virtual TResult? DefaultVisit(SyntaxNode node, TArgument argument)
	{
		return default;
	}

	public virtual TResult Visit(SyntaxToken token, TArgument argument) => DefaultVisit(token, argument)!;
}