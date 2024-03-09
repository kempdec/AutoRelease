using System.CommandLine;

namespace KempDec.AutoRelease.Configurations.Attributes;

/// <summary>
/// Define que uma propriedade de configuração é uma opção de um comando do tipo especificado.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="ConfigOptionAttribute{T}"/>.</remarks>
/// <typeparam name="T">O tipo da opção de um comando da propriedade de configuração.</typeparam>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal class ConfigOptionAttribute<T>() : ConfigOptionAttribute(typeof(T)) where T : Option
{
}

/// <summary>
/// Define que uma propriedade de configuração é uma opção de um comando do tipo especificado.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="ConfigOptionAttribute"/>.</remarks>
/// <param name="type">O tipo da opção de um comando da propriedade de configuração.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal class ConfigOptionAttribute(Type type) : Attribute()
{
    /// <summary>
    /// Obtém o tipo da opção de um comando da propriedade de configuração.
    /// </summary>
    public Type Type { get; } = type;
}
