namespace KempDec.AutoRelease.Configurations.Commands;

/// <summary>
/// Fornece abstração para a associação recursiva das configurações do subcomando de geração automática das notas do
/// release.
/// </summary>
internal interface INoteSubCommandConfig
{
    /// <summary>
    /// Obtém ou inicializa os tipos das mensagens de commit.
    /// </summary>
    public List<string>? Types { get; init; }

    /// <summary>
    /// Obtém ou inicializa as substituições do início das mensagens de commit.
    /// </summary>
    public Dictionary<string, string>? Replaces { get; init; }

    /// <summary>
    /// Obtém ou define os inícios das mensagens de commit que serão ignoradas.
    /// </summary>
    public List<string>? Ignores { get; set; }

    /// <summary>
    /// Obtém ou define um sinalizador indicando se o autor deve ser exibido no final das mensagens de commit.
    /// </summary>
    public bool? ShowAuthor { get; set; }
}
