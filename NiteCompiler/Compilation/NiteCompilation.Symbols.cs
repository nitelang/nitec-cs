using NiteCompiler.Symbols;
using NiteCompiler.Symbols.Source;
using NiteCompiler.Utilities;

namespace NiteCompiler.Compilation;

public partial class NiteCompilation
{
	public LibrarySymbol Library
	{
		get
		{
			if (field == null)
			{
				// TODO[low-priority]: _libraryName is nullable
				LibrarySymbol library = new SourceLibrarySymbol(this, Declarations, _libraryName);

				Interlocked.CompareExchange(ref field, library, null);
			}

			return field;
		}
	}

	public ModuleSymbol GlobalModule
	{
		get
		{
			if (field == null)
			{
				var modules = ArrayBuilder<ModuleSymbol>.GetInstance();

				modules.Add(Library.GlobalModule);
				/*foreach (LibrarySymbol lib in GetDependencyLibraries())
				{
					modules.Add(lib.GlobalModule);
				}*/

				MergedModuleSymbol module = new(modules.ToImmutableAndFree());

				Interlocked.CompareExchange(ref field, module, null);
			}

			return field;
		}
	}
}