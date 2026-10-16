using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.Diagnostics;

public static class DiagnosticDescriptors
{
	public static readonly DiagnosticDescriptor
		UnterminatedBlockComment = new("unterminated-block-comment", "unterminated block comment"),
		ExpectedToken = new("expected-token", "expected `{0}`, found `{1}`"),
		ExpectedType = new("expected-type", "type expected"),
		ExpectedIdentifier = new("expected-identifier", "identifier expected"),
		ExpectedBlock = new("expected-block", "block statement excepted"),
		UnexpectedToken = new("unexpected-token", "unexpected token `{0}`"),
		UnexpectedCharacter = new("unexpected-character", "unexpected character `{0}`"),
		UnterminatedEscapedIdentifier = new("unterminated-escaped-identifier", "unterminated escaped identifier"),
		NewlineInEscapedIdentifier = new("newline-in-escaped-identifier", "escaped identifier cannot contain a newline"),
		InvalidUnicodeEscape = new("invalid-unicode-escape", "invalid unicode escape sequence"),
		InvalidEscapeSequence = new("invalid-escape-sequence", "invalid escape sequence `\\{0}`"),
		NumericLiteralTooLarge = new("numeric-literal-too-large", "numeric literal exceed allowed magnitude");

	private static string TokenText(SyntaxKind kind)
	{
		return kind == SyntaxKind.EndOfFile ? "end of file" : kind.Text ?? kind.ToString();
	}

	extension(IDiagnosticBag bag)
	{
		public void ReportUnterminatedBlockComment(Location location)
		{
			bag.Add(UnterminatedBlockComment.Instantiate(location));
		}

		public void ReportExpectedToken(Location location, SyntaxKind expectedKind, SyntaxKind foundKind)
		{
			bag.Add(ExpectedToken.Instantiate(location, TokenText(expectedKind), TokenText(foundKind)));
		}

		public void ReportExpectedType(Location location)
		{
			bag.Add(ExpectedType.Instantiate(location));
		}

		public void ReportExpectedIdentifier(Location location)
		{
			bag.Add(ExpectedIdentifier.Instantiate(location));
		}

		public void ReportExpectedBlock(Location location)
		{
			bag.Add(ExpectedBlock.Instantiate(location));
		}

		public void ReportUnexpectedCharacter(Location location, string character)
		{
			bag.Add(UnexpectedCharacter.Instantiate(location, character));
		}

		public void ReportUnexpectedToken(Location location, SyntaxKind foundKind)
		{
			bag.Add(UnexpectedToken.Instantiate(TokenText(foundKind), location));
		}

		public void ReportUnterminatedEscapedIdentifier(Location location)
		{
			bag.Add(UnterminatedEscapedIdentifier.Instantiate(location));
		}

		public void ReportNewlineInEscapedIdentifier(Location location)
		{
			bag.Add(NewlineInEscapedIdentifier.Instantiate(location));
		}

		public void ReportInvalidUnicodeEscape(Location location)
		{
			bag.Add(InvalidUnicodeEscape.Instantiate(location));
		}

		public void ReportInvalidEscapeSequence(Location location, char escape)
		{
			bag.Add(InvalidEscapeSequence.Instantiate(escape, location));
		}

		public void ReportNumberLiteralTooLarge(Location location)
		{
			bag.Add(NumericLiteralTooLarge.Instantiate(location));
		}
	}
}