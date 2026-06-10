using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção de um sinalizador indicando se o release é um pre-release.
/// </summary>
internal class PreReleaseOption : Option<bool>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="PreReleaseOption"/>.
    /// </summary>
    public PreReleaseOption() : base("--pre-release")
    {
        Description = "Um sinalizador indicando se o release é um pre-release.";
        DefaultValueFactory = _ => false;
    }
}
