using System.Collections.Immutable;
using NiteCompiler.Symbols;

namespace NiteCompiler.Compilation;

public readonly struct SymbolInfo : IEquatable<SymbolInfo>
{
	public Symbol? Symbol { get; }

	public ImmutableArray<Symbol> CandidateSymbols { get; }

	public CandidateReason CandidateReason { get; }

	internal SymbolInfo(Symbol symbol) : this(symbol, ImmutableArray<Symbol>.Empty, CandidateReason.None)
	{ }

	internal SymbolInfo(ImmutableArray<Symbol> candidateSymbols, CandidateReason candidateReason)
		: this(symbol: null, candidateSymbols, candidateReason)
	{ }

	private SymbolInfo(Symbol? symbol, ImmutableArray<Symbol> candidateSymbols, CandidateReason candidateReason)
	{
		Symbol = symbol;
		CandidateSymbols = candidateSymbols;
		CandidateReason = candidateReason;
	}

	public bool Equals(SymbolInfo other)
	{
		return Equals(Symbol, other.Symbol) &&
			   CandidateSymbols.Equals(other.CandidateSymbols);
	}

	public override bool Equals(object? obj)
	{
		return obj is SymbolInfo other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Symbol, CandidateSymbols);
	}
}