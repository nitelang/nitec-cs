using NiteCompiler.Diagnostics;

namespace NiteCompiler.Compilation;

public interface IEditorConfigResolver
{
	EditorConfig GetEditorConfig();
}