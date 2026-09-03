using System.Diagnostics;

namespace NiteCompiler.CodeAnalysis.Binding.Operators;

internal enum UnaryOperatorKind
{
	Error = 0,
	Plus, // +x
	Negate, // -x
	BitwiseNot, // ~x
	LogicalNot, // !x
	Circumflex, // ^x, keep in sync last item with FromIndex
}

internal static class UnaryOperatorKindExtensions
{
	extension(UnaryOperatorKind kind)
	{
		public int ToIndex() => (int)kind - 1;

		public static UnaryOperatorKind FromIndex(int index)
		{
			Debug.Assert(index is > 0 and <= (int)UnaryOperatorKind.Circumflex);

			return (UnaryOperatorKind)index + 1;
		}
	}
}