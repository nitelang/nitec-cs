namespace NiteCompiler.Symbols;

public enum SymbolKind
{
	None,
	Library,
	Module,
	Type,
	Function,
	Field,
	Property,
	Parameter,
	LifetimeTemplateParameter, // <'a>
	GenericTemplateParameter, // <T, Size: usize>
}