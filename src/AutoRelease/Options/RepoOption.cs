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
    public RepoOption() : base("--repo", "-r")
    {
        Description = "O repositório que contém os commits no GitHub";
        Required = true;
        CustomParser = result =>
        {
            string? repo = result.GetSingleTokenOrDefault();

            if (string.IsNullOrWhiteSpace(repo))
            {
                result.AddError("O repositório que contém os commits no GitHub não pode estar vazio.");

                return default;
            }

            (string? repoOwner, string? repoName) = repo.Split('/');

            if (string.IsNullOrWhiteSpace(repoOwner) || string.IsNullOrWhiteSpace(repoName))
            {
                result.AddError("""
                O nome do proprietário ou do repositório não podem estar vazios.

                Especifique no seguinte formato:

                owner/repository
                """);

                return default;
            }

            return (repoOwner, repoName);
        };
    }
}
