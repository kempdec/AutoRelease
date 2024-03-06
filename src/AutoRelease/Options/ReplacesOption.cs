using KempDec.AutoRelease.Extensions;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção de substituições do início das mensagens de commit.
/// </summary>
internal class ReplacesOption : Option<List<(string OldValue, string NewValue)>>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="ReplacesOption"/>.
    /// </summary>
    public ReplacesOption() : base(
        name: "--replaces",
        description: "As substituições do início das mensagens de commit.",
        parseArgument: ParsedArgument) => AllowMultipleArgumentsPerToken = true;

    /// <summary>
    /// Analisa o resultado do argumento especificado para o tipo esperado da opção.
    /// </summary>
    /// <param name="result">O resultado do argumento da opção.</param>
    /// <returns>O tipo esperado da opção.</returns>
    private static List<(string OldValue, string NewValue)> ParsedArgument(ArgumentResult result)
    {
        var replaces = new List<(string OldValue, string NewValue)>();

        foreach (Token token in result.Tokens)
        {
            (string? oldValue, string? newValue) = token.Value.Split('=');

            if (string.IsNullOrWhiteSpace(oldValue) || string.IsNullOrWhiteSpace(newValue))
            {
                result.ErrorMessage = """
                    Não foi possível definir as substituições do início das mensagens de commit.

                    As substituições do início das mensagens de commit devem estar no formato:

                    oldValue1=newValue1 oldValue2=newValue2
                    """;

                return default!;
            }

            replaces.Add((oldValue, newValue));
        }

        return replaces;
    }
}
