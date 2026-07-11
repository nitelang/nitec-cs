using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NiteCompiler.SourceGenerators.Data;

namespace NiteCompiler.SourceGenerators;

internal sealed class GenerationContent
{
    private readonly Dictionary<string, SyntaxKind> _tokens = [];
    private readonly Dictionary<string, NodeKind> _nodes = [];
    public IEnumerable<SyntaxKind> SyntaxKinds => _tokens.Values;
    public IEnumerable<NodeKind> Nodes => _nodes.Values;

    private void Add(SyntaxKind syntaxKind)
    {
        _tokens.Add(syntaxKind.Name, syntaxKind);
    }

    private void Add(NodeKind nodeKind)
    {
        _nodes.Add(nodeKind.YamlName, nodeKind);
    }
    
    public void AddDefaultTokens()
    {
        Add(new SyntaxKind("None") { IsToken = true, EvaluatedIndex = 0 });
        Add(new SyntaxKind("EndOfFile") { IsToken = true, EvaluatedIndex = uint.MaxValue });
    }

    public void ConsumeData(SyntaxData data)
    {
        foreach (var token in data.Tokens)
        {
            Add(new SyntaxKind(token)
            {
                IsToken = true
            });
        }

        foreach (var node in data.Nodes)
        {
            Add(new NodeKind(node.Name)
            {
                Abstract = node.Abstract
            });
        }

        foreach (var node in data.Nodes)
        {
            var kind = _nodes[node.Name];

            if (node.Kinds.Length > 0)
            {
                kind.RelatedKinds = node.Kinds.Select(t =>
                {
                    var tempKind = new SyntaxKind(t)
                    {
                        IsToken = node.OnlyTokensKinds
                    };
                    Add(tempKind);
                    return tempKind;
                }).ToArray();
            }

            if (node.Members.Length > 0)
            {
                kind.Members = node.Members.Select(m =>
                {
                    if (m.Type is List<object> typeList)
                    {
                        var rawType = typeList[0].ToString()!;
                        var resolvedType = ResolveTypeName(rawType);
                        return new MemberKind(m.Name, $"ImmutableArray<{resolvedType}>",
                            isArray: true, elementType: resolvedType);
                    }

                    var resolved = ResolveTypeName(m.Type.ToString()!);
                    return new MemberKind(m.Name, resolved);
                }).ToArray();
            }

            if (node.Base != null)
            {
                kind.Base = _nodes[node.Base];
            }

            if (!kind.Abstract && kind.RelatedKinds.Length == 0)
            {
                var autoKind = new SyntaxKind(kind.YamlName);
                Add(autoKind);
                kind.AutoKind = autoKind;
            }
        }
    }

    public void EvaluateIndices()
    {
        Dictionary<uint, SyntaxKind> kinds = [];
        foreach (var syntaxKind in _tokens.Values.Where(t => t.EvaluatedIndex != null))
        {
            kinds[syntaxKind.EvaluatedIndex!.Value] = syntaxKind;
        }

        uint index = 0;
        foreach (var syntaxKind in _tokens.Values.Where(t => t.IsToken && t.EvaluatedIndex == null))
        {
            while (kinds.ContainsKey(index))
            {
                index++;
            }
            
            kinds[index] = syntaxKind;
        }

        index = Math.Max(index, 0x200);
        foreach (var nodeKind in _tokens.Values.Where(t => !t.IsToken && t.EvaluatedIndex == null))
        {
            while (kinds.ContainsKey(index))
            {
                index++;
            }
            
            kinds[index] = nodeKind;
        }

        foreach (var pair in kinds.Where(t => t.Value.EvaluatedIndex == null))
        {
            pair.Value.EvaluatedIndex = pair.Key;
        }
    }

    private string ResolveTypeName(string rawName)
    {
        if (_nodes.TryGetValue(rawName, out var node))
            return node.CodeName;

        return rawName;
    }
}

internal sealed record NodeKind
{
    public string YamlName { get; }
    public string CodeName { get; }
    public bool Abstract { get; set; } = false;
    public NodeKind? Base { get; set; }
    public SyntaxKind[] RelatedKinds { get; set; } = [];
    public MemberKind[] Members { get; set; } = [];
    public SyntaxKind? AutoKind { get; set; }

    public NodeKind(string yamlName)
    {
        YamlName = yamlName;

        if (YamlName.StartsWith("Syntax") || YamlName.EndsWith("Syntax"))
        {
            CodeName = YamlName;
        }
        else
        {
            CodeName = YamlName + "Syntax";
        }
    }
}

internal sealed record MemberKind
{
    public string Name { get; }
    public string Type { get; }
    public bool IsArray { get; }
    public string ElementType { get; }

    public MemberKind(string name, string type, bool isArray = false, string elementType = "")
    {
        Name = name;
        Type = type;
        IsArray = isArray;
        ElementType = elementType;
    }
}

internal sealed record SyntaxKind
{
    public readonly string Name;
    public readonly string? Text;
    public bool IsToken { get; set; }
    public uint? EvaluatedIndex { get; set; }
    
    public SyntaxKind(string internalName, [Optional] string? text)
    {
        Name = internalName;
        Text = text;
    }

    public SyntaxKind(TokenData data)
    {
        Name = data.Name;
        Text = data.Text;
        IsToken = true;
    }
}