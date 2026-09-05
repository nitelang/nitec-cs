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
			Window.Start();

			switch (Window.Current)
			{
				case SlidingWindow.InvalidCharacter:
					done = true;
					break;
				case '#' when Window.Next == '!':
					ReadShebang();
					trivia.Add(new SyntaxTrivia(SyntaxKind.ShebangTrivia, Window.LexemeSpan, SyntaxTree));
					break;
				case '/':
					switch (Window.Next)
					{
						case '/':
							if (Window.Peek(2) == '/')
							{
								ReadDocsComment();
								trivia.Add(new SyntaxTrivia(SyntaxKind.SingleLineComment, Window.LexemeSpan, SyntaxTree));
							}
							else
							{
								ReadSingleLineComment();
								trivia.Add(new SyntaxTrivia(SyntaxKind.SingleLineComment, Window.LexemeSpan, SyntaxTree));
							}
							break;
						case '*':
							ReadMultiLineComment();
							trivia.Add(new SyntaxTrivia(SyntaxKind.MultiLineComment, Window.LexemeSpan, SyntaxTree));
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
					trivia.Add(new SyntaxTrivia(SyntaxKind.LineBreakTrivia, Window.LexemeSpan, SyntaxTree));
					break;
				case ' ':
				case '\t':
					ReadWhiteSpace();
					trivia.Add(new SyntaxTrivia(SyntaxKind.EmptySpaceTrivia, Window.LexemeSpan, SyntaxTree));
					break;
				default:
					if (char.IsWhiteSpace(Window.Current))
					{
						ReadWhiteSpace();
						trivia.Add(new SyntaxTrivia(SyntaxKind.EmptySpaceTrivia, Window.LexemeSpan, SyntaxTree));
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
		Window.AdvancePastNewLine();
	}

	private void ReadWhiteSpace()
	{
		bool done = false;

		while (!done)
		{
			switch (Window.Current)
			{
				case SlidingWindow.InvalidCharacter:
				case '\r':
				case '\n':
					done = true;
					break;
				default:
					if (!char.IsWhiteSpace(Window.Current))
						done = true;
					else
						Window.Advance();
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
		Window.Advance(2);
		bool done = false;

		while (!done)
		{
			switch (Window.Current)
			{
				case '\0':
				case '\r':
				case '\n':
					done = true;
					break;
				default:
					Window.Advance();
					break;
			}
		}
	}

	private void ReadMultiLineComment()
	{
		Window.Advance(2);
		bool done = false;

		while (!done)
		{
			switch (Window.Current)
			{
				case SlidingWindow.InvalidCharacter:
					TextSpan span = Window.LexemeSpan;
					//TODO: _diagnostics.ReportUnterminatedMultiLineComment(span.Contextualize(_syntaxTree));
					done = true;
					break;
				case '*':
					if (Window.Next == '/')
					{
						Window.Advance();
						done = true;
					}
					Window.Advance();
					break;
				default:
					Window.Advance();
					break;
			}
		}
	}
}