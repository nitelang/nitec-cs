using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Utilities;

namespace NiteCompiler.Text;

public sealed class LineCollection : IEnumerable<SourceLine>
{
	private readonly SourceLine[] _lines;
	public SourceText Source { get; }

	public int Count => _lines.Length;

	public LineCollection(SourceText source)
	{
		Source = source ?? throw new ArgumentNullException(nameof(source));

		var lines = ArrayBuilder<SourceLine>.GetInstance(32);

		int start = 0;
		for (int i = 0; i < Source.Length;)
		{
			int width = SyntaxFacts.GetLineBreakWidth(Source, i);
			if (width == 0)
			{
				i++;
				continue;
			}

			int length = i - start + width;
			lines.Add(new SourceLine(lines.Count, start, length));
			i += width;
			start = i;
		}

		if (start < Source.Length)
		{
			lines.Add(new SourceLine(lines.Count, start, Source.Length - start));
		}

		_lines = lines.ToArrayAndFree();
		VerifyLines();
	}

	[Conditional("DEBUG")]
	private void VerifyLines()
	{
		Debug.Assert(_lines.Sum(t => t.Span.Length) == Source.Length);
	}

	public SourceLine this[int lineIndex]
	{
		get
		{
			if ((uint)lineIndex >= (uint)_lines.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(lineIndex));
			}

			return _lines[lineIndex];
		}
	}

	public SourceLine? GetLineByCharacterPosition(int characterPosition)
	{
		if (characterPosition < 0 || characterPosition > Source.Length)
			return null;

		if (characterPosition == Source.Length)
			return _lines[^1];

		int left = 0;
		int right = _lines.Length - 1;

		while (left <= right)
		{
			int mid = left + ((right - left) >> 1);
			var line = _lines[mid];

			if (line.Span.Contains(characterPosition))
				return line;

			if (characterPosition < line.Position)
				right = mid - 1;
			else
				left = mid + 1;
		}

		// we raise an exception, this is signal that the line table is ill-formed, not actually a user problem
		throw ExceptionUtilities.Unreachable();
	}

	public IEnumerator<SourceLine> GetEnumerator()
	{
		// apparently, arrays do not implement IEnumerable<T>
		for (int i = 0; i < _lines.Length; i++)
		{
			yield return _lines[i];
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}