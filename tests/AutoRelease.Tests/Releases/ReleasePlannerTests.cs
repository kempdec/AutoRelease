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
}
