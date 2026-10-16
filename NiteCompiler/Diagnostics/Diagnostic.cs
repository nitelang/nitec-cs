using System.Collections.Immutable;

namespace NiteCompiler.Diagnostics;

public sealed class Diagnostic
{
	public DiagnosticDescriptor Descriptor { get; }
	public string Message { get; }
	public ImmutableArray<DiagnosticSelection> Selections { get; init; }
	public ImmutableArray<Attachment> Attachments { get; init; }

	internal Diagnostic(DiagnosticDescriptor descriptor, string message)
	{
		Descriptor = descriptor;
		Message = message;
	}

	internal Diagnostic(DiagnosticDescriptor descriptor, string message, Location location) : this(descriptor, message)
	{
		Selections = [location];
	}
}