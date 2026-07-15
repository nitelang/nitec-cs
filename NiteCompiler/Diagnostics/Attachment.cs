namespace NiteCompiler.Diagnostics;

public readonly struct Attachment
{
	public readonly AttachmentType Type;
	public readonly string Message;

	public Attachment(AttachmentType type, string message)
	{
		Guard.ValueExists(Type = type);
		Message = message;
	}
}