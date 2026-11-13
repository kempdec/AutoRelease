using KempDec.AutoRelease.Cli;
using KempDec.AutoRelease.Configuration;
using KempDec.AutoRelease.Errors;
using KempDec.AutoRelease.Tests.Fakes;
using Octokit;

namespace KempDec.AutoRelease.Tests.Cli;

public sealed class AutoReleaseCliTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("autorelease-cli-").FullName;

    private readonly FakeReleaseRepository _repository = new();

    private readonly StringWriter _output = new();

    private readonly StringWriter _error = new();

    private readonly Dictionary<string, string?> _environment = new()
    {
        ["AutoRelease_Token"] = "token-de-teste",
        ["AutoRelease_Repo"] = "kempdec/Exemplo"
    };

    private (string Token, string Owner, string Name)? _repositoryArgs;

    private IReadOnlyList<string>? _gitArgs;

    private string _gitOutput = string.Empty;

    public void Dispose()
    {
        Directory.Delete(_directory, recursive: true);
        _output.Dispose();
        _error.Dispose();
    }

    private Task<int> RunAsync(params string[] args)
    {
        var context = new CliContext
        {
            Output = _output,
            Error = _error,
            Environment = _environment,
            WorkingDirectory = _directory,
            CreateRepository = (token, owner, name, _) =>
            {
                _repositoryArgs = (token, owner, name);

                return _repository;
            },
            Clock = new FixedClock(new DateTimeOffset(2026, 11, 4, 23, 30, 0, TimeSpan.Zero)),
            RunGit = (args, _) =>
            {
                _gitArgs = args;

                return Task.FromResult(_gitOutput);
            }
        };

        return new AutoReleaseCli(context).InvokeAsync(args, TestContext.Current.CancellationToken);
    }

    private void WriteConfig(string json) =>
        File.WriteAllText(Path.Combine(_directory, ConfigurationLoader.DefaultFileName), json);

    private string Stdout => _output.ToString().ReplaceLineEndings("\n");

    private string Stderr => _error.ToString().ReplaceLineEndings("\n");

    [Fact]
    public async Task Create_cria_a_release_do_commit_e_escreve_so_o_id_na_saida()
    {
        _repository.Commit("feat: Adicionar a busca.");
        _repository.Tag("v1.0.0");
        string target = _repository.Commit("feat: Adicionar o filtro.");

        int exitCode = await RunAsync("create");

        Assert.Equal(0, exitCode);
        Assert.Equal("1001\n", Stdout);
        Assert.Contains("Release v1.1.0 criada", Stderr, StringComparison.Ordinal);

        var created = Assert.Single(_repository.Created);

        Assert.Equal("v1.1.0", created.TagName);
        Assert.Equal(target, created.TargetCommitSha);
        Assert.Equal("Exemplo v1.1.0 (2026-11-04)", created.Name);
        Assert.Equal("# Novidades\n\n- Adicionar o filtro.\n", created.Body);
        Assert.Equal(("token-de-teste", "kempdec", "Exemplo"), _repositoryArgs);
    }

    [Fact]
    public async Task No_github_actions_a_release_sai_do_commit_que_disparou_o_fluxo()
    {
        _repository.Commit("feat: Um.");
        _repository.Tag("v1.0.0");
        string pushed = _repository.Commit("fix: Aprovado.");

        _repository.Commit("feat: Entrou depois.");
        _environment["GITHUB_SHA"] = pushed;
        _environment["GITHUB_REF_NAME"] = "main";
        WriteConfig("""{ "Branch": "main" }""");

        Assert.Equal(0, await RunAsync("create", "--output-type", "Tag"));
        Assert.Equal("v1.0.1\n", Stdout);
        Assert.Equal(pushed, Assert.Single(_repository.Created).TargetCommitSha);
    }

    [Fact]
    public async Task O_ensaio_mostra_a_release_e_nao_cria_nada()
    {
        _repository.Commit("feat: Um.");

        Assert.Equal(0, await RunAsync("create", "--dry-run"));
        Assert.Empty(_repository.Created);
        Assert.Contains("Ensaio: nada foi criado.", Stdout, StringComparison.Ordinal);
        Assert.Contains("Tag: v1.0.0", Stdout, StringComparison.Ordinal);
        Assert.Contains("nenhuma (é a primeira release)", Stdout, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Sem_nada_para_anunciar_e_com_skip_empty_nao_cria_e_sai_com_zero()
    {
        _repository.Commit("feat: Um.");
        _repository.Tag("v1.0.0");
        _repository.Commit("feat#: Prévia interna.");
        WriteConfig("""{ "CreateCommand": { "SkipEmpty": true } }""");

        Assert.Equal(0, await RunAsync("create"));
        Assert.Empty(_repository.Created);
        Assert.Equal(string.Empty, Stdout);
        Assert.Contains("Nada novo para anunciar desde v1.0.0", Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_release_que_ja_existe_falha_com_um_e_em_duas_partes()
    {
        _repository.Commit("feat: Um.");
        _repository.Tag("v1.0.0");

        Assert.Equal(1, await RunAsync("create", "--version", "1.0.0"));
        Assert.Empty(_repository.Created);
        Assert.Equal(string.Empty, Stdout);
        Assert.Equal(
            "Erro: A release v1.0.0 já existe.\n"
            + "Confira a página de releases. Para outra versão, faça um commit novo ou informe AutoRelease_Version.\n",
            Stderr);
    }

    [Fact]
    public async Task A_tag_criada_a_mao_ganha_a_release()
    {
        _repository.Commit("feat: Um.");
        _repository.Tag("v1.0.0");
        _repository.Commit("fix: Dois.");
        _repository.Tag("v1.0.1", withRelease: false);
        _environment["AutoRelease_Version"] = "v1.0.1";

        Assert.Equal(0, await RunAsync("create", "--output-type", "version"));
        Assert.Equal("1.0.1\n", Stdout);
        Assert.Equal("# Correções\n\n- Dois.\n", Assert.Single(_repository.Created).Body);
    }

    [Fact]
    public async Task Sem_token_falha_com_um_e_diz_onde_passar()
    {
        _environment.Remove("AutoRelease_Token");

        Assert.Equal(1, await RunAsync("version"));
        Assert.StartsWith("Erro: Falta o token de acesso ao GitHub.\nPasse o token em AutoRelease_Token.", Stderr,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task No_github_actions_o_token_e_o_repositorio_vem_do_ambiente()
    {
        _environment.Remove("AutoRelease_Token");
        _environment.Remove("AutoRelease_Repo");
        _environment["GITHUB_TOKEN"] = "do-actions";
        _environment["GITHUB_REPOSITORY"] = "kempdec/DoActions";
        _repository.Commit("feat: Um.");

        Assert.Equal(0, await RunAsync("version"));
        Assert.Equal(("do-actions", "kempdec", "DoActions"), _repositoryArgs);
    }

    [Fact]
    public async Task O_token_no_arquivo_gera_um_aviso()
    {
        WriteConfig("""{ "Token": "ghp_exposto" }""");
        _repository.Commit("feat: Um.");

        Assert.Equal(0, await RunAsync("version"));
        Assert.Contains("Aviso: O token está no arquivo de configuração", Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_falha_do_github_sai_com_um_sem_pilha_de_chamadas()
    {
        _repository.Failure = new AutoReleaseException("O GitHub recusou o token ao ler as tags.", "Confira o token.");

        Assert.Equal(1, await RunAsync("note"));
        Assert.Equal("Erro: O GitHub recusou o token ao ler as tags.\nConfira o token.\n", Stderr);
    }

    [Fact]
    public async Task O_erro_inesperado_tambem_sai_com_um_e_aponta_onde_relatar()
    {
        _repository.Failure = new InvalidOperationException("quebrou");

        Assert.Equal(1, await RunAsync("note"));
        Assert.Contains("Algo inesperado aconteceu: quebrou", Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("   at ", Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Note_le_os_tipos_da_secao_do_create_e_escreve_as_notas()
    {
        WriteConfig("""
            {
              "CreateCommand": {
                "Types": ["feat=Novidades desta versão", "fix=Correções"],
                "Replaces": { "Adicionar a ": "Foi adicionada a ", "Adicionar": "Foi adicionado" }
              }
            }
            """);
        _repository.Commit("feat: Adicionar a busca.");

        Assert.Equal(0, await RunAsync("note"));
        Assert.Equal("# Novidades desta versão\n\n- Foi adicionada a busca.\n", Stdout);
    }

    [Fact]
    public async Task Version_escreve_so_a_versao()
    {
        _repository.Commit("feat: Um.");
        _repository.Tag("v2.3.4");
        _repository.Commit("fix!: Quebrar.");

        Assert.Equal(0, await RunAsync("version"));
        Assert.Equal("3.0.0\n", Stdout);
    }

    [Fact]
    public async Task O_argumento_desconhecido_falha_com_codigo_diferente_de_zero()
    {
        Assert.NotEqual(0, await RunAsync("create", "--nao-existe"));
    }

    [Fact]
    public async Task Lint_aceita_a_mensagem_dentro_das_regras()
    {
        Assert.Equal(0, await RunAsync("lint", "--message", "feat: Adicionar a busca."));
        Assert.Contains("A mensagem segue as regras do projeto.", Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Lint_le_o_arquivo_do_gancho_e_recusa_com_um()
    {
        WriteConfig("""{ "CreateCommand": { "Types": ["feat=Novidades"] }, "LintCommand": { "RequirePeriod": true } }""");
        File.WriteAllText(Path.Combine(_directory, "COMMIT_EDITMSG"), "fix: corrigir\n# comentário do Git\n");

        Assert.Equal(1, await RunAsync("lint", "--file", "COMMIT_EDITMSG"));
        Assert.Contains("✗ fix: corrigir", Stderr, StringComparison.Ordinal);
        Assert.Contains("O tipo \"fix\" não está entre os do projeto.", Stderr, StringComparison.Ordinal);
        Assert.Contains("O título não termina em ponto final.", Stderr, StringComparison.Ordinal);
        Assert.Contains("1 mensagem está fora das regras do projeto.", Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Lint_le_o_intervalo_pelo_git_e_aponta_o_commit()
    {
        _gitOutput = "aaaaaaa1111\u001ffeat: Adicionar a busca.\n\u001e\nbbbbbbb2222\u001fajuste rápido\n\u001e\n";

        Assert.Equal(1, await RunAsync("lint", "--from", "origin/main"));
        Assert.Equal(["log", "--no-merges", "--format=%H%x1f%B%x1e", "origin/main..HEAD"], _gitArgs);
        Assert.Contains("✗ bbbbbbb ajuste rápido", Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("aaaaaaa", Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Lint_sem_fonte_diz_o_que_passar()
    {
        Assert.Equal(1, await RunAsync("lint"));
        Assert.Contains("Falta dizer o que conferir.", Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task O_erro_do_octokit_nao_escapa_do_repositorio_de_verdade()
    {
        // O repositório de verdade traduz as exceções do Octokit; aqui se confere que uma que escape vira erro limpo.
        _repository.Failure = new NotFoundException("Not Found", System.Net.HttpStatusCode.NotFound);

        Assert.Equal(1, await RunAsync("version"));
        Assert.StartsWith("Erro: Algo inesperado aconteceu", Stderr, StringComparison.Ordinal);
    }

    /// <summary>
    /// Representa um relógio parado num instante.
    /// </summary>
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
