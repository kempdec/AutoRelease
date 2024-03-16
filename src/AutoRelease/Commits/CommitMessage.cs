using KempDec.AutoRelease.Commits.Types;
using KempDec.AutoRelease.SubCommands;
using KempDec.AutoRelease.SubCommands.Inputs;
using KempDec.StarterDotNet.Reflection;
using Octokit;

namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Representa uma mensagem de commit.
/// </summary>
internal class CommitMessage
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="CommitMessage"/>.
    /// </summary>
    /// <param name="commit">O commit.</param>
    /// <param name="inputs">As entradas do subcomando <see cref="NoteSubCommand"/>.</param>
    public CommitMessage(Commit commit, INoteSubCommandInputs inputs)
        : this(commit.Message, inputs.Types, inputs.Replaces, inputs.Ignores,
            inputs.ShowAuthor ? commit.Author.Name : null)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="CommitMessage"/>.
    /// </summary>
    /// <param name="message">A mensagem de commit.</param>
    /// <param name="types">Os tipos de mensagem de commit.</param>
    /// <param name="replaces">As substituições do início das mensagens de commit.</param>
    /// <param name="ignores">Os inícios das mensagens de commit que serão ignoradas.</param>
    /// <param name="authorName">O nome do autor da mensagem de commit.</param>
    public CommitMessage(string message, List<CommitMessageType>? types = null,
        List<(string OldValue, string NewValue)>? replaces = null, List<string>? ignores = null,
        string? authorName = null)
    {
        (string? type, string description, string? body) = SplitMessage(message);

        bool useBodyAsReleaseDescription = false;

        if (type is not null)
        {
            switch (type.Last())
            {
                case '#':
                    Ignore = true;
                    break;

                case '^':
                    useBodyAsReleaseDescription = true;
                    break;

                case '!':
                    IsBreakingChange = true;
                    break;
            }
        }

        ICommitMessageType? commitType = null;

        if (commitType is null && type is not null)
        {
            commitType = types is { Count: > 0 } ? types.SingleOrDefault(e => e.Key == type) : GetType(type);
        }

        OriginMessage = message;
        Type = commitType ?? new DefaultCommitMessageType();
        Description = description;
        Body = body;
        Ignore = Ignore || ignores?.Any(Description.StartsWith) is true;

        if (useBodyAsReleaseDescription && body is not null)
        {
            ReleaseDescription = body;
        }
        else
        {
            ReleaseDescription = Description;

            if (body is not null)
            {
                ReleaseDescription += DoubleHtmlBreakLine + body;
            }
        }

        if (!string.IsNullOrWhiteSpace(authorName))
        {
            ReleaseDescription += $" [{authorName}]";
        }

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
    /// A quebra de linha dupla em HTML.
    /// </summary>
    private const string DoubleHtmlBreakLine = "<br><br>";

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
    /// Obtém um sinalizador indicando se a mensagem de commit traz uma quebra de compatibilidade.
    /// </summary>
    public bool IsBreakingChange { get; }

    /// <summary>
    /// Obtém um sinalizador indicando se a mensagem de commit deve ser ignorada.
    /// </summary>
    public bool Ignore { get; }

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
        string[] lines = message.Split("\n\n");
        string[] messages = lines[0].Split(": ");

        (string? type, string description) = messages switch
        {
            { Length: >= 2 } => (messages[0], messages[1]),
            _ => (null, messages[0])
        };

        string? body = lines.Length > 1 ? string.Join(DoubleHtmlBreakLine, lines[1..]) : null;

        return (type, description, body);
    }

    /// <inheritdoc/>
    public override string ToString() => OriginMessage;
}
