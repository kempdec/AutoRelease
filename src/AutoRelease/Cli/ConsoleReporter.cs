using KempDec.AutoRelease.Errors;

namespace KempDec.AutoRelease.Cli;

/// <summary>
/// Classe com métodos extensivos para escrever os avisos e os erros na saída de erros.
/// </summary>
internal static class ConsoleReporter
{
    /// <summary>
    /// Escreve um erro em duas partes: o que aconteceu e o próximo passo.
    /// </summary>
    /// <param name="writer">A saída de erros.</param>
    /// <param name="exception">O erro.</param>
    public static void WriteError(this TextWriter writer, AutoReleaseException exception)
    {
        writer.WriteLine($"Erro: {exception.Problem}");
        writer.WriteLine(exception.NextStep);
    }

    /// <summary>
    /// Escreve um aviso em duas partes: o que pede atenção e o que fazer.
    /// </summary>
    /// <param name="writer">A saída de erros.</param>
    /// <param name="problem">O que pede atenção.</param>
    /// <param name="nextStep">O que fazer.</param>
    public static void WriteWarning(this TextWriter writer, string problem, string nextStep)
    {
        writer.WriteLine($"Aviso: {problem}");
        writer.WriteLine(nextStep);
    }
}
