using KempDec.AutoRelease.Resources;

namespace KempDec.AutoRelease.Commits.Types;

/// <summary>
/// Representa o tipo da mensagem de commit para tarefas de build, configuração, etc., que não estão relacionadas
/// diretamente ao código fonte.
/// </summary>
internal record ChoreCommitMessageType : CommitMessageTypeBase
{
    /// <inheritdoc/>
    public override string Label { get; } = CommitTypesResource.Chore;

    /// <inheritdoc/>
    public override string Key { get; } = "chore";

    /// <inheritdoc/>
    public override byte Ordering { get; } = (byte)CommitMessageTypeOrder.Chore;
}
