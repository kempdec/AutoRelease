using System.Diagnostics.CodeAnalysis;

namespace KempDec.AutoRelease.SubCommands.Results;

/// <summary>
/// Representa o resultado de um subcomando.
/// </summary>
internal class SubCommandResult
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="SubCommandResult"/>.
    /// </summary>
    public SubCommandResult()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="SubCommandResult"/>.
    /// </summary>
    /// <param name="errorMsg">Uma mensagem de erro.</param>
    [SetsRequiredMembers]
    public SubCommandResult(string errorMsg)
    {
        Succeeded = false;
        Value = errorMsg;
    }

    /// <summary>
    /// Obtém ou inicializa um sinalizador indicando se o resultado foi bem-sucedido.
    /// </summary>
    public required bool Succeeded { get; init; }

    /// <summary>
    /// Obtém ou inicializa o valor do resultado.
    /// </summary>
    public required string? Value { get; init; }
}
