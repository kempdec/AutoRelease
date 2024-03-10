using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Inputs;
using System.CommandLine.Binding;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Representa o fichário do subcomando <see cref="CreateSubCommand"/>.
/// </summary>
internal class CreateSubCommandBinder
    : GlobalBinder<CreateSubCommandInputs, ICreateSubCommandInputs>, ICreateSubCommandBinder
{
    /// <inheritdoc/>
    public VersionOption VersionOption { get; init; } = new();

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
    public TypesOption TypesOption { get; init; } = new();

    /// <inheritdoc/>
    public ReplacesOption ReplacesOption { get; init; } = new();

    /// <inheritdoc/>
    public IgnoresOption IgnoresOption { get; init; } = new();

    #endregion

    #region Version.

    /// <inheritdoc/>
    public FirstVersionOption FirstVersionOption { get; init; } = new();

    /// <inheritdoc/>
    public VersionPrefixOption VersionPrefixOption { get; init; } = new();

    #endregion

    /// <inheritdoc/>
    protected override CreateSubCommandInputs GetBoundValue(BindingContext bindingContext)
    {
        CreateSubCommandInputs inputs = base.GetBoundValue(bindingContext);

        inputs.Version = bindingContext.ParseResult.GetValueForOption(VersionOption);
        inputs.ProjectName = bindingContext.ParseResult.GetValueForOption(ProjectNameOption);
        inputs.Draft = bindingContext.ParseResult.GetValueForOption(DraftOption);
        inputs.PreRelease = bindingContext.ParseResult.GetValueForOption(PreReleaseOption);
        inputs.OutputType = bindingContext.ParseResult.GetValueForOption(OutputTypeOption);

        #region Notes.

        inputs.Types = bindingContext.ParseResult.GetValueForOption(TypesOption)!;
        inputs.Replaces = bindingContext.ParseResult.GetValueForOption(ReplacesOption)!;
        inputs.Ignores = bindingContext.ParseResult.GetValueForOption(IgnoresOption)!;

        #endregion

        #region Version.

        inputs.FirstVersion = bindingContext.ParseResult.GetValueForOption(FirstVersionOption);
        inputs.VersionPrefix = bindingContext.ParseResult.GetValueForOption(VersionPrefixOption)!;

        #endregion

        return inputs;
    }
}
