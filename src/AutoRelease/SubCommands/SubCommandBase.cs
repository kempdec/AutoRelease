using KempDec.AutoRelease.SubCommands.Binders;
using KempDec.AutoRelease.SubCommands.Inputs;
using KempDec.AutoRelease.SubCommands.Results;
using System.CommandLine;

namespace KempDec.AutoRelease.SubCommands;

/// <summary>
/// Fornece abstração para um subcomando.
/// </summary>
/// <typeparam name="TBinder">O tipo do ficheiro do subcomando.</typeparam>
/// <typeparam name="TInputs">O tipo das entradas do subcomando.</typeparam>
/// <typeparam name="TIInputs">O tipo da interface das entradas do subcomando.</typeparam>
internal abstract class SubCommandBase<TBinder, TInputs, TIInputs> : Command
    where TBinder : GlobalBinder<TInputs, TIInputs>, new()
    where TInputs : TIInputs, new()
    where TIInputs : IGlobalInputs
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="SubCommandBase{TBinder, TInputs, TIInputs}"/>.
    /// </summary>
    /// <inheritdoc/>
    protected SubCommandBase(string name, string? description = null) : base(name, description)
    {
        var binder = new TBinder();

        binder.AddOptionsTo(command: this);

        this.SetHandler(HandleAsync, binder);
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="SubCommandResult"/> com uma mensagem de erro.
    /// </summary>
    /// <param name="message">A mensagem de erro.</param>
    /// <returns>Uma nova instância de <see cref="SubCommandResult"/> com uma mensagem de erro.</returns>
    protected static SubCommandResult Error(string message) => new(errorMsg: message);

    /// <summary>
    /// Manipula o comando.
    /// </summary>
    /// <param name="inputs">As entradas do subcomando.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    protected virtual async Task HandleAsync(TInputs inputs)
    {
        SubCommandResult result = await HandleResultAsync(inputs);

        if (!result.Succeeded)
        {
            // Define um código de saída diferente de zero para que falhas (por exemplo, no GitHub Actions) sejam
            // sinalizadas corretamente, em vez de o processo encerrar como sucesso.
            Environment.ExitCode = 1;

            if (!string.IsNullOrWhiteSpace(result.Value))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(result.Value))
        {
            await Console.Out.WriteLineAsync(result.Value);
        }
    }

    /// <summary>
    /// Manipula o resultado do comando.
    /// </summary>
    /// <param name="inputs">As entradas do subcomando.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo o resultado do comando.</returns>
    public abstract Task<SubCommandResult> HandleResultAsync(TIInputs inputs);

    /// <summary>
    /// Inicializa uma nova instância de <see cref="SubCommandResult"/> com o valor do resultado bem-sucedido.
    /// </summary>
    /// <param name="value">O valor do resultado bem-sucedido.</param>
    /// <returns>Uma nova instância de <see cref="SubCommandResult"/> com o valor do resultado bem-sucedido.</returns>
    protected static SubCommandResult Succeeded(string value) => new()
    {
        Succeeded = true,
        Value = value
    };
}
