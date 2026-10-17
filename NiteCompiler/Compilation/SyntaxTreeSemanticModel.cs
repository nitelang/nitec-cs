using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;
using NiteCompiler.Symbols;

namespace NiteCompiler.Compilation;

/// <summary>
/// <see cref="SemanticModel"/> over a single <see cref="SyntaxTree"/>.
/// </summary>
internal sealed class SyntaxTreeSemanticModel : SemanticModel
{
	private readonly NiteCompilation _compilation;

	internal SyntaxTreeSemanticModel(NiteCompilation compilation, SyntaxTree syntaxTree)
	{
		_compilation = compilation;
		SyntaxTree = syntaxTree;
	}

	public override SyntaxTree SyntaxTree { get; }

	public override NiteCompilation Compilation => _compilation;

	public override ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default)
	{
		return [.. SyntaxTree.GetDiagnostics(cancellationToken)];
	}

	// TODO: implement once the binder and symbol layer works.
	public override SymbolInfo GetSymbolInfo(SyntaxNode node, CancellationToken cancellationToken = default) => throw new NotImplementedException();

	public override SymbolInfo GetSymbolInfo(SyntaxToken token, CancellationToken cancellationToken = default) => throw new NotImplementedException();

	public override TypeInfo GetTypeInfo(SyntaxNode node, CancellationToken cancellationToken = default) => throw new NotImplementedException();

	public override TypeInfo GetTypeInfo(SyntaxToken token, CancellationToken cancellationToken = default) => throw new NotImplementedException();

	public override Symbol? GetDeclaredSymbol(SyntaxNode declaration, CancellationToken cancellationToken = default) => throw new NotImplementedException();

	public override ImmutableArray<Symbol> LookupSymbols(int position, string? name = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}