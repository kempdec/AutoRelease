﻿using KempDec.AutoRelease.Commits;
using Semver;

namespace KempDec.AutoRelease.SubCommands.Inputs;

/// <summary>
/// Representa as entradas do subcomando <see cref="NoteSubCommand"/>.
/// </summary>
internal class NoteSubCommandInputs : GlobalInputs, INoteSubCommandInputs
{
    /// <inheritdoc/>
    public SemVersion? Version { get; set; }

    /// <inheritdoc/>
    public List<CommitMessageType> Types { get; set; } = null!;

    /// <inheritdoc/>
    public List<(string OldValue, string NewValue)> Replaces { get; set; } = null!;

    /// <inheritdoc/>
    public List<string> Ignores { get; set; } = null!;

    /// <inheritdoc/>
    public bool ShowAuthor { get; set; }
}
