using KempDec.AutoRelease.SubCommands.Models;
using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção do tipo de saída da criação automática do release.
/// </summary>
internal class CreateOutputTypeOption : Option<CreateOutputType>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="CreateOutputTypeOption"/>.
    /// </summary>
    public CreateOutputTypeOption() : base("--output-type")
    {
        Description = "O tipo de saída da criação automática do release.";
        DefaultValueFactory = _ => CreateOutputType.Id;
    }
}
