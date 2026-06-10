using KempDec.AutoRelease.Extensions;
using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do token para acesso ao repositório no GitHub.
/// </summary>
internal class TokenOption : Option<string>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="TokenOption"/>.
    /// </summary>
    public TokenOption() : base("--token", "-t")
    {
        Description = "O token para acesso ao repositório no GitHub.";
        Required = true;
        CustomParser = result =>
        {
            string? token = result.GetSingleTokenOrDefault();

            if (string.IsNullOrWhiteSpace(token))
            {
                result.AddError("O token para acesso ao repositório no GitHub não pode estar vazio.");

                return default!;
            }

            return token;
        };
    }
}
