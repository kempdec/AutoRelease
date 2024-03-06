﻿using KempDec.AutoRelease.Commits.Types;
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
    public CommitMessage(string message, List<CommitMessageType>? types = null)
    {
        OriginMessage = message;

        var messages = message.Split(": ");

        if (messages.Length >= 2)
        {
            ICommitMessageType? type = types is { Count: > 0 }
                ? types.OrderBy(e => e.Ordering).SingleOrDefault(e => e.Key == messages[0])
                : GetType(messages[0]);

            Type = type ?? new DefaultCommitMessageType();
            Description = messages[1];
        }
        else
        {
            Type = new DefaultCommitMessageType();
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
    public ICommitMessageType Type { get; }

    /// <summary>
    /// Obtém a descrição da mensagem de commit.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Obtém a representação do tipo da mensagem de commit especificado.
    /// </summary>
    /// <param name="type">O tipo da mensagem de commit.</param>
    /// <returns>A representação do tipo da mensagem de commit especificado.</returns>
    private static ICommitMessageType? GetType(string type)
    {
        IEnumerable<ICommitMessageType?> types = AssemblyHelper.GetAllClassesWithInterface<ICommitMessageType>();

        return types.OrderBy(e => e?.Ordering).SingleOrDefault(e => e?.Key == type);
    }

    /// <inheritdoc/>
    public override string ToString() => OriginMessage;
}
