using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static TUnit.Assertions.Assert;
using System.Threading.Tasks;

namespace NiteCompiler.Tests;

public class ParsingTests
{
	[Test]
	public async Task RandomCharactersTest()
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

		// Need check that random characters do not break the lexer and parser logic, and cause no exception :)
		await That(true).IsTrue();
	}
}