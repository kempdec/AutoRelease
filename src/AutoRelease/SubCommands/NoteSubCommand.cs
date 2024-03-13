using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Helper;
using KempDec.AutoRelease.SubCommands.Binders;
using KempDec.AutoRelease.SubCommands.Inputs;
using KempDec.AutoRelease.SubCommands.Results;
using Octokit;
using System.Text;

namespace KempDec.AutoRelease.SubCommands;

/// <summary>
/// Responsável pelo gerenciamento do subcomando de geração automática das notas do release.
/// </summary>
internal class NoteSubCommand : SubCommandBase<NoteSubCommandBinder, NoteSubCommandInputs, INoteSubCommandInputs>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="NoteSubCommand"/>.
    /// </summary>
    public NoteSubCommand() : base(name: "note", description: "Gera as notas do release.")
    {
    }

    /// <inheritdoc/>
    public override async Task<SubCommandResult> HandleResultAsync(INoteSubCommandInputs inputs)
    {
        GitHubClient github = GitHubClientHelper.Create(inputs.Token);

        DateTimeOffset? since = null;

        try
        {
            Release githubLatestRelease = await github.Repository.Release.GetLatest(inputs.Repo.Owner,
                inputs.Repo.Name);

            since = githubLatestRelease.PublishedAt;
        }
        catch (AuthorizationException)
        {
            return Error("Não foi possível autorizar. Isso geralmente acontece quando o token é inválido.");
        }
        catch (NotFoundException)
        {
        }

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
            .Select(e => new CommitMessage(e.Commit, inputs))
            .Where(e => e.Type.Ordering != (byte)CommitMessageTypeOrder.Ignore
                && !inputs.Ignores.Any(e.Description.StartsWith))
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

            builder.AppendLine($"# {commitMessageGroup.Key.Label}");
            builder.AppendLine();

            foreach (CommitMessage commitMessage in commitMessageGroup.ToList())
            {
                builder.AppendLine($"- {commitMessage.ReleaseDescription}");
            }
        }

        return Succeeded(value: builder.ToString());
    }
}
