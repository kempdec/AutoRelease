using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção de um sinalizador indicando se o release é um rascunho.
/// </summary>
internal class DraftOption : Option<bool>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="DraftOption"/>.
    /// </summary>
    public DraftOption() : base("--draft")
    {
        Description = "Um sinalizador indicando se o release é um rascunho.";
        DefaultValueFactory = _ => false;
    }
}
