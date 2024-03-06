using Octokit;

namespace KempDec.AutoRelease.Helper;

/// <summary>
/// Classe com métodos auxiliares para <see cref="GitHubClient"/>.
/// </summary>
internal class GitHubClientHelper
{
    /// <summary>
    /// Cria uma nova instância de <see cref="GitHubClient"/>.
    /// </summary>
    /// <param name="token">O token para acesso ao repositório no GitHub.</param>
    /// <returns>A nova instância de <see cref="GitHubClient"/>.</returns>
    public static GitHubClient Create(string token)
    {
        var githubHeader = new ProductHeaderValue(name: "AutoRelease");
        var github = new GitHubClient(githubHeader)
        {
            Credentials = new Credentials(token)
        };

        return github;
    }
}
