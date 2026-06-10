using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Inputs;
using System.CommandLine;
using VersionOption = KempDec.AutoRelease.Options.VersionOption;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Representa o ficheiro do subcomando <see cref="NoteSubCommand"/>.
/// </summary>
internal class NoteSubCommandBinder
    : GlobalBinder<NoteSubCommandInputs, INoteSubCommandInputs>, INoteSubCommandBinder
{
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

    /// <inheritdoc/>
    public override NoteSubCommandInputs GetInputs(ParseResult parseResult)
    {
        NoteSubCommandInputs inputs = base.GetInputs(parseResult);

        inputs.Version = parseResult.GetValue(VersionOption);
        inputs.Types = parseResult.GetValue(TypesOption)!;
        inputs.Replaces = parseResult.GetValue(ReplacesOption)!;
        inputs.Ignores = parseResult.GetValue(IgnoresOption)!;
        inputs.ShowAuthor = parseResult.GetValue(ShowAuthorOption);

        return inputs;
    }
}
