namespace NiteCompiler.Symbols;

public enum Accessibility
{
	None = 0,

	/// <summary>
	/// Symbol is accessible only from same library.
	/// </summary>
	Internal,

	/// <summary>
	/// Symbol is always accessible.
	/// </summary>
	Public,

	/// <summary>
	/// Symbol is accessible only within declaring member context or within declaring type descendants.
	/// </summary>
	Protected,

	/// <summary>
	/// Symbol is accessible only within declaring member context.
	/// </summary>
	Private,
}