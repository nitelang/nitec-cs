using System.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Lexer
{
	#region Numbers

	private const char Separator = '\'';

	private enum NumericFormat
	{
		Decimal,
		Hexadecimal,
		Binary,
		Float,
		FloatENotation,
	}

	private void ReadNumber(ref TokenInfo tokenInfo)
	{
		tokenInfo.Kind = SyntaxKind.NumericLiteral;
		NumericFormat format;

		if (_window.Current == '0')
		{
			switch (_window.Next)
			{
				case 'b':
					_window.Advance(2);
					format = NumericFormat.Binary;
					ReadBinaryInteger();
					return;
				case 'x':
					_window.Advance(2);
					format = NumericFormat.Hexadecimal;
					ReadHexNumber();
					return;

				// 0123 is decimal literal, not octal
				default:
					format = NumericFormat.Decimal;
					ReadDecimalLiteral();
					break; // decimal literal can be part of float literal
			}
		}
		else
		{
			format = NumericFormat.Decimal;
			ReadDecimalLiteral();
		}

		if (_window.Current == '.') // read precision part
		{
			format = NumericFormat.Float;
			_window.Advance();
			ReadDecimalLiteral();
		}

		if (_window.Current is 'e' or 'E') // read E-notation part
		{
			format = NumericFormat.FloatENotation;
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

		ReadNumericSuffix(ref tokenInfo);
		ParseNumericValue(ref tokenInfo, format);
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

	private void ReadNumericSuffix(ref TokenInfo tokenInfo)
	{
		if (!char.IsAsciiLetter(_window.Current)) return;

		Span<char> buf = stackalloc char[5];
		int len = 0;

		const int max = 5;
		while (len < max && char.IsAsciiLetterOrDigit(_window.Peek(len)))
		{
			buf[len] = _window.Peek(len++);
		}

		switch (len)
		{
			case 5:
				switch (buf[..5])
				{
					case "usize":
						tokenInfo.ValueType = SpecialType.UNativeInt;
						goto default;
					case "isize":
						tokenInfo.ValueType = SpecialType.SNativeInt;
						goto default;
					default:
						_window.Advance(5);
						break;
				}
				break;
			case 4:
				switch (buf[..4])
				{
					case "u128":
						tokenInfo.ValueType = SpecialType.UInt128;
						goto default;
					case "i128":
						tokenInfo.ValueType = SpecialType.SInt128;
						goto default;
					default:
						_window.Advance(4);
						break;
				}
				break;
			case 3:
				switch (buf[..3])
				{
					case "u16":
						tokenInfo.ValueType = SpecialType.UInt16;
						goto default;
					case "u32":
						tokenInfo.ValueType = SpecialType.UInt32;
						goto default;
					case "u64":
						tokenInfo.ValueType = SpecialType.UInt64;
						goto default;
					case "i16":
						tokenInfo.ValueType = SpecialType.SInt16;
						goto default;
					case "i32":
						tokenInfo.ValueType = SpecialType.SInt32;
						goto default;
					case "i64":
						tokenInfo.ValueType = SpecialType.UInt64;
						goto default;
					case "f16":
						tokenInfo.ValueType = SpecialType.Float16;
						goto default;
					case "f32":
						tokenInfo.ValueType = SpecialType.Float32;
						goto default;
					case "f64":
						tokenInfo.ValueType = SpecialType.Float64;
						goto default;
					default:
						_window.Advance(3);
						break;
				}

				break;
			case 2:
				switch (buf[..2])
				{
					case "u8":
						tokenInfo.ValueType = SpecialType.UInt8;
						_window.Advance(2);
						break;
					case "i8":
						tokenInfo.ValueType = SpecialType.SInt8;
						_window.Advance(2);
						break;
				}

				break;
		}
	}

	#endregion

	private static void ParseNumericValue(ref TokenInfo tokenInfo, NumericFormat format)
	{
		throw new NotImplementedException();
	}
}