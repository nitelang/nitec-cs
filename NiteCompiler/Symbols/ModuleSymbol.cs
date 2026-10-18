namespace NiteCompiler.Symbols;

public abstract class ModuleSymbol : ContainerSymbol
{
	private protected ModuleSymbol() { }

	public sealed override SymbolKind Kind => SymbolKind.Module;

	public bool IsGlobalModule => ContainingSymbol is not ModuleSymbol;
}