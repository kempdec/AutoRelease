using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Extensions;
using KempDec.AutoRelease.Helper;
using KempDec.AutoRelease.SubCommands.Binders;
using KempDec.AutoRelease.SubCommands.Inputs;
using KempDec.AutoRelease.SubCommands.Results;
using Octokit;
using Semver;
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
            RepositoryTag? lastTag = null;

            if (inputs.Version is not null)
            {
                IReadOnlyList<RepositoryTag> tags = await github.Repository.GetAllTags(inputs.Repo.Owner,
                    inputs.Repo.Name);

                // Entre todas as tags com versão anterior à versão informada, seleciona a de maior precedência (a
                // versão imediatamente anterior). Antes, apenas as 10 tags mais recentes eram consideradas e a
                // primeira correspondência (em ordem não garantida) era usada, o que podia ignorar a tag correta.
                lastTag = tags
                    .Select(tag => (Tag: tag, Version: tag.Name.ToSemVersion()))
                    .Where(e => e.Version is not null && inputs.Version.ComparePrecedenceTo(e.Version) > 0)
                    .OrderByDescending(e => e.Version!, SemVersion.PrecedenceComparer)
                    .Select(e => e.Tag)
                    .FirstOrDefault();
            }

            Release githubPreviousRelease = lastTag is not null
                ? await github.Repository.Release.Get(inputs.Repo.Owner, inputs.Repo.Name, lastTag.Name)
                : await github.Repository.Release.GetLatest(inputs.Repo.Owner, inputs.Repo.Name);

            since = githubPreviousRelease.PublishedAt;
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
            .Where(e => !e.Ignore)
            .Reverse()
            .DistinctBy(e => e.ReleaseDescription)
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
