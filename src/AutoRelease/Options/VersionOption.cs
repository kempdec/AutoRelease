using KempDec.AutoRelease.Extensions;
using Semver;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção da versão do release.
/// </summary>
internal class VersionOption : Option<SemVersion?>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="VersionOption"/>.
    /// </summary>
    public VersionOption() : base(
        name: "--version",
        description: "A versão do release.",
        parseArgument: ParseArgument) => AddAlias("-v");

    /// <summary>
    /// Analisa o resultado do argumento especificado para o tipo esperado da opção.
    /// </summary>
    /// <param name="result">O resultado do argumento da opção.</param>
    /// <returns>O tipo esperado da opção.</returns>
    private static SemVersion? ParseArgument(ArgumentResult result)
    {
        string? version = result.GetSingleTokenOrDefault();

        if (string.IsNullOrWhiteSpace(version))
        {
            return null;
        }

        if (!SemVersion.TryParse(version, SemVersionStyles.Strict, out SemVersion semVersion))
        {
            result.ErrorMessage = "A versão não é uma versão semântica 2.0 válida.";

            return default!;
        }

        return semVersion;
    }
}
