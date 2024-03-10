using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.SubCommands.Models;
using Semver;

namespace KempDec.AutoRelease.SubCommands.Inputs;

/// <summary>
/// Representa as entradas do subcomando <see cref="CreateSubCommand"/>.
/// </summary>
internal class CreateSubCommandInputs : GlobalInputs, ICreateSubCommandInputs
{
    /// <inheritdoc/>
    public SemVersion? Version { get; set; }

    /// <inheritdoc/>
    public string? ProjectName { get; set; }

    /// <inheritdoc/>
    public bool Draft { get; set; }

    /// <inheritdoc/>
    public bool PreRelease { get; set; }

    /// <inheritdoc/>
    public CreateOutputType OutputType { get; set; }

    #region Notes.

    /// <inheritdoc/>
    public List<CommitMessageType> Types { get; set; } = null!;

    /// <inheritdoc/>
    public List<(string OldValue, string NewValue)> Replaces { get; set; } = null!;

    /// <inheritdoc/>
    public List<string> Ignores { get; set; } = null!;

    /// <inheritdoc/>
    public bool ShowAuthor { get; set; }

    #endregion

    #region Version.

    /// <inheritdoc/>
    public SemVersion? FirstVersion { get; set; }

    /// <inheritdoc/>
    public string VersionPrefix { get; set; } = null!;

    #endregion
}
