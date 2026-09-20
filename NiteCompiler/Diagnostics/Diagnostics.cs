using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.Diagnostics;

public static class Diagnostics
{
	public static readonly DiagnosticDescriptor
		UnterminatedBlockComment = new("unterminated-block-comment", "unterminated block comment"),
		ExpectedToken = new("expected-token", "expected `{0}`, found `{1}`"),
		UnexpectedCharacter = new("unexpected-character", "unexpected character `{0}`"),
		UnterminatedEscapedIdentifier = new("unterminated-escaped-identifier", "unterminated escaped identifier"),
		NewlineInEscapedIdentifier = new("newline-in-escaped-identifier", "escaped identifier cannot contain a newline"),
		InvalidUnicodeEscape = new("invalid-unicode-escape", "invalid unicode escape sequence"),
		InvalidEscapeSequence = new("invalid-escape-sequence", "invalid escape sequence `\\{0}`");

	private static string TokenText(SyntaxKind kind)
	{
		return kind == SyntaxKind.EndOfFile ? "end of file" : kind.Text ?? kind.ToString();
	}

	extension(IDiagnosticBag bag)
	{
		public void ReportUnterminatedBlockComment()
		{
			bag.Add(new Diagnostic(UnterminatedBlockComment, "unterminated block comment"));
		}

		public void ReportExpectedToken(SyntaxKind expectedKind, SyntaxKind foundKind)
		{
			bag.Add(new Diagnostic(ExpectedToken,
				string.Format(ExpectedToken.MessageFormat, TokenText(expectedKind), TokenText(foundKind))));
		}

		public void ReportUnexpectedCharacter(string character)
		{
			bag.Add(new Diagnostic(UnexpectedCharacter, string.Format(UnexpectedCharacter.MessageFormat, character)));
		}

		public void ReportUnterminatedEscapedIdentifier()
		{
			bag.Add(new Diagnostic(UnterminatedEscapedIdentifier, "unterminated escaped identifier"));
		}

		public void ReportNewlineInEscapedIdentifier()
		{
			bag.Add(new Diagnostic(NewlineInEscapedIdentifier, "escaped identifier cannot contain a newline"));
		}

		public void ReportInvalidUnicodeEscape()
		{
			bag.Add(new Diagnostic(InvalidUnicodeEscape, "invalid unicode escape sequence"));
		}

		public void ReportInvalidEscapeSequence(char escape)
		{
			bag.Add(new Diagnostic(InvalidEscapeSequence,
				string.Format(InvalidEscapeSequence.MessageFormat, escape)));
		}
	}
}