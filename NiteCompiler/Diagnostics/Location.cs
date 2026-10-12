namespace NiteCompiler.Diagnostics;

/// <summary>
/// A program location in source code.
/// </summary>
public abstract class Location
{
	private protected Location()
	{
	}

	public abstract LocationKind Kind { get; }
}