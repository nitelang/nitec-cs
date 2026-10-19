using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Text;

namespace NiteCompiler.Diagnostics;

/// <summary>
/// A program location in source code.
/// </summary>
public abstract class Location
{
	public abstract LocationKind Kind { get; }

	public virtual SyntaxTree? SourceTree => null;
	public virtual TextSpan SourceSpan => default;

	private protected Location() { }
}