namespace KempDec.AutoRelease.Configurations.Commands;

/// <summary>
/// Fornece abstração para a associação recursiva das configurações do subcomando de geração automática da versão do
/// release.
/// </summary>
internal interface IVersionSubCommandConfig
{
    /// <summary>
    /// Obtém ou inicializa a primeira versão do repositório.
    /// </summary>
    public string? FirstVersion { get; init; }

    /// <summary>
    /// Obtém ou inicializa o prefixo das versões do repositório.
    /// </summary>
    public string? VersionPrefix { get; init; }
}
