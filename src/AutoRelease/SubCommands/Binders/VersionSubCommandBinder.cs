using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Inputs;
using System.CommandLine;

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
    public override VersionSubCommandInputs GetInputs(ParseResult parseResult)
    {
        VersionSubCommandInputs inputs = base.GetInputs(parseResult);

        inputs.FirstVersion = parseResult.GetValue(FirstVersionOption);
        inputs.VersionPrefix = parseResult.GetValue(VersionPrefixOption)!;

        return inputs;
    }
}
