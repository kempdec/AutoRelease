using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Fornece abstração para o ficheiro do subcomando <see cref="VersionSubCommand"/>.
/// </summary>
internal interface IVersionSubCommandBinder : IGlobalBinder
{
    /// <summary>
    /// Obtém ou inicializa a opção da primeira versão do repositório.
    /// </summary>
    public FirstVersionOption FirstVersionOption { get; init; }

    /// <summary>
    /// Obtém ou inicializa a opção do prefixo das versões do repositório.
    /// </summary>
    public VersionPrefixOption VersionPrefixOption { get; init; }
}
