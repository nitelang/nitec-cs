using NiteCompiler.CodeAnalysis.Syntax;
using static TUnit.Assertions.Assert;

namespace NiteCompiler.Tests;

[Timeout(10_000)]
public class ParsingTests
{
	[Test]
	public async Task ZeroLengthFileTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText(string.Empty, cancellationToken: cancellationToken);

		await That(tree).IsNotNull();
	}

	[Test]
	public async Task FileFilledWithSpacesTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText(" \n\r\n\t  \t\v", cancellationToken: cancellationToken);

		await That(tree).IsNotNull();
	}

	[Test]
	public async Task ShebangTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("#! usr/bin/python3", cancellationToken: cancellationToken);

		await That(tree).IsNotNull();
	}

	[Test]
	public async Task RandomCharactersTest(CancellationToken cancellationToken)
	{
		Random rand = Random.Shared;

		int length = rand.Next(2048, 2048 * 12);
		char[] c = new char[length];

		unsafe
		{
			fixed (char* pC = c)
			{
				Span<byte> bytes = new(pC, length * sizeof(char));
				rand.NextBytes(bytes);
			}
		}
		string randomText = new(c);

		var tree = SyntaxTree.FromText(randomText, cancellationToken: cancellationToken);

		await That(tree).IsNotNull();
	}

	[Test]
	public async Task TMP_ExpressionsTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("3 + 2 / 4", cancellationToken: cancellationToken);

		await That(tree).IsNotNull();
	}
}