using System.Text.RegularExpressions;

namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Representa uma mensagem de commit lida no formato do Conventional Commits, com os operadores do Auto Release.
/// </summary>
/// <remarks>
/// O título segue <c>tipo(escopo)operadores: descrição</c>, em que o escopo e os operadores são opcionais. Os
/// operadores ficam colados ao tipo, antes dos dois-pontos: <c>!</c> marca a quebra de compatibilidade, <c>#</c> tira o
/// commit das notas e da conta da versão, e <c>^</c> usa o corpo no lugar da descrição. Uma mensagem fora do formato
/// continua sendo lida: ela fica sem tipo, e a descrição é o título inteiro.
/// </remarks>
internal sealed partial record CommitMessage
{
    /// <summary>
    /// Obtém a mensagem original, como veio do commit.
    /// </summary>
    public required string Original { get; init; }

    /// <summary>
    /// Obtém o tipo, em minúsculas, ou <see langword="null"/> quando a mensagem não segue o formato.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Obtém o escopo, ou <see langword="null"/> quando a mensagem não tem.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    /// Obtém a descrição: o título depois dos dois-pontos, ou o título inteiro quando a mensagem não segue o formato.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Obtém as linhas do corpo, sem as linhas em branco das pontas e sem os rodapés do Git.
    /// </summary>
    public IReadOnlyList<string> Body { get; init; } = [];

    /// <summary>
    /// Obtém um sinalizador indicando se o commit quebra a compatibilidade, pelo operador <c>!</c> ou pelo rodapé
    /// <c>BREAKING CHANGE:</c>.
    /// </summary>
    public bool IsBreakingChange { get; init; }

    /// <summary>
    /// Obtém um sinalizador indicando se o commit leva o operador <c>#</c>, que o tira das notas e da conta da versão.
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// Obtém um sinalizador indicando se o commit leva o operador <c>^</c>, que usa o corpo no lugar da descrição.
    /// </summary>
    public bool UsesBodyAsDescription { get; init; }

    /// <summary>
    /// Obtém um sinalizador indicando se a mensagem segue o formato <c>tipo: descrição</c>.
    /// </summary>
    public bool IsConventional => Type is not null;

    /// <summary>
    /// Lê uma mensagem de commit.
    /// </summary>
    /// <param name="message">A mensagem, com o título na primeira linha.</param>
    /// <returns>A mensagem lida.</returns>
    public static CommitMessage Parse(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        string[] lines = message.ReplaceLineEndings("\n").Split('\n');
        string header = lines[0].Trim();
        (IReadOnlyList<string> body, IReadOnlyList<string> footers) = SplitBodyAndFooters(lines[1..]);
        bool breakingFooter = footers.Any(footer => BreakingFooterRegex().IsMatch(footer));

        Match match = HeaderRegex().Match(header);

        if (!match.Success)
        {
            return new CommitMessage
            {
                Original = message,
                Description = header,
                Body = body,
                IsBreakingChange = breakingFooter
            };
        }

        string operators = match.Groups["operators"].Value;
        string scope = match.Groups["scope"].Value.Trim();

        return new CommitMessage
        {
            Original = message,
            Type = match.Groups["type"].Value.ToLowerInvariant(),
            Scope = scope.Length > 0 ? scope : null,
            Description = match.Groups["description"].Value.Trim(),
            Body = body,
            IsBreakingChange = operators.Contains('!') || breakingFooter,
            IsHidden = operators.Contains('#'),
            UsesBodyAsDescription = operators.Contains('^')
        };
    }

    /// <summary>
    /// Separa as linhas depois do título em corpo e rodapés.
    /// </summary>
    /// <remarks>
    /// Os rodapés são o último parágrafo quando todas as linhas dele têm a forma <c>Chave: valor</c> do Git (como
    /// <c>Co-Authored-By</c> e <c>Signed-off-by</c>): eles servem à ferramenta, e não a quem lê as notas.
    /// </remarks>
    /// <param name="lines">As linhas depois do título.</param>
    /// <returns>O corpo e os rodapés.</returns>
    private static (IReadOnlyList<string> Body, IReadOnlyList<string> Footers) SplitBodyAndFooters(string[] lines)
    {
        List<string> remaining = TrimBlankLines(lines);
        int lastParagraphStart = remaining.FindLastIndex(string.IsNullOrWhiteSpace) + 1;
        List<string> lastParagraph = remaining[lastParagraphStart..];

        if (lastParagraph.Count == 0 || !lastParagraph.All(line => FooterRegex().IsMatch(line)))
        {
            return (remaining, []);
        }

        return (TrimBlankLines(remaining[..lastParagraphStart]), lastParagraph);
    }

    /// <summary>
    /// Tira as linhas em branco do começo e do fim.
    /// </summary>
    /// <param name="lines">As linhas.</param>
    /// <returns>As linhas sem as linhas em branco das pontas, cada uma sem os espaços do fim.</returns>
    private static List<string> TrimBlankLines(IEnumerable<string> lines)
    {
        List<string> result = [.. lines.Select(line => line.TrimEnd())];

        while (result.Count > 0 && result[0].Length == 0)
        {
            result.RemoveAt(0);
        }

        while (result.Count > 0 && result[^1].Length == 0)
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    /// <summary>
    /// O título no formato <c>tipo(escopo)operadores: descrição</c>.
    /// </summary>
    [GeneratedRegex(@"^(?<type>[A-Za-z][A-Za-z0-9-]*)(?:\((?<scope>[^()]*)\))?(?<operators>[!#^]*):\s+(?<description>\S.*)$")]
    private static partial Regex HeaderRegex();

    /// <summary>
    /// Uma linha de rodapé do Git: <c>Chave: valor</c> ou <c>Chave #valor</c>.
    /// </summary>
    /// <remarks>
    /// Só valem as chaves com hífen (<c>Co-Authored-By</c>, <c>Signed-off-by</c>, <c>BREAKING-CHANGE</c>),
    /// <c>BREAKING CHANGE</c> e as referências de issue. Uma chave de uma palavra só, como <c>Nota: ...</c>, é texto
    /// do corpo, e tirá-la das notas apagaria o que a pessoa escreveu.
    /// </remarks>
    [GeneratedRegex(@"^(?:BREAKING CHANGE|[A-Za-z]+(?:-[A-Za-z]+)+|(?i:refs?|fix(?:es)?|close[sd]?|resolve[sd]?))(?:: | #)\S")]
    private static partial Regex FooterRegex();

    /// <summary>
    /// O rodapé que marca a quebra de compatibilidade.
    /// </summary>
    [GeneratedRegex(@"^BREAKING[ -]CHANGE: ")]
    private static partial Regex BreakingFooterRegex();
}
