using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.Configurations;

/// <summary>
/// Associação recursiva das configurações do subcomando de criação automática de um release.
/// </summary>
internal class CreateSubCommandConfig
{
    /// <summary>
    /// Obtém ou define a versão do release.
    /// </summary>
    [ConfigOption<VersionOption>]
    public string? Version { get; set; }

    /// <summary>
    /// Obtém ou define o nome do projeto.
    /// </summary>
    [ConfigOption<ProjectNameOption>]
    public string? ProjectName { get; set; }

    /// <summary>
    /// Obtém ou define um sinalizador indicando se o release é um rascunho.
    /// </summary>
    [ConfigOption<DraftOption>]
    public bool? Draft { get; set; }

    /// <summary>
    /// Obtém ou define um sinalizador indicando se o release é um pre-release.
    /// </summary>
    [ConfigOption<PreReleaseOption>]
    public bool? PreRelease { get; set; }
}
