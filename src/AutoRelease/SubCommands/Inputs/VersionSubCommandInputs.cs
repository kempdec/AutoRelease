using Semver;

namespace KempDec.AutoRelease.SubCommands.Inputs;

/// <summary>
/// Representa as entradas do subcomando <see cref="VersionSubCommand"/>.
/// </summary>
internal class VersionSubCommandInputs : GlobalInputs, IVersionSubCommandInputs
{
    /// <inheritdoc/>
    public SemVersion? FirstVersion { get; set; }

    /// <inheritdoc/>
    public string VersionPrefix { get; set; } = null!;
}
