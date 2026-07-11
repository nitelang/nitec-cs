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

        context.AddSource("SyntaxKind.g.cs", WriteSyntaxKindFile(content));
        foreach (var node in content.Nodes)
        {
            context.AddSource($"{node.CodeName}.g.cs", WriteNodeFile(node));
        }
    }

    private static string WriteSyntaxKindFile(GenerationContent content)
    {
        SourceWriter writer = new();
        writer.WriteDisclaimer();
        
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

    private static string WriteNodeFile(NodeKind kind)
    {
        SourceWriter writer = new();
        writer.WriteDisclaimer();

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
                    if (member.Type != "SyntaxKind" && !IsPrimitiveType(member.Type))
                    {
                        writer.WriteLine($"System.Diagnostics.Debug.Assert({ToCamelCase(member.Name)} != null);");
                    }
                }

                if (!kind.Abstract && kind.RelatedKinds.Length > 0)
                {
                    var assertConditions = string.Join(" || ", kind.RelatedKinds.Select(k => $"kind == SyntaxKind.{k.Name}"));
                    writer.WriteLine($"System.Diagnostics.Debug.Assert({assertConditions});");
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

    private static string EscapeXml(string text)
    {
        return text.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;");
    }
}