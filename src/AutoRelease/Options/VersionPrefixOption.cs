using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do prefixo das versões do repositório.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="VersionPrefixOption"/>.</remarks>
internal class VersionPrefixOption() : Option<string>(
    name: "--version-prefix",
    description: "O prefixo das versões do repositório.",
    getDefaultValue: () => "v")
{
}
