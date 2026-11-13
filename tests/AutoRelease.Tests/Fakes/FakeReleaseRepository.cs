using KempDec.AutoRelease.Errors;
using KempDec.AutoRelease.GitHub;
using KempDec.AutoRelease.Releases;

namespace KempDec.AutoRelease.Tests.Fakes;

/// <summary>
/// Representa um repositório em memória, com uma história linear de commits, para os testes rodarem sem o GitHub.
/// </summary>
internal sealed class FakeReleaseRepository : IReleaseRepository
{
    /// <summary>
    /// Obtém os commits, do mais antigo ao mais novo.
    /// </summary>
    public List<(string Sha, string Author, string Message)> History { get; } = [];

    /// <summary>
    /// Obtém os branches e o commit da ponta de cada um.
    /// </summary>
    public Dictionary<string, string> Branches { get; } = [];

    /// <summary>
    /// Obtém as tags.
    /// </summary>
    public List<RepositoryTagInfo> Tags { get; } = [];

    /// <summary>
    /// Obtém as tags que já têm release.
    /// </summary>
    public HashSet<string> Releases { get; } = [];

    /// <summary>
    /// Obtém os pedidos de criação de release recebidos.
    /// </summary>
    public List<NewReleaseRequest> Created { get; } = [];

    /// <summary>
    /// Obtém ou define o erro que toda chamada lança, para simular uma falha do GitHub.
    /// </summary>
    public Exception? Failure { get; set; }

    /// <summary>
    /// Acrescenta um commit no fim da história e move o branch <c>main</c> para ele.
    /// </summary>
    /// <param name="message">A mensagem.</param>
    /// <param name="author">O autor.</param>
    /// <returns>O identificador do commit.</returns>
    public string Commit(string message, string author = "Ana")
    {
        string sha = $"{History.Count + 1:x2}".PadRight(40, 'a');

        History.Add((sha, author, message));
        Branches["main"] = sha;

        return sha;
    }

    /// <summary>
    /// Cria uma tag no último commit.
    /// </summary>
    /// <param name="name">O nome da tag.</param>
    /// <param name="withRelease">Se a tag já tem release.</param>
    public void Tag(string name, bool withRelease = true)
    {
        Tags.Add(new RepositoryTagInfo(name, History[^1].Sha));

        if (withRelease)
        {
            Releases.Add(name);
        }
    }

    /// <inheritdoc/>
    public Task<string> ResolveCommitAsync(string? reference, CancellationToken cancellationToken)
    {
        ThrowIfFailing();

        string key = reference ?? "main";

        if (Branches.TryGetValue(key, out string? sha))
        {
            return Task.FromResult(sha);
        }

        if (History.Any(commit => commit.Sha == key))
        {
            return Task.FromResult(key);
        }

        throw new AutoReleaseException($"O branch ou commit \"{key}\" não existe.", "Confira o nome.");
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<RepositoryTagInfo>> GetTagsAsync(CancellationToken cancellationToken)
    {
        ThrowIfFailing();

        return Task.FromResult<IReadOnlyList<RepositoryTagInfo>>([.. Tags]);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ReleaseCommit>> GetCommitsAsync(string? baseSha, string headSha,
        CancellationToken cancellationToken)
    {
        ThrowIfFailing();

        int start = baseSha is null ? 0 : History.FindIndex(commit => commit.Sha == baseSha) + 1;
        int end = History.FindIndex(commit => commit.Sha == headSha);

        return Task.FromResult<IReadOnlyList<ReleaseCommit>>([.. History
            .Skip(start)
            .Take(Math.Max(0, end - start + 1))
            .Select(commit => ReleaseCommit.Create(commit.Sha, commit.Author, commit.Message))]);
    }

    /// <inheritdoc/>
    public Task<bool> ReleaseExistsAsync(string tagName, CancellationToken cancellationToken)
    {
        ThrowIfFailing();

        return Task.FromResult(Releases.Contains(tagName));
    }

    /// <inheritdoc/>
    public Task<CreatedRelease> CreateReleaseAsync(NewReleaseRequest request, CancellationToken cancellationToken)
    {
        ThrowIfFailing();

        Created.Add(request);
        Releases.Add(request.TagName);

        return Task.FromResult(new CreatedRelease(Created.Count + 1000,
            $"https://uploads.github.test/{request.TagName}", $"https://github.test/releases/{request.TagName}"));
    }

    /// <summary>
    /// Lança o erro configurado, quando há.
    /// </summary>
    private void ThrowIfFailing()
    {
        if (Failure is not null)
        {
            throw Failure;
        }
    }
}
