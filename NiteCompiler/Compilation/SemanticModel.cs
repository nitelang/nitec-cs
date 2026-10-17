using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;
using NiteCompiler.Symbols;

namespace NiteCompiler.Compilation;

/// <summary>
/// Gives a compiler-backed view over a single <see cref="SyntaxTree"/>: which symbol a syntax node or
/// token refers to, what its type is, and what is declared at a given position.
/// </summary>
public abstract class SemanticModel
{
	private protected SemanticModel() { }

	/// <summary>
	/// The tree this model describes.
	/// </summary>
	public abstract SyntaxTree SyntaxTree { get; }

	/// <summary>
	/// The compilation this model was created from.
	/// </summary>
	public abstract NiteCompilation Compilation { get; }

	/// <summary>
	/// The symbol a node resolves to.
	/// </summary>
	public abstract SymbolInfo GetSymbolInfo(SyntaxNode node, CancellationToken cancellationToken = default);

	/// <summary>
	/// The symbol a token resolves to.
	/// </summary>
	public abstract SymbolInfo GetSymbolInfo(SyntaxToken token, CancellationToken cancellationToken = default);

	/// <summary>
	/// The type of the expression a node represents.
	/// </summary>
	public abstract TypeInfo GetTypeInfo(SyntaxNode node, CancellationToken cancellationToken = default);

	/// <summary>
	/// The type of the expression a token represents.
	/// </summary>
	public abstract TypeInfo GetTypeInfo(SyntaxToken token, CancellationToken cancellationToken = default);

	/// <summary>
	/// The symbol declared by the given declaration node, or <see langword="null"/> if the node is not a declaration.
	/// </summary>
	public abstract Symbol? GetDeclaredSymbol(SyntaxNode declaration, CancellationToken cancellationToken = default);

	/// <summary>
	/// Symbols visible at the given source position.
	/// </summary>
	public abstract ImmutableArray<Symbol> LookupSymbols(int position, string? name = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Diagnostics produced for <see cref="SyntaxTree"/> during the stages this model covers.
	/// </summary>
	public abstract ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default);
}