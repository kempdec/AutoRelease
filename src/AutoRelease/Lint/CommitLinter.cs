using KempDec.AutoRelease.Commits;

namespace KempDec.AutoRelease.Lint;

/// <summary>
/// Classe com métodos auxiliares para conferir as mensagens de commit contra as regras do projeto, antes de elas
/// chegarem às notas.
/// </summary>
internal static class CommitLinter
{
    /// <summary>
    /// Os tipos do Conventional Commits, aceitos quando o projeto não configura os dele.
    /// </summary>
    public static IReadOnlyList<string> ConventionalTypes { get; } =
        ["feat", "fix", "docs", "style", "refactor", "perf", "test", "build", "ci", "chore", "revert"];

    /// <summary>
    /// Os começos das mensagens que o próprio Git escreve, que não passam pela conferência.
    /// </summary>
    private static readonly string[] s_gitGeneratedStarts = ["Merge ", "Revert \"", "fixup! ", "squash! ", "amend! "];

    /// <summary>
    /// Confere uma mensagem.
    /// </summary>
    /// <param name="message">A mensagem, com o título na primeira linha.</param>
    /// <param name="rules">As regras.</param>
    /// <returns>Os problemas da mensagem, vazios quando ela passa.</returns>
    public static IReadOnlyList<LintProblem> Lint(string message, LintRules rules)
    {
        string text = StripComments(message);
        string header = text.Split('\n')[0].Trim();

        if (header.Length == 0)
        {
            return [new LintProblem("A mensagem está vazia.", "Escreva o título, como \"feat: Adicionar a busca.\".")];
        }

        if (s_gitGeneratedStarts.Any(start => header.StartsWith(start, StringComparison.Ordinal))
            || rules.Ignores.Any(start => header.StartsWith(start, StringComparison.Ordinal)))
        {
            return [];
        }

        var parsed = CommitMessage.Parse(text);

        if (!parsed.IsConventional)
        {
            return [new LintProblem(
                "O título não segue o formato \"tipo: Descrição\".",
                $"Comece pelo tipo e dois-pontos, como \"{AllowedTypes(rules)[0]}: {Example(rules)}\".")];
        }

        var problems = new List<LintProblem>();
        IReadOnlyList<string> allowed = AllowedTypes(rules);

        if (!allowed.Contains(parsed.Type!, StringComparer.Ordinal))
        {
            problems.Add(new LintProblem(
                $"O tipo \"{parsed.Type}\" não está entre os do projeto.",
                $"Use um destes: {string.Join(", ", allowed)}."));
        }

        if (rules.RequireLeadingWord && rules.Rewriter.Replaces.Count > 0 && !rules.Rewriter.Matches(parsed.Description))
        {
            string first = parsed.Description.Split(' ', 2)[0];

            problems.Add(new LintProblem(
                $"A descrição começa por \"{first}\", que não está entre os começos do projeto.",
                $"Comece por um destes: {string.Join(", ", rules.Rewriter.LeadingWords)}."));
        }

        if (rules.RequirePeriod && !parsed.Description.EndsWith('.'))
        {
            problems.Add(new LintProblem("O título não termina em ponto final.", "Ponha o ponto no fim do título."));
        }

        if (rules.MaxHeaderLength is int max && header.Length > max)
        {
            problems.Add(new LintProblem(
                $"O título tem {header.Length} caracteres, e o máximo do projeto é {max}.",
                "Encurte o título e leve o detalhe para o corpo da mensagem."));
        }

        foreach (string trailer in rules.ForbiddenTrailers)
        {
            if (text.Split('\n').Any(line => line.TrimStart().StartsWith(trailer + ":", StringComparison.OrdinalIgnoreCase)))
            {
                problems.Add(new LintProblem(
                    $"A mensagem tem o rodapé \"{trailer}\", que o projeto não aceita.",
                    $"Tire a linha \"{trailer}: ...\" da mensagem."));
            }
        }

        return problems;
    }

    /// <summary>
    /// Obtém os tipos aceitos: os do projeto, ou os do Conventional Commits quando ele não configura nenhum.
    /// </summary>
    /// <param name="rules">As regras.</param>
    /// <returns>Os tipos aceitos, na ordem da configuração.</returns>
    private static IReadOnlyList<string> AllowedTypes(LintRules rules) =>
        rules.Types.IsConfigured ? [.. rules.Types.Types.Select(type => type.Key)] : ConventionalTypes;

    /// <summary>
    /// Escreve um exemplo de descrição com o primeiro começo do projeto.
    /// </summary>
    /// <param name="rules">As regras.</param>
    /// <returns>O exemplo.</returns>
    private static string Example(LintRules rules) =>
        rules.Rewriter.LeadingWords.Count > 0 ? $"{rules.Rewriter.LeadingWords[0]} a busca." : "Adicionar a busca.";

    /// <summary>
    /// Tira as linhas de comentário que o Git põe no arquivo da mensagem, as que começam com <c>#</c>.
    /// </summary>
    /// <param name="message">A mensagem.</param>
    /// <returns>A mensagem sem os comentários.</returns>
    private static string StripComments(string message) =>
        string.Join('\n', message.ReplaceLineEndings("\n").Split('\n').Where(line => !line.StartsWith('#'))).Trim();
}

/// <summary>
/// Representa um problema numa mensagem de commit.
/// </summary>
/// <param name="Problem">O que está fora da regra.</param>
/// <param name="NextStep">Como corrigir.</param>
internal sealed record LintProblem(string Problem, string NextStep);

/// <summary>
/// Representa as regras da conferência das mensagens de commit.
/// </summary>
internal sealed record LintRules
{
    /// <summary>
    /// Obtém ou inicializa os tipos aceitos.
    /// </summary>
    public CommitTypeCatalog Types { get; init; } = CommitTypeCatalog.Default;

    /// <summary>
    /// Obtém ou inicializa as trocas do começo das descrições, de onde saem os começos aceitos.
    /// </summary>
    public DescriptionRewriter Rewriter { get; init; } = DescriptionRewriter.None;

    /// <summary>
    /// Obtém ou inicializa os começos das mensagens que não passam pela conferência, como "Merge".
    /// </summary>
    public IReadOnlyList<string> Ignores { get; init; } = [];

    /// <summary>
    /// Obtém ou inicializa um sinalizador indicando se a descrição precisa começar por uma das trocas, quando o
    /// projeto tem trocas.
    /// </summary>
    public bool RequireLeadingWord { get; init; } = true;

    /// <summary>
    /// Obtém ou inicializa um sinalizador indicando se o título precisa terminar em ponto final.
    /// </summary>
    public bool RequirePeriod { get; init; }

    /// <summary>
    /// Obtém ou inicializa o tamanho máximo do título, ou <see langword="null"/> sem limite.
    /// </summary>
    public int? MaxHeaderLength { get; init; }

    /// <summary>
    /// Obtém ou inicializa os rodapés que o projeto não aceita, como <c>Co-Authored-By</c>.
    /// </summary>
    public IReadOnlyList<string> ForbiddenTrailers { get; init; } = [];
}
