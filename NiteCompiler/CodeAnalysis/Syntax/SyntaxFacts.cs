using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal static class SyntaxFacts
{
	public static int GetLineBreakWidth(SourceText source, int index)
	{
		char c = source[index];
		char next = index + 1 < source.Length ? source[index + 1] : '\0';

		if (c == '\r' && next == '\n') return 2;
		if (c == '\r' || c == '\n') return 1;

		return 0;
	}
	public static bool IsValidHexDigit(char number)
	{
		return number
			is >= '0' and <= '9'
			or >= 'A' and <= 'F'
			or >= 'a' and <= 'f';
	}

	public static bool IsValidDecimalDigit(char number)
	{
		return number is >= '0' and <= '9';
	}

	public static bool IsValidBinaryDigit(char number)
	{
		return number is '0' or '1';
	}

	public static bool TryGetEscapedCharacter(ref char escapedCharacter)
	{
		switch (escapedCharacter)
		{
			case '\'':
				escapedCharacter = '\'';
				return true;
			case '\"':
				escapedCharacter = '\"';
				return true;
			case '`':
				escapedCharacter = '`';
				return true;
			case '$':
				escapedCharacter = '$';
				return true;
			case '\\':
				escapedCharacter = '\\';
				return true;
			case 'n':
				escapedCharacter = '\n';
				return true;
			case 'r':
				escapedCharacter = '\r';
				return true;
			case 't':
				escapedCharacter = '\t';
				return true;
			case 'b':
				escapedCharacter = '\b';
				return true;
			case 'v':
				escapedCharacter = '\v';
				return true;
			case '0':
				escapedCharacter = '\x00';
				return true;
			default:
				return false;
		}
	}
}