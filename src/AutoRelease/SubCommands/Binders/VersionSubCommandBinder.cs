using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Inputs;
using System.CommandLine.Binding;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Representa o ficheiro do subcomando <see cref="VersionSubCommand"/>.
/// </summary>
internal class VersionSubCommandBinder
    : GlobalBinder<VersionSubCommandInputs, IVersionSubCommandInputs>, IVersionSubCommandBinder
{
    /// <inheritdoc/>
    public FirstVersionOption FirstVersionOption { get; init; } = new();

    /// <inheritdoc/>
    public VersionPrefixOption VersionPrefixOption { get; init; } = new();

    /// <inheritdoc/>
    protected override VersionSubCommandInputs GetBoundValue(BindingContext bindingContext)
    {
        VersionSubCommandInputs inputs = base.GetBoundValue(bindingContext);

        inputs.FirstVersion = bindingContext.ParseResult.GetValueForOption(FirstVersionOption);
        inputs.VersionPrefix = bindingContext.ParseResult.GetValueForOption(VersionPrefixOption)!;

        return inputs;
    }
}
