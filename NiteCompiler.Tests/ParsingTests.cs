using NiteCompiler.CodeAnalysis.Syntax;
using static TUnit.Assertions.Assert;

namespace NiteCompiler.Tests;

[Timeout(10_000)]
public class ParsingTests
{
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
}