using System.ComponentModel;

namespace NiteCompiler.CodeAnalysis.Syntax;

public partial class RootSyntax
{
    /// <summary>
    /// Root node always returns <see langword="null"/>.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override SyntaxNode? Parent => null;
}