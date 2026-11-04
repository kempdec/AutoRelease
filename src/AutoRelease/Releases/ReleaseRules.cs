using KempDec.AutoRelease.Commits;

namespace KempDec.AutoRelease.Releases;

/// <summary>
/// Representa as regras de leitura dos commits de um projeto: os tipos, as trocas de começo, os commits ignorados e se o
/// autor aparece nas notas.
/// </summary>
internal sealed record ReleaseRules
{
    /// <summary>
    /// Obtém as regras padrão: os tipos padrão, sem trocas, sem ignorados e sem o autor.
    /// </summary>
    public static ReleaseRules Default { get; } = new();

    /// <summary>
    /// Obtém ou inicializa os tipos de commit.
    /// </summary>
    public CommitTypeCatalog Types { get; init; } = CommitTypeCatalog.Default;

    /// <summary>
    /// Obtém ou inicializa as trocas do começo das descrições.
    /// </summary>
    public DescriptionRewriter Rewriter { get; init; } = DescriptionRewriter.None;

    /// <summary>
    /// Obtém ou inicializa os começos de descrição que tiram o commit das notas e da conta da versão, como "Merge".
    /// </summary>
    public IReadOnlyList<string> Ignores { get; init; } = [];

    /// <summary>
    /// Obtém ou inicializa um sinalizador indicando se o nome do autor aparece no fim de cada linha das notas.
    /// </summary>
    public bool ShowAuthor { get; init; }

    /// <summary>
    /// Diz se um commit fica fora das notas e da conta da versão: pelo operador <c>#</c>, ou porque a descrição começa
    /// por um dos <see cref="Ignores"/>.
    /// </summary>
    /// <param name="message">A mensagem do commit.</param>
    /// <returns><see langword="true"/> quando o commit é ignorado.</returns>
    public bool IsIgnored(CommitMessage message) =>
        message.IsHidden || Ignores.Any(start => message.Description.StartsWith(start, StringComparison.Ordinal));
}
