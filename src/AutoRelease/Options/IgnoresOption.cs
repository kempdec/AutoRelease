using System.CommandLine;

namespace KempDec.AutoRelease.Options;

/// <summary>
/// Representa a opção dos inícios das mensagens de commit que serão ignoradas.
/// </summary>
internal class IgnoresOption : Option<List<string>>
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="IgnoresOption"/>.
    /// </summary>
    public IgnoresOption() : base(
        name: "--ignores",
        description: "Os inícios das mensagens de commit que serão ignoradas.") =>
            AllowMultipleArgumentsPerToken = true;
}
