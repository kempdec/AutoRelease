using System.Text;
using KempDec.AutoRelease.Commits;

namespace KempDec.AutoRelease.Releases;

/// <summary>
/// Classe com métodos auxiliares para escrever as notas de uma release a partir dos commits.
/// </summary>
internal static class ReleaseNotesBuilder
{
    /// <summary>
    /// A quebra de linha dentro de um item das notas, que mantém o corpo do commit no mesmo item da lista.
    /// </summary>
    private const string LineBreak = "<br>";

    /// <summary>
    /// Escreve as notas: um grupo por tipo, na ordem dos tipos, com um item por commit, do mais antigo ao mais novo.
    /// </summary>
    /// <remarks>
    /// Os commits ignorados ficam de fora, e um texto repetido entra uma vez só. Cada item é o escopo em negrito, se
    /// houver, a descrição com o começo trocado (ou o corpo, com o operador <c>^</c>), o autor, se configurado, e o
    /// corpo, na mesma linha.
    /// </remarks>
    /// <param name="commits">Os commits, do mais antigo ao mais novo.</param>
    /// <param name="rules">As regras de leitura dos commits.</param>
    /// <returns>As notas em Markdown, ou um texto vazio quando nenhum commit entra.</returns>
    public static string Build(IEnumerable<ReleaseCommit> commits, ReleaseRules rules)
    {
        IOrderedEnumerable<IGrouping<CommitType, (CommitType Type, string Text)>> groups = commits
            .Where(commit => !rules.IsIgnored(commit.Message))
            .Select(commit => (Type: rules.Types.Resolve(commit.Message), Text: GetItemText(commit, rules)))
            .DistinctBy(item => item.Text)
            .GroupBy(item => item.Type)
            .OrderBy(group => group.Key.Order);

        var builder = new StringBuilder();

        foreach (IGrouping<CommitType, (CommitType Type, string Text)> group in groups)
        {
            if (builder.Length > 0)
            {
                builder.Append('\n');
            }

            builder.Append("# ").Append(group.Key.Label).Append("\n\n");

            foreach ((_, string text) in group)
            {
                builder.Append("- ").Append(text).Append('\n');
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Escreve o texto do item de um commit.
    /// </summary>
    /// <param name="commit">O commit.</param>
    /// <param name="rules">As regras de leitura dos commits.</param>
    /// <returns>O texto do item.</returns>
    private static string GetItemText(ReleaseCommit commit, ReleaseRules rules)
    {
        CommitMessage message = commit.Message;
        string body = string.Join(LineBreak, message.Body);
        bool bodyOnly = message.UsesBodyAsDescription && body.Length > 0;
        var builder = new StringBuilder();

        if (message.Scope is not null)
        {
            builder.Append("**").Append(message.Scope).Append(":** ");
        }

        builder.Append(rules.Rewriter.Rewrite(bodyOnly ? body : message.Description));

        if (rules.ShowAuthor && !string.IsNullOrWhiteSpace(commit.AuthorName))
        {
            builder.Append(" [").Append(commit.AuthorName.Trim()).Append(']');
        }

        if (!bodyOnly && body.Length > 0)
        {
            builder.Append(LineBreak).Append(body);
        }

        return builder.ToString();
    }
}
