﻿namespace KempDec.AutoRelease.Commits.Types;

/// <summary>
/// Representa um tipo de uma mensagem de commit.
/// </summary>
internal sealed record CommitMessageType : ICommitMessageType
{
    /// <inheritdoc/>
    public required string Label { get; init; }

    /// <inheritdoc/>
    public required string Key { get; init; }
}
