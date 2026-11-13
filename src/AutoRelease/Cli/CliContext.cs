using System.Collections;
using KempDec.AutoRelease.GitHub;

namespace KempDec.AutoRelease.Cli;

/// <summary>
/// Representa o que a linha de comando usa do mundo de fora: as saídas, as variáveis de ambiente, a pasta, o acesso
/// ao GitHub, o relógio e o Git. Os testes trocam cada um.
/// </summary>
internal sealed record CliContext
{
    /// <summary>
    /// Obtém ou inicializa a saída do resultado, que um script pode ler.
    /// </summary>
    public required TextWriter Output { get; init; }

    /// <summary>
    /// Obtém ou inicializa a saída dos avisos e dos erros.
    /// </summary>
    public required TextWriter Error { get; init; }

    /// <summary>
    /// Obtém ou inicializa as variáveis de ambiente.
    /// </summary>
    public required IReadOnlyDictionary<string, string?> Environment { get; init; }

    /// <summary>
    /// Obtém ou inicializa a pasta em que o comando roda.
    /// </summary>
    public required string WorkingDirectory { get; init; }

    /// <summary>
    /// Obtém ou inicializa o que cria o acesso ao repositório, a partir do token, do dono, do nome e do endereço da
    /// API.
    /// </summary>
    public required Func<string, string, string, string?, IReleaseRepository> CreateRepository { get; init; }

    /// <summary>
    /// Obtém ou inicializa o relógio, para a data no título da release.
    /// </summary>
    public required TimeProvider Clock { get; init; }

    /// <summary>
    /// Obtém ou inicializa o que roda o Git na pasta e devolve a saída dele, para o comando <c>lint</c> ler um
    /// intervalo de commits.
    /// </summary>
    public required Func<IReadOnlyList<string>, CancellationToken, Task<string>> RunGit { get; init; }

    /// <summary>
    /// Cria o contexto do processo: o console, o ambiente, a pasta atual, o GitHub de verdade e o relógio do sistema.
    /// </summary>
    /// <returns>O contexto.</returns>
    public static CliContext FromProcess() => new()
    {
        Output = Console.Out,
        Error = Console.Error,
        Environment = System.Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => entry.Value as string,
                OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal),
        WorkingDirectory = Directory.GetCurrentDirectory(),
        CreateRepository = GitHubReleaseRepository.Create,
        Clock = TimeProvider.System,
        RunGit = GitRunner.RunAsync
    };

    /// <summary>
    /// Lê uma variável de ambiente.
    /// </summary>
    /// <param name="name">O nome da variável.</param>
    /// <returns>O valor, ou <see langword="null"/> quando ela não existe ou está vazia.</returns>
    public string? GetEnvironment(string name) =>
        Environment.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
}
