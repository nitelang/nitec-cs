namespace NiteCompiler.Symbols;

public abstract class NamedTypeSymbol : TypeSymbol
{
	public abstract override string Name { get; }

	private protected NamedTypeSymbol() : base() { }
}