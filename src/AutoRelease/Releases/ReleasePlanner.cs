using KempDec.AutoRelease.GitHub;
using Semver;

namespace KempDec.AutoRelease.Releases;

/// <summary>
/// Responsável por montar o que uma release leva: o commit, a versão anterior, os commits desde ela, a versão nova e as
/// notas.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="ReleasePlanner"/>.</remarks>
/// <param name="repository">O repositório.</param>
internal sealed class ReleasePlanner(IReleaseRepository repository)
{
    /// <summary>
    /// Monta o que a release leva.
    /// </summary>
    /// <remarks>
    /// O commit é lido uma vez, no começo, e tudo sai dele: o que entra no branch enquanto o Auto Release trabalha não
    /// vai junto. A versão anterior é a maior tag de versão (abaixo da versão informada, quando há uma), e os commits
    /// são os que o commit da release tem e o da tag não.
    /// </remarks>
    /// <param name="request">O que se pede da release.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo o que a release leva.</returns>
    public async Task<ReleasePlan> PlanAsync(ReleasePlanRequest request, CancellationToken cancellationToken)
    {
        string targetSha = await repository.ResolveCommitAsync(request.Reference, cancellationToken);
        IReadOnlyList<RepositoryTagInfo> tags = await repository.GetTagsAsync(cancellationToken);
        RepositoryVersionTag? previous = FindPreviousTag(tags, request.VersionPrefix, request.Version);
        IReadOnlyList<ReleaseCommit> commits =
            await repository.GetCommitsAsync(previous?.CommitSha, targetSha, cancellationToken);

        SemVersion version = request.Version
            ?? VersionCalculator.Next(previous?.Version, commits, request.Rules, request.FirstVersion);
        string tagName = request.VersionPrefix + version;

        return new ReleasePlan
        {
            TargetSha = targetSha,
            PreviousTag = previous,
            Version = version,
            TagName = tagName,
            ExistingTag = tags.FirstOrDefault(tag => tag.Name == tagName),
            Commits = commits,
            Notes = ReleaseNotesBuilder.Build(commits, request.Rules),
            IsEmpty = commits.All(commit => request.Rules.IsIgnored(commit.Message))
        };
    }

    /// <summary>
    /// Procura a tag da versão anterior.
    /// </summary>
    /// <param name="tags">As tags do repositório.</param>
    /// <param name="prefix">O prefixo das versões, como <c>v</c>.</param>
    /// <param name="version">A versão da release, quando informada: a anterior é a maior abaixo dela.</param>
    /// <returns>A tag da versão anterior, ou <see langword="null"/> quando não há.</returns>
    public static RepositoryVersionTag? FindPreviousTag(IEnumerable<RepositoryTagInfo> tags, string prefix,
        SemVersion? version) =>
        tags
            .Select(tag => TryParseTag(tag, prefix))
            .OfType<RepositoryVersionTag>()
            .Where(tag => version is null || tag.Version.ComparePrecedenceTo(version) < 0)
            .OrderByDescending(tag => tag.Version, SemVersion.PrecedenceComparer)
            .FirstOrDefault();

    /// <summary>
    /// Lê uma tag como versão, com o prefixo das versões do projeto.
    /// </summary>
    /// <param name="tag">A tag.</param>
    /// <param name="prefix">O prefixo das versões, como <c>v</c>.</param>
    /// <returns>A tag lida, ou <see langword="null"/> quando ela não é uma versão do projeto.</returns>
    private static RepositoryVersionTag? TryParseTag(RepositoryTagInfo tag, string prefix)
    {
        if (!tag.Name.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return SemVersion.TryParse(tag.Name[prefix.Length..], SemVersionStyles.Strict, out SemVersion? version)
            ? new RepositoryVersionTag(tag.Name, tag.CommitSha, version)
            : null;
    }
}

/// <summary>
/// Representa o que se pede de uma release.
/// </summary>
internal sealed record ReleasePlanRequest
{
    /// <summary>
    /// Obtém ou inicializa a referência da release (um commit ou um branch), ou <see langword="null"/> para o branch
    /// padrão.
    /// </summary>
    public string? Reference { get; init; }

    /// <summary>
    /// Obtém ou inicializa a versão da release, quando ela não deve ser calculada.
    /// </summary>
    public SemVersion? Version { get; init; }

    /// <summary>
    /// Obtém ou inicializa o prefixo das versões.
    /// </summary>
    public string VersionPrefix { get; init; } = "v";

    /// <summary>
    /// Obtém ou inicializa a primeira versão do projeto.
    /// </summary>
    public SemVersion FirstVersion { get; init; } = new(1, 0, 0);

    /// <summary>
    /// Obtém ou inicializa as regras de leitura dos commits.
    /// </summary>
    public ReleaseRules Rules { get; init; } = ReleaseRules.Default;
}
