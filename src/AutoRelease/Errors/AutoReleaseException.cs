namespace KempDec.AutoRelease.Errors;

/// <summary>
/// Representa um erro esperado do Auto Release, que a pessoa consegue resolver: ele diz o que aconteceu e qual é o
/// próximo passo, sem pilha de chamadas nem nome de classe.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="AutoReleaseException"/>.</remarks>
/// <param name="problem">O que aconteceu, numa frase.</param>
/// <param name="nextStep">O que a pessoa pode fazer a seguir, numa frase.</param>
/// <param name="innerException">A exceção que causou o erro, se houver.</param>
internal sealed class AutoReleaseException(string problem, string nextStep, Exception? innerException = null)
    : Exception($"{problem} {nextStep}", innerException)
{
    /// <summary>
    /// Obtém o que aconteceu.
    /// </summary>
    public string Problem { get; } = problem;

    /// <summary>
    /// Obtém o que a pessoa pode fazer a seguir.
    /// </summary>
    public string NextStep { get; } = nextStep;
}
