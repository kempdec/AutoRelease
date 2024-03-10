using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção de um sinalizador indicando se o release é um rascunho.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="DraftOption"/>.</remarks>
internal class DraftOption() : Option<bool>(
    name: "--draft",
    description: "Um sinalizador indicando se o release é um rascunho.",
    getDefaultValue: () => false)
{
}
