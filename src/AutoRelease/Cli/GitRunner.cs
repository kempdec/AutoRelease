using System.ComponentModel;
using System.Diagnostics;
using KempDec.AutoRelease.Errors;

namespace KempDec.AutoRelease.Cli;

/// <summary>
/// Classe com métodos auxiliares para rodar o Git na pasta atual.
/// </summary>
internal static class GitRunner
{
    /// <summary>
    /// Roda o Git com os argumentos informados.
    /// </summary>
    /// <param name="arguments">Os argumentos, sem o <c>git</c>.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo a saída do Git.</returns>
    /// <exception cref="AutoReleaseException">Quando o Git não está instalado ou termina com erro.</exception>
    public static async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        Process process;

        try
        {
            process = Process.Start(startInfo)
                ?? throw new AutoReleaseException("O Git não abriu.", "Confira se o Git está instalado e no PATH.");
        }
        catch (Win32Exception exception)
        {
            throw new AutoReleaseException(
                "O Git não foi encontrado.",
                "Instale o Git e confira se ele está no PATH, ou passe as mensagens com --message ou --file.",
                exception);
        }

        using (process)
        {
            Task<string> output = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> error = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new AutoReleaseException(
                    $"O Git recusou o pedido: {(await error).Trim()}",
                    "Confira se o comando roda dentro do repositório e se os commits de --from e --to existem "
                    + "(no GitHub Actions, faça o checkout com fetch-depth: 0).");
            }

            return await output;
        }
    }
}
