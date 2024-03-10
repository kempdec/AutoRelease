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

    /// <summary>
    /// Obtém ou define os inícios das mensagens de commit que serão ignoradas.
    /// </summary>
    public List<string> Ignores { get; set; }

    /// <summary>
    /// Obtém ou define um sinalizador indicando se o autor deve ser exibido no final das mensagens de commit.
    /// </summary>
    public bool ShowAuthor { get; set; }
}
