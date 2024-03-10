using KempDec.AutoRelease.Commits.Types;
using KempDec.StarterDotNet.Reflection;

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
    /// <param name="types">Os tipos de mensagem de commit.</param>
    /// <param name="replaces">As substituições do início das mensagens de commit.</param>
    public CommitMessage(string message, List<CommitMessageType>? types = null,
        List<(string OldValue, string NewValue)>? replaces = null)
    {
        (string? type, string description, string? body) = SplitMessage(message);

        ICommitMessageType? commitType = null;

        if (type is not null)
        {
            switch (type.Last())
            {
                case '#':
                    commitType = new IgnoreCommitMessageType();
                    break;

                case '!':
                    IsBreakingChange = true;
                    break;
            }
        }

        if (commitType is null && type is not null)
        {
            commitType = types is { Count: > 0 } ? types.SingleOrDefault(e => e.Key == type) : GetType(type);
        }

        OriginMessage = message;
        Type = commitType ?? new DefaultCommitMessageType();
        Description = description;
        Body = body;
        ReleaseDescription = body ?? Description;
        IsBreakingChange = IsBreakingChange || body?.Contains("BREAKING CHANGE:") is true;

        if (replaces is not { Count: > 0 })
        {
            return;
        }

        foreach ((string oldValue, string newValue) in replaces)
        {
            if (ReleaseDescription.StartsWith(oldValue, StringComparison.InvariantCultureIgnoreCase))
            {
                ReleaseDescription = newValue + ReleaseDescription[oldValue.Length..];
            }
        }
    }

    /// <summary>
    /// Obtém a mensagem de origem da mensagem de commit.
    /// </summary>
    public string OriginMessage { get; }

    /// <summary>
    /// Obtém o tipo da mensagem de commit.
    /// </summary>
    public ICommitMessageType Type { get; }

    /// <summary>
    /// Obtém a descrição da mensagem de commit.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Obtém a descrição de release da mensagem de commit.
    /// </summary>
    public string ReleaseDescription { get; }

    /// <summary>
    /// Obtém o corpo da mensagem de commit.
    /// </summary>
    public string? Body { get; }

    /// <summary>
    /// Obtém um sinalizador indicando se a mensagem de commit é traz uma quebra de compatibilidade.
    /// </summary>
    public bool IsBreakingChange { get; }

    /// <summary>
    /// Obtém a representação do tipo da mensagem de commit especificado.
    /// </summary>
    /// <param name="type">O tipo da mensagem de commit.</param>
    /// <returns>A representação do tipo da mensagem de commit especificado.</returns>
    private static ICommitMessageType? GetType(string type)
    {
        IEnumerable<ICommitMessageType?> types = AssemblyHelper.GetAllClassesWithInterface<ICommitMessageType>();

        return types.SingleOrDefault(e => e?.Key == type);
    }

    /// <summary>
    /// Separa a mensagem de commit especificada em tipo, descrição e corpo.
    /// </summary>
    /// <param name="message">A mensagem de commit a ser separada.</param>
    /// <returns>A mensagem de commit separada em tipo, descrição e corpo.</returns>
    private static (string? Type, string Description, string? Body) SplitMessage(string message)
    {
        var messages = message.Split(": ");

        string description;
        string? type = null;
        string? body = null;

        if (messages.Length >= 2)
        {
            type = messages[0];
            description = messages[1];
        }
        else
        {
            description = messages[0];
        }

        string doubleBreakLine = "\n\n";

        if (description.Contains(doubleBreakLine))
        {
            var descriptions = description.Split(doubleBreakLine);

            description = descriptions[0];
            body = string.Join($"{doubleBreakLine}  ", descriptions[1..]);
        }

        return (type, description, body);
    }

    /// <inheritdoc/>
    public override string ToString() => OriginMessage;
}
