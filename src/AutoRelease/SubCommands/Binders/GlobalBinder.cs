using KempDec.AutoRelease.Options;
using KempDec.AutoRelease.SubCommands.Inputs;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KempDec.AutoRelease.SubCommands.Binders;

/// <summary>
/// Representa o ficheiro global dos subcomandos.
/// </summary>
/// <typeparam name="TInputs">O tipo das entradas do ficheiro global dos comandos.</typeparam>
/// <typeparam name="TIInputs">O tipo da interface das entradas do ficheiro global dos comandos.</typeparam>
internal class GlobalBinder<TInputs, TIInputs> : BinderBase<TInputs>, IGlobalBinder
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

            command.AddOption(value);
        }
    }

    /// <inheritdoc/>
    protected override TInputs GetBoundValue(BindingContext bindingContext) => new()
    {
        Token = bindingContext.ParseResult.GetValueForOption(TokenOption)!,
        Repo = bindingContext.ParseResult.GetValueForOption(RepoOption),
        Branch = bindingContext.ParseResult.GetValueForOption(BranchOption)
    };
}
