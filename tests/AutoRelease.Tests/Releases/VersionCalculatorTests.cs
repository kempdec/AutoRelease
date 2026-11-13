using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Releases;
using Semver;

namespace KempDec.AutoRelease.Tests.Releases;

public sealed class VersionCalculatorTests
{
    private static readonly SemVersion s_first = new(1, 0, 0);

    private static ReleaseCommit[] Commits(params string[] messages) =>
        [.. messages.Select((message, index) => ReleaseCommit.Create($"sha{index}", "Ana", message))];

    private static string Next(string? previous, ReleaseRules rules, params string[] messages) =>
        VersionCalculator.Next(previous is null ? null : SemVersion.Parse(previous), Commits(messages), rules, s_first)
            .ToString();

    [Fact]
    public void Sem_versao_anterior_vale_a_primeira()
    {
        Assert.Equal("1.0.0", Next(null, ReleaseRules.Default, "feat: Adicionar a busca."));
    }

    [Theory]
    [InlineData("1.2.3", "fix: Corrigir a busca.", "1.2.4")]
    [InlineData("1.2.3", "feat: Adicionar a busca.", "1.3.0")]
    [InlineData("1.2.3", "fix!: Remover a rota antiga.", "2.0.0")]
    [InlineData("1.2.3", "Mensagem fora do formato", "1.2.4")]
    public void Sobe_a_versao_conforme_os_commits(string previous, string message, string expected)
    {
        Assert.Equal(expected, Next(previous, ReleaseRules.Default, message));
    }

    [Fact]
    public void O_commit_escondido_nao_conta()
    {
        Assert.Equal("1.2.4", Next("1.2.3", ReleaseRules.Default, "feat#: Adicionar a prévia.", "fix: Corrigir."));
    }

    [Fact]
    public void O_commit_ignorado_pela_configuracao_nao_conta()
    {
        var rules = new ReleaseRules { Ignores = ["Atualizar a versão"] };

        Assert.Equal("1.2.4", Next("1.2.3", rules, "feat: Atualizar a versão do pacote.", "fix: Corrigir."));
    }

    [Fact]
    public void O_primeiro_tipo_configurado_sobe_a_minor()
    {
        var rules = new ReleaseRules { Types = CommitTypeCatalog.Parse(["novidade=Novidades", "feat=Outra coisa"]) };

        Assert.Equal("1.3.0", Next("1.2.3", rules, "novidade: Adicionar a busca."));
        Assert.Equal("1.2.4", Next("1.2.3", rules, "feat: Adicionar a busca."));
    }

    [Theory]
    [InlineData("2.0.0-beta.1", "feat: Adicionar.", "2.0.0")]
    [InlineData("2.0.0-beta.1", "fix!: Quebrar.", "2.0.0")]
    [InlineData("2.1.0-beta.1", "fix: Corrigir.", "2.1.0")]
    [InlineData("2.1.0-beta.1", "fix!: Quebrar.", "3.0.0")]
    [InlineData("2.1.1-beta.1", "feat: Adicionar.", "2.2.0")]
    public void Depois_de_uma_pre_release_vem_a_estavel_dela(string previous, string message, string expected)
    {
        Assert.Equal(expected, Next(previous, ReleaseRules.Default, message));
    }
}
