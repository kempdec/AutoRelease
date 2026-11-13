using System.Reflection;

namespace KempDec.AutoRelease;

/// <summary>
/// Classe com métodos auxiliares para as informações da própria ferramenta.
/// </summary>
internal static class AutoReleaseInfo
{
    /// <summary>
    /// Obtém a versão da ferramenta, sem os metadados de compilação.
    /// </summary>
    public static string Version { get; } = ReadVersion();

    /// <summary>
    /// Obtém o endereço em que os problemas da ferramenta são relatados.
    /// </summary>
    public static string IssuesUrl => "https://github.com/kempdec/AutoRelease/issues";

    /// <summary>
    /// Lê a versão da ferramenta da montagem.
    /// </summary>
    /// <returns>A versão, como <c>2.0.0</c>.</returns>
    private static string ReadVersion()
    {
        string? version = typeof(AutoReleaseInfo).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        return version?.Split('+')[0] ?? "0.0.0";
    }
}
