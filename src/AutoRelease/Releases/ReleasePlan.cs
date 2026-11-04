using KempDec.AutoRelease.GitHub;
using Semver;

namespace KempDec.AutoRelease.Releases;

/// <summary>
/// Representa o que uma release leva, antes de ser criada.
/// </summary>
internal sealed record ReleasePlan
{
    /// <summary>
    /// Obtém o commit da release.
    /// </summary>
    public required string TargetSha { get; init; }

    /// <summary>
    /// Obtém a tag da versão anterior, ou <see langword="null"/> na primeira release.
    /// </summary>
    public RepositoryVersionTag? PreviousTag { get; init; }

    /// <summary>
    /// Obtém a versão da release.
    /// </summary>
    public required SemVersion Version { get; init; }

    /// <summary>
    /// Obtém a tag da release, com o prefixo.
    /// </summary>
    public required string TagName { get; init; }

    /// <summary>
    /// Obtém a tag da release quando ela já existe no repositório, como no fluxo em que a pessoa cria a tag e o Auto
    /// Release cria a release dela.
    /// </summary>
    public RepositoryTagInfo? ExistingTag { get; init; }

    /// <summary>
    /// Obtém os commits desde a versão anterior, do mais antigo ao mais novo.
    /// </summary>
    public required IReadOnlyList<ReleaseCommit> Commits { get; init; }

    /// <summary>
    /// Obtém as notas da release em Markdown.
    /// </summary>
    public required string Notes { get; init; }

    /// <summary>
    /// Obtém um sinalizador indicando se não há nada para anunciar: nenhum commit desde a versão anterior, ou todos
    /// ignorados.
    /// </summary>
    public required bool IsEmpty { get; init; }
}

/// <summary>
/// Representa uma tag do repositório lida como versão.
/// </summary>
/// <param name="Name">O nome da tag, como <c>v1.2.0</c>.</param>
/// <param name="CommitSha">O commit para o qual a tag aponta.</param>
/// <param name="Version">A versão lida da tag.</param>
internal sealed record RepositoryVersionTag(string Name, string CommitSha, SemVersion Version);
