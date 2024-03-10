using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Fornece abstração para o ficheiro do subcomando <see cref="CreateSubCommand"/>.
/// </summary>
internal interface ICreateSubCommandBinder : INoteSubCommandBinder, IVersionSubCommandBinder
{
    /// <summary>
    /// Obtém ou inicializa a opção da versão do release.
    /// </summary>
    public VersionOption VersionOption { get; init; }

    /// <summary>
    /// Obtém ou inicializa a opção do nome do projeto.
    /// </summary>
    public ProjectNameOption ProjectNameOption { get; init; }

    /// <summary>
    /// Obtém ou inicializa a opção de um sinalizador indicando se o release é um rascunho.
    /// </summary>
    public DraftOption DraftOption { get; init; }

    /// <summary>
    /// Obtém ou inicializa a opção de um sinalizador indicando se o release é um pre-release.
    /// </summary>
    public PreReleaseOption PreReleaseOption { get; init; }
}
