using System.CommandLine;
using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Configuration;
using KempDec.AutoRelease.Errors;
using KempDec.AutoRelease.GitHub;
using KempDec.AutoRelease.Lint;
using KempDec.AutoRelease.Releases;
using Microsoft.Extensions.Configuration;
using Semver;

namespace KempDec.AutoRelease.Cli;

/// <summary>
/// Responsável pela linha de comando do Auto Release: os comandos <c>create</c>, <c>note</c>, <c>version</c> e
/// <c>lint</c>.
/// </summary>
/// <remarks>
/// O resultado (a versão, as notas, o identificador da release) vai para a saída padrão, e os avisos e os erros para a
/// saída de erros, então <c>$(autorelease create)</c> guarda só o resultado. O código de saída é 0 quando deu certo e 1
/// quando algo falhou, para o GitHub Actions marcar o passo como falho.
/// </remarks>
/// <remarks>Inicializa uma nova instância de <see cref="AutoReleaseCli"/>.</remarks>
/// <param name="context">O que a linha de comando usa do mundo de fora.</param>
internal sealed class AutoReleaseCli(CliContext context)
{
    /// <summary>
    /// O código de saída quando algo falhou.
    /// </summary>
    public const int FailureExitCode = 1;

    /// <summary>
    /// O código de saída quando deu certo.
    /// </summary>
    public const int SuccessExitCode = 0;

    private readonly Option<string> _token = new("--token", "-t")
    {
        Description = "O token de acesso ao GitHub. Prefira a variável AutoRelease_Token, guardada como segredo."
    };

    private readonly Option<string> _repo = new("--repo", "-r")
    {
        Description = "O repositório no GitHub, como dono/nome. No GitHub Actions, vem sozinho."
    };

    private readonly Option<string> _branch = new("--branch", "-b")
    {
        Description = "O branch da release, quando ele não é o do commit em que o GitHub Actions roda."
    };

    private readonly Option<string> _commit = new("--commit")
    {
        Description = "O commit exato da release. No GitHub Actions, é o commit que disparou o fluxo."
    };

    private readonly Option<string> _config = new("--config")
    {
        Description = "O caminho do arquivo de configuração, em vez do autorelease.config.json da pasta."
    };

    private readonly Option<string> _version = new("--version", "-v")
    {
        Description = "A versão da release, quando ela não deve ser calculada pelos commits."
    };

    private readonly Option<string[]> _types = new("--types")
    {
        Description = "Os tipos dos commits, na ordem das notas, como feat=Novidades fix=Correções. O primeiro sobe "
            + "a versão minor.",
        AllowMultipleArgumentsPerToken = true
    };

    private readonly Option<string[]> _replaces = new("--replaces")
    {
        Description = "As trocas do começo das descrições, como \"Adicionar a \"=\"Foi adicionada a \".",
        AllowMultipleArgumentsPerToken = true
    };

    private readonly Option<string[]> _ignores = new("--ignores")
    {
        Description = "Os começos das descrições que ficam fora das notas e da versão, como Merge.",
        AllowMultipleArgumentsPerToken = true
    };

    private readonly Option<bool?> _showAuthor = new("--show-author")
    {
        Description = "Mostra o nome do autor em cada linha das notas."
    };

    private readonly Option<string> _firstVersion = new("--first-version")
    {
        Description = "A versão da primeira release. Padrão: 1.0.0."
    };

    private readonly Option<string> _versionPrefix = new("--version-prefix")
    {
        Description = "O prefixo das tags de versão. Padrão: v."
    };

    private readonly Option<string> _projectName = new("--project-name")
    {
        Description = "O nome do projeto no título da release. Padrão: o nome do repositório."
    };

    private readonly Option<bool?> _draft = new("--draft")
    {
        Description = "Cria a release como rascunho."
    };

    private readonly Option<bool?> _preRelease = new("--pre-release")
    {
        Description = "Marca a release como pre-release."
    };

    private readonly Option<string> _outputType = new("--output-type")
    {
        Description = "O que o comando escreve na saída: Id (padrão), UploadUrl, Version, Tag ou HtmlUrl."
    };

    private readonly Option<bool?> _dryRun = new("--dry-run")
    {
        Description = "Mostra a release que seria criada, sem criar nada."
    };

