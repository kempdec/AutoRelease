using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Inputs;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Representa o ficheiro global dos subcomandos.
/// </summary>
/// <typeparam name="TInputs">O tipo das entradas do ficheiro global dos comandos.</typeparam>
/// <typeparam name="TIInputs">O tipo da interface das entradas do ficheiro global dos comandos.</typeparam>
internal class GlobalBinder<TInputs, TIInputs> : IGlobalBinder
    where TInputs : TIInputs, new()
    where TIInputs : IGlobalInputs
{
    /// <inheritdoc/>
    public TokenOption TokenOption { get; init; } = new();

    /// <inheritdoc/>
    public RepoOption RepoOption { get; init; } = new();

    /// <inheritdoc/>
    public BranchOption BranchOption { get; init; } = new();

    /// <inheritdoc/>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public void AddOptionsTo(Command command)
    {
        PropertyInfo[] properties = GetType().GetProperties();

        foreach (PropertyInfo property in properties)
        {
            if (property.GetValue(obj: this) is not Option value)
            {
                continue;
            }

            command.Options.Add(value);
        }
    }

    /// <summary>
    /// Obtém as entradas do subcomando a partir do resultado da análise especificado.
    /// </summary>
    /// <param name="parseResult">O resultado da análise dos argumentos.</param>
    /// <returns>As entradas do subcomando.</returns>
    public virtual TInputs GetInputs(ParseResult parseResult) => new()
    {
        Token = parseResult.GetValue(TokenOption)!,
        Repo = parseResult.GetValue(RepoOption),
        Branch = parseResult.GetValue(BranchOption)
    };
}
