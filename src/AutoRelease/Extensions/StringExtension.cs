using Semver;
using System.Text.RegularExpressions;

namespace KempDec.AutoRelease.Extensions;

/// <summary>
/// Classe com métodos extensivos para <see cref="string"/>.
/// </summary>
internal static class StringExtension
{
    /// <summary>
    /// Converte a versão especificada para uma versão semântica.
    /// </summary>
    /// <param name="version">A versão a ser convertida.</param>
    /// <returns>A versão semânticada da versão especificada.</returns>
    public static SemVersion? ToSemVersion(this string version)
    {
        // Remove qualquer caractere anterior ao primeiro digito da versão. Usando como exemplo "v1.0.0", o "v" será
        // removido, restando apenas "1.0.0".
        version = Regex.Replace(version, @"^\D*", string.Empty);

        SemVersion.TryParse(version, SemVersionStyles.Strict, out SemVersion? semVersion);

        return semVersion;
    }
}
