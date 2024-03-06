using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Helper;
using KempDec.AutoRelease.Options;
using Octokit;
using Semver;
using System.CommandLine;

namespace KempDec.AutoRelease.SubCommands;

/// <summary>
/// Responsável pelo gerenciamento do subcomando de geração automática da versão do release.
/// </summary>
internal class VersionSubCommand : Command
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="VersionSubCommand"/>.
    /// </summary>
    public VersionSubCommand() : base(name: "version", description: "Gere a versão do release.")
    {
        var token = new TokenOption();
        var repo = new RepoOption();
        var branch = new BranchOption();
        var types = new TypesOption();
        var firstVersion = new FirstVersionOption();
        var versionPrefix = new VersionPrefixOption();

        AddOption(token);
        AddOption(repo);
        AddOption(branch);
        AddOption(types);
        AddOption(firstVersion);
        AddOption(versionPrefix);

        this.SetHandler(HandleAsync, token, repo, branch, types, firstVersion, versionPrefix);
    }

    /// <summary>
    /// Manipula o comando.
    /// </summary>
    /// <param name="token">O token para acesso ao repositório no GitHub.</param>
    /// <param name="repo">O repositório que contém os commits no GitHub.</param>
    /// <param name="branch">O branch do repositório no GitHub.</param>
    /// <param name="types">Os tipos das mensagens de commit.</param>
    /// <param name="firstVersion">A primeira versão do repositório.</param>
    /// <param name="versionPrefix">O prefixo das versões do repositório.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    private async Task HandleAsync(string token, (string Owner, string Name) repo, string? branch,
        List<CommitMessageType> types, SemVersion? firstVersion,
        string versionPrefix)
    {
        GitHubClient github = GitHubClientHelper.Create(token);
        SemVersion? version;
        DateTimeOffset? since = null;

        try
        {
            Release githubLatestRelease = await github.Repository.Release.GetLatest(repo.Owner, repo.Name);

            string tagVersion = githubLatestRelease.TagName.StartsWith(versionPrefix)
                ? githubLatestRelease.TagName[versionPrefix.Length..]
                : githubLatestRelease.TagName;

            if (!SemVersion.TryParse(tagVersion, SemVersionStyles.Strict, out version))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"A versão {tagVersion} é inválida.");
                Console.ResetColor();
            }

            since = githubLatestRelease.PublishedAt;
        }
        catch (NotFoundException)
        {
            version = firstVersion;
        }

        version ??= FirstVersionOption.Default;

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
            .Select(e => new CommitMessage(e.Commit.Message, types))
            .ToList();

        if (commitMessages.Any(e => e.Type.Ordering == (byte)CommitMessageTypeOrder.Feat))
        {
            version = version.WithMinor(version.Minor + 1);
        }
        else
        {
            version = version.WithPatch(version.Patch + 1);
        }

        await Console.Out.WriteLineAsync(version.ToString());
    }
}
