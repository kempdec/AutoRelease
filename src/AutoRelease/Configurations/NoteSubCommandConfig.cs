using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.Configurations;

/// <summary>
/// Associação recursiva das configurações do subcomando de geração automática das notas do release.
/// </summary>
internal record NoteSubCommandConfig
{
    /// <summary>
    /// Obtém ou inicializa os tipos das mensagens de commit.
    /// </summary>
    [ConfigOption<TypesOption>]
    public List<string>? Types { get; init; }

    /// <summary>
    /// Obtém ou inicializa as substituições do início das mensagens de commit.
    /// </summary>
    [ConfigOption<ReplacesOption>]
    public Dictionary<string, string>? Replaces { get; init; }
}
