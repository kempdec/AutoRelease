using KempDec.AutoRelease.Errors;
using KempDec.AutoRelease.Resources;

namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Representa os tipos de mensagem de commit que o projeto usa, na ordem em que aparecem nas notas.
/// </summary>
/// <remarks>
/// O primeiro tipo é o das novidades: um commit dele sobe a versão <c>minor</c>, e os demais sobem a <c>patch</c>. Sem
/// tipos configurados, valem <c>feat</c>, <c>fix</c> e <c>chore</c>. O commit de um tipo fora da lista, ou fora do
/// formato, vai para o grupo <see cref="Other"/>, no fim das notas.
/// </remarks>
internal sealed class CommitTypeCatalog
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="CommitTypeCatalog"/>.
    /// </summary>
    /// <param name="types">Os tipos, na ordem das notas, com o das novidades primeiro.</param>
    /// <param name="isConfigured">Se os tipos vieram da configuração do projeto.</param>
    private CommitTypeCatalog(IReadOnlyList<CommitType> types, bool isConfigured)
    {
        Types = types;
        IsConfigured = isConfigured;
    }

    /// <summary>
    /// Obtém o grupo dos commits de tipo desconhecido ou fora do formato.
    /// </summary>
    public static CommitType Other { get; } = new("commit", CommitTypesResource.Commit, int.MaxValue);

    /// <summary>
    /// Obtém os tipos padrão, quando o projeto não configura nenhum.
    /// </summary>
    public static CommitTypeCatalog Default { get; } = new(
        [
            new CommitType("feat", CommitTypesResource.Feat, 0),
            new CommitType("fix", CommitTypesResource.Fix, 1),
            new CommitType("chore", CommitTypesResource.Chore, 2)
        ],
        isConfigured: false);

    /// <summary>
    /// Obtém os tipos, na ordem das notas.
    /// </summary>
    public IReadOnlyList<CommitType> Types { get; }

    /// <summary>
    /// Obtém um sinalizador indicando se os tipos vieram da configuração do projeto.
    /// </summary>
    public bool IsConfigured { get; }

    /// <summary>
    /// Obtém o tipo das novidades, que sobe a versão <c>minor</c>.
    /// </summary>
    public CommitType MinorType => Types[0];

    /// <summary>
    /// Cria os tipos a partir da configuração, no formato <c>chave=Título</c>.
    /// </summary>
    /// <param name="entries">Os tipos, na ordem das notas, como <c>feat=Novidades</c>.</param>
    /// <returns>Os tipos, ou <see cref="Default"/> quando nenhum é informado.</returns>
    /// <exception cref="AutoReleaseException">Quando um tipo não está no formato ou se repete.</exception>
    public static CommitTypeCatalog Parse(IReadOnlyList<string> entries)
    {
        if (entries.Count == 0)
        {
            return Default;
        }

        List<CommitType> types = [];

        foreach (string entry in entries)
        {
            string[] parts = entry.Split('=', count: 2);
            string key = parts[0].Trim().ToLowerInvariant();
            string label = parts.Length == 2 ? parts[1].Trim() : string.Empty;

            if (key.Length == 0 || label.Length == 0)
            {
                throw new AutoReleaseException(
                    $"O tipo \"{entry}\" não está no formato chave=Título.",
                    "Escreva cada tipo como \"feat=Novidades\", um por item da lista Types.");
            }

            if (types.Any(type => type.Key == key))
            {
                throw new AutoReleaseException(
                    $"O tipo \"{key}\" aparece duas vezes em Types.",
                    "Deixe cada tipo uma vez só, na posição em que o grupo dele deve aparecer nas notas.");
            }

            types.Add(new CommitType(key, label, types.Count));
        }

        return new CommitTypeCatalog(types, isConfigured: true);
    }

    /// <summary>
    /// Obtém o tipo de uma mensagem de commit.
    /// </summary>
    /// <param name="message">A mensagem.</param>
    /// <returns>O tipo, ou <see cref="Other"/> quando ele é desconhecido ou a mensagem está fora do formato.</returns>
    public CommitType Resolve(CommitMessage message) =>
        Find(message.Type) ?? Other;

    /// <summary>
    /// Procura um tipo pela chave.
    /// </summary>
    /// <param name="key">A chave, em qualquer caixa.</param>
    /// <returns>O tipo, ou <see langword="null"/> quando ele não está na lista.</returns>
    public CommitType? Find(string? key) =>
        key is null ? null : Types.FirstOrDefault(type => string.Equals(type.Key, key, StringComparison.OrdinalIgnoreCase));
}
