using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Inputs;
using System.CommandLine.Binding;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Representa o ficheiro do subcomando <see cref="NoteSubCommand"/>.
/// </summary>
internal class NoteSubCommandBinder
    : GlobalBinder<NoteSubCommandInputs, INoteSubCommandInputs>, INoteSubCommandBinder
{
    /// <inheritdoc/>
    public TypesOption TypesOption { get; init; } = new();

    /// <inheritdoc/>
    public ReplacesOption ReplacesOption { get; init; } = new();

    /// <inheritdoc/>
    public IgnoresOption IgnoresOption { get; init; } = new();

    /// <inheritdoc/>
    protected override NoteSubCommandInputs GetBoundValue(BindingContext bindingContext)
    {
        NoteSubCommandInputs inputs = base.GetBoundValue(bindingContext);

        inputs.Types = bindingContext.ParseResult.GetValueForOption(TypesOption)!;
        inputs.Replaces = bindingContext.ParseResult.GetValueForOption(ReplacesOption)!;
        inputs.Ignores = bindingContext.ParseResult.GetValueForOption(IgnoresOption)!;

        return inputs;
    }
}
