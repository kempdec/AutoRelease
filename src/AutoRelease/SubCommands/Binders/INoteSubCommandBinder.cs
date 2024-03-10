using KempDec.AutoRelease.Options;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Fornece abstração para o ficheiro do subcomando <see cref="NoteSubCommand"/>.
/// </summary>
internal interface INoteSubCommandBinder : IGlobalBinder
{
    /// <summary>
    /// Obtém ou inicializa a opção dos tipos das mensagens de commit.
    /// </summary>
    public TypesOption TypesOption { get; init; }

    /// <summary>
    /// Obtém ou inicializa a opção das substituições do início das mensagens de commit.
    /// </summary>
    public ReplacesOption ReplacesOption { get; init; }

    /// <summary>
    /// Obtém ou inicializa a opção dos inícios das mensagens de commit que serão ignoradas.
    /// </summary>
    public IgnoresOption IgnoresOption { get; init; }

    /// <summary>
    /// Obtém ou inicializa a opção de um sinalizador indicando se o autor deve ser exibido no final das mensagens de
    /// commit.
    /// </summary>
    public ShowAuthorOption ShowAuthorOption { get; init; }
}
