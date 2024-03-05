using System.CommandLine.Parsing;

namespace KempDec.AutoRelease.Extensions;

/// <summary>
/// Classe com métodos extensivos para <see cref="ArgumentResult"/>.
/// </summary>
internal static class ArgumentResultExtension
{
    public static string? GetSingleTokenOrDefault(this ArgumentResult result) => result.Tokens.SingleOrDefault()?.Value;
}
