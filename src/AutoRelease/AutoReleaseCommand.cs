using KempDec.AutoRelease.Configurations;
using KempDec.AutoRelease.SubCommands;
using System.CommandLine;

namespace KempDec.AutoRelease;

/// <summary>
/// Responsável pelo comando de geração automática de uma release.
/// </summary>
internal class AutoReleaseCommand : RootCommand
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="AutoReleaseCommand"/>.
    /// </summary>
    public AutoReleaseCommand() : base(
        description: "Gere releases automaticamente do seu projeto no GitHub usando o Auto Release.")
    {
        AddCommand(new NoteSubCommand());
        AddCommand(new VersionSubCommand());
    }

    /// <summary>
    /// Analisa e invoca o comando de geração automática de uma release.
    /// </summary>
    /// <param name="args">Os argumentos a serem analisados.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    public static Task InvokeAsync(string[] args)
    {
        if (args.Length > 0)
        {
            args = AutoReleaseConfig.Instance.SetArgs(args);
        }

        return new AutoReleaseCommand().InvokeAsync(args);
    }
}
