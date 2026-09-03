using System.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.Compilation;

public sealed class NiteParseOptions
{
	public static NiteParseOptions Default { get; } = new();

	public NiteVersion Version { get; }

	public NiteParseOptions(NiteVersion? version = null)
	{
		Debug.Assert(version?.IsValid() ?? true); // either null or valid

		Version = version ?? NiteVersion.LatestStable;
	}
}