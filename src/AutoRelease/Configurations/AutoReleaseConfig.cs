using KempDec.AutoRelease.Configurations.Attributes;
using KempDec.StarterDotNet.Console;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KempDec.AutoRelease.Configurations;

/// <summary>
/// Associação recursiva das configurações do Auto Release.
/// </summary>
internal partial record AutoReleaseConfig : AppSettingsBase<AutoReleaseConfig>
{
    /// <summary>
    /// A instância estática de <see cref="AutoReleaseConfig"/>.
    /// </summary>
    private static AutoReleaseConfig? s_instance;

    /// <summary>
    /// Obtém a instância estática de <see cref="AutoReleaseConfig"/>.
    /// </summary>
    public static AutoReleaseConfig Instance =>
        s_instance ??= GetInstance(fileName: "autorelease.config.json", optional: true,
            environmentVariablePrefix: "AutoRelease_");

    /// <summary>
    /// Define os argumentos a serem analisados com base nas configurações do Auto Release.
    /// </summary>
    /// <param name="args">Os argumentos a serem analisados.</param>
    /// <returns>Os argumentos a serem analisados que foram definidos com base nas configurações do Auto
    /// Release.</returns>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public string[] SetArgs(string[] args)
    {
        var resultArgs = new List<string>(args);

        PropertyInfo[] properties = GetType().GetProperties();

        foreach (PropertyInfo property in properties)
        {
            string[]? propertyArgs = GetCommandArgs(args, property) ?? GetOptionArgs(args, property, obj: this);

            if (propertyArgs is { Length: > 0 })
            {
                resultArgs.AddRange(propertyArgs);
            }
        }

        return [.. resultArgs];
    }

    /// <summary>
    /// Obtém os argumentos de comando da propriedade especificada.
    /// </summary>
    /// <remarks>Para o método obter os argumentos, a propriedade deve estar decorada com o atributo
    /// <see cref="ConfigCommandAttribute"/>. Caso os argumentos para o comando já exista em <paramref name="args"/>,
    /// <see langword="null"/> será retornado.</remarks>
    /// <param name="args">Os argumentos a serem analisados.</param>
    /// <param name="property">A propriedade a obter os argumentos.</param>
    /// <returns>Os argumentos de comando da propriedade especificada.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    private string[]? GetCommandArgs(string[] args, PropertyInfo property)
    {
        object? propertyValue = property.GetValue(obj: this);

        if (propertyValue is null)
        {
            return null;
        }

        ConfigCommandAttribute? commandAttr = property.GetCustomAttribute<ConfigCommandAttribute>(inherit: false);

        if (commandAttr is null)
        {
            return null;
        }

        Command command = Activator.CreateInstance(commandAttr.Type) as Command
            ?? throw new InvalidOperationException($"Não foi possível converter a propriedade de configuração " +
                $"{property.Name} para o tipo de um comando.");

        // Verifica se nos argumentos NÃO foi definido o comando.
        if (!command.Aliases.Any(args.Contains))
        {
            return null;
        }

        var resultArgs = new List<string>();

        PropertyInfo[] commandProperties = property.PropertyType.GetProperties();

        foreach (PropertyInfo commandProperty in commandProperties)
        {
            string[]? commandPropertyArgs = GetOptionArgs(args, commandProperty, obj: propertyValue);

            if (commandPropertyArgs is { Length: > 0 })
            {
                resultArgs.AddRange(commandPropertyArgs);
            }
        }

        return resultArgs.Count > 0 ? [.. resultArgs] : null;
    }

    /// <summary>
    /// Obtém os argumentos de opção da propriedade especificada.
    /// </summary>
    /// <remarks>Para o método obter os argumentos, a propriedade deve estar decorada com o atributo
    /// <see cref="ConfigOptionAttribute"/>. Caso os argumentos para a opção já exista em <paramref name="args"/>,
    /// <see langword="null"/> será retornado.</remarks>
    /// <param name="args">Os argumentos a serem analisados.</param>
    /// <param name="property">A propriedade ao qual os argumentos serão obtidos.</param>
    /// <param name="obj">O objeto da propriedade ao qual os argumentos serão obtidos.</param>
    /// <returns>Os argumentos de opção da propriedade especificada.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    private static string[]? GetOptionArgs(string[] args, PropertyInfo property, object obj)
    {
        ConfigOptionAttribute? optionAttr = property.GetCustomAttribute<ConfigOptionAttribute>(inherit: false);

        if (optionAttr is null)
        {
            return null;
        }

        Option option = Activator.CreateInstance(optionAttr.Type) as Option
            ?? throw new InvalidOperationException($"Não foi possível converter a propriedade de configuração " +
                $"{property.Name} para o tipo da opção de um comando.");

        // Verifica se nos argumentos já foi definida a opção.
        if (option.Aliases.Any(args.Contains))
        {
            return null;
        }

        object? propertyValue = property.GetValue(obj);

        if (propertyValue is Dictionary<string, string> dictionary)
        {
            if (dictionary.Count is 0)
            {
                return null;
            }

            return [option.Aliases.First(), .. dictionary.Select(e => $"{e.Key}={e.Value}")];
        }
        else if (propertyValue is List<string> list)
        {
            if (list.Count is 0)
            {
                return null;
            }

            return [option.Aliases.First(), .. list];
        }

        string? resultValue = propertyValue?.ToString();

        if (string.IsNullOrWhiteSpace(resultValue))
        {
            return null;
        }

        return [option.Aliases.First(), resultValue];
    }
}
