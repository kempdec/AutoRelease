using KempDec.AutoRelease.Options;
using System.CommandLine;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Fornece abstração para o ficheiro global dos comandos.
/// </summary>
internal interface IGlobalBinder
{
    /// <summary>
    /// Obtém ou inicializa a opção do token para acesso ao repositório no GitHub.
    /// </summary>
    public TokenOption TokenOption { get; init; }

    /// <summary>
    /// Obtém ou inicializa a opção do repositório que contém os commits no GitHub.
    /// </summary>
    public RepoOption RepoOption { get; init; }

    /// <summary>
    /// A opção do branch do repositório no GitHub.
    /// </summary>
    public BranchOption BranchOption { get; init; }

    /// <summary>
    /// Adiciona as opções do ficheiro para o comando especificado.
    /// </summary>
    /// <param name="command">O comando a ter as opções do ficheiro adicionadas.</param>
    public void AddOptionsTo(Command command);
}
