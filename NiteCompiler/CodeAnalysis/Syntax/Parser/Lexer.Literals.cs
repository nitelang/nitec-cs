using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiteCompiler.Diagnostics;
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
						_window.Advance(5);
						break;
					case "isize":
						tokenInfo.ExplicitType = SpecialType.SNativeInt;
						_window.Advance(5);
						break;
				}
				break;
			case 4:
				switch (buf[..4])
				{
					case "u128":
						tokenInfo.ExplicitType = SpecialType.UInt128;
						_window.Advance(4);
						break;
					case "i128":
						tokenInfo.ExplicitType = SpecialType.SInt128;
						_window.Advance(4);
						break;
				}
				break;
			case 3:
				switch (buf[..3])
				{
					case "u16":
						tokenInfo.ExplicitType = SpecialType.UInt16;
						_window.Advance(3);
						break;
					case "u32":
						tokenInfo.ExplicitType = SpecialType.UInt32;
						_window.Advance(3);
						break;
					case "u64":
						tokenInfo.ExplicitType = SpecialType.UInt64;
						_window.Advance(3);
						break;
					case "i16":
						tokenInfo.ExplicitType = SpecialType.SInt16;
						_window.Advance(3);
						break;
					case "i32":
						tokenInfo.ExplicitType = SpecialType.SInt32;
						_window.Advance(3);
						break;
					case "i64":
						tokenInfo.ExplicitType = SpecialType.SInt64;
						_window.Advance(3);
						break;
					case "f16":
						tokenInfo.ExplicitType = SpecialType.Float16;
						_window.Advance(3);
						break;
					case "f32":
						tokenInfo.ExplicitType = SpecialType.Float32;
						_window.Advance(3);
						break;
					case "f64":
						tokenInfo.ExplicitType = SpecialType.Float64;
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


	private void ParseNumericValue(ref TokenInfo tokenInfo, NumericFormat format, ReadOnlySpan<char> literal)
	{
		if (format is NumericFormat.Float or NumericFormat.FloatENotation ||
			format is NumericFormat.Decimal && tokenInfo.ExplicitType is SpecialType.Float16 or SpecialType.Float32 or SpecialType.Float64)
		{
			ParseNumericFloatValue(ref tokenInfo, literal);
		}
		else
		{
			ParseNumericIntegerValue(ref tokenInfo, format, literal);
		}
	}

	private void ParseNumericFloatValue(ref TokenInfo tokenInfo, scoped ReadOnlySpan<char> literal)
	{
		if (literal.Contains(Separator))
		{
			Span<char> cleaned = stackalloc char[literal.Length];
			int length = 0;
			foreach (char c in literal)
			{
				if (c != Separator) cleaned[length++] = c;
			}

			ParseNumericFloatValue(ref tokenInfo, cleaned);
			return;
		}

		bool isOverflow = false;
		switch (tokenInfo.ExplicitType)
		{
			case SpecialType.Float16:
				tokenInfo.ValueType = SpecialType.Float16;
				isOverflow = !FloatParser.TryParseF16(literal, out Half f16);

				tokenInfo.F16Value = f16;
				break;
			case SpecialType.Float64:
				tokenInfo.ValueType = SpecialType.Float64;
				isOverflow = !FloatParser.TryParseF64(literal, out double f64);

				tokenInfo.F64Value = f64;
				break;
			default:
				tokenInfo.ValueType = SpecialType.Float32;
				isOverflow = !FloatParser.TryParseF32(literal, out float f32);

				tokenInfo.F32Value = f32;
				break;
		}

		if (isOverflow)
		{
			Diagnostics.ReportFloatingLiteralTooLarge(CreateCurrentWindowLocation());
		}
	}

	private void ParseNumericIntegerValue(ref TokenInfo tokenInfo, NumericFormat format, ReadOnlySpan<char> literal)
	{
		Debug.Assert(format.IsValid());
		Debug.Assert(format is not (NumericFormat.Float or NumericFormat.FloatENotation));

		int index = 0;
		int radix = 10;
		if (format == NumericFormat.Hexadecimal)
		{
			Debug.Assert(literal.StartsWith("0x"));
			index = 2; radix = 16;
		}
		else if (format == NumericFormat.Binary)
		{
			Debug.Assert(literal.StartsWith("0b"));
			index = 2; radix = 2;
		}

		bool success = ParseNumericIntegerValueFallback(ref tokenInfo, format, literal, index, radix);

		if (!success)
		{
			ParseNumericIntegerValueSlowFallback(ref tokenInfo, format, literal, index, radix);
		}
	}

	/// <returns>
	/// Logical value, whatever this method was able to parse passed literal. In 99.9% cases will return <see langword="true"/>.
	/// </returns>
	private bool ParseNumericIntegerValueFallback(ref TokenInfo tokenInfo, NumericFormat format,
		ReadOnlySpan<char> literal, int index, int radix)
	{
		ulong magnitude = 0;
		for (; index < literal.Length; index++)
		{
			char c = literal[index];
			if (c == Separator) continue;

			int digit;
			if (c is >= '0' and <= '9')
			{
				digit = c - '0';
			}
			else
			{
				Debug.Assert((char)(c & ~0x20) is >= 'A' and <= 'F');
				digit = (char)(c & ~0x20) - 'A' + 10;
			}

			ulong scaled = magnitude * (ulong)radix;
			if (scaled / (ulong)radix != magnitude)
			{
				return false;
			}

			magnitude = scaled + (ulong)digit;
			if (magnitude < scaled)
			{
				return false;
			}
		}

		tokenInfo.ValueType = magnitude <= uint.MaxValue ? SpecialType.UInt32 : SpecialType.UInt64;
		tokenInfo.Magnitude = magnitude;
		return true;
	}

	private void ParseNumericIntegerValueSlowFallback(ref TokenInfo tokenInfo, NumericFormat format,
		ReadOnlySpan<char> literal, int index, int radix)
	{
		UInt128 magnitude = 0;
		bool overflow = false;
		for (; index < literal.Length; index++)
		{
			char c = literal[index];
			if (c == Separator) continue;

			int digit;
			if (c is >= '0' and <= '9')
			{
				digit = c - '0';
			}
			else
			{
				Debug.Assert((char)(c & ~0x20) is >= 'A' and <= 'F');
				digit = (char)(c & ~0x20) - 'A' + 10;
			}

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

	#endregion
}