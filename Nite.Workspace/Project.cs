namespace Nite.Workspace;

public class Project
{
	public string Name { get; }
	public IEnumerable<Project> Dependencies { get; }
}