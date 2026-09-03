using System.Diagnostics;

namespace NiteCompiler.CodeAnalysis.Binding.Operators;

internal enum BinaryOperatorKind
{
	Error = 0,
	Addition,
	Subtraction,
	Multiplication,
	Division,
	Modulo,
	LeftArithmeticShift,
	RightArithmeticShift,
	RightUnsignedShift,
	Equal,
	NotEqual,
	Greater,
	Less,
	GreaterOrEqual,
	LessOrEqual,
	And,
	Xor,
	Or,
	Tilde // keep in sync last item with FromIndex
}

internal static class BinaryOperatorKindExtensions
{
	extension(BinaryOperatorKind kind)
	{
		public int ToIndex() => (int)kind - 1;

		public static BinaryOperatorKind FromIndex(int index)
		{
			Debug.Assert(index is > 0 and <= (int)BinaryOperatorKind.Tilde);

			return (BinaryOperatorKind)index + 1;
		}
	}
}