using KempDec.AutoRelease.Commits;

namespace KempDec.AutoRelease.Releases;

/// <summary>
/// Representa um commit que entra na conta de uma release.
/// </summary>
/// <param name="Sha">O identificador do commit.</param>
/// <param name="AuthorName">O nome de quem escreveu o commit.</param>
/// <param name="Message">A mensagem lida.</param>
internal sealed record ReleaseCommit(string Sha, string AuthorName, CommitMessage Message)
{
    /// <summary>
    /// Cria o commit a partir da mensagem crua.
    /// </summary>
    /// <param name="sha">O identificador do commit.</param>
    /// <param name="authorName">O nome de quem escreveu o commit.</param>
    /// <param name="message">A mensagem, com o título na primeira linha.</param>
    /// <returns>O commit.</returns>
    public static ReleaseCommit Create(string sha, string authorName, string message) =>
        new(sha, authorName, CommitMessage.Parse(message));
}
