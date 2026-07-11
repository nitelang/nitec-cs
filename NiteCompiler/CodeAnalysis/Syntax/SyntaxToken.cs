using System.Diagnostics;

namespace NiteCompiler.CodeAnalysis.Syntax;

// SyntaxToken doesn't have auto-generated brother-of-file, it's simpler to work out this by hand.

public sealed class SyntaxToken : SyntaxNode
{
    public override SyntaxKind Kind { get; }
    public override TextSpan Span { get; }
    public override TextSpan FullSpan => Span; // TODO: replace with proper realization

    public SyntaxToken(SyntaxKind kind, int position, int width, SyntaxTree syntaxTree) : base(syntaxTree)
    {
        Debug.Assert(kind.IsToken);
        Debug.Assert(position >= 0);
        Debug.Assert(width >= 0);
        
        Kind = kind;
        Span = new TextSpan(position, width);
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

    public override SyntaxNode? GetSlot(int index) => null;
    public override IEnumerable<SyntaxNode> GetChildNodes() => [];
    public override IEnumerable<SyntaxNode> GetChildNodesAndTokens() => [];
    public override IEnumerable<SyntaxToken> GetChildTokens() => [];
}