using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do nome do projeto.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="ProjectNameOption"/>.</remarks>
internal class ProjectNameOption() : Option<string?>(name: "--project-name", description: "O nome do projeto.")
{
}
