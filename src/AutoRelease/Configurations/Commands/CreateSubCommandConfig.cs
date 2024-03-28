using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Models;

namespace KempDec.AutoRelease.Configurations.Commands;

/// <summary>
/// Associação recursiva das configurações do subcomando de criação automática de um release.
/// </summary>
internal class CreateSubCommandConfig : INoteSubCommandConfig, IVersionSubCommandConfig
{
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

    /// <summary>
    /// Obtém ou define o tipo de saída da criação automática do release.
    /// </summary>
    [ConfigOption<CreateOutputTypeOption>]
    public CreateOutputType? OutputType { get; set; }

    #region Note.

    /// <inheritdoc/>
    [ConfigOption<VersionOption>]
    public string? Version { get; set; }

    /// <inheritdoc/>
    [ConfigOption<TypesOption>]
    public List<string>? Types { get; init; }

    /// <inheritdoc/>
    [ConfigOption<ReplacesOption>]
    public Dictionary<string, string>? Replaces { get; init; }

    /// <inheritdoc/>
    [ConfigOption<IgnoresOption>]
    public List<string>? Ignores { get; set; }

    /// <inheritdoc/>
    [ConfigOption<ShowAuthorOption>]
    public bool? ShowAuthor { get; set; }

    #endregion

    #region Version.

    /// <inheritdoc/>
    [ConfigOption<FirstVersionOption>]
    public string? FirstVersion { get; init; }

    /// <inheritdoc/>
    [ConfigOption<VersionPrefixOption>]
    public string? VersionPrefix { get; init; }

    #endregion
}
