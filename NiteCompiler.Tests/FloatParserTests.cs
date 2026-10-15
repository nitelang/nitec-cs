global using static TUnit.Assertions.Assert;

using System.Globalization;
using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.Tests;

public class FloatParserTests
{
	[Test]
	[Arguments("0")]
	[Arguments("1")]
	[Arguments("0.5")]
	[Arguments("0.1")]
	[Arguments("3.141592653589793")]
	[Arguments("123456789.123456789")]
	[Arguments("1.00000000000000000000000000000000001")]
	[Arguments("2.2250738585072011e-308")]
	[Arguments("1.7976931348623157e308")]
	[Arguments("5e-324")]
	public async Task ParseF64MatchesSystemParser(string literal)
	{
		await That(FloatParser.TryParseF64(literal, out double value)).IsTrue();
		await That(value).IsEqualTo(double.Parse(literal, NumberStyles.Float, CultureInfo.InvariantCulture));
	}

	[Test]
	[Arguments("0")]
	[Arguments("1")]
	[Arguments("0.5")]
	[Arguments("0.1")]
	[Arguments("3.141592653589793")]
	[Arguments("123456789.123456789")]
	[Arguments("1.00000000000000000000000000000000001")]
	[Arguments("1.1754942106924411e-38")]
	[Arguments("3.4028234663852886e38")]
	[Arguments("1e-45")]
	public async Task ParseF32MatchesSystemParser(string literal)
	{
		await That(FloatParser.TryParseF32(literal, out float value)).IsTrue();
		await That(value).IsEqualTo(float.Parse(literal, NumberStyles.Float, CultureInfo.InvariantCulture));
	}

	[Test]
	[Arguments("0", 0x0000)]
	[Arguments("1", 0x3C00)]
	[Arguments("0.5", 0x3800)]
	[Arguments("0.1", 0x2E66)]
	[Arguments("65504", 0x7BFF)]
	[Arguments("65519", 0x7BFF)]
	[Arguments("5.9604644775390625e-8", 0x0001)]
	[Arguments("1e-8", 0x0000)]
	public async Task ParseF16RoundsToNearest(string literal, int expectedBits)
	{
		await That(FloatParser.TryParseF16(literal, out Half value)).IsTrue();
		await That(BitConverter.HalfToUInt16Bits(value)).IsEqualTo((ushort)expectedBits);
	}

	[Test]
	[Arguments("65520")]
	[Arguments("1e10")]
	public async Task ParseF16OverflowYieldsInfinity(string literal)
	{
		await That(FloatParser.TryParseF16(literal, out Half value)).IsFalse();
		await That(value).IsEqualTo(Half.PositiveInfinity);
	}

	[Test]
	public async Task ParseF32OverflowYieldsInfinity()
	{
		await That(FloatParser.TryParseF32("1e40", out float value)).IsFalse();
		await That(value).IsEqualTo(float.PositiveInfinity);
	}

	[Test]
	public async Task ParseF64OverflowYieldsInfinity()
	{
		await That(FloatParser.TryParseF64("1e400", out double value)).IsFalse();
		await That(value).IsEqualTo(double.PositiveInfinity);
	}

	[Test]
	public async Task ParseF64UnderflowYieldsZero()
	{
		await That(FloatParser.TryParseF64("1e-400", out double value)).IsTrue();
		await That(value).IsEqualTo(0.0);
	}
}