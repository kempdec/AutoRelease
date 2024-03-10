using Semver;

namespace KempDec.AutoRelease.SubCommands.Inputs;

/// <summary>
/// Fornece abstração para as entradas do subcomando <see cref="VersionSubCommand"/>.
/// </summary>
internal interface IVersionSubCommandInputs : IGlobalInputs
{
    /// <summary>
    /// Obtém ou define a primeira versão do repositório.
    /// </summary>
    public SemVersion? FirstVersion { get; set; }

    /// <summary>
    /// Obtém ou define o prefixo das versões do repositório
    /// </summary>
    public string VersionPrefix { get; set; }
}
