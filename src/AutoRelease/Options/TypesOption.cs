using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Extensions;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção dos tipos das mensagens de commit.
/// </summary>
internal class TypesOption : Option<List<CommitMessageType>>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="TypesOption"/>.
    /// </summary>
    public TypesOption() : base("--types")
    {
        Description = "Os tipos das mensagens de commit.";
        CustomParser = ParseArgument;
        AllowMultipleArgumentsPerToken = true;
    }

    /// <summary>
    /// Analisa o resultado do argumento especificado para o tipo esperado da opção.
    /// </summary>
    /// <param name="result">O resultado do argumento da opção.</param>
    /// <returns>O tipo esperado da opção.</returns>
    private static List<CommitMessageType> ParseArgument(ArgumentResult result)
    {
        var types = new List<CommitMessageType>();

        for (byte i = 0; i < result.Tokens.Count; i++)
        {
            Token token = result.Tokens[i];
            (string? key, string? label) = token.Value.Split('=');

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(label))
            {
                result.AddError("""
                    Não foi possível definir os tipos das mensagens de commit.

                    Os tipos das mensagens de commit devem estar no formato:

                    type1=value1 type2=value2
                    """);

                return default!;
            }

            types.Add(new CommitMessageType
            {
                Key = key,
                Label = label,
                Ordering = i
            });
        }

        return types;
    }
}
