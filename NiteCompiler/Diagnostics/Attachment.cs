using System.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.Diagnostics;

/// <summary>
/// Attachments is an additive message to the diagnostic.
/// </summary>
public readonly struct Attachment
{
	public readonly AttachmentType Type;
	public readonly string Message;

	public Attachment(AttachmentType type, string message)
	{
		Debug.Assert(type.IsValid());
		Type = type;
		Message = message;
	}
}