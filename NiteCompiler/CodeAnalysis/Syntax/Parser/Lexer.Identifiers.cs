using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Lexer
{
	private void ReadIdentifier(ref TokenInfo info)
	{
		char c = _window.Current;
		if (char.IsAsciiLetter(c) || c is '_' || IsIdentifierCharacterSlow(c))
		{
			_window.Advance();
			c = _window.Current;

			// TODO: Pooling
			StringBuilder sb = new();
			sb.Append(c);

			while (char.IsAsciiLetterOrDigit(c) || c is '_' || IsIdentifierCharacterSlow(c))
			{
				_window.Advance();
				c = _window.Current;
				sb.Append(c);
			}

			info.Identifier = sb.ToString();
			info.Kind = SyntaxKind.Identifier;
		}
	}

	private static bool IsIdentifierCharacterSlow(char c)
	{
		UnicodeCategory cat = char.GetUnicodeCategory(c);

		return cat is UnicodeCategory.UppercaseLetter
			or UnicodeCategory.LowercaseLetter
			or UnicodeCategory.TitlecaseLetter
			or UnicodeCategory.ModifierLetter
			or UnicodeCategory.OtherLetter
			or UnicodeCategory.LetterNumber;
	}

	private void ReadEscapedIdentifier(ref TokenInfo info)
	{
		Debug.Assert(_window.PeekThenAdvance() == '`');
		info.Kind = SyntaxKind.EscapedIdentifier;

		bool done = false;
		// TODO: Pooling
		StringBuilder sb = new();
		while (!done)
		{
			if (_window.IsAtTheEnd)
			{
				done = true;
				// TODO: Report diagnostic
				continue;
			}

			switch (_window.Current)
			{
				default:
					sb.Append(_window.Current);
					_window.Advance();
					break;
				case '`':
					done = true;
					break;
				case '\n':
				case '\r':
					done = true;
					// TODO: Report end of line
					break;
				case '\\':
					_window.Advance();
					switch (_window.Current)
					{
						case '\\':
							sb.Append('\\');
							break;
						case '`':
							sb.Append('`');
							break;
						case 'a':
							sb.Append('\a');
							break;
						case 'b':
							sb.Append('\b');
							break;
						case 'f':
							sb.Append('\f');
							break;
						case 'n':
							sb.Append('\n');
							break;
						case 'r':
							sb.Append('\r');
							break;
						case 't':
							sb.Append('\t');
							break;
						case 'v':
							sb.Append('\v');
							break;
						case '0':
							sb.Append('\0');
							break;
						case 'u':
							int rest = 4;
							while (SyntaxFacts.IsValidHexDigit(_window.Current) && rest != 0)
							{
								_window.Advance();
								rest--;
							}

							if (rest != 0)
							{
								// TODO: Report diagnostic
							}
							else
							{
								// TODO: resolve hex-code
							}

							break;
						case 'U':
							rest = 8;
							while (SyntaxFacts.IsValidHexDigit(_window.Current) && rest != 0)
							{
								_window.Advance();
								rest--;
							}

							if (rest != 0)
							{
								// TODO: Report diagnostic
							}
							else
							{
								// TODO: resolve hex-code
							}

							break;
						default:
							sb.Append('\\');
							// TODO: Report diagnostic invalid escape sequence
							break;
					}

					break;
			}
		}

		info.Identifier = sb.ToString();
	}
}