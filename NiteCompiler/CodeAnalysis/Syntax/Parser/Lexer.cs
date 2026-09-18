using System.Diagnostics;
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

	public Lexer(SyntaxTree syntaxTree, SourceText source, NiteParseOptions options, DiagnosticBag diagnostics)
	{
		SyntaxTree = syntaxTree;
		Options = options;
		Diagnostics = diagnostics;
		_window = new SlidingWindow(source);
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

		return new SyntaxToken(info.Kind, span, leading, trailing, SyntaxTree);
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
					_window.Advance();
					info.Kind = SyntaxKind.BadToken;
				}

				break;
		}
	}
}