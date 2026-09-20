namespace NiteCompiler.Diagnostics;

public enum DiagnosticSeverity
{
	/// <summary>
	/// A critical error in source code syntax, semantic, etc. Prevents code from compilation.
	/// </summary>
	Error,

	/// <summary>
	/// Questionable code patterns. Does not prevent code from compilation.
	/// </summary>
	Warning,

	/// <summary>
	/// Information diagnostic. Used to help developer, simplify code, etc.
	/// </summary>
	Information
}