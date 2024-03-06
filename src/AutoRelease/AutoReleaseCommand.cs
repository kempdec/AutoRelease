using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Options;
using Octokit;
using System.CommandLine;
using System.Text;

namespace KempDec.AutoRelease;

/// <summary>
/// Responsável pelo comando de geração automática de uma release.
/// </summary>
internal class AutoReleaseCommand : RootCommand
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="AutoReleaseCommand"/>.
    /// </summary>
    public AutoReleaseCommand() : base(
        description: "Gere releases automaticamente do seu projeto no GitHub usando o Auto Release.")
    {
        var token = new TokenOption();
        var repo = new RepoOption();
        var branch = new BranchOption();
        var types = new TypesOption();

        AddOption(token);
        AddOption(repo);
        AddOption(branch);
        AddOption(types);

        this.SetHandler(HandleDefaultAsync, token, repo, branch, types);
    }

    /// <summary>
    /// Manipula o comando padrão.
    /// </summary>
    /// <param name="token">O token para acesso ao repositório no GitHub.</param>
    /// <param name="repo">O repositório que contém os commits no GitHub.</param>
    /// <param name="branch">O branch do repositório no GitHub.</param>
    /// <param name="types">Os tipos das mensagens de commit.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    private async Task HandleDefaultAsync(string token, (string Owner, string Name) repo, string? branch,
        List<CommitMessageType> types)
    {
        var githubHeader = new ProductHeaderValue(name: "AutoRelease");
        var github = new GitHubClient(githubHeader)
        {
            Credentials = new Credentials(token)
        };

        var githubCommitRequest = new CommitRequest();

        if (!string.IsNullOrWhiteSpace(branch))
        {
            githubCommitRequest.Sha = branch;
        }

        IReadOnlyList<GitHubCommit> githubCommits = await github.Repository.Commit
            .GetAll(repo.Owner, repo.Name, githubCommitRequest);

        var commitMessages = githubCommits
            .Select(e => new CommitMessage(e.Commit.Message, types))
            .GroupBy(e => e.Type)
            .ToList();

        var builder = new StringBuilder();

        foreach (IGrouping<ICommitMessageType, CommitMessage>? commitMessageGroup in commitMessages)
        {
            if (commitMessageGroup is null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.AppendLine($"## {commitMessageGroup.Key.Label}");
            builder.AppendLine();

            foreach (CommitMessage commitMessage in commitMessageGroup.ToList())
            {
                builder.AppendLine($"- {commitMessage.Description}");
            }
        }

        await Console.Out.WriteLineAsync(builder);
    }

    /// <summary>
    /// Analisa e invoca o comando de geração automática de uma release.
    /// </summary>
    /// <param name="args">Os argumentos a serem analisados.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    public static Task InvokeAsync(string[] args) => new AutoReleaseCommand().InvokeAsync(args);
}
