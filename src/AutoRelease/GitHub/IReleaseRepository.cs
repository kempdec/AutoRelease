using KempDec.AutoRelease.Releases;

namespace KempDec.AutoRelease.GitHub;

/// <summary>
/// Fornece abstração para o repositório em que as releases são lidas e criadas.
/// </summary>
internal interface IReleaseRepository
{
    /// <summary>
    /// Obtém o identificador do commit de uma referência (um branch, uma tag ou um commit).
    /// </summary>
    /// <param name="reference">A referência, ou <see langword="null"/> para o branch padrão do repositório.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo o identificador do commit.</returns>
    Task<string> ResolveCommitAsync(string? reference, CancellationToken cancellationToken);

    /// <summary>
    /// Obtém todas as tags do repositório.
    /// </summary>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo as tags.</returns>
    Task<IReadOnlyList<RepositoryTagInfo>> GetTagsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Obtém os commits que um commit tem e outro não: os de <paramref name="headSha"/> desde
    /// <paramref name="baseSha"/>, ou a história inteira de <paramref name="headSha"/> quando não há base.
    /// </summary>
    /// <param name="baseSha">O commit da versão anterior, ou <see langword="null"/> na primeira release.</param>
    /// <param name="headSha">O commit da release.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo os commits, do mais antigo ao mais
    /// novo.</returns>
    Task<IReadOnlyList<ReleaseCommit>> GetCommitsAsync(string? baseSha, string headSha,
        CancellationToken cancellationToken);

    /// <summary>
    /// Diz se já existe uma release para uma tag.
    /// </summary>
    /// <param name="tagName">O nome da tag.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo se a release existe.</returns>
    Task<bool> ReleaseExistsAsync(string tagName, CancellationToken cancellationToken);

    /// <summary>
    /// Cria uma release.
    /// </summary>
    /// <param name="request">O que a release leva.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo a release criada.</returns>
    Task<CreatedRelease> CreateReleaseAsync(NewReleaseRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Representa uma tag do repositório.
/// </summary>
/// <param name="Name">O nome da tag, como <c>v1.2.0</c>.</param>
/// <param name="CommitSha">O identificador do commit para o qual a tag aponta.</param>
internal sealed record RepositoryTagInfo(string Name, string CommitSha);

/// <summary>
/// Representa o pedido de criação de uma release.
/// </summary>
/// <param name="TagName">O nome da tag da release.</param>
/// <param name="Name">O título da release.</param>
/// <param name="Body">As notas da release.</param>
/// <param name="TargetCommitSha">O commit em que a tag nasce, quando ela ainda não existe.</param>
/// <param name="Draft">Se a release é um rascunho.</param>
/// <param name="Prerelease">Se a release é uma pre-release.</param>
internal sealed record NewReleaseRequest(string TagName, string Name, string Body, string TargetCommitSha, bool Draft,
    bool Prerelease);

/// <summary>
/// Representa uma release criada.
/// </summary>
/// <param name="Id">O identificador da release no GitHub.</param>
/// <param name="UploadUrl">O endereço para enviar arquivos à release.</param>
/// <param name="HtmlUrl">O endereço da release no GitHub.</param>
internal sealed record CreatedRelease(long Id, string UploadUrl, string HtmlUrl);
