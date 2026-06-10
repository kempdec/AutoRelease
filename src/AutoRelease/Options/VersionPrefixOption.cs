using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do prefixo das versões do repositório.
/// </summary>
internal class VersionPrefixOption : Option<string>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="VersionPrefixOption"/>.
    /// </summary>
    public VersionPrefixOption() : base("--version-prefix")
    {
        Description = "O prefixo das versões do repositório.";
        DefaultValueFactory = _ => "v";
    }
}
