using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using NiteCompiler.Utilities;

namespace NiteCompiler.Diagnostics;

public sealed class DiagnosticDescriptor
{
	public string Id { get; }
	public string MessageFormat { get; }
	public DiagnosticSeverity Severity { get; }

	public DiagnosticDescriptor(string id, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string messageFormat,
		DiagnosticSeverity severity = DiagnosticSeverity.Error)
	{
		Guard.NotEmptyNorNull(Id = id);
		Guard.NotEmptyNorNull(MessageFormat = messageFormat);
		Guard.IsValid(Severity = severity);
	}

	public Diagnostic Instantiate(string message)
	{
		return new Diagnostic(this, message);
	}

	public Diagnostic Instantiate(string message, Location location)
	{
		return new Diagnostic(this, message, location);
	}

	public Diagnostic Instantiate()
	{
		return new Diagnostic(this, MessageFormat);
	}

	public Diagnostic Instantiate(Location location)
	{
		return new Diagnostic(this, MessageFormat, location);
	}

	public Diagnostic Instantiate(params ReadOnlySpan<object> args)
	{
		return new Diagnostic(this, string.Format(MessageFormat, args));
	}

	public Diagnostic Instantiate(Location location, params ReadOnlySpan<object> args)
	{
		return new Diagnostic(this, string.Format(MessageFormat, args), location);
	}
}