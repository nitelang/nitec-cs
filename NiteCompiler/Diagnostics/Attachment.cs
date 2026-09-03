using System.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.Diagnostics;

public readonly struct Attachment
{
	public readonly AttachmentType Type;
	public readonly string Message;

	public Attachment(AttachmentType type, string message)
	{
		Debug.Assert(type.IsValid());
		Message = message;
	}
}