using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do branch do repositório no GitHub.
/// </summary>
internal class BranchOption : Option<string?>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="BranchOption"/>.
    /// </summary>
    public BranchOption() : base(name: "--branch",
        description: "O branch do repositório no GitHub.") => AddAlias("-b");
}
