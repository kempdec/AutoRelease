using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Inputs;
using System.CommandLine;
using VersionOption = KempDec.AutoRelease.Options.VersionOption;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Representa o fichário do subcomando <see cref="CreateSubCommand"/>.
/// </summary>
internal class CreateSubCommandBinder
    : GlobalBinder<CreateSubCommandInputs, ICreateSubCommandInputs>, ICreateSubCommandBinder
{
    /// <inheritdoc/>
    public ProjectNameOption ProjectNameOption { get; init; } = new();

    /// <inheritdoc/>
    public DraftOption DraftOption { get; init; } = new();

    /// <inheritdoc/>
    public PreReleaseOption PreReleaseOption { get; init; } = new();

    /// <inheritdoc/>
    public CreateOutputTypeOption OutputTypeOption { get; init; } = new();

    #region Notes.

    /// <inheritdoc/>
    public VersionOption VersionOption { get; init; } = new();

    /// <inheritdoc/>
    public TypesOption TypesOption { get; init; } = new();

    /// <inheritdoc/>
    public ReplacesOption ReplacesOption { get; init; } = new();

    /// <inheritdoc/>
    public IgnoresOption IgnoresOption { get; init; } = new();

    /// <inheritdoc/>
    public ShowAuthorOption ShowAuthorOption { get; init; } = new();

    #endregion

    #region Version.

    /// <inheritdoc/>
    public FirstVersionOption FirstVersionOption { get; init; } = new();

    /// <inheritdoc/>
    public VersionPrefixOption VersionPrefixOption { get; init; } = new();

    #endregion

    /// <inheritdoc/>
    public override CreateSubCommandInputs GetInputs(ParseResult parseResult)
    {
        CreateSubCommandInputs inputs = base.GetInputs(parseResult);

        inputs.Version = parseResult.GetValue(VersionOption);
        inputs.ProjectName = parseResult.GetValue(ProjectNameOption);
        inputs.Draft = parseResult.GetValue(DraftOption);
        inputs.PreRelease = parseResult.GetValue(PreReleaseOption);
        inputs.OutputType = parseResult.GetValue(OutputTypeOption);

        #region Notes.

        inputs.Types = parseResult.GetValue(TypesOption)!;
        inputs.Replaces = parseResult.GetValue(ReplacesOption)!;
        inputs.Ignores = parseResult.GetValue(IgnoresOption)!;
        inputs.ShowAuthor = parseResult.GetValue(ShowAuthorOption);

        #endregion

        #region Version.

        inputs.FirstVersion = parseResult.GetValue(FirstVersionOption);
        inputs.VersionPrefix = parseResult.GetValue(VersionPrefixOption)!;

        #endregion

        return inputs;
    }
}
