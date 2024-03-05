using KempDec.AutoRelease.Resources;

namespace KempDec.AutoRelease.Commits.Types;

/// <summary>
/// Representa o tipo da mensagem de commit para corrigir um bug ou problema existente no código.
/// </summary>
internal record FixCommitMessageType : CommitMessageTypeBase
{
    /// <inheritdoc/>
    public override string Label { get; } = CommitTypesResource.Fix;

    /// <inheritdoc/>
    public override string Key { get; } = "fix";
}
