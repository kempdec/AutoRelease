using KempDec.AutoRelease.Resources;

namespace KempDec.AutoRelease.Commits.Types;

/// <summary>
/// Representa o tipo da mensagem de commit para introduzir uma nova funcionalidade no código.
/// </summary>
internal record FeatCommitMessageType : CommitMessageTypeBase
{
    /// <inheritdoc/>
    public override string Label { get; } = CommitTypesResource.Feat;

    /// <inheritdoc/>
    public override string Key { get; } = "feat";

    /// <inheritdoc/>
    public override byte Ordering { get; } = (byte)CommitMessageTypeOrder.Feat;
}
