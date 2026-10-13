using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiteCompiler.Compilation;
using NiteCompiler.Diagnostics;
using NiteCompiler.Text;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal sealed partial class Lexer
{
	public SyntaxTree SyntaxTree { get; }
	public NiteParseOptions Options { get; }
	public DiagnosticBag Diagnostics { get; }
	private SlidingWindow _window;
	private readonly Dictionary<string, string> _identifiers;

	private SourceLocation CreateCurrentWindowLocation()
	{
		return new SourceLocation(SyntaxTree, _window.LexemeSpan);
	}

	public Lexer(SyntaxTree syntaxTree, SourceText source, NiteParseOptions options, DiagnosticBag diagnostics)
	{
		SyntaxTree = syntaxTree;
		Options = options;
		Diagnostics = diagnostics;
		_window = new SlidingWindow(source);
		_identifiers = new Dictionary<string, string>(StringComparer.Ordinal);
	}

	private string InternIdentifier(ReadOnlySpan<char> text)
	{
		var lookup = _identifiers.GetAlternateLookup<ReadOnlySpan<char>>();

		if (lookup.TryGetValue(text, out string? interned))
		{
			return interned;
		}

		string created = text.ToString();
		_identifiers[created] = created;
		return created;
	}

	private string InternIdentifier(string text)
	{
		if (_identifiers.TryGetValue(text, out string? interned))
		{
			return interned;
		}

		_identifiers[text] = text;
		return text;
	}

	private ResetPoint GetResetPoint()
	{
		return new ResetPoint(_window.Position);
	}

	private void Reset(ResetPoint rp)
	{
		_window.Reset(rp.Position);
	}

	internal ref struct TokenInfo
	{
		public SyntaxKind Kind;
		public string? Identifier; // Pure identifier value
		public SpecialType ValueType;

		public Half F16Value;
		public float F32Value;
		public double F64Value;

		public byte U8Value;
		public sbyte I8Value;
		public ushort U16Value;
		public short I16Value;
		public uint U32Value;
		public int I32Value;
		public ulong U64Value;
		public long I64Value;
		public UInt128 U128Value;
		public Int128 I128Value;
	}

	public SyntaxToken Lex()
	{
		TokenInfo info = default;

		var trivia = ArrayBuilder<SyntaxTrivia>.GetInstance();

		ReadTrivia(true, trivia);
		var leading = trivia.ToImmutableAndClear();

		_window.Start();
		ReadToken(ref info);
		TextSpan span = _window.LexemeSpan;

		ReadTrivia(false, trivia);
		var trailing = trivia.ToImmutableAndFree();

		return ProduceToken(in info, span, leading, trailing, SyntaxTree);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private SyntaxToken ProduceToken(in TokenInfo info, TextSpan span, ImmutableArray<SyntaxTrivia> leading, ImmutableArray<SyntaxTrivia> trailing, SyntaxTree syntaxTree)
	{
		switch (info.Kind)
		{
			case SyntaxKind.Identifier:
			case SyntaxKind.EscapedIdentifier:
				Debug.Assert(info.Identifier != null);
				return new SyntaxTokenIdentifier(info.Kind, span, leading, trailing, syntaxTree, info.Identifier);
			case SyntaxKind.NumericLiteral:
				return ProduceNumericToken(in info, span, leading, trailing, syntaxTree);
			default:
				return new SyntaxToken(info.Kind, span, leading, trailing, syntaxTree);
		}
	}

	private static SyntaxToken ProduceNumericToken(in TokenInfo info, TextSpan span,
		ImmutableArray<SyntaxTrivia> leading, ImmutableArray<SyntaxTrivia> trailing, SyntaxTree syntaxTree)
	{
		const SyntaxKind kind = SyntaxKind.NumericLiteral;

		return info.ValueType switch
		{
			SpecialType.SInt8 => new SyntaxTokenWithValue<sbyte>(kind, span, leading, trailing, syntaxTree, info.I8Value),
			SpecialType.SInt16 => new SyntaxTokenWithValue<short>(kind, span, leading, trailing, syntaxTree, info.I16Value),
			SpecialType.SInt32 => new SyntaxTokenWithValue<int>(kind, span, leading, trailing, syntaxTree, info.I32Value),
			SpecialType.SInt64 or SpecialType.SNativeInt => new SyntaxTokenWithValue<long>(kind, span, leading, trailing, syntaxTree, info.I64Value),
			SpecialType.SInt128 => new SyntaxTokenWithValue<Int128>(kind, span, leading, trailing, syntaxTree, info.I128Value),
			SpecialType.UInt8 => new SyntaxTokenWithValue<byte>(kind, span, leading, trailing, syntaxTree, info.U8Value),
			SpecialType.UInt16 => new SyntaxTokenWithValue<ushort>(kind, span, leading, trailing, syntaxTree, info.U16Value),
			SpecialType.UInt32 => new SyntaxTokenWithValue<uint>(kind, span, leading, trailing, syntaxTree, info.U32Value),
			SpecialType.UInt64 or SpecialType.UNativeInt => new SyntaxTokenWithValue<ulong>(kind, span, leading, trailing, syntaxTree, info.U64Value),
			SpecialType.UInt128 => new SyntaxTokenWithValue<UInt128>(kind, span, leading, trailing, syntaxTree, info.U128Value),
			SpecialType.Float16 => new SyntaxTokenWithValue<Half>(kind, span, leading, trailing, syntaxTree, info.F16Value),
			SpecialType.Float32 => new SyntaxTokenWithValue<float>(kind, span, leading, trailing, syntaxTree, info.F32Value),
			SpecialType.Float64 => new SyntaxTokenWithValue<double>(kind, span, leading, trailing, syntaxTree, info.F64Value),
			_ => throw ExceptionUtilities.Unreachable(),
		};
	}

	private void ReadToken(ref TokenInfo info)
	{
		if (_window.IsAtTheEnd)
		{
			info.Kind = SyntaxKind.EndOfFile;
			return;
		}

		switch (_window.Current)
		{
			case '.':
				if (_window.Next is '.')
				{
					if (_window.Peek(2) is '=')
					{
						_window.Advance(3);
						info.Kind = SyntaxKind.InclusiveRange;
					}
					else
					{
						_window.Advance(2);
						info.Kind = SyntaxKind.Range;
					}
					break;
				}

				_window.Advance();
				info.Kind = SyntaxKind.Dot;
				break;
			case ',':
				_window.Advance();
				info.Kind = SyntaxKind.Comma;
				break;
			case >= '0' and <= '9':
				ReadNumber(ref info);
				Debug.Assert(_window.Width > 0);
				break;
			case >= 'A' and <= 'Z':
			case >= 'a' and <= 'z':
				ReadIdentifier(ref info);
				Debug.Assert(_window.Width > 0);

				SyntaxKind keyword = SyntaxKind.GetKeyword(info.Identifier!);
				info.Kind = keyword == SyntaxKind.None ? info.Kind : keyword;
				break;
			case '`':
				ReadEscapedIdentifier(ref info);
				break;
			case '_':
				ReadIdentifier(ref info);
				Debug.Assert(_window.Width > 0);
				if (_window.Width == 1)
				{
					info.Kind = SyntaxKind.Discard;
				}
				break;
			case ':':
				if (_window.Next is ':')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.DoubleColon;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Colon;
				}
				break;
			case ';':
				_window.Advance();
				info.Kind = SyntaxKind.Semicolon;
				break;
			case '+':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.PlusEquals;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Plus;
				}
				break;
			case '-':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.MinusEquals;
				}
				else if (_window.Next is '>')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.Arrow;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Minus;
				}
				break;
			case '*':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.AsteriskEquals;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Asterisk;
				}
				break;
			case '/':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.SlashEquals;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Slash;
				}
				break;
			case '%':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.PercentEquals;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Percent;
				}
				break;
			case '<':
				if (_window.Next is '<')
				{
					if (_window.Peek(2) is '=')
					{
						_window.Advance(3);
						info.Kind = SyntaxKind.LeftShiftEquals;
					}
					else
					{
						_window.Advance(2);
						info.Kind = SyntaxKind.LeftShift;
					}
				}
				else if (_window.Next is '=')
				{
					_window.Advance(1);
					info.Kind = SyntaxKind.LessEquals;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Less;
				}
				break;
			case '>':
				_window.Advance();
				info.Kind = SyntaxKind.Greater;
				break;
			case '~':
				_window.Advance();
				info.Kind = SyntaxKind.Tilde;
				break;
			case '!':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.NotEquals;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Exclamation;
				}
				break;
			case '=':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.DoubleEquals;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Equals;
				}

				break;
			case '&':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.AmpersandEquals;
				}
				else if (_window.Next is '&')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.DoubleAmpersand;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Ampersand;
				}
				break;
			case '|':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.BarEquals;
				}
				else if (_window.Next is '|')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.DoubleBar;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Bar;
				}
				break;
			case '^':
				if (_window.Next is '=')
				{
					_window.Advance(2);
					info.Kind = SyntaxKind.CaretEquals;
				}
				else
				{
					_window.Advance();
					info.Kind = SyntaxKind.Caret;
				}
				break;
			case '{':
				_window.Advance();
				info.Kind = SyntaxKind.OpenBrace;
				break;
			case '}':
				_window.Advance();
				info.Kind = SyntaxKind.CloseBrace;
				break;
			case '(':
				_window.Advance();
				info.Kind = SyntaxKind.OpenParen;
				break;
			case ')':
				_window.Advance();
				info.Kind = SyntaxKind.CloseParen;
				break;
			case '[':
				_window.Advance();
				info.Kind = SyntaxKind.OpenBracket;
				break;
			case ']':
				_window.Advance();
				info.Kind = SyntaxKind.CloseBracket;
				break;
			default:
				ReadIdentifier(ref info);

				if (_window.Width == 0)
				{
					char unexpected = _window.Current;
					_window.Advance();
					info.Kind = SyntaxKind.BadToken;
					Diagnostics.ReportUnexpectedCharacter(CreateCurrentWindowLocation(), unexpected.ToString());
				}

				break;
		}
	}
}