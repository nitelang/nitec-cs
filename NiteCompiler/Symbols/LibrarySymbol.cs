namespace NiteCompiler.Symbols;

public abstract class LibrarySymbol : ContainerSymbol
{
	private protected LibrarySymbol() { }

	public override SymbolKind Kind => SymbolKind.Library;

	public sealed override LibrarySymbol? ContainingLibrary => null;
	public sealed override ModuleSymbol? ContainingModule => null;
	public sealed override Symbol? ContainingSymbol => null;

	public abstract ModuleSymbol GlobalModule { get; }
}