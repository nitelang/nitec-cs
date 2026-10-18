using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.Compilation;
using NiteCompiler.Diagnostics;
using NiteCompiler.Symbols.Source;
using NiteCompiler.Utilities;

namespace NiteCompiler.Symbols;

public abstract class Symbol
{
	public abstract SymbolKind Kind { get; }
	public abstract Symbol? ContainingSymbol { get; }

	private protected Symbol() { }

	/// <summary>
	/// Returns symbol's name, or empty string if not applicable.
	/// </summary>
	public virtual string Name => string.Empty;

	/// <summary>
	/// Return symbol lifetime parameter count.
	/// </summary>
	public virtual int LifetimeArity => 0;

	/// <summary>
	/// Return symbol template parameter count.
	/// </summary>
	public virtual int Arity => 0;

	/// <summary>
	/// Returns symbol accessibility level, or <see cref="Accessibility.None"/> if not applicable.
	/// </summary>
	public virtual Accessibility Accessibility => Accessibility.None;

	public virtual LibrarySymbol? ContainingLibrary
	{
		get
		{
			if (ContainingSymbol is LibrarySymbol lib)
			{
				return lib;
			}

			return ContainingSymbol?.ContainingLibrary;
		}
	}

	public virtual ModuleSymbol? ContainingModule
	{
		get
		{
			if (ContainingSymbol is ModuleSymbol lib)
			{
				return lib;
			}

			return ContainingSymbol?.ContainingModule;
		}
	}

	public virtual NiteCompilation? DeclaringCompilation
	{
		get
		{
			Symbol? containingSymbol = ContainingSymbol;
			while (containingSymbol != null)
			{
				NiteCompilation? declaring = containingSymbol.DeclaringCompilation;
				if (declaring != null) return declaring;

				containingSymbol = containingSymbol.ContainingSymbol;
			}

			return null;
		}
	}

	public virtual ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	internal virtual void ForceComplete(Predicate<Symbol>? filter, CancellationToken cancellationToken = default)
	{
		throw ExceptionUtilities.Unreachable();
	}

	internal static void ForceCompleteMemberConditionally(Predicate<Symbol>? filter, Symbol member, CancellationToken cancellationToken = default)
	{
		if (filter.Passes(member))
		{
			cancellationToken.ThrowIfCancellationRequested();
			member.ForceComplete(filter, cancellationToken);
		}
	}

	internal virtual bool HasComplete(CompletionPart part) => true;

	internal virtual void AddDeclarationDiagnostics(PooledDiagnosticBag diagnostics)
	{
		if (!diagnostics.IsEmpty)
		{
			NiteCompilation? compilation = DeclaringCompilation;
			Debug.Assert(compilation != null);

			compilation.DeclarationDiagnostics.AddRange(diagnostics.Diagnostics);
		}
	}
}