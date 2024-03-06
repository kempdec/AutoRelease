using KempDec.AutoRelease.Commits.Types;

namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Representa a ordem de um tipo de uma mensagem de commit.
/// </summary>
internal enum CommitMessageTypeOrder
{
    /// <summary>
    /// <see cref="FeatCommitMessageType"/>.
    /// </summary>
    Feat,

    /// <summary>
    /// <see cref="FixCommitMessageType"/>.
    /// </summary>
    Fix,

    /// <summary>
    /// <see cref="ChoreCommitMessageType"/>.
    /// </summary>
    Chore,

    /// <summary>
    /// <see cref="DefaultCommitMessageType"/>.
    /// </summary>
    Default = 255
}
