using KempDec.AutoRelease.Resources;

namespace KempDec.AutoRelease.Commits.Types;

/// <summary>
/// Representa o tipo da mensagem de commit padrão.
/// </summary>
internal record DefaultCommitMessageType : CommitMessageTypeBase
{
    /// <inheritdoc/>
    public override string Label { get; } = CommitTypesResource.Commit;

    /// <inheritdoc/>
    public override string Key { get; } = "commit";

    /// <inheritdoc/>
    public override byte Ordering { get; } = (byte)CommitMessageTypeOrder.Default;
}
