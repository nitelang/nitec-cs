namespace NiteCompiler.Diagnostics;

public readonly struct DiagnosticSelection(Location location, string? message = null)
{
	public Location Location { get; } = location;
	public string? Message { get; } = message;

	public static implicit operator DiagnosticSelection(Location location)
	{
		return new(location);
	}
}