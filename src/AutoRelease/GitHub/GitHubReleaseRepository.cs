using System.Net;
using KempDec.AutoRelease.Errors;
using KempDec.AutoRelease.Releases;
using Octokit;

namespace KempDec.AutoRelease.GitHub;

/// <summary>
/// Responsável pela leitura e pela criação das releases num repositório do GitHub.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="GitHubReleaseRepository"/>.</remarks>
/// <param name="client">O cliente do GitHub, já com o token.</param>
/// <param name="owner">O dono do repositório.</param>
/// <param name="name">O nome do repositório.</param>
internal sealed class GitHubReleaseRepository(IGitHubClient client, string owner, string name) : IReleaseRepository
{
    /// <summary>
    /// Quantos commits cada página da comparação traz, o máximo que o GitHub aceita.
    /// </summary>
    private const int ComparePageSize = 100;

    /// <summary>
    /// O repositório, como <c>dono/nome</c>, para as mensagens.
    /// </summary>
    private string FullName => $"{owner}/{name}";

    /// <summary>
    /// Cria o acesso a um repositório do GitHub.
    /// </summary>
    /// <remarks>
    /// Com a variável <c>GITHUB_API_URL</c> apontando para outro endereço (o GitHub Enterprise Server, que o GitHub
    /// Actions preenche sozinho), o acesso usa esse endereço.
    /// </remarks>
    /// <param name="token">O token de acesso ao GitHub.</param>
    /// <param name="owner">O dono do repositório.</param>
    /// <param name="name">O nome do repositório.</param>
    /// <param name="apiUrl">O endereço da API do GitHub, ou <see langword="null"/> para o github.com.</param>
    /// <returns>O acesso ao repositório.</returns>
    public static GitHubReleaseRepository Create(string token, string owner, string name, string? apiUrl)
    {
        var header = new ProductHeaderValue("AutoRelease", AutoReleaseInfo.Version);
        GitHubClient client = apiUrl is { Length: > 0 } && Uri.TryCreate(apiUrl, UriKind.Absolute, out Uri? uri)
            && !uri.Host.Equals("api.github.com", StringComparison.OrdinalIgnoreCase)
                ? new GitHubClient(header, uri)
                : new GitHubClient(header);

        client.Credentials = new Credentials(token);

        return new GitHubReleaseRepository(client, owner, name);
    }

    /// <inheritdoc/>
    public Task<string> ResolveCommitAsync(string? reference, CancellationToken cancellationToken) =>
        RunAsync(
            async () =>
            {
                string target = reference ?? (await client.Repository.Get(owner, name)).DefaultBranch;

                // O GetSha1 do Octokit devolve um texto vazio em vez do commit (conferido no Octokit 14 contra o
                // github.com), e a release sairia sem commit. A leitura do commit inteiro traz o identificador certo.
                GitHubCommit commit = await client.Repository.Commit.Get(owner, name, target);

                return string.IsNullOrWhiteSpace(commit.Sha)
                    ? throw new AutoReleaseException(
                        $"O GitHub não disse qual é o commit de \"{target}\" em {FullName}.",
                        "Rode de novo; se continuar, informe o commit em AutoRelease_Commit (ou --commit).")
                    : commit.Sha;
            },
            "ler o commit da release",
            reference is null
                ? NotFoundRepository()
                : new AutoReleaseException(
                    $"O branch ou commit \"{reference}\" não existe em {FullName}.",
                    "Confira o nome em AutoRelease_Branch (ou --branch) e o commit em AutoRelease_Commit (ou --commit)."));

    /// <inheritdoc/>
    public Task<IReadOnlyList<RepositoryTagInfo>> GetTagsAsync(CancellationToken cancellationToken) =>
        RunAsync<IReadOnlyList<RepositoryTagInfo>>(
            async () => [.. (await client.Repository.GetAllTags(owner, name))
                .Select(tag => new RepositoryTagInfo(tag.Name, tag.Commit.Sha))],
            "ler as tags",
            NotFoundRepository());

    /// <inheritdoc/>
    public Task<IReadOnlyList<ReleaseCommit>> GetCommitsAsync(string? baseSha, string headSha,
        CancellationToken cancellationToken) =>
        RunAsync(
            () => baseSha is null ? GetHistoryAsync(headSha) : CompareAsync(baseSha, headSha, cancellationToken),
            "ler os commits",
            NotFoundRepository());

    /// <inheritdoc/>
    public Task<bool> ReleaseExistsAsync(string tagName, CancellationToken cancellationToken) =>
        RunAsync(
            async () =>
            {
                try
                {
                    _ = await client.Repository.Release.Get(owner, name, tagName);

                    return true;
                }
                catch (NotFoundException)
                {
                    return false;
                }
            },
            "ler as releases",
            NotFoundRepository());

    /// <inheritdoc/>
    public Task<CreatedRelease> CreateReleaseAsync(NewReleaseRequest request, CancellationToken cancellationToken) =>
        RunAsync(
            async () =>
            {
                var release = new NewRelease(request.TagName)
                {
                    Name = request.Name,
                    Body = request.Body,
                    Draft = request.Draft,
                    Prerelease = request.Prerelease,
                    TargetCommitish = request.TargetCommitSha
                };

                try
                {
                    Release created = await client.Repository.Release.Create(owner, name, release);

                    return new CreatedRelease(created.Id, created.UploadUrl, created.HtmlUrl);
                }
                catch (ApiValidationException exception)
                    when (exception.ApiError?.Errors?.Any(error => error.Code == "already_exists") is true)
                {
                    throw new AutoReleaseException(
                        $"A release {request.TagName} já existe em {FullName}.",
                        "Ela pode ter sido criada por uma execução anterior: confira a página de releases antes de "
                        + "rodar de novo.",
                        exception);
                }
            },
            "criar a release",
            NotFoundRepository());

