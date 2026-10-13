using System.Diagnostics;
using System.Globalization;
using NiteCompiler.Diagnostics;

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
					goto SKIP_FLOAT;
				case 'x':
					_window.Advance(2);
					format = NumericFormat.Hexadecimal;
					ReadHexNumber();
					goto SKIP_FLOAT;

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

	SKIP_FLOAT:

		int literalLength = _window.Width;
		ReadNumericSuffix(ref tokenInfo);
		ParseNumericValue(ref tokenInfo, format, _window.LexemeChars[..literalLength]);
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
						tokenInfo.ExplicitType = SpecialType.UNativeInt;
						goto default;
					case "isize":
						tokenInfo.ExplicitType = SpecialType.SNativeInt;
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
						tokenInfo.ExplicitType = SpecialType.UInt128;
						goto default;
					case "i128":
						tokenInfo.ExplicitType = SpecialType.SInt128;
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
						tokenInfo.ExplicitType = SpecialType.UInt16;
						goto default;
					case "u32":
						tokenInfo.ExplicitType = SpecialType.UInt32;
						goto default;
					case "u64":
						tokenInfo.ExplicitType = SpecialType.UInt64;
						goto default;
					case "i16":
						tokenInfo.ExplicitType = SpecialType.SInt16;
						goto default;
					case "i32":
						tokenInfo.ExplicitType = SpecialType.SInt32;
						goto default;
					case "i64":
						tokenInfo.ExplicitType = SpecialType.SInt64;
						goto default;
					case "f16":
						tokenInfo.ExplicitType = SpecialType.Float16;
						goto default;
					case "f32":
						tokenInfo.ExplicitType = SpecialType.Float32;
						goto default;
					case "f64":
						tokenInfo.ExplicitType = SpecialType.Float64;
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
						tokenInfo.ExplicitType = SpecialType.UInt8;
						_window.Advance(2);
						break;
					case "i8":
						tokenInfo.ExplicitType = SpecialType.SInt8;
						_window.Advance(2);
						break;
				}

				break;
		}
	}

	#endregion

	private void ParseNumericValue(ref TokenInfo tokenInfo, NumericFormat format, ReadOnlySpan<char> literal)
	{
		// Strip prefix for hex/binary
		int index = 0;
		int radix = 10;
		if (format == NumericFormat.Hexadecimal && literal is ['0', 'x', ..])
		{
			index = 2; radix = 16;
		}
		else if (format == NumericFormat.Binary && literal is ['0', 'b', ..])
		{
			index = 2; radix = 2;
		}

		if (format is NumericFormat.Float or NumericFormat.FloatENotation)
		{
			throw new NotImplementedException();
		}

		// TODO[Perf]: Using UInt128 here is very imperformant, maybe we can use UInt128 version as fallback, if UInt64 is overflowed
		UInt128 magnitude = 0;
		bool overflow = false;
		for (; index < literal.Length; index++)
		{
			char c = literal[index];
			if (c == Separator) continue;

			int digit = -1;
			if (c is >= '0' and <= '9')
			{
				digit = c - '0';
			}
			else if ((char)(c & ~0x20) is >= 'A' and <= 'F')
			{
				digit = (char)(c & ~0x20) - 'A' + 10;
			}
			Debug.Assert(digit != -1);

			if (digit < 0 || digit >= radix) continue;

			UInt128 scaled = magnitude * (UInt128)radix;
			if (scaled / (UInt128)radix != magnitude)
			{
				overflow = true; // wrapped
			}

			magnitude = scaled + (UInt128)digit;
			if (magnitude < scaled)
			{
				overflow = true;
			}
		}

		if (overflow)
		{
			Diagnostics.ReportIntegralLiteralTooLarge(CreateCurrentWindowLocation());
			magnitude = UInt128.MaxValue;
		}

		tokenInfo.ValueType
			= magnitude <= uint.MaxValue ? SpecialType.UInt32
			: magnitude <= ulong.MaxValue ? SpecialType.UInt64
			: SpecialType.UInt128;
		tokenInfo.Magnitude = magnitude;
	}
}