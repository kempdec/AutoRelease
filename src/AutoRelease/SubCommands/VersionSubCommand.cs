using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Helper;
using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Binders;
using KempDec.AutoRelease.SubCommands.Inputs;
using KempDec.AutoRelease.SubCommands.Results;
using Octokit;
using Semver;

namespace KempDec.AutoRelease.SubCommands;

/// <summary>
/// Responsável pelo gerenciamento do subcomando de geração automática da versão do release.
/// </summary>
internal class VersionSubCommand
    : SubCommandBase<VersionSubCommandBinder, VersionSubCommandInputs, IVersionSubCommandInputs>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="VersionSubCommand"/>.
    /// </summary>
    public VersionSubCommand() : base(name: "version", description: "Gera a versão do release.")
    {
    }

    /// <inheritdoc/>
    public override async Task<SubCommandResult> HandleResultAsync(IVersionSubCommandInputs inputs)
    {
        GitHubClient github = GitHubClientHelper.Create(inputs.Token);
        SemVersion? version;
        DateTimeOffset? since = null;

        try
        {
            Release githubLatestRelease = await github.Repository.Release.GetLatest(inputs.Repo.Owner,
                inputs.Repo.Name);

            string tagVersion = githubLatestRelease.TagName.StartsWith(inputs.VersionPrefix)
                ? githubLatestRelease.TagName[inputs.VersionPrefix.Length..]
                : githubLatestRelease.TagName;

            if (!SemVersion.TryParse(tagVersion, SemVersionStyles.Strict, out version))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"A versão {tagVersion} é inválida.");
                Console.ResetColor();
            }

            since = githubLatestRelease.PublishedAt;
        }
        catch (AuthorizationException)
        {
            return Error("Não foi possível autorizar. Isso geralmente acontece quando o token é inválido.");
        }
        catch (NotFoundException)
        {
            version = inputs.FirstVersion;
        }

        version ??= FirstVersionOption.Default;

        var githubCommitRequest = new CommitRequest
        {
            Sha = inputs.Branch,
            Since = since
        };

        IReadOnlyList<GitHubCommit> githubCommits;

        try
        {
            githubCommits = await github.Repository.Commit.GetAll(inputs.Repo.Owner, inputs.Repo.Name,
                githubCommitRequest);
        }
        catch (NotFoundException)
        {
            return Error("O repositório ou branch não foi encontrado.");
        }

        var commitMessages = githubCommits
            .Select(e => new CommitMessage(e.Commit.Message))
            .ToList();

        if (commitMessages.Any(e => e.Type.Ordering == (byte)CommitMessageTypeOrder.Feat))
        {
            version = version.With(version.Major, version.Minor + 1, patch: 0);
        }
        else
        {
            version = version.WithPatch(version.Patch + 1);
        }

        return Succeeded(value: version.ToString());
    }
}
