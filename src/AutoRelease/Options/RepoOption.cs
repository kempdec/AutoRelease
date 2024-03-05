using KempDec.AutoRelease.Extensions;
using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do repositório que contém os commits no GitHub.
/// </summary>
internal class RepoOption : Option<(string Owner, string Name)>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="RepoOption"/>.
    /// </summary>
    public RepoOption() : base(name: "--repo",
        description: "O repositório que contém os commits no GitHub",
        isDefault: true,
        parseArgument: result =>
        {
            string? repo = result.GetSingleTokenOrDefault();

            if (string.IsNullOrWhiteSpace(repo))
            {
                result.ErrorMessage = "O repositório que contém os commits no GitHub não pode estar vazio.";

                return default;
            }

            (string? repoOwner, string? repoName) = repo.Split('/');

            if (string.IsNullOrWhiteSpace(repoOwner) || string.IsNullOrWhiteSpace(repoName))
            {
                result.ErrorMessage = """
                O nome do proprietário ou do repositório não podem estar vazios.
                
                Especifique no seguinte formato:

                owner/repository
                """;

                return default;
            }

            return (repoOwner, repoName);
        }) => IsRequired = true;
}
