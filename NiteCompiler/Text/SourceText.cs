namespace NiteCompiler.Text;

// simple wrapper, can be expanded in future?
public class SourceText
{
	private readonly string _text;

	public int Length => _text.Length;

	private SourceText(string text)
	{
		_text = text;
	}

	// TODO: implement EditorConfigResolver
	// TODO: add EditorConfig? parameter
	public static SourceText FromText(string text)
	{
		return new(text);
	}

	public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		_text.CopyTo(sourceIndex, destination, destinationIndex, count);
	}

	public char this[int index]
	{
		get
		{
#if DEBUG
			if (index < 0 || index >= _text.Length)
			{
				throw new IndexOutOfRangeException();
			}
#endif

			return _text[index];
		}
	}
}