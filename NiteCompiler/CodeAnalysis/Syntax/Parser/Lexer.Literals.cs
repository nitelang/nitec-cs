namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Lexer
{
	#region Numbers

	private const char Separator = '\'';

	private void ReadNumber(ref TokenInfo tokenInfo)
	{
		// TODO: preparse numbers
		tokenInfo.Kind = SyntaxKind.NumericLiteral;

		if (_window.Current == '0')
		{
			switch (_window.Next)
			{
				case 'b':
					_window.Advance(2);
					ReadBinaryInteger();
					return;
				case 'x':
					_window.Advance(2);
					ReadHexNumber();
					return;

				// 0123 is decimal literal, not octal
				default:
					ReadDecimalLiteral();
					break; // decimal literal can be part of float literal
			}
		}
		else
		{
			ReadDecimalLiteral();
		}

		if (_window.Current == '.') // read precision part
		{
			_window.Advance();
			ReadDecimalLiteral();
		}

		if (_window.Current is 'e' or 'E') // read E-notation part
		{
			if (_window.Next is '+' or '-')
			{
				_window.Advance(2);
			}
			else
			{
				_window.Advance();
			}

			ReadDecimalLiteral();
		}

		ReadNumericSuffix();
	}

	private void ReadDecimalLiteral()
	{
		if (char.IsAsciiDigit(_window.Current))
		{
			_window.Advance();
			ReadDecimalNumber();
		}
	}

	private void ReadDecimalNumber()
	{
		char c = _window.Current;
		while (SyntaxFacts.IsValidDecimalDigit(c) || c is Separator)
		{
			_window.Advance();
			c = _window.Current;
		}
	}

	private void ReadHexNumber()
	{
		char c = _window.Current;
		while (SyntaxFacts.IsValidHexDigit(c) || c is Separator)
		{
			_window.Advance();
			c = _window.Current;
		}
	}

	private void ReadBinaryInteger()
	{
		char c = _window.Current;
		while (SyntaxFacts.IsValidBinaryDigit(c) || c is Separator)
		{
			_window.Advance();
			c = _window.Current;
		}
	}

	private void ReadNumericSuffix()
	{
		if (!char.IsAsciiLetter(_window.Current)) return;

		Span<char> buf = stackalloc char[3];
		int len = 0;

		int max = 3;
		while (len < max && char.IsAsciiLetterOrDigit(_window.Peek(len)))
		{
			buf[len++] = _window.Current;
		}

		switch (len)
		{
			case 5:
				switch (buf[..5])
				{
					case "usize":
					case "isize":
						_window.Advance(5);
						break;
				}
				break;
			case 4:
				switch (buf[..4])
				{
					case "u128":
					case "i128":
						_window.Advance(4);
						break;
				}
				break;
			case 3:
				switch (buf[..3])
				{
					case "u16":
					case "u32":
					case "u64":
					case "i16":
					case "i32":
					case "i64":
					case "f16":
					case "f32":
					case "f64":
						_window.Advance(3);
						break;
				}

				break;
			case 2:
				switch (buf[..2])
				{
					case "u8":
					case "i8":
						_window.Advance(2);
						break;
				}

				break;
			case 1:
				// New standard removed these suffixes
				// switch (buf[0])
				// {
				// 	case 'u':
				// 	case 'i':
				// 		break;
				// }

				break;
		}
	}

	#endregion
}