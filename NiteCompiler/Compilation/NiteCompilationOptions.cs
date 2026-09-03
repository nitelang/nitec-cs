namespace NiteCompiler.Compilation;

public sealed class NiteCompilationOptions
{
	public bool ConcurrentBuild { get; }

	public NiteCompilationOptions(bool concurrentBuildBuild = true)
	{
		ConcurrentBuild = concurrentBuildBuild;
	}
}