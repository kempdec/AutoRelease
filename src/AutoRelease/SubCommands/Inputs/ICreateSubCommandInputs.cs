using Semver;

namespace KempDec.AutoRelease.SubCommands.Inputs;

/// <summary>
/// Fornece abstração para as entradas do subcomando <see cref="CreateSubCommand"/>.
/// </summary>
internal interface ICreateSubCommandInputs : INoteSubCommandInputs, IVersionSubCommandInputs
{
    /// <summary>
    /// Obtém ou define a versão do release.
    /// </summary>
    public SemVersion? Version { get; set; }

    /// <summary>
    /// Obtém ou define o nome do projeto.
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Obtém ou define um sinalizador indicando se o release é um rascunho.
    /// </summary>
    public bool Draft { get; set; }

    /// <summary>
    /// Obtém ou define um sinalizador indicando se o release é um pre-release.
    /// </summary>
    public bool PreRelease { get; set; }
}
