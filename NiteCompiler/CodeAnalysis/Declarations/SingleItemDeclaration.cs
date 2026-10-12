using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;

namespace NiteCompiler.CodeAnalysis.Declarations;

internal abstract class SingleItemDeclaration : Declaration
{
	public SyntaxNode SyntaxReference { get; }
	public SourceLocation NameLocation { get; }
	public SourceLocation Location => SyntaxReference.GetLocation();
	public ImmutableArray<Diagnostic> Diagnostics { get; }

	protected SingleItemDeclaration(string name, SyntaxNode syntax, SourceLocation nameLocation,
		ImmutableArray<Diagnostic> diagnostics) : base(name)
	{
		SyntaxReference = syntax;
		NameLocation = nameLocation;
		Diagnostics = diagnostics;
	}

	protected override ImmutableArray<Declaration> GetDeclarationMembers()
	{
		return ImmutableArray<Declaration>.CastUp(GetModuleOrTypeDeclarationMembers());
	}

	public new ImmutableArray<SingleItemDeclaration> Members => GetModuleOrTypeDeclarationMembers();

	protected abstract ImmutableArray<SingleItemDeclaration> GetModuleOrTypeDeclarationMembers();

}