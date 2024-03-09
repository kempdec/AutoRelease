using System.CommandLine;

namespace KempDec.AutoRelease.Configurations.Attributes;

/// <summary>
/// Define que uma propriedade de configuração é um comando do tipo especificado.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="ConfigCommandAttribute{T}"/>.</remarks>
/// <typeparam name="T">O tipo do comando da propriedade de configuração.</typeparam>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal class ConfigCommandAttribute<T>() : ConfigCommandAttribute(typeof(T)) where T : Command
{
}

/// <summary>
/// Define que uma propriedade de configuração é um comando do tipo especificado.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="ConfigOptionAttribute"/>.</remarks>
/// <param name="type">O tipo do comando da propriedade de configuração.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal class ConfigCommandAttribute(Type type) : Attribute()
{
    /// <summary>
    /// Obtém o tipo do comando da propriedade de configuração.
    /// </summary>
    public Type Type { get; } = type;
}
