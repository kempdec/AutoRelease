using KempDec.AutoRelease.Resources;

namespace KempDec.AutoRelease.Commits.Types;

/// <summary>
/// Representa o tipo da mensagem de commit ignorada.
/// </summary>
internal record IgnoreCommitMessageType : CommitMessageTypeBase
{
    /// <inheritdoc/>
    public override string Label { get; } = CommitTypesResource.Ignore;

    /// <inheritdoc/>
    public override string Key { get; } = "ignore";

    /// <inheritdoc/>
    public override byte Ordering { get; } = (byte)CommitMessageTypeOrder.Ignore;
}
