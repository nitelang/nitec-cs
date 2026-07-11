using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NiteCompiler.SourceGenerators.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NiteCompiler.SourceGenerators;

[Generator]
public class Generator : IIncrementalGenerator
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> syntaxFiles = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileName(file.Path) == "syntax.yml");

        context.RegisterSourceOutput(syntaxFiles, Build);
    }

    private static void Build(SourceProductionContext context, AdditionalText source)
    {
        string? sourceText = source.GetText(context.CancellationToken)?.ToString();

        if (sourceText == null)
            return;

        GenerationContent content = new();
        SyntaxData data;

        try
        {
            data = Deserializer.Deserialize<SyntaxData>(sourceText);
        }
        catch (Exception exception)
        {
            DiagnosticDescriptor descriptor = new("GEN001", "YAML parsing failed", "{0}", "Syntax", DiagnosticSeverity.Error, true);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.Create(
                source.Path,
                TextSpan.FromBounds(0, 1),
                new LinePositionSpan(LinePosition.Zero, LinePosition.Zero)),
                messageArgs: exception.ToString()));

            return;
        }

        content.AddDefaultTokens();
        content.ConsumeData(data);
        content.EvaluateIndices();

        var nodeTypeNames = new HashSet<string>(content.Nodes.Select(n => n.CodeName))
        {
            "SyntaxNode",
            "SyntaxToken"
        };

        context.AddSource("SyntaxKind.g.cs", WriteSyntaxKindFile(content));
        context.AddSource("SyntaxVisitor.g.cs", WriteVisitorFile(content));
        foreach (var node in content.Nodes)
        {
            context.AddSource($"{node.CodeName}.g.cs", WriteNodeFile(node, nodeTypeNames));
        }
    }

    private static string WriteSyntaxKindFile(GenerationContent content)
    {
        SourceWriter writer = new();
        writer.WriteDisclaimer();
        writer.WriteLine("#nullable enable");
        writer.WriteLine("using System.Collections.Immutable;");
        writer.WriteLine("using System.Diagnostics;");

        writer.WriteLine("namespace NiteCompiler.CodeAnalysis.Syntax;");
        writer.WriteLine();
        
        writer.WriteLine("public enum SyntaxKind : uint");
        writer.EnterScope("{");
        {
            foreach (SyntaxKind kind in content.SyntaxKinds)
            {
                if (kind.Text != null)
                {
                    writer.WriteLine($"/// <summary>Represents <c>{EscapeXml(kind.Text)}</c> token.</summary>");
                }
                writer.WriteLine($"{kind.Name} = {kind.EvaluatedIndex},");
            }
        }
        writer.ExitScope("}");
        writer.WriteLine();
        
        writer.WriteLine("public static class SyntaxKindExtensions");
        writer.EnterScope("{");
        {
            writer.WriteLine("extension(SyntaxKind kind)");
            writer.EnterScope("{");
            {
                writer.WriteLine("public bool IsToken => (uint)kind < 512;");
                writer.WriteLine("public bool IsNode => (uint)kind >= 512 && kind != SyntaxKind.EndOfFile;");
                writer.WriteLine();
            }
            writer.ExitScope("}");
        }
        writer.ExitScope("}");
        
        return writer.ToString();
    }

    private static string WriteNodeFile(NodeKind kind, HashSet<string> nodeTypeNames)
    {
        SourceWriter writer = new();
        writer.WriteDisclaimer();
        writer.WriteLine("#nullable enable");
        writer.WriteLine("using System.Collections.Immutable;");
        writer.WriteLine("using System.Diagnostics;");

        writer.WriteLine("namespace NiteCompiler.CodeAnalysis.Syntax;");
        writer.WriteLine();

        writer.WriteLine($"public {(kind.Abstract ? "abstract" : "sealed")} partial class {kind.CodeName}" + (kind.Base != null ? " : " + kind.Base.CodeName : string.Empty));
        writer.EnterScope("{");
        {
            var allMembers = GetAllMembers(kind).ToArray();
            var ownMembers = kind.Members;
            var baseMembers = allMembers.Take(allMembers.Length - ownMembers.Length).ToArray();

            foreach (var member in ownMembers)
            {
                writer.WriteLine($"public {member.Type} {member.Name} {{ get; }}");
            }

            if (!kind.Abstract && kind.RelatedKinds.Length > 0)
            {
                writer.WriteLine("public override SyntaxKind Kind { get; }");
            }
            else if (!kind.Abstract && kind.AutoKind != null)
            {
                writer.WriteLine($"public override SyntaxKind Kind => SyntaxKind.{kind.AutoKind.Name};");
            }

            var parameters = new List<string>();
            if (!kind.Abstract && kind.RelatedKinds.Length > 0)
            {
                parameters.Add("SyntaxKind kind");
            }
            parameters.AddRange(allMembers.Select(m => $"{m.Type} {ToCamelCase(m.Name)}"));


            var accessModifier = kind.Abstract ? "private protected" : "public";
            writer.WriteLine($"{accessModifier} {kind.CodeName}({string.Join(", ", parameters)})");

            if (baseMembers.Length > 0)
            {
                var baseArgs = string.Join(", ", baseMembers.Select(m => ToCamelCase(m.Name)));
                writer.EnterScope($": base({baseArgs})");
                writer.ExitScope();
            }

            writer.EnterScope("{");
            {
                foreach (var member in ownMembers)
                {
                    if (!member.IsArray && member.Type != "SyntaxKind" && !IsPrimitiveType(member.Type))
                    {
                        writer.WriteLine($"Debug.Assert({ToCamelCase(member.Name)} != null);");
                    }
                }

                if (!kind.Abstract && kind.RelatedKinds.Length > 0)
                {
                    var assertConditions = string.Join(" || ", kind.RelatedKinds.Select(k => $"kind == SyntaxKind.{k.Name}"));
                    writer.WriteLine($"Debug.Assert({assertConditions});");
                }

                foreach (var member in ownMembers)
                {
                    writer.WriteLine($"{member.Name} = {ToCamelCase(member.Name)};");
                }

                if (!kind.Abstract && kind.RelatedKinds.Length > 0)
                {
                    writer.WriteLine("Kind = kind;");
                }
            }
            writer.ExitScope("}");
            
            // Visitor/Accept pattern generation
            if (!kind.Abstract)
            {
                writer.WriteLine();
                var visitName = GetVisitName(kind);
                writer.WriteLine($"public override void Accept(SyntaxVisitor visitor) => visitor.{visitName}(this);");
                writer.WriteLine($"public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor) => visitor.{visitName}(this);");
                writer.WriteLine($"public override TResult Accept<TResult, TArgument>(SyntaxVisitor<TResult, TArgument> visitor, TArgument argument) => visitor.{visitName}(this, argument);");
            }

            // Slot/child/span methods for sealed nodes
            if (!kind.Abstract)
            {
                var allChildren = allMembers.Where(m =>
                    m.IsArray ? nodeTypeNames.Contains(m.ElementType) : nodeTypeNames.Contains(m.Type)).ToArray();
                var slotChildren = allChildren.Where(m => !m.IsArray).ToArray();

                writer.WriteLine();
                writer.WriteLine("public override SyntaxNode? GetSlot(int index)");
                writer.EnterScope("{");
                writer.WriteLine("return index switch");
                writer.EnterScope("{");
                for (int i = 0; i < slotChildren.Length; i++)
                {
                    writer.WriteLine($"{i} => {slotChildren[i].Name},");
                }
                writer.WriteLine("_ => null");
                writer.ExitScope("};");
                writer.ExitScope("}");

                writer.WriteLine("public override IEnumerable<SyntaxNode> GetChildNodesAndTokens()");
                writer.EnterScope("{");
                foreach (var member in allChildren)
                {
                    if (member.IsArray)
                    {
                        writer.WriteLine($"foreach (var child in {member.Name})");
                        writer.EnterScope("{");
                        writer.WriteLine("yield return child;");
                        writer.ExitScope("}");
                    }
                    else
                    {
                        writer.WriteLine($"yield return {member.Name};");
                    }
                }
                writer.WriteLine("yield break;");
                writer.ExitScope("}");

                writer.WriteLine("public override IEnumerable<SyntaxNode> GetChildNodes()");
                writer.EnterScope("{");
                foreach (var member in allChildren)
                {
                    if (member.IsArray)
                    {
                        if (member.ElementType != "SyntaxToken")
                        {
                            writer.WriteLine($"foreach (var child in {member.Name})");
                            writer.EnterScope("{");
                            writer.WriteLine("yield return child;");
                            writer.ExitScope("}");
                        }
                    }
                    else if (member.Type != "SyntaxToken")
                    {
                        writer.WriteLine($"yield return {member.Name};");
                    }
                }
                writer.WriteLine("yield break;");
                writer.ExitScope("}");

                writer.WriteLine("public override IEnumerable<SyntaxToken> GetChildTokens()");
                writer.EnterScope("{");
                foreach (var member in allChildren)
                {
                    if (member.IsArray)
                    {
                        if (member.ElementType == "SyntaxToken")
                        {
                            writer.WriteLine($"foreach (var child in {member.Name})");
                            writer.EnterScope("{");
                            writer.WriteLine("yield return child;");
                            writer.ExitScope("}");
                        }
                    }
                    else if (member.Type == "SyntaxToken")
                    {
                        writer.WriteLine($"yield return {member.Name};");
                    }
                }
                writer.WriteLine("yield break;");
                writer.ExitScope("}");

                writer.WriteLine("public override TextSpan Span");
                writer.EnterScope("{");
                writer.WriteLine("get");
                writer.EnterScope("{");
                writer.WriteLine("TextSpan span = default;");
                writer.WriteLine("bool any = false;");
                writer.WriteLine("foreach (var child in GetChildNodesAndTokens())");
                writer.EnterScope("{");
                writer.WriteLine("if (!any)");
                writer.EnterScope("{");
                writer.WriteLine("span = child.Span;");
                writer.WriteLine("any = true;");
                writer.ExitScope("}");
                writer.WriteLine("else");
                writer.EnterScope("{");
                writer.WriteLine("span = TextSpan.FromBounds(span.Start, child.Span.End);");
                writer.ExitScope("}");
                writer.ExitScope("}");
                writer.WriteLine("return span;");
                writer.ExitScope("}");
                writer.ExitScope("}");

                writer.WriteLine("public override TextSpan FullSpan");
                writer.EnterScope("{");
                writer.WriteLine("get");
                writer.EnterScope("{");
                writer.WriteLine("TextSpan span = default;");
                writer.WriteLine("bool any = false;");
                writer.WriteLine("foreach (var child in GetChildNodesAndTokens())");
                writer.EnterScope("{");
                writer.WriteLine("if (!any)");
                writer.EnterScope("{");
                writer.WriteLine("span = child.FullSpan;");
                writer.WriteLine("any = true;");
                writer.ExitScope("}");
                writer.WriteLine("else");
                writer.EnterScope("{");
                writer.WriteLine("span = TextSpan.FromBounds(span.Start, child.FullSpan.End);");
                writer.ExitScope("}");
                writer.ExitScope("}");
                writer.WriteLine("return span;");
                writer.ExitScope("}");
                writer.ExitScope("}");
            }
        }
        writer.ExitScope("}");

        return writer.ToString();
    }

    private static IEnumerable<MemberKind> GetAllMembers(NodeKind kind)
    {
        if (kind.Base != null)
        {
            foreach (var member in GetAllMembers(kind.Base))
            {
                yield return member;
            }
        }

        foreach (var member in kind.Members)
        {
            yield return member;
        }
    }

    private static string ToCamelCase(string pascalCase)
    {
        return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
    }

    private static bool IsPrimitiveType(string type)
    {
        return type switch
        {
            "bool" or "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong"
            or "float" or "double" or "decimal" or "char" or "string" or "object" or "nint" or "nuint" => true,
            _ => false
        };
    }

    private static string WriteVisitorFile(GenerationContent content)
    {
        SourceWriter writer = new();
        writer.WriteDisclaimer();
        writer.WriteLine("#nullable enable");
        writer.WriteLine("using System.Collections.Immutable;");
        writer.WriteLine("using System.Diagnostics;");

        writer.WriteLine("namespace NiteCompiler.CodeAnalysis.Syntax;");
        writer.WriteLine();

        writer.WriteLine("public abstract partial class SyntaxVisitor");
        writer.EnterScope("{");
        {
            foreach (var node in content.Nodes)
            {
                writer.WriteLine($"public virtual void {GetVisitName(node)}({node.CodeName} node) => DefaultVisit(node);");
            }
        }
        writer.ExitScope("}");
        writer.WriteLine();

        writer.WriteLine("public abstract partial class SyntaxVisitor<TResult>");
        writer.EnterScope("{");
        {
            foreach (var node in content.Nodes)
            {
                writer.WriteLine($"public virtual TResult {GetVisitName(node)}({node.CodeName} node) => DefaultVisit(node)!;");
            }
        }
        writer.ExitScope("}");
        writer.WriteLine();

        writer.WriteLine("public abstract partial class SyntaxVisitor<TResult, TArgument>");
        writer.EnterScope("{");
        {
            foreach (var node in content.Nodes)
            {
                writer.WriteLine($"public virtual TResult {GetVisitName(node)}({node.CodeName} node, TArgument argument) => DefaultVisit(node, argument)!;");
            }
        }
        writer.ExitScope("}");

        return writer.ToString();
    }

    private static string GetVisitName(NodeKind kind)
    {
        var name = kind.CodeName;
        if (name.EndsWith("Syntax") && name != "SyntaxNode")
        {
            name = name.Substring(0, name.Length - "Syntax".Length);
        }
        return "Visit" + name;
    }

    private static string EscapeXml(string text)
    {
        return text.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;");
    }
}