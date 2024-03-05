namespace KempDec.AutoRelease.Commits.Types;

/// <summary>
/// Fornece abstração para um tipo de uma mensagem de commit.
/// </summary>
internal abstract record CommitMessageTypeBase : ICommitMessageType
{
    /// <inheritdoc/>
    public abstract string Label { get; }

    /// <inheritdoc/>
    public abstract string Key { get; }
}
