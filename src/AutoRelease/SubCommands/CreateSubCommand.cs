using KempDec.AutoRelease.Helper;
using KempDec.AutoRelease.SubCommands.Binders;
using KempDec.AutoRelease.SubCommands.Inputs;
using KempDec.AutoRelease.SubCommands.Models;
using KempDec.AutoRelease.SubCommands.Results;
using Octokit;

namespace KempDec.AutoRelease.SubCommands;

/// <summary>
/// Responsável pelo gerenciamento do subcomando de criação automática de um release.
/// </summary>
internal class CreateSubCommand
    : SubCommandBase<CreateSubCommandBinder, CreateSubCommandInputs, ICreateSubCommandInputs>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="CreateSubCommand"/>.
    /// </summary>
    public CreateSubCommand() : base(name: "create", description: "Cria uma release.")
    {
    }

    /// <inheritdoc/>
    public override async Task<SubCommandResult> HandleResultAsync(ICreateSubCommandInputs inputs)
    {
        SubCommandResult noteResult = await new NoteSubCommand().HandleResultAsync(inputs);

        string version;

        if (inputs.Version is not null)
        {
            version = inputs.Version.ToString();
        }
        else
        {
            SubCommandResult versionResult = await new VersionSubCommand().HandleResultAsync(inputs);

            version = versionResult.Value!;
        }

        GitHubClient github = GitHubClientHelper.Create(inputs.Token);

        var release = new NewRelease(version)
        {
            Name = $"{inputs.ProjectName ?? inputs.Repo.Name} v{version} ({DateTime.Now:yyyy-MM-dd})",
            Body = noteResult.Value,
            Draft = inputs.Draft,
            Prerelease = inputs.PreRelease,
            TargetCommitish = inputs.Branch
        };

        Release releaseResult = await github.Repository.Release.Create(inputs.Repo.Owner, inputs.Repo.Name, release);

        string output = inputs.OutputType switch
        {
            CreateOutputType.Id => releaseResult.Id.ToString(),
            CreateOutputType.UploadUrl => releaseResult.UploadUrl.ToString(),
            CreateOutputType.Version => version,
            _ => throw new IndexOutOfRangeException("O tipo de saída está fora do intervalo.")
        };

        return Succeeded(output);
    }
}