    private readonly Option<bool?> _skipEmpty = new("--skip-empty")
    {
        Description = "Não cria a release quando não há nada novo para anunciar (todos os commits ignorados)."
    };

    private readonly Option<string> _message = new("--message", "-m")
    {
        Description = "A mensagem de commit a conferir."
    };

    private readonly Option<string> _file = new("--file")
    {
        Description = "O arquivo com a mensagem a conferir, como o que o gancho commit-msg do Git recebe."
    };

    private readonly Option<string> _from = new("--from")
    {
        Description = "O commit anterior ao intervalo a conferir (exclusivo), lido pelo Git da pasta."
    };

    private readonly Option<string> _to = new("--to")
    {
        Description = "O último commit do intervalo a conferir. Padrão: HEAD."
    };

    /// <summary>
    /// Roda a linha de comando no processo atual.
    /// </summary>
    /// <param name="args">Os argumentos.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo o código de saída.</returns>
    public static Task<int> RunAsync(string[] args) => new AutoReleaseCli(CliContext.FromProcess()).InvokeAsync(args);

    /// <summary>
    /// Lê os argumentos e roda o comando pedido.
    /// </summary>
    /// <param name="args">Os argumentos.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo o código de saída.</returns>
    public Task<int> InvokeAsync(string[] args, CancellationToken cancellationToken = default)
    {
        RootCommand root = BuildRootCommand();
        ParseResult parseResult = root.Parse(args);

        return parseResult.InvokeAsync(new InvocationConfiguration { Output = context.Output, Error = context.Error },
            cancellationToken);
    }

    /// <summary>
    /// Monta o comando raiz e os subcomandos.
    /// </summary>
    /// <returns>O comando raiz.</returns>
    public RootCommand BuildRootCommand()
    {
        var root = new RootCommand("Gera as releases do seu projeto no GitHub a partir das mensagens de commit.");

        root.Subcommands.Add(BuildCommand("create", "Cria a release: calcula a versão, escreve as notas e publica.",
            [_projectName, _draft, _preRelease, _outputType, _dryRun, _skipEmpty], CreateAsync));
        root.Subcommands.Add(BuildCommand("note", "Escreve as notas da próxima release, sem criar nada.", [],
            NoteAsync));
        root.Subcommands.Add(BuildCommand("version", "Calcula a versão da próxima release, sem criar nada.", [],
            VersionAsync));
        root.Subcommands.Add(BuildLintCommand());

        return root;
    }

    /// <summary>
    /// Monta um subcomando de release, com as opções comuns e as dele.
    /// </summary>
    /// <param name="name">O nome do subcomando.</param>
    /// <param name="description">O que ele faz.</param>
    /// <param name="extraOptions">As opções só dele.</param>
    /// <param name="handler">O que ele roda.</param>
    /// <returns>O subcomando.</returns>
    private Command BuildCommand(string name, string description, IEnumerable<Option> extraOptions,
        Func<ParseResult, CancellationToken, Task> handler)
    {
        var command = new Command(name, description);
        Option[] options =
        [
            _token, _repo, _branch, _commit, _config, _version, _types, _replaces, _ignores, _showAuthor,
            _firstVersion, _versionPrefix
        ];

        foreach (Option option in options.Concat(extraOptions))
        {
            command.Options.Add(option);
        }

        command.SetAction((parseResult, cancellationToken) => RunSafelyAsync(handler, parseResult, cancellationToken));

        return command;
    }

    /// <summary>
    /// Monta o subcomando que confere as mensagens de commit.
    /// </summary>
    /// <returns>O subcomando.</returns>
    private Command BuildLintCommand()
    {
        var command = new Command("lint",
            "Confere as mensagens de commit contra as regras do projeto. Sai com 1 quando alguma está fora.");

        foreach (Option option in new Option[] { _message, _file, _from, _to, _config, _types, _replaces, _ignores })
        {
            command.Options.Add(option);
        }

        command.SetAction((parseResult, cancellationToken) => RunSafelyAsync(LintAsync, parseResult, cancellationToken));

        return command;
    }

