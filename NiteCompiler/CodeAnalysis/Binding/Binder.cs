using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Compilation;
using NiteCompiler.Symbols;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Binding;

internal abstract class Binder
{
	protected NiteCompilation Compilation { get; }

	public Binder? Parent { get; }
	public BinderFlags Flags { get; }

	public virtual Symbol? ContainingMember
	{
		get
		{
			Debug.Assert(Parent != null);
			return Parent.ContainingMember;
		}
	}

	protected Binder(NiteCompilation compilation)
	{
		Compilation = compilation;
	}

	protected Binder(Binder parent)
	{
		Compilation = parent.Compilation;
		Parent = parent;
	}

	protected Binder(Binder parent, BinderFlags flags)
	{
		Compilation = parent.Compilation;
		Parent = parent;
		Flags = flags;
	}

	public virtual Binder? GetBinder(SyntaxNode node)
	{
		Debug.Assert(Parent != null);
		return Parent.GetBinder(node);
	}

#if DEBUG
	internal Binder[] GetAllBinders()
	{
		var binders = ArrayBuilder<Binder>.GetInstance();
		for (Binder? binder = this; binder != null; binder = binder.Parent)
		{
			binders.Add(binder);
		}
		return binders.ToArrayAndFree();
	}
#endif
}