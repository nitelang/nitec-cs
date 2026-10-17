namespace NiteCompiler.Compilation;

/// <summary>
/// Indicates the reasons why a candidate (or set of candidate) symbols were not considered
/// correct in SemanticInfo. Higher values take precedence over lower values, so if, for
/// example, there a symbol with a given name that was inaccessible, and other with the wrong
/// arity, only the inaccessible one would be reported in the SemanticInfo.
/// </summary>
public enum CandidateReason
{
	/// <summary>
	/// No candidates.
	/// </summary>
	None = 0,
}