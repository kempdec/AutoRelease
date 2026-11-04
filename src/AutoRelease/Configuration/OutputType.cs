namespace KempDec.AutoRelease.Configuration;

/// <summary>
/// Representa o que o comando <c>create</c> escreve na saída depois de criar a release.
/// </summary>
internal enum OutputType
{
    /// <summary>
    /// O identificador da release no GitHub.
    /// </summary>
    Id,

    /// <summary>
    /// O endereço para enviar arquivos à release.
    /// </summary>
    UploadUrl,

    /// <summary>
    /// A versão da release, sem o prefixo.
    /// </summary>
    Version,

    /// <summary>
    /// A tag da release, com o prefixo.
    /// </summary>
    Tag,

    /// <summary>
    /// O endereço da release no GitHub.
    /// </summary>
    HtmlUrl
}
