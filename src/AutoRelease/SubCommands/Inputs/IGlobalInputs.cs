namespace KempDec.AutoRelease.SubCommands.Inputs;

/// <summary>
/// Fornece abstração para as entradas globais.
/// </summary>
internal interface IGlobalInputs
{
    /// <summary>
    /// Obtém ou inicializa o token para acesso ao repositório no GitHub.
    /// </summary>
    public string Token { get; init; }

    /// <summary>
    /// Obtém ou inicializa o repositório que contém os commits no GitHub.
    /// </summary>
    public (string Owner, string Name) Repo { get; init; }

    /// <summary>
    /// Obtém ou inicializa o branch do repositório no GitHub.
    /// </summary>
    public string? Branch { get; init; }
}
