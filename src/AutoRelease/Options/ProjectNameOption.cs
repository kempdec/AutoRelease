using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do nome do projeto.
/// </summary>
internal class ProjectNameOption : Option<string?>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="ProjectNameOption"/>.
    /// </summary>
    public ProjectNameOption() : base("--project-name") =>
        Description = "O nome do projeto.";
}
