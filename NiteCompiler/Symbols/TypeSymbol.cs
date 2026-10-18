namespace NiteCompiler.Symbols;

public abstract class TypeSymbol : Symbol
{
	public sealed override SymbolKind Kind => SymbolKind.Type;

	private protected TypeSymbol() : base() { }
}