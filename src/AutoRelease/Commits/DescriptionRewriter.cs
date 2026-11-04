using KempDec.AutoRelease.Errors;

namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Responsável por trocar o começo das descrições nas notas, como "Adicionar a " por "Foi adicionada a ".
/// </summary>
/// <remarks>
/// Vale a troca de começo mais longo que casa, uma vez só e sem diferenciar maiúsculas: assim "Adicionar a página"
/// usa "Adicionar a " e não "Adicionar", qualquer que seja a ordem das trocas na configuração.
/// </remarks>
internal sealed class DescriptionRewriter
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="DescriptionRewriter"/>.
    /// </summary>
    /// <param name="replaces">As trocas, do começo mais longo para o mais curto.</param>
    private DescriptionRewriter(IReadOnlyList<KeyValuePair<string, string>> replaces) => Replaces = replaces;

    /// <summary>
    /// Obtém o reescritor que não troca nada.
    /// </summary>
    public static DescriptionRewriter None { get; } = new([]);

    /// <summary>
    /// Obtém as trocas, do começo mais longo para o mais curto.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> Replaces { get; }

    /// <summary>
    /// Obtém as palavras que abrem as trocas, como "Adicionar" e "Corrigir", sem repetir.
    /// </summary>
    public IReadOnlyList<string> LeadingWords =>
        [.. Replaces
            .Select(replace => replace.Key.Trim().Split(' ', 2)[0])
            .Where(word => word.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// Cria o reescritor a partir das trocas da configuração.
    /// </summary>
    /// <param name="replaces">As trocas: o começo procurado e o texto que entra no lugar dele.</param>
    /// <returns>O reescritor.</returns>
    /// <exception cref="AutoReleaseException">Quando um começo procurado está vazio.</exception>
    public static DescriptionRewriter Create(IEnumerable<KeyValuePair<string, string>> replaces)
    {
        List<KeyValuePair<string, string>> list = [.. replaces];

        if (list.FirstOrDefault(replace => replace.Key.Trim().Length == 0) is { Key: not null } empty)
        {
            throw new AutoReleaseException(
                $"Uma troca de Replaces tem o começo vazio (\"{empty.Key}\"=\"{empty.Value}\").",
                "Informe o começo da descrição que deve ser trocado, como \"Adicionar\": \"Foi adicionado\".");
        }

        return list.Count == 0
            ? None
            : new DescriptionRewriter([.. list.OrderByDescending(replace => replace.Key.Length)]);
    }

    /// <summary>
    /// Troca o começo de uma descrição.
    /// </summary>
    /// <param name="description">A descrição.</param>
    /// <returns>A descrição com o começo trocado, ou a mesma descrição quando nenhuma troca casa.</returns>
    public string Rewrite(string description)
    {
        foreach ((string oldStart, string newStart) in Replaces)
        {
            if (description.StartsWith(oldStart, StringComparison.OrdinalIgnoreCase))
            {
                return newStart + description[oldStart.Length..];
            }
        }

        return description;
    }

    /// <summary>
    /// Diz se uma descrição começa por uma das trocas.
    /// </summary>
    /// <param name="description">A descrição.</param>
    /// <returns><see langword="true"/> quando alguma troca casa com o começo dela.</returns>
    public bool Matches(string description) =>
        Replaces.Any(replace => description.StartsWith(replace.Key, StringComparison.OrdinalIgnoreCase));
}
