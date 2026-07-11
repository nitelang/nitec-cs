namespace NiteCompiler.CodeAnalysis.Syntax;

public enum Precedence : byte
{
    Expression = 0,
    Assignment = Expression, // conditional separation; += -= *= ...
    Ternary, // ?:
    ConditionalOr, // fast ||
    ConditionalAnd, // fast &&
    BitwiseOr, // bitwise | (aka logical "or" operator)
    BitwiseXor, // ^
    BitwiseAnd, // &
    Equality, // == !=
    Relational, // > >= < <=
    Shift,
    Additive, // + -
    Multiplicative, // * / %
    Range, // .. ..=
    Unary, // - + * &
    Cast, // x as Y
    Primary // 32 "string" 'c' identifier
}