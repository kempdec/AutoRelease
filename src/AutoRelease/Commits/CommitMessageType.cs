namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Representa um tipo de uma mensagem de commit.
/// </summary>
internal sealed record CommitMessageType : ICommitMessageType
{
    /// <inheritdoc/>
    public required string Label { get; init; }

    /// <inheritdoc/>
    public required string Key { get; init; }

    /// <inheritdoc/>
    public required byte Ordering { get; init; }
}
