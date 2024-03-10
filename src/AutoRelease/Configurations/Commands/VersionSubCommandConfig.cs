using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.Configurations.Commands;

/// <summary>
/// Associação recursiva das configurações do subcomando de geração automática da versão do release.
/// </summary>
internal record VersionSubCommandConfig : IVersionSubCommandConfig
{
    /// <inheritdoc/>
    [ConfigOption<FirstVersionOption>]
    public string? FirstVersion { get; init; }

    /// <inheritdoc/>
    [ConfigOption<VersionPrefixOption>]
    public string? VersionPrefix { get; init; }
}
