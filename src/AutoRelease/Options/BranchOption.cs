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
    public BranchOption() : base("--branch", "-b") =>
        Description = "O branch do repositório no GitHub.";
}
