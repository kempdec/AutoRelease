namespace KempDec.AutoRelease.SubCommands.Models;

/// <summary>
/// Representa o tipo de saída da criação automática do release.
/// </summary>
internal enum CreateOutputType
{
    /// <summary>
    /// O identificador do release.
    /// </summary>
    Id,

    /// <summary>
    /// A URL de upload do release.
    /// </summary>
    UploadUrl,

    /// <summary>
    /// A versão do release.
    /// </summary>
    Version
}