    /// <summary>
    /// Roda um subcomando e transforma as falhas em mensagens em duas partes e no código de saída.
    /// </summary>
    /// <param name="handler">O que o subcomando roda; ele lança <see cref="LintFailedException"/> para sair com 1 sem
    /// escrever outro erro.</param>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo o código de saída.</returns>
    private async Task<int> RunSafelyAsync(Func<ParseResult, CancellationToken, Task> handler, ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler(parseResult, cancellationToken);

            return SuccessExitCode;
        }
        catch (LintFailedException)
        {
            return FailureExitCode;
        }
        catch (AutoReleaseException exception)
        {
            context.Error.WriteError(exception);

            return FailureExitCode;
        }
        catch (OperationCanceledException)
        {
            context.Error.WriteError(new AutoReleaseException("A execução foi cancelada.", "Rode o comando de novo."));

            return FailureExitCode;
        }
        catch (Exception exception)
        {
            context.Error.WriteError(new AutoReleaseException(
                $"Algo inesperado aconteceu: {exception.Message}",
                $"Rode de novo; se continuar, abra uma issue em {AutoReleaseInfo.IssuesUrl} com esta mensagem."));

            return FailureExitCode;
        }
    }

    /// <summary>
    /// Cria a release.
    /// </summary>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    private async Task CreateAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        SettingsReader reader = LoadSettings(parseResult, ["CreateCommand"]);
        (IReleaseRepository repository, ReleasePlanRequest request, string repoName) =
            BuildRequest(parseResult, reader);
        ReleasePlan plan = await new ReleasePlanner(repository).PlanAsync(request, cancellationToken);

        bool skipEmpty = reader.GetBool("SkipEmpty", parseResult.GetValue(_skipEmpty));

        if (plan.IsEmpty && skipEmpty)
        {
            context.Error.WriteLine(
                $"Nada novo para anunciar desde {plan.PreviousTag?.Name ?? "o começo do repositório"}: nenhuma release "
                + "foi criada.");

            return;
        }

        OutputType outputType = ParseOutputType(reader.GetString("OutputType", parseResult.GetValue(_outputType)));
        string title = $"{reader.GetString("ProjectName", parseResult.GetValue(_projectName)) ?? repoName} "
            + $"{plan.TagName} ({context.Clock.GetUtcNow():yyyy-MM-dd})";
        bool draft = reader.GetBool("Draft", parseResult.GetValue(_draft));
        bool preRelease = reader.GetBool("PreRelease", parseResult.GetValue(_preRelease));

        if (reader.GetBool("DryRun", parseResult.GetValue(_dryRun)))
        {
            WritePreview(plan, title, draft, preRelease);

            return;
        }

        if (await repository.ReleaseExistsAsync(plan.TagName, cancellationToken))
        {
            throw new AutoReleaseException(
                $"A release {plan.TagName} já existe.",
                "Confira a página de releases. Para outra versão, faça um commit novo ou informe AutoRelease_Version.");
        }

        if (plan.ExistingTag is { } tag && tag.CommitSha != plan.TargetSha)
        {
            context.Error.WriteWarning(
                $"A tag {tag.Name} já existe no commit {Short(tag.CommitSha)}, e não no {Short(plan.TargetSha)}.",
                "A release vai usar a tag que já existe; apague a tag antes se ela foi criada no commit errado.");
        }

        CreatedRelease release = await repository.CreateReleaseAsync(
            new NewReleaseRequest(plan.TagName, title, plan.Notes, plan.TargetSha, draft, preRelease),
            cancellationToken);

        context.Error.WriteLine($"Release {plan.TagName} criada no commit {Short(plan.TargetSha)}: {release.HtmlUrl}");
        context.Output.WriteLine(outputType switch
        {
            OutputType.UploadUrl => release.UploadUrl,
            OutputType.Version => plan.Version.ToString(),
            OutputType.Tag => plan.TagName,
            OutputType.HtmlUrl => release.HtmlUrl,
            _ => release.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)
        });
    }

    /// <summary>
    /// Escreve as notas da próxima release.
    /// </summary>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    private async Task NoteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        SettingsReader reader = LoadSettings(parseResult, ["NoteCommand", "CreateCommand"]);
        (IReleaseRepository repository, ReleasePlanRequest request, _) = BuildRequest(parseResult, reader);
        ReleasePlan plan = await new ReleasePlanner(repository).PlanAsync(request, cancellationToken);

        if (plan.Notes.Length == 0)
        {
            context.Error.WriteLine(
                $"Nenhum commit entra nas notas desde {plan.PreviousTag?.Name ?? "o começo do repositório"}.");

            return;
        }

        context.Output.Write(plan.Notes);
    }

    /// <summary>
    /// Escreve a versão da próxima release.
    /// </summary>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    private async Task VersionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        SettingsReader reader = LoadSettings(parseResult, ["VersionCommand", "CreateCommand"]);
        (IReleaseRepository repository, ReleasePlanRequest request, _) = BuildRequest(parseResult, reader);
        ReleasePlan plan = await new ReleasePlanner(repository).PlanAsync(request, cancellationToken);

        context.Output.WriteLine(plan.Version.ToString());
    }

    /// <summary>
    /// Confere as mensagens de commit.
    /// </summary>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona.</returns>
    /// <exception cref="LintFailedException">Quando alguma mensagem está fora das regras.</exception>
    private async Task LintAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        SettingsReader reader = LoadSettings(parseResult, ["LintCommand", "CreateCommand"]);
        LintRules rules = new()
        {
            Types = CommitTypeCatalog.Parse(reader.GetList("Types", parseResult.GetValue(_types))),
            Rewriter = DescriptionRewriter.Create(reader.GetPairs("Replaces", parseResult.GetValue(_replaces))),
            Ignores = reader.GetList("Ignores", parseResult.GetValue(_ignores)),
            RequireLeadingWord = reader.GetBool("RequireLeadingWord", defaultValue: true),
            RequirePeriod = reader.GetBool("RequirePeriod"),
            MaxHeaderLength = ParseOptionalInt(reader, "MaxHeaderLength"),
            ForbiddenTrailers = reader.GetList("ForbiddenTrailers")
        };

        IReadOnlyList<(string? Sha, string Message)> messages = await ReadLintMessagesAsync(parseResult,
            cancellationToken);
        int failed = 0;

        foreach ((string? sha, string message) in messages)
        {
            IReadOnlyList<LintProblem> problems = CommitLinter.Lint(message, rules);

            if (problems.Count == 0)
            {
                continue;
            }

            failed++;
            string header = message.ReplaceLineEndings("\n").Split('\n')[0].Trim();

            context.Error.WriteLine(sha is null ? $"✗ {header}" : $"✗ {Short(sha)} {header}");

            foreach (LintProblem problem in problems)
            {
                context.Error.WriteLine($"  {problem.Problem} {problem.NextStep}");
            }
        }

        if (failed > 0)
        {
            context.Error.WriteLine(failed == 1
                ? "1 mensagem está fora das regras do projeto."
                : $"{failed} mensagens estão fora das regras do projeto.");

            throw new LintFailedException();
        }

        context.Error.WriteLine(messages.Count == 1
            ? "A mensagem segue as regras do projeto."
            : $"As {messages.Count} mensagens seguem as regras do projeto.");
    }

    /// <summary>
    /// Lê as mensagens que o <c>lint</c> confere: a de <c>--message</c>, a do arquivo de <c>--file</c> ou as do
    /// intervalo de <c>--from</c> a <c>--to</c>.
    /// </summary>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A <see cref="Task"/> que representa a operação assíncrona, contendo as mensagens e os commits delas,
    /// quando vêm do Git.</returns>
    /// <exception cref="AutoReleaseException">Quando nenhuma fonte é informada ou o arquivo não existe.</exception>
    private async Task<IReadOnlyList<(string? Sha, string Message)>> ReadLintMessagesAsync(ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (parseResult.GetValue(_message) is { } message)
        {
            return [(null, message)];
        }

        if (parseResult.GetValue(_file) is { } file)
        {
            string path = Path.GetFullPath(file, context.WorkingDirectory);

            return File.Exists(path)
                ? [(null, await File.ReadAllTextAsync(path, cancellationToken))]
                : throw new AutoReleaseException($"O arquivo {path} não existe.", "Confira o caminho em --file.");
        }

        if (parseResult.GetValue(_from) is { } from)
        {
            string to = parseResult.GetValue(_to) ?? "HEAD";
            string log = await context.RunGit(["log", "--no-merges", "--format=%H%x1f%B%x1e", $"{from}..{to}"],
                cancellationToken);

            return [.. log.Split('\x1e', StringSplitOptions.RemoveEmptyEntries)
                .Select(entry => entry.Trim('\n', '\r'))
                .Where(entry => entry.Length > 0)
                .Select(entry => entry.Split('\x1f', 2))
                .Select(parts => ((string?)parts[0], parts.Length > 1 ? parts[1] : string.Empty))];
        }

        throw new AutoReleaseException(
            "Falta dizer o que conferir.",
            "Passe a mensagem em --message, o arquivo em --file (no gancho commit-msg) ou o intervalo em --from e --to.");
    }

    /// <summary>
    /// Carrega as configurações de um comando: o arquivo, as variáveis de ambiente e as seções dele.
    /// </summary>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <param name="sections">As seções do comando, da mais específica para a mais geral.</param>
    /// <returns>O leitor das configurações.</returns>
    private SettingsReader LoadSettings(ParseResult parseResult, IReadOnlyList<string> sections)
    {
        (IConfiguration configuration, bool fileHasToken) = ConfigurationLoader.Load(context.WorkingDirectory,
            parseResult.GetValue(_config), context.Environment);

        if (fileHasToken)
        {
            context.Error.WriteWarning(
                "O token está no arquivo de configuração, que costuma ir junto para o repositório.",
                "Tire o Token do arquivo, revogue esse token e passe um novo em AutoRelease_Token, guardado como "
                + "segredo.");
        }

        return new SettingsReader(configuration, sections);
    }

    /// <summary>
    /// Monta o acesso ao repositório e o pedido da release a partir das configurações.
    /// </summary>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <param name="reader">O leitor das configurações.</param>
    /// <returns>O acesso ao repositório, o pedido da release e o nome do repositório.</returns>
    /// <exception cref="AutoReleaseException">Quando falta o token ou o repositório.</exception>
    private (IReleaseRepository Repository, ReleasePlanRequest Request, string RepoName) BuildRequest(
        ParseResult parseResult, SettingsReader reader)
    {
        string token = reader.GetString("Token", parseResult.GetValue(_token))
            ?? context.GetEnvironment("GITHUB_TOKEN")
            ?? throw new AutoReleaseException(
                "Falta o token de acesso ao GitHub.",
                "Passe o token em AutoRelease_Token. No GitHub Actions, use AutoRelease_Token: ${{ secrets.GITHUB_TOKEN }}.");

        string repo = reader.GetString("Repo", parseResult.GetValue(_repo))
            ?? context.GetEnvironment("GITHUB_REPOSITORY")
            ?? throw new AutoReleaseException(
                "Falta dizer qual é o repositório.",
                "Passe o repositório em AutoRelease_Repo (ou --repo), como dono/nome.");

        string[] parts = repo.Split('/');

        if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace))
        {
            throw new AutoReleaseException(
                $"O repositório \"{repo}\" não está no formato dono/nome.",
                "Escreva como kempdec/AutoRelease em AutoRelease_Repo (ou --repo).");
        }

        string prefix = reader.GetString("VersionPrefix", parseResult.GetValue(_versionPrefix)) ?? "v";
        var request = new ReleasePlanRequest
        {
            Reference = ResolveReference(reader, parseResult),
            Version = reader.GetVersion("Version", parseResult.GetValue(_version), prefix),
            VersionPrefix = prefix,
            FirstVersion = reader.GetVersion("FirstVersion", parseResult.GetValue(_firstVersion), prefix)
                ?? new SemVersion(1, 0, 0),
            Rules = new ReleaseRules
            {
                Types = CommitTypeCatalog.Parse(reader.GetList("Types", parseResult.GetValue(_types))),
                Rewriter = DescriptionRewriter.Create(reader.GetPairs("Replaces", parseResult.GetValue(_replaces))),
                Ignores = reader.GetList("Ignores", parseResult.GetValue(_ignores)),
                ShowAuthor = reader.GetBool("ShowAuthor", parseResult.GetValue(_showAuthor))
            }
        };

        IReleaseRepository repository = context.CreateRepository(token, parts[0].Trim(), parts[1].Trim(),
            context.GetEnvironment("GITHUB_API_URL"));

        return (repository, request, parts[1].Trim());
    }

    /// <summary>
    /// Escolhe o commit da release.
    /// </summary>
    /// <remarks>
    /// Vale, nesta ordem: o commit informado; o branch informado, quando ele não é o do GitHub Actions; o commit que
    /// disparou o GitHub Actions (<c>GITHUB_SHA</c>); o branch informado; e, sem nada disso, o branch padrão. Assim, no
    /// GitHub Actions, a release sai do commit que foi aprovado, e não da ponta do branch no momento em que roda.
    /// </remarks>
    /// <param name="reader">O leitor das configurações.</param>
    /// <param name="parseResult">Os argumentos lidos.</param>
    /// <returns>A referência da release, ou <see langword="null"/> para o branch padrão.</returns>
    private string? ResolveReference(SettingsReader reader, ParseResult parseResult)
    {
        string? commit = reader.GetString("Commit", parseResult.GetValue(_commit));

        if (commit is not null)
        {
            return commit;
        }

        string? branch = reader.GetString("Branch", parseResult.GetValue(_branch));
        string? actionsSha = context.GetEnvironment("GITHUB_SHA");

        if (branch is not null && (actionsSha is null
            || !string.Equals(branch, context.GetEnvironment("GITHUB_REF_NAME"), StringComparison.Ordinal)))
        {
            return branch;
        }

        return actionsSha ?? branch;
    }

    /// <summary>
    /// Escreve o resumo do ensaio, sem criar nada.
    /// </summary>
    /// <param name="plan">O que a release leva.</param>
    /// <param name="title">O título da release.</param>
    /// <param name="draft">Se a release seria um rascunho.</param>
    /// <param name="preRelease">Se a release seria uma pre-release.</param>
    private void WritePreview(ReleasePlan plan, string title, bool draft, bool preRelease)
    {
        TextWriter output = context.Output;

        output.WriteLine("Ensaio: nada foi criado.");
        output.WriteLine($"Título: {title}{(draft ? " (rascunho)" : string.Empty)}{(preRelease ? " (pre-release)" : string.Empty)}");
        output.WriteLine($"Tag: {plan.TagName}, no commit {Short(plan.TargetSha)}");
        output.WriteLine($"Versão anterior: {plan.PreviousTag?.Name ?? "nenhuma (é a primeira release)"}");
        output.WriteLine($"Commits desde ela: {plan.Commits.Count}");
        output.WriteLine();
        output.Write(plan.Notes.Length > 0 ? plan.Notes : "(nenhum commit entra nas notas)\n");
    }

    /// <summary>
    /// Lê o tipo de saída do <c>create</c>.
    /// </summary>
    /// <param name="value">O valor configurado.</param>
    /// <returns>O tipo de saída, <see cref="OutputType.Id"/> quando não há valor.</returns>
    /// <exception cref="AutoReleaseException">Quando o valor não é um dos tipos.</exception>
    private static OutputType ParseOutputType(string? value)
    {
        if (value is null)
        {
            return OutputType.Id;
        }

        return Enum.TryParse(value, ignoreCase: true, out OutputType type) && Enum.IsDefined(type)
            ? type
            : throw new AutoReleaseException(
                $"O tipo de saída \"{value}\" não existe.",
                $"Use um destes em OutputType: {string.Join(", ", Enum.GetNames<OutputType>())}.");
    }

    /// <summary>
    /// Lê um número inteiro opcional da configuração.
    /// </summary>
    /// <param name="reader">O leitor das configurações.</param>
    /// <param name="key">A chave.</param>
    /// <returns>O número, ou <see langword="null"/> quando não há.</returns>
    /// <exception cref="AutoReleaseException">Quando o valor não é um número positivo.</exception>
    private static int? ParseOptionalInt(SettingsReader reader, string key)
    {
        string? text = reader.GetString(key);

        if (text is null)
        {
            return null;
        }

        return int.TryParse(text, System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture, out int value) && value > 0
            ? value
            : throw new AutoReleaseException(
                $"O valor de {key} é \"{text}\", e ele precisa ser um número inteiro positivo.",
                $"Corrija {key} no autorelease.config.json.");
    }

    /// <summary>
    /// Encurta o identificador de um commit para os 7 primeiros caracteres, como o Git mostra.
    /// </summary>
    /// <param name="sha">O identificador.</param>
    /// <returns>O identificador curto.</returns>
    private static string Short(string sha) => sha.Length > 7 ? sha[..7] : sha;

    /// <summary>
    /// Representa a falha do <c>lint</c>, que já escreveu os problemas e só precisa do código de saída.
    /// </summary>
    private sealed class LintFailedException : Exception;
}
