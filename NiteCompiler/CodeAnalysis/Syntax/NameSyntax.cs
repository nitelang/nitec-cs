namespace NiteCompiler.CodeAnalysis.Syntax;

public partial class NameSyntax
{
	public abstract PlainNameSyntax UnqualifiedName { get; }
}

public partial class PathNameSyntax
{
	public override PlainNameSyntax UnqualifiedName => Right;
}

public partial class PlainNameSyntax
{
	public sealed override PlainNameSyntax UnqualifiedName => this;

	public abstract string GetIdentifierValue();
}

public partial class IdentifierNameSyntax
{
	public override string GetIdentifierValue()
	{
		return Identifier.IdentifierValue ?? string.Empty;
	}
}