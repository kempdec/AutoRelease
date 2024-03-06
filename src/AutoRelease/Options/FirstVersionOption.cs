using KempDec.AutoRelease.Extensions;
using Semver;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção da primeira versão do repositório.
/// </summary>
internal class FirstVersionOption : Option<SemVersion>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="FirstVersionOption"/>.
    /// </summary>
    public FirstVersionOption() : base(
        name: "--first-version",
        description: "A primeira versão do repositório.",
        parseArgument: ParseArgument) => AddAlias("-v");

    /// <summary>
    /// Obtém a primeira versão padrão.
    /// </summary>
    public static SemVersion Default { get; } = new(major: 1, minor: 0, patch: 0);

    /// <summary>
    /// Analisa o resultado do argumento especificado para o tipo esperado da opção.
    /// </summary>
    /// <param name="result">O resultado do argumento da opção.</param>
    /// <returns>O tipo esperado da opção.</returns>
    private static SemVersion ParseArgument(ArgumentResult result)
    {
        string? version = result.GetSingleTokenOrDefault();

        if (string.IsNullOrWhiteSpace(version))
        {
            return Default;
        }

        if (!SemVersion.TryParse(version, SemVersionStyles.Strict, out SemVersion semVersion))
        {
            result.ErrorMessage = "A primeira versão não é uma versão semântica 2.0 válida.";

            return default!;
        }

        return semVersion;
    }
}
