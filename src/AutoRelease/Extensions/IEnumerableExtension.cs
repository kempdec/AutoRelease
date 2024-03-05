namespace KempDec.AutoRelease.Extensions;

/// <summary>
/// Classe com métodos extensivos para <see cref="IEnumerable{T}"/>.
/// </summary>
internal static class IEnumerableExtension
{
    /// <summary>
    /// Destrói um enumerador em seus elementos individuais, retornando o primeiro e segundo elementos.
    /// </summary>
    /// <remarks>Este método é útil quando você deseja dividir um enumerador em seus elementos individuais, permitindo
    /// um fácil acesso ao primeiro e segundo elementos.</remarks>
    /// <typeparam name="T">O tipo dos elementos no enumerador.</typeparam>
    /// <param name="list">O enumerador a ser destruído.</param>
    /// <param name="first">O primeiro elemento do enumerador, ou o valor padrão de <typeparamref name="T"/> se a lista
    /// tiver menos de 1 elemento.</param>
    /// <param name="second">O segundo elemento no enumerador, ou o valor padrão de <typeparamref name="T"/> se a lista
    /// tiver menos de 2 elementos.</param>
    public static void Deconstruct<T>(this IEnumerable<T>? list, out T? first, out T? second)
    {
        if (list is null)
        {
            first = default;
            second = default;

            return;
        }

        first = list.ElementAtOrDefault(0);
        second = list.ElementAtOrDefault(1);
    }
}
