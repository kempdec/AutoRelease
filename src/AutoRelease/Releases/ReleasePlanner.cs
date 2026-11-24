using System.Globalization;
using System.Numerics;
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
    /// vai junto. Os commits são os que o commit da release tem e o da tag anterior não.
    /// <para>
    /// Numa versão final, a anterior é a maior versão final (abaixo da versão informada, quando há uma), e as
    /// pre-releases ficam de fora: a <c>1.5.0</c> leva as notas desde a <c>1.4.0</c>, com tudo o que veio nas
    /// <c>1.5.0-beta.N</c>, e a conta da versão parte da <c>1.4.0</c>. Numa pre-release com o rótulo de
    /// <see cref="ReleasePlanRequest.PreReleaseLabel"/>, a versão é a final que sairia agora com o rótulo e o número
    /// seguinte (<c>1.5.0-beta.2</c> depois da <c>1.5.0-beta.1</c>), e as notas vêm desde a tag anterior, seja ela
    /// pre-release ou final.
    /// </para>
    /// </remarks>
    /// <param name="request">O que se pede da release.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo o que a release leva.</returns>
    public async Task<ReleasePlan> PlanAsync(ReleasePlanRequest request, CancellationToken cancellationToken)
    {
        string targetSha = await repository.ResolveCommitAsync(request.Reference, cancellationToken);
        IReadOnlyList<RepositoryTagInfo> tags = await repository.GetTagsAsync(cancellationToken);
        List<RepositoryVersionTag> versions = [.. tags
            .Select(tag => TryParseTag(tag, request.VersionPrefix))
            .OfType<RepositoryVersionTag>()];

        SemVersion version;
        RepositoryVersionTag? previous;
        IReadOnlyList<ReleaseCommit> commits;

        if (request.Version is { } informed && (informed.IsPrerelease || request.PreReleaseLabel is null))
        {
            version = informed;
            previous = FindPreviousTag(versions, informed, stableOnly: !informed.IsPrerelease);
            commits = await repository.GetCommitsAsync(previous?.CommitSha, targetSha, cancellationToken);
        }
        else
        {
            RepositoryVersionTag? stable = FindPreviousTag(versions, request.Version, stableOnly: true);
            IReadOnlyList<ReleaseCommit> stableCommits =
                await repository.GetCommitsAsync(stable?.CommitSha, targetSha, cancellationToken);
            SemVersion final = request.Version ?? NextFinal(stable, stableCommits, versions, request);

            if (request.PreReleaseLabel is not { } label)
            {
                version = final;
                previous = stable;
                commits = stableCommits;
            }
            else
            {
                version = NextPreRelease(final, label, versions);
                previous = FindPreviousTag(versions, version, stableOnly: false);
                commits = previous?.CommitSha == stable?.CommitSha
                    ? stableCommits
                    : await repository.GetCommitsAsync(previous?.CommitSha, targetSha, cancellationToken);
            }
        }

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
        FindPreviousTag(tags.Select(tag => TryParseTag(tag, prefix)).OfType<RepositoryVersionTag>(), version,
            stableOnly: false);

    /// <summary>
    /// Procura a tag da versão anterior entre as tags de versão.
    /// </summary>
    /// <param name="versions">As tags de versão do repositório.</param>
    /// <param name="version">A versão da release, quando informada: a anterior é a maior abaixo dela.</param>
    /// <param name="stableOnly">Se só as versões finais contam, deixando as pre-releases de fora.</param>
    /// <returns>A tag da versão anterior, ou <see langword="null"/> quando não há.</returns>
    private static RepositoryVersionTag? FindPreviousTag(IEnumerable<RepositoryVersionTag> versions,
        SemVersion? version, bool stableOnly) =>
        versions
            .Where(tag => !stableOnly || !tag.Version.IsPrerelease)
            .Where(tag => version is null || tag.Version.ComparePrecedenceTo(version) < 0)
            .OrderByDescending(tag => tag.Version, SemVersion.PrecedenceComparer)
            .FirstOrDefault();

    /// <summary>
    /// Calcula a próxima versão final a partir da última versão final.
    /// </summary>
    /// <remarks>
    /// Uma pre-release acima da última final (como a <c>2.0.0-beta.1</c> depois da <c>1.4.0</c>) já anunciou a versão
    /// que vem, então a final não fica abaixo dela: sai a <c>2.0.0</c>, ou mais, se os commits pedirem.
    /// </remarks>
    /// <param name="stable">A tag da última versão final, ou <see langword="null"/> quando não há.</param>
    /// <param name="commits">Os commits desde ela.</param>
    /// <param name="versions">As tags de versão do repositório.</param>
    /// <param name="request">O que se pede da release.</param>
    /// <returns>A próxima versão final.</returns>
    private static SemVersion NextFinal(RepositoryVersionTag? stable, IReadOnlyList<ReleaseCommit> commits,
        IEnumerable<RepositoryVersionTag> versions, ReleasePlanRequest request)
    {
        SemVersion next = VersionCalculator.Next(stable?.Version, commits, request.Rules, request.FirstVersion);
        SemVersion? announced = versions
            .Where(tag => tag.Version.IsPrerelease)
            .Where(tag => stable is null || tag.Version.ComparePrecedenceTo(stable.Version) > 0)
            .Select(tag => new SemVersion(tag.Version.Major, tag.Version.Minor, tag.Version.Patch))
            .OrderByDescending(candidate => candidate, SemVersion.PrecedenceComparer)
            .FirstOrDefault();

        return announced is not null && announced.ComparePrecedenceTo(next) > 0 ? announced : next;
    }

    /// <summary>
    /// Calcula a próxima pre-release de uma versão final: o rótulo e o número seguinte ao maior que já existe.
    /// </summary>
    /// <param name="final">A versão final, como <c>1.5.0</c>.</param>
    /// <param name="label">O rótulo, como <c>beta</c>.</param>
    /// <param name="versions">As tags de versão do repositório.</param>
    /// <returns>A pre-release, como <c>1.5.0-beta.1</c> ou, depois dela, <c>1.5.0-beta.2</c>.</returns>
    private static SemVersion NextPreRelease(SemVersion final, string label, IEnumerable<RepositoryVersionTag> versions)
    {
        BigInteger last = versions
            .Select(tag => tag.Version)
            .Where(version => version.Major == final.Major && version.Minor == final.Minor
                && version.Patch == final.Patch)
            .Where(version => version.PrereleaseIdentifiers.Count == 2
                && version.PrereleaseIdentifiers[0].Value == label
                && version.PrereleaseIdentifiers[1].NumericValue is not null)
            .Select(version => version.PrereleaseIdentifiers[1].NumericValue!.Value)
            .DefaultIfEmpty(BigInteger.Zero)
            .Max();

        return new SemVersion(final.Major, final.Minor, final.Patch,
            [label, (last + 1).ToString(CultureInfo.InvariantCulture)]);
    }

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
    /// Obtém ou inicializa o rótulo da pre-release, como <c>beta</c>, ou <see langword="null"/> para uma versão final.
    /// Com ele, a release é a próxima versão final com o rótulo e um número que sobe a cada pre-release dela, como
    /// <c>1.5.0-beta.1</c>.
    /// </summary>
    public string? PreReleaseLabel { get; init; }

    /// <summary>
    /// Obtém ou inicializa a primeira versão do projeto.
    /// </summary>
    public SemVersion FirstVersion { get; init; } = new(1, 0, 0);

    /// <summary>
    /// Obtém ou inicializa as regras de leitura dos commits.
    /// </summary>
    public ReleaseRules Rules { get; init; } = ReleaseRules.Default;
}
