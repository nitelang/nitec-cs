using System.Diagnostics;
using NiteCompiler.Text;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Lexer
{
	private void ReadTrivia(bool leading, ArrayBuilder<SyntaxTrivia> trivia)
	{
		Debug.Assert(trivia.IsEmpty);

		bool done = false;

		while (!done)
		{
			_window.Start();

			switch (_window.Current)
			{
				case SlidingWindow.InvalidCharacter:
					done = true;
					break;
				case '#' when _window.Next == '!':
					ReadShebang();
					trivia.Add(new SyntaxTrivia(SyntaxKind.ShebangTrivia, _window.LexemeSpan, SyntaxTree));
					break;
				case '/':
					switch (_window.Next)
					{
						case '/':
							if (_window.Peek(2) == '/')
							{
								ReadDocsComment();
								trivia.Add(new SyntaxTrivia(SyntaxKind.SingleLineComment, _window.LexemeSpan, SyntaxTree));
							}
							else
							{
								ReadSingleLineComment();
								trivia.Add(new SyntaxTrivia(SyntaxKind.SingleLineComment, _window.LexemeSpan, SyntaxTree));
							}
							break;
						case '*':
							ReadMultiLineComment();
							trivia.Add(new SyntaxTrivia(SyntaxKind.MultiLineComment, _window.LexemeSpan, SyntaxTree));
							break;
						default:
							done = true;
							break;
					}
					break;
				case '\n':
				case '\r':
					if (!leading)
						done = true;
					ReadLineBreak();
					trivia.Add(new SyntaxTrivia(SyntaxKind.LineBreakTrivia, _window.LexemeSpan, SyntaxTree));
					break;
				case ' ':
				case '\t':
					ReadWhiteSpace();
					trivia.Add(new SyntaxTrivia(SyntaxKind.EmptySpaceTrivia, _window.LexemeSpan, SyntaxTree));
					break;
				default:
					if (char.IsWhiteSpace(_window.Current))
					{
						ReadWhiteSpace();
						trivia.Add(new SyntaxTrivia(SyntaxKind.EmptySpaceTrivia, _window.LexemeSpan, SyntaxTree));
					}
					else
						done = true;
					break;
			}
		}
	}

	private void ReadShebang()
	{
		ReadSingleLineComment();
	}

	private void ReadLineBreak()
	{
		_window.AdvancePastNewLine();
	}

	private void ReadWhiteSpace()
	{
		bool done = false;

		while (!done)
		{
			switch (_window.Current)
			{
				case SlidingWindow.InvalidCharacter:
				case '\r':
				case '\n':
					done = true;
					break;
				default:
					if (!char.IsWhiteSpace(_window.Current))
						done = true;
					else
						_window.Advance();
					break;
			}
		}
	}

	private void ReadDocsComment()
	{
		ReadSingleLineComment();
	}

	private void ReadSingleLineComment()
	{
		_window.Advance(2);
		bool done = false;

		while (!done)
		{
			switch (_window.Current)
			{
				case SlidingWindow.InvalidCharacter:
				case '\r':
				case '\n':
					done = true;
					break;
				default:
					_window.Advance();
					break;
			}
		}
	}

	private void ReadMultiLineComment()
	{
		_window.Advance(2);
		bool done = false;

		while (!done)
		{
			switch (_window.Current)
			{
				case SlidingWindow.InvalidCharacter:
					TextSpan span = _window.LexemeSpan;
					//TODO: _diagnostics.ReportUnterminatedMultiLineComment(span.Contextualize(_syntaxTree));
					done = true;
					break;
				case '*':
					if (_window.Next == '/')
					{
						_window.Advance();
						done = true;
					}
					_window.Advance();
					break;
				default:
					_window.Advance();
					break;
			}
		}
	}
}