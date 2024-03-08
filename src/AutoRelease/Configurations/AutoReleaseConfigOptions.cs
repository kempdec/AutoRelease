using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.Configurations;

internal partial record AutoReleaseConfig
{
    /// <summary>
    /// Obtém ou inicializa o token para acesso ao repositório no GitHub.
    /// </summary>
    [ConfigOption<TokenOption>]
    public string? Token { get; init; }

    /// <summary>
    /// Obtém ou inicializa o repositório que contém os commits no GitHub.
    /// </summary>
    [ConfigOption<RepoOption>]
    public string? Repo { get; init; }

    /// <summary>
    /// Obtém ou incializa o branch do repositório no GitHub.
    /// </summary>
    [ConfigOption<BranchOption>]
    public string? Branch { get; init; }

    /// <summary>
    /// Obtém ou inicializa os tipos das mensagens de commit.
    /// </summary>
    [ConfigOption<TypesOption>]
    public string? Types { get; init; }

    /// <summary>
    /// Obtém ou inicializa as substituições do início das mensagens de commit.
    /// </summary>
    [ConfigOption<ReplacesOption>]
    public string? Replaces { get; init; }

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
