using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.Configurations;

/// <summary>
/// Associação recursiva das configurações do subcomando de geração automática da versão do release.
/// </summary>
internal record VersionSubCommandConfig
{
    /// <summary>
    /// Obtém ou inicializa a primeira versão do repositório.
    /// </summary>
    [ConfigOption<FirstVersionOption>]
    public string? FirstVersion { get; init; }

    /// <summary>
    /// Obtém ou inicializa o prefixo das versões do repositório.
    /// </summary>
    [ConfigOption<VersionPrefixOption>]
    public string? VersionPrefix { get; init; }
}
