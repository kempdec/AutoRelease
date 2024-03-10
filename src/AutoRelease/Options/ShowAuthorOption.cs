using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção de um sinalizador indicando se o autor deve ser exibido no final das mensagens de commit.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="ShowAuthorOption"/>.</remarks>
internal class ShowAuthorOption() : Option<bool>(
    name: "--show-author",
    description: "Um sinalizador indicando se o autor deve ser exibido no final das mensagens de commit.",
    getDefaultValue: () => false)
{
}
