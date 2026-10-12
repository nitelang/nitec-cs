using System.Collections.Immutable;

namespace NiteCompiler.Diagnostics;

public sealed class Diagnostic
{
	public DiagnosticDescriptor Descriptor { get; }
	public string Message { get; }
	public ImmutableArray<DiagnosticSelection> Selections { get; init; }
	public ImmutableArray<Attachment> Attachments { get; init; }

	public Diagnostic(DiagnosticDescriptor descriptor, string message)
	{
		Descriptor = descriptor;
		Message = message;
	}

	public Diagnostic(DiagnosticDescriptor descriptor, string message, Location location) : this(descriptor, message)
	{
		Selections = [location];
	}
}