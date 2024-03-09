using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands;

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
    /// Obtém ou inicializa as configurações do subcomando de geração automática das notas do release.
    /// </summary>
    [ConfigCommand<NoteSubCommand>]
    public NoteSubCommandConfig? NoteCommand { get; init; }

    /// <summary>
    /// Obtém ou inicializa as configurações do subcomando de geração automática da versão do release.
    /// </summary>
    [ConfigCommand<VersionSubCommand>]
    public VersionSubCommandConfig? VersionCommand { get; init; }
}
