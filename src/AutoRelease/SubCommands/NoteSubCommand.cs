using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Helper;
using KempDec.AutoRelease.Options;
using Octokit;
using System.CommandLine;
using System.Text;

namespace KempDec.AutoRelease.SubCommands;

/// <summary>
/// Responsável pelo gerenciamento do subcomando de geração automática das notas do release.
/// </summary>
internal class NoteSubCommand : Command
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="NoteSubCommand"/>.
    /// </summary>
    public NoteSubCommand() : base(name: "note", description: "Gera as notas do release.")
    {
        var token = new TokenOption();
        var repo = new RepoOption();
        var branch = new BranchOption();
        var types = new TypesOption();
        var replaces = new ReplacesOption();

        AddOption(token);
        AddOption(repo);
        AddOption(branch);
        AddOption(types);
        AddOption(replaces);

        this.SetHandler(HandleAsync, token, repo, branch, types, replaces);
    }

    /// <summary>
    /// Manipula o comando.
    /// </summary>
    /// <param name="token">O token para acesso ao repositório no GitHub.</param>
    /// <param name="repo">O repositório que contém os commits no GitHub.</param>
    /// <param name="branch">O branch do repositório no GitHub.</param>
    /// <param name="types">Os tipos das mensagens de commit.</param>
    /// <param name="replaces">As substituições do início das mensagens de commit.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    private async Task HandleAsync(string token, (string Owner, string Name) repo, string? branch,
        List<CommitMessageType> types, List<(string OldValue, string NewValue)> replaces)
    {
        GitHubClient github = GitHubClientHelper.Create(token);
        DateTimeOffset? since = null;

        try
        {
            Release githubLatestRelease = await github.Repository.Release.GetLatest(repo.Owner, repo.Name);

            since = githubLatestRelease.PublishedAt;
        }
        catch (NotFoundException)
        {
        }

        var githubCommitRequest = new CommitRequest
        {
            Sha = branch,
            Since = since
        };

        IReadOnlyList<GitHubCommit> githubCommits;

        try
        {
            githubCommits = await github.Repository.Commit.GetAll(repo.Owner, repo.Name, githubCommitRequest);
        }
        catch (NotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("O repositório ou branch não foi encontrado.");
            Console.ResetColor();

            return;
        }

        var commitMessages = githubCommits
            .Select(e => new CommitMessage(e.Commit.Message, types, replaces))
            .GroupBy(e => e.Type)
            .OrderBy(e => e.Key.Ordering)
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
                builder.AppendLine($"- {commitMessage.ReleaseDescription}");
            }
        }

        await Console.Out.WriteLineAsync(builder);
    }
}
