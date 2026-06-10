using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção de um sinalizador indicando se o autor deve ser exibido no final das mensagens de commit.
/// </summary>
internal class ShowAuthorOption : Option<bool>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="ShowAuthorOption"/>.
    /// </summary>
    public ShowAuthorOption() : base("--show-author")
    {
        Description = "Um sinalizador indicando se o autor deve ser exibido no final das mensagens de commit.";
        DefaultValueFactory = _ => false;
    }
}
