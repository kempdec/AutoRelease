using KempDec.AutoRelease.Configurations.Attributes;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KempDec.AutoRelease.Configurations;

/// <summary>
/// Associação recursiva das configurações do Auto Release.
/// </summary>
internal partial record AutoReleaseConfig
{
    /// <summary>
    /// A instância estática de <see cref="AutoReleaseConfig"/>.
    /// </summary>
    private static AutoReleaseConfig? s_instance;

    /// <summary>
    /// Obtém a instância estática de <see cref="AutoReleaseConfig"/>.
    /// </summary>
    public static AutoReleaseConfig Instance
    {
        get
        {
            if (s_instance is null)
            {
                string configPath = Path.Combine(Environment.CurrentDirectory, "autorelease.config.json");

                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .AddJsonFile(configPath, optional: true)
                    .AddEnvironmentVariables(prefix: "AutoRelease_")
                    .Build();

                s_instance = configuration.Get<AutoReleaseConfig>() ?? new AutoReleaseConfig();
            }

            return s_instance;
        }
    }

    /// <summary>
    /// Define os argumentos a serem analisados com base nas configurações do Auto Release.
    /// </summary>
    /// <param name="args">Os argumentos a serem analisados.</param>
    /// <returns>Os argumentos a serem analisados que foram definidos com base nas configurações do Auto
    /// Release.</returns>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public string[] SetArgs(string[] args)
    {
        var arguments = new List<string>(args);

        PropertyInfo[] properties = GetType().GetProperties();

        foreach (PropertyInfo property in properties)
        {
            ConfigOptionAttribute? optionAttr = property.GetCustomAttribute<ConfigOptionAttribute>(inherit: false);

            if (optionAttr is null)
            {
                continue;
            }

            Option option = Activator.CreateInstance(optionAttr.Type) as Option
                ?? throw new InvalidOperationException($"Não foi possível converter a propriedade de configuração " +
                    $"{property.Name} para o tipo de um comando.");

            // Verifica se nos argumentos já foi definida a opção.
            if (option.Aliases.Any(args.Contains))
            {
                continue;
            }

            string? value = property.GetValue(obj: this)?.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            arguments.Add(option.Aliases.First());
            arguments.Add(value);
        }

        return [.. arguments];
    }
}
