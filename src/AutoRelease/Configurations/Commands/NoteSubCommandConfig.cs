using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.Configurations.Commands;

/// <summary>
/// Associação recursiva das configurações do subcomando de geração automática das notas do release.
/// </summary>
internal record NoteSubCommandConfig : INoteSubCommandConfig
{
    /// <inheritdoc/>
    [ConfigOption<TypesOption>]
    public List<string>? Types { get; init; }

    /// <inheritdoc/>
    [ConfigOption<ReplacesOption>]
    public Dictionary<string, string>? Replaces { get; init; }

    /// <inheritdoc/>
    [ConfigOption<IgnoresOption>]
    public List<string>? Ignores { get; set; }
}
