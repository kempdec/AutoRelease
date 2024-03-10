using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção de um sinalizador indicando se o release é um pre-release.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="PreReleaseOption"/>.</remarks>
internal class PreReleaseOption() : Option<bool>(
    name: "--pre-release",
    description: "Um sinalizador indicando se o release é um pre-release.",
    getDefaultValue: () => false)
{
}
