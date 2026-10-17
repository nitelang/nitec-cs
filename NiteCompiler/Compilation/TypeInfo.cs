using NiteCompiler.Symbols;

namespace NiteCompiler.Compilation;

public readonly struct TypeInfo : IEquatable<TypeInfo>
{
	public TypeSymbol? Type { get; }

	internal TypeInfo(TypeSymbol? type)
	{
		Type = type;
	}

	public bool Equals(TypeInfo other)
	{
		return Equals(Type, other.Type);
	}

	public override bool Equals(object? obj)
	{
		return obj is TypeInfo other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type);
	}
}