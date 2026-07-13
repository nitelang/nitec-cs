namespace NiteCompiler.Text;

public class SourceText
{
	private readonly string _text;

	private SourceText(string text)
	{
		_text = text;
	}

	public static SourceText FromText(string text)
	{
		return new(text);
	}
}