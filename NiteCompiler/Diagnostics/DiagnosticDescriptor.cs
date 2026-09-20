using System.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.Diagnostics;

public sealed class DiagnosticDescriptor
{
	public string Id { get; }
	public string MessageFormat { get; }
	public DiagnosticSeverity Severity { get; }

	public DiagnosticDescriptor(string id, string messageFormat, DiagnosticSeverity severity = DiagnosticSeverity.Error)
	{
		Guard.NotEmptyNorNull(Id = id);
		Guard.NotEmptyNorNull(MessageFormat = messageFormat);
		Guard.IsValid(Severity = severity);
	}
}