    /// <summary>
    /// Lê a história inteira de um commit, para a primeira release.
    /// </summary>
    /// <param name="headSha">O commit da release.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo os commits, do mais antigo ao mais
    /// novo.</returns>
    private async Task<IReadOnlyList<ReleaseCommit>> GetHistoryAsync(string headSha)
    {
        IReadOnlyList<GitHubCommit> commits = await client.Repository.Commit.GetAll(owner, name,
            new CommitRequest { Sha = headSha });

        return [.. commits.Reverse().Select(ToReleaseCommit)];
    }

    /// <summary>
    /// Lê os commits de um commit desde outro, página a página.
    /// </summary>
    /// <remarks>
    /// A comparação conta a partir do ancestral comum, então um commit refeito com data antiga entra, e um branch que
    /// divergiu não traz os commits de outro. Uma página que não traz nada novo encerra a leitura, para uma resposta
    /// que repete a página não virar um laço sem fim.
    /// </remarks>
    /// <param name="baseSha">O commit da versão anterior.</param>
    /// <param name="headSha">O commit da release.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo os commits, do mais antigo ao mais
    /// novo.</returns>
    private async Task<IReadOnlyList<ReleaseCommit>> CompareAsync(string baseSha, string headSha,
        CancellationToken cancellationToken)
    {
        var commits = new List<ReleaseCommit>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (int page = 1; ; page++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CompareResult result = await client.Repository.Commit.Compare(owner, name, baseSha, headSha,
                new ApiOptions { PageSize = ComparePageSize, PageCount = 1, StartPage = page });

            int before = commits.Count;

            foreach (GitHubCommit commit in result.Commits.Where(commit => seen.Add(commit.Sha)))
            {
                commits.Add(ToReleaseCommit(commit));
            }

            if (commits.Count == before || commits.Count >= result.TotalCommits
                || result.Commits.Count < ComparePageSize)
            {
                return commits;
            }
        }
    }

    /// <summary>
    /// Converte um commit do GitHub.
    /// </summary>
    /// <param name="commit">O commit do GitHub.</param>
    /// <returns>O commit.</returns>
    private static ReleaseCommit ToReleaseCommit(GitHubCommit commit) =>
        ReleaseCommit.Create(commit.Sha, commit.Commit.Author?.Name ?? string.Empty, commit.Commit.Message);

    /// <summary>
    /// O erro de repositório não encontrado.
    /// </summary>
    /// <returns>O erro.</returns>
    private AutoReleaseException NotFoundRepository() => new(
        $"O repositório {FullName} não foi encontrado, ou o token não tem acesso a ele.",
        "Confira o nome em AutoRelease_Repo (ou --repo, no formato dono/nome) e se o token enxerga o repositório.");

    /// <summary>
    /// Executa uma chamada ao GitHub e traduz as falhas dela em erros que a pessoa consegue resolver.
    /// </summary>
    /// <typeparam name="T">O tipo do resultado.</typeparam>
    /// <param name="call">A chamada.</param>
    /// <param name="action">O que a chamada faz, para as mensagens, como "criar a release".</param>
    /// <param name="notFound">O erro quando o GitHub responde que não encontrou.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo o resultado da chamada.</returns>
    private static async Task<T> RunAsync<T>(Func<Task<T>> call, string action, AutoReleaseException notFound)
    {
        try
        {
            return await call();
        }
        catch (AutoReleaseException)
        {
            throw;
        }
        catch (AuthorizationException exception)
        {
            throw new AutoReleaseException(
                $"O GitHub recusou o token ao {action}.",
                "Confira se o token é válido e não expirou. No GitHub Actions, use AutoRelease_Token: "
                + "${{ secrets.GITHUB_TOKEN }}.",
                exception);
        }
        catch (RateLimitExceededException exception)
        {
            throw new AutoReleaseException(
                $"O limite de pedidos ao GitHub acabou ao {action}.",
                $"Tente de novo depois de {exception.Reset.UtcDateTime:yyyy-MM-dd HH:mm} (UTC).",
                exception);
        }
        catch (ForbiddenException exception)
        {
            throw new AutoReleaseException(
                $"O token não tem permissão para {action}.",
                "No GitHub Actions, dê ao job a permissão \"contents: write\"; com um token pessoal, dê a ele acesso "
                + "de escrita ao repositório.",
                exception);
        }
        catch (NotFoundException exception)
        {
            throw new AutoReleaseException(notFound.Problem, notFound.NextStep, exception);
        }
        catch (ApiException exception) when (exception.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new AutoReleaseException(
                $"O GitHub falhou ao {action} (erro {(int)exception.StatusCode}).",
                "Costuma ser passageiro: confira githubstatus.com e rode de novo.",
                exception);
        }
        catch (ApiException exception)
        {
            throw new AutoReleaseException(
                $"O GitHub recusou o pedido ao {action}: {exception.Message}",
                "Confira a configuração do Auto Release e rode de novo.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new AutoReleaseException(
                $"Não deu para falar com o GitHub ao {action}.",
                "Confira a conexão com a internet (ou o endereço em GITHUB_API_URL) e rode de novo.",
                exception);
        }
        catch (TaskCanceledException exception) when (exception.InnerException is TimeoutException)
        {
            throw new AutoReleaseException(
                $"O GitHub demorou demais para responder ao {action}.",
                "Rode de novo; se continuar, confira githubstatus.com.",
                exception);
        }
    }
}
