namespace NiteCompiler.Compilation;

public enum NiteVersion : uint
{
	Nite1 = 1,
	Preview = 0,
}

public static class NiteVersionExtensions
{
	extension(NiteVersion version)
	{
		// use property, so prevent version reevaluation
		public static NiteVersion LatestStable => NiteVersion.Nite1;
	}
}