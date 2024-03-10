using KempDec.AutoRelease.SubCommands.Models;
using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do tipo de saída da criação automática do release.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="CreateOutputTypeOption"/>.</remarks>
internal class CreateOutputTypeOption() : Option<CreateOutputType>(
    name: "--output-type",
    description: "O tipo de saída da criação automática do release.",
    getDefaultValue: () => CreateOutputType.Id)
{
}
