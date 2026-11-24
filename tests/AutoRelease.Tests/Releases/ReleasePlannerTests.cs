using KempDec.AutoRelease.GitHub;
using KempDec.AutoRelease.Releases;
using KempDec.AutoRelease.Tests.Fakes;
using Semver;

namespace KempDec.AutoRelease.Tests.Releases;

public sealed class ReleasePlannerTests
{
    [Fact]
    public async Task Le_os_commits_desde_a_tag_da_versao_anterior_e_nao_pela_data()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Adicionar a busca.");
        repository.Tag("v1.0.0");
        repository.Commit("fix: Corrigir a busca.");
        string target = repository.Commit("fix: Corrigir o filtro.");

        ReleasePlan plan = await new ReleasePlanner(repository).PlanAsync(new ReleasePlanRequest(),
            TestContext.Current.CancellationToken);

        Assert.Equal(target, plan.TargetSha);
        Assert.Equal("v1.0.0", plan.PreviousTag?.Name);
        Assert.Equal(2, plan.Commits.Count);
        Assert.Equal("1.0.1", plan.Version.ToString());
        Assert.Equal("v1.0.1", plan.TagName);
        Assert.False(plan.IsEmpty);
        Assert.Null(plan.ExistingTag);
    }

    [Fact]
    public async Task A_release_sai_do_commit_informado_e_nao_da_ponta_do_branch()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Adicionar a busca.");
        repository.Tag("v1.0.0");
        string approved = repository.Commit("fix: Corrigir a busca.");

        repository.Commit("feat: Adicionar depois da aprovação.");

        ReleasePlan plan = await new ReleasePlanner(repository).PlanAsync(
            new ReleasePlanRequest { Reference = approved }, TestContext.Current.CancellationToken);

        Assert.Equal(approved, plan.TargetSha);
        Assert.Single(plan.Commits);
        Assert.Equal("1.0.1", plan.Version.ToString());
    }

    [Fact]
    public async Task A_versao_anterior_e_a_maior_tag_com_o_prefixo_e_abaixo_da_versao_informada()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.9.0");
        repository.Commit("feat: Dois.");
        repository.Tag("v1.10.0");
        repository.Commit("feat: Três.");
        repository.Tag("release-9.0.0");
        repository.Tag("v2.0.0", withRelease: false);

        ReleasePlan plan = await new ReleasePlanner(repository).PlanAsync(
            new ReleasePlanRequest { Version = SemVersion.Parse("2.0.0") }, TestContext.Current.CancellationToken);

        Assert.Equal("v1.10.0", plan.PreviousTag?.Name);
        Assert.Equal("v2.0.0", plan.TagName);
        Assert.NotNull(plan.ExistingTag);
        Assert.Single(plan.Commits);
    }

    [Fact]
    public async Task Sem_tag_le_a_historia_inteira_e_usa_a_primeira_versao()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Commit("fix: Dois.");

        ReleasePlan plan = await new ReleasePlanner(repository).PlanAsync(
            new ReleasePlanRequest { FirstVersion = SemVersion.Parse("0.1.0") }, TestContext.Current.CancellationToken);

        Assert.Null(plan.PreviousTag);
        Assert.Equal(2, plan.Commits.Count);
        Assert.Equal("0.1.0", plan.Version.ToString());
    }

    [Fact]
    public async Task So_commits_ignorados_deixam_a_release_vazia()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.0.0");
        repository.Commit("feat#: Prévia.");

        ReleasePlan plan = await new ReleasePlanner(repository).PlanAsync(new ReleasePlanRequest(),
            TestContext.Current.CancellationToken);

        Assert.True(plan.IsEmpty);
        Assert.Equal(string.Empty, plan.Notes);
    }

    [Fact]
    public void Procura_a_tag_anterior_pela_precedencia_da_versao()
    {
        RepositoryVersionTag? previous = ReleasePlanner.FindPreviousTag(
            [
                new RepositoryTagInfo("v1.2.0", "a"),
                new RepositoryTagInfo("v1.10.0", "b"),
                new RepositoryTagInfo("v1.10.0-beta.1", "c"),
                new RepositoryTagInfo("vlixo", "d")
            ],
            "v",
            version: null);

        Assert.Equal("v1.10.0", previous?.Name);
    }

    private static async Task<ReleasePlan> PlanAsync(FakeReleaseRepository repository, string? label = null,
        string? version = null) =>
        await new ReleasePlanner(repository).PlanAsync(
            new ReleasePlanRequest
            {
                PreReleaseLabel = label,
                Version = version is null ? null : SemVersion.Parse(version)
            },
            TestContext.Current.CancellationToken);

    [Fact]
    public async Task A_primeira_beta_e_a_proxima_versao_com_o_numero_1_e_as_notas_desde_a_final()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.4.0");
        repository.Commit("feat: Adicionar a busca.");

        ReleasePlan plan = await PlanAsync(repository, "beta");

        Assert.Equal("v1.5.0-beta.1", plan.TagName);
        Assert.True(plan.Version.IsPrerelease);
        Assert.Equal("v1.4.0", plan.PreviousTag?.Name);
        Assert.Equal("# Novidades\n\n- Adicionar a busca.\n", plan.Notes);
    }

    [Fact]
    public async Task A_beta_seguinte_sobe_o_numero_e_leva_so_as_notas_desde_a_beta_anterior()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.4.0");
        repository.Commit("feat: Adicionar a busca.");
        repository.Tag("v1.5.0-beta.1");
        repository.Commit("fix: Corrigir a busca.");

        ReleasePlan plan = await PlanAsync(repository, "beta");

        Assert.Equal("v1.5.0-beta.2", plan.TagName);
        Assert.Equal("v1.5.0-beta.1", plan.PreviousTag?.Name);
        Assert.Single(plan.Commits);
        Assert.Equal("# Correções\n\n- Corrigir a busca.\n", plan.Notes);
    }

    [Fact]
    public async Task A_versao_final_depois_das_betas_leva_as_notas_desde_a_ultima_final()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.4.0");
        repository.Commit("feat: Adicionar a busca.");
        repository.Tag("v1.5.0-beta.1");
        repository.Commit("fix: Corrigir a busca.");
        repository.Tag("v1.5.0-beta.2");
        repository.Commit("fix: Corrigir o filtro.");

        ReleasePlan plan = await PlanAsync(repository);

        Assert.Equal("v1.5.0", plan.TagName);
        Assert.Equal("v1.4.0", plan.PreviousTag?.Name);
        Assert.Equal(3, plan.Commits.Count);
        Assert.Equal(
            "# Novidades\n\n- Adicionar a busca.\n\n# Correções\n\n- Corrigir a busca.\n- Corrigir o filtro.\n",
            plan.Notes);
    }

    [Fact]
    public async Task A_versao_final_informada_tambem_leva_as_notas_desde_a_ultima_final()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.4.0");
        repository.Commit("feat: Adicionar a busca.");
        repository.Tag("v1.5.0-beta.1");
        repository.Commit("fix: Corrigir a busca.");

        ReleasePlan plan = await PlanAsync(repository, version: "1.5.0");

        Assert.Equal("v1.4.0", plan.PreviousTag?.Name);
        Assert.Equal(2, plan.Commits.Count);
    }

    [Fact]
    public async Task A_beta_nao_faz_a_final_saltar_mais_uma_versao()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.4.0");
        repository.Commit("feat: Adicionar a busca.");
        repository.Tag("v1.5.0-beta.1");
        repository.Commit("feat: Adicionar o filtro.");

        Assert.Equal("v1.5.0-beta.2", (await PlanAsync(repository, "beta")).TagName);
        Assert.Equal("v1.5.0", (await PlanAsync(repository)).TagName);
    }

    [Fact]
    public async Task A_final_nao_fica_abaixo_da_versao_que_a_pre_release_anunciou()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.4.0");
        repository.Commit("fix: Corrigir a busca.");
        repository.Tag("v2.0.0-beta.1");
        repository.Commit("fix: Corrigir o filtro.");

        ReleasePlan plan = await PlanAsync(repository);

        Assert.Equal("v2.0.0", plan.TagName);
        Assert.Equal("v1.4.0", plan.PreviousTag?.Name);
    }

    [Fact]
    public async Task Cada_rotulo_tem_a_sua_contagem_e_a_versao_informada_ganha_o_rotulo()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.4.0");
        repository.Commit("feat: Adicionar a busca.");
        repository.Tag("v1.5.0-beta.1");
        repository.Tag("v1.5.0-beta.2");
        repository.Commit("fix: Corrigir a busca.");

        Assert.Equal("v1.5.0-rc.1", (await PlanAsync(repository, "rc")).TagName);
        Assert.Equal("v2.0.0-beta.1", (await PlanAsync(repository, "beta", "2.0.0")).TagName);
    }

    [Fact]
    public async Task Depois_da_final_a_beta_seguinte_parte_dela()
    {
        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Um.");
        repository.Tag("v1.4.0");
        repository.Commit("feat: Adicionar a busca.");
        repository.Tag("v1.5.0-beta.1");
        repository.Commit("fix: Corrigir a busca.");
        repository.Tag("v1.5.0");
        repository.Commit("fix: Corrigir o filtro.");

        ReleasePlan plan = await PlanAsync(repository, "beta");

        Assert.Equal("v1.5.1-beta.1", plan.TagName);
        Assert.Equal("v1.5.0", plan.PreviousTag?.Name);
        Assert.Single(plan.Commits);
    }
}
