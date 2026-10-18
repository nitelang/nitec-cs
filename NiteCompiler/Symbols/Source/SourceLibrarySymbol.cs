using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Declarations;
using NiteCompiler.Compilation;
using NiteCompiler.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.Symbols.Source;

internal sealed class SourceLibrarySymbol : LibrarySymbol
{
	public override NiteCompilation DeclaringCompilation { get; }
	public override string Name { get; }

	private readonly DeclarationTable _declarationTable;
	public SourceLibrarySymbol(NiteCompilation compilation, DeclarationTable declarationTable, string name)
	{
		_declarationTable = declarationTable;
		DeclaringCompilation = compilation;
		Name = name;
	}

	public override ModuleSymbol GlobalModule
	{
		get
		{
			if (field == null)
			{
				var diagnostics = PooledDiagnosticBag.GetInstance();

				SourceModuleSymbol globalModule = new SourceModuleSymbol(this, null,
					DeclaringCompilation.MergedRootDeclaration, diagnostics);
				if (Interlocked.CompareExchange(ref field, globalModule, null) == null)
				{
					AddDeclarationDiagnostics(diagnostics);
				}

				diagnostics.Free();
			}

			return field;
		}
	}

	public override ImmutableArray<Symbol> GetMembersUnordered()
	{
		return [GlobalModule];
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return [GlobalModule];
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return name == string.Empty ? [GlobalModule] : [];
	}

	public override ImmutableArray<TypeSymbol> GetTypeMembers()
	{
		return [];
	}

	public override ImmutableArray<TypeSymbol> GetTypeMembers(string name, int? arity)
	{
		return [];
	}

	private CompletionPart _state;
	internal override void ForceComplete(Predicate<Symbol>? filter, CancellationToken cancellationToken = default)
	{
		if (!filter.Passes(this)) return;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var incompletePart = _state.NextIncompletePart;
			switch (incompletePart)
			{
				case CompletionPart.None:
					return;
				case CompletionPart.MembersCompleted:
					GlobalModule.ForceComplete(null, cancellationToken);

					_state.NotePartComplete(CompletionPart.MembersCompleted);
					break;
				default:
					_state.NotePartComplete(CompletionPart.All & ~CompletionPart.LibrarySymbolAll);
					break;
			}

			_state.SpinWaitComplete(incompletePart, cancellationToken);
		}
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return _state.HasComplete(part);
	}
}