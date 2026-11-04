using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Lint;

namespace KempDec.AutoRelease.Tests.Lint;

public sealed class CommitLinterTests
{
    /// <summary>
    /// As regras de um projeto como os da KempDec: tipos, verbos, ponto final e sem rodapé de coautoria.
    /// </summary>
    private static readonly LintRules s_kempdec = new()
    {
        Types = CommitTypeCatalog.Parse(["feat=Novidades", "fix=Correções", "chore=Notas técnicas"]),
        Rewriter = DescriptionRewriter.Create(
            [new("Adicionar", "Foi adicionado"), new("Adicionar a ", "Foi adicionada a "), new("Corrigir", "Foi corrigido")]),
        Ignores = ["Merge"],
        RequirePeriod = true,
        MaxHeaderLength = 60,
        ForbiddenTrailers = ["Co-Authored-By"]
    };

    [Theory]
    [InlineData("feat: Adicionar a busca.")]
    [InlineData("fix#: Corrigir o teste interno.")]
    [InlineData("Merge branch 'develop' into main")]
    [InlineData("Revert \"feat: Adicionar a busca.\"")]
    [InlineData("fixup! feat: Adicionar a busca.")]
    [InlineData("feat: Adicionar a busca.\n# Comentário do Git, que não conta.")]
    public void Aceita_a_mensagem_dentro_das_regras(string message)
    {
        Assert.Empty(CommitLinter.Lint(message, s_kempdec));
    }

    [Fact]
    public void Recusa_o_titulo_fora_do_formato_com_um_exemplo()
    {
        LintProblem problem = Assert.Single(CommitLinter.Lint("Adicionar a busca.", s_kempdec));

        Assert.Contains("tipo: Descrição", problem.Problem, StringComparison.Ordinal);
        Assert.Contains("feat: Adicionar a busca.", problem.NextStep, StringComparison.Ordinal);
    }

    [Fact]
    public void Recusa_o_tipo_fora_da_lista_e_diz_os_aceitos()
    {
        LintProblem problem = Assert.Single(CommitLinter.Lint("infra: Corrigir o servidor.", s_kempdec));

        Assert.Equal("Use um destes: feat, fix, chore.", problem.NextStep);
    }

    [Fact]
    public void Recusa_o_verbo_fora_das_trocas_e_diz_os_aceitos()
    {
        LintProblem problem = Assert.Single(CommitLinter.Lint("fix: Ajustar o filtro.", s_kempdec));

        Assert.Contains("\"Ajustar\"", problem.Problem, StringComparison.Ordinal);
        Assert.Equal("Comece por um destes: Adicionar, Corrigir.", problem.NextStep);
    }

    [Fact]
    public void Aponta_o_ponto_o_tamanho_e_o_rodape_proibido_juntos()
    {
        IReadOnlyList<LintProblem> problems = CommitLinter.Lint(
            "feat: Adicionar a busca com um título comprido demais para a regra\n\nCo-Authored-By: Alguém <a@b.c>",
            s_kempdec);

        Assert.Equal(3, problems.Count);
    }

    [Fact]
    public void Sem_tipos_configurados_aceita_os_do_conventional_commits()
    {
        Assert.Empty(CommitLinter.Lint("docs: escrever o guia", new LintRules()));
        Assert.NotEmpty(CommitLinter.Lint("infra: mudar o servidor", new LintRules()));
    }

    [Fact]
    public void A_mensagem_vazia_e_recusada()
    {
        Assert.Single(CommitLinter.Lint("# só comentário\n\n", s_kempdec));
    }
}
