using System.Diagnostics;
using System.Runtime.Serialization;

namespace NiteCompiler.Text;

/// <summary>
/// Struct describes a line within text.
/// </summary>
/// <param name="Index">
/// Zero-based index of line.
/// </param>
/// <param name="Position">
/// Position of the first character within this line.
/// </param>
/// <param name="LengthIncludingLineBreak">
/// Length of the full line span, including line break.
/// </param>
public readonly record struct SourceLine(int Index, int Position, int LengthIncludingLineBreak)
{
	public int End => Position + LengthIncludingLineBreak;

	public TextSpan Span => new(Position, LengthIncludingLineBreak);

	public bool IsEmpty => LengthIncludingLineBreak == 0;

	public int Number => Index + 1;

	public int GetColumnIndex(int globalPosition)
	{
		return globalPosition - Position;
	}

	public override string ToString()
	{
		return $"#{Number}: {Span} Width:{Span.Length}";
	}
}