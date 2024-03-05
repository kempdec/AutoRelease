namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Representa uma mensagem de commit.
/// </summary>
internal class CommitMessage
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="CommitMessage"/>.
    /// </summary>
    /// <param name="message">A mensagem de commit. É ideal que a mensagem de commit esteja estruturada da seguinte
    /// forma: &lt;tipo&gt;: &lt;descrição&gt;.</param>
    public CommitMessage(string message)
    {
        OriginMessage = message;

        var messages = message.Split(": ");

        if (messages.Length >= 2)
        {
            Type = messages[0];
            Description = messages[1];
        }
        else
        {
            Description = messages[0];
        }
    }

    /// <summary>
    /// Obtém a mensagem de origem da mensagem de commit.
    /// </summary>
    public string OriginMessage { get; }

    /// <summary>
    /// Obtém o tipo da mensagem de commit.
    /// </summary>
    public string? Type { get; }

    /// <summary>
    /// Obtém a descrição da mensagem de commit.
    /// </summary>
    public string Description { get; }

    /// <inheritdoc/>
    public override string ToString() => OriginMessage;
}
