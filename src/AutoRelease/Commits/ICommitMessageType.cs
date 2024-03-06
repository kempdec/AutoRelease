namespace KempDec.AutoRelease.Commits;

/// <summary>
/// Fornece abstração para um tipo de uma mensagem de commit.
/// </summary>
internal interface ICommitMessageType
{
    /// <summary>
    /// Obtém a chave do tipo de uma mensagem de commit.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Obtém o rótulo do tipo de uma mensagem de commit.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Obtém a ordenação do tipo de uma mensagem de commit.
    /// </summary>
    public byte Ordering { get; }
}
