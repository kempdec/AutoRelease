using KempDec.AutoRelease.Commits;

namespace KempDec.AutoRelease.SubCommands.Inputs;

/// <summary>
/// Fornece abstração para as entradas do subcomando <see cref="NoteSubCommand"/>.
/// </summary>
internal interface INoteSubCommandInputs : IGlobalInputs
{
    /// <summary>
    /// Obtém ou define os tipos das mensagens de commit.
    /// </summary>
    public List<CommitMessageType> Types { get; set; }

    /// <summary>
    /// Obtém ou define as substituições do início das mensagens de commit.
    /// </summary>
    public List<(string OldValue, string NewValue)> Replaces { get; set; }
}
