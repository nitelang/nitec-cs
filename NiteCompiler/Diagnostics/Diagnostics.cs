using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.Diagnostics;

public static class Diagnostics
{
	public static readonly DiagnosticDescriptor
		UnterminatedBlockComment = new("unterminated-block-comment", "unterminated block comment"),
		ExpectedToken = new("expected-token", "expected `{0}`, found `{1}`"),
		UnexpectedToken = new("unexpected-token", "unexpected token `{0}`"),
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
		public void ReportUnterminatedBlockComment(Location location)
		{
			bag.Add(new Diagnostic(UnterminatedBlockComment, "unterminated block comment", location));
		}

		public void ReportExpectedToken(Location location, SyntaxKind expectedKind, SyntaxKind foundKind)
		{
			bag.Add(new Diagnostic(ExpectedToken,
				string.Format(ExpectedToken.MessageFormat, TokenText(expectedKind), TokenText(foundKind)),
				location));
		}

		public void ReportUnexpectedCharacter(Location location, string character)
		{
			bag.Add(new Diagnostic(UnexpectedCharacter,
				string.Format(UnexpectedCharacter.MessageFormat, character),
				location));
		}

		public void ReportUnexpectedToken(Location location, SyntaxKind foundKind)
		{
			bag.Add(new Diagnostic(UnexpectedToken,
				string.Format(UnexpectedToken.MessageFormat, TokenText(foundKind)),
				location));
		}

		public void ReportUnterminatedEscapedIdentifier(Location location)
		{
			bag.Add(new Diagnostic(UnterminatedEscapedIdentifier, "unterminated escaped identifier", location));
		}

		public void ReportNewlineInEscapedIdentifier(Location location)
		{
			bag.Add(new Diagnostic(NewlineInEscapedIdentifier, "escaped identifier cannot contain a newline", location));
		}

		public void ReportInvalidUnicodeEscape(Location location)
		{
			bag.Add(new Diagnostic(InvalidUnicodeEscape, "invalid unicode escape sequence", location));
		}

		public void ReportInvalidEscapeSequence(Location location, char escape)
		{
			bag.Add(new Diagnostic(InvalidEscapeSequence,
				string.Format(InvalidEscapeSequence.MessageFormat, escape), location));
		}
	}
}