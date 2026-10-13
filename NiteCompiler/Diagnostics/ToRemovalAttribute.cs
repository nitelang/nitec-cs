namespace NiteCompiler.Diagnostics;

/// <summary>
/// Members marked with this attribute is part of bad internal or public API.
/// </summary>
/// <param name="reason">The reason why this bad api exists, or what's blocking its removal.</param>
internal sealed class ToRemovalAttribute(string? reason = null) : Attribute
{
	/// <summary>
	/// The reason why this bad api exists, or what's blocking its removal.
	/// </summary>
	public string? Reason { get; } = reason;
}