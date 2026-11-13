using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Errors;

namespace KempDec.AutoRelease.Tests.Commits;

public sealed class DescriptionRewriterTests
{
    /// <summary>
    /// As trocas na ordem em que o próprio Auto Release as tinha na configuração: a curta antes da longa.
    /// </summary>
    private static readonly KeyValuePair<string, string>[] s_replaces =
    [
        new("Adicionar", "Foi adicionado"),
        new("Adicionar o ", "Foi adicionado o "),
        new("Adicionar a ", "Foi adicionada a "),
        new("Corrigir", "Foi corrigido")
    ];

    [Fact]
    public void Usa_a_troca_mais_longa_qualquer_que_seja_a_ordem()
    {
        DescriptionRewriter rewriter = DescriptionRewriter.Create(s_replaces);

        Assert.Equal("Foi adicionada a busca.", rewriter.Rewrite("Adicionar a busca."));
        Assert.Equal("Foi adicionado o filtro.", rewriter.Rewrite("Adicionar o filtro."));
        Assert.Equal("Foi adicionado suporte.", rewriter.Rewrite("Adicionar suporte."));
    }

    [Fact]
    public void Troca_uma_vez_so_e_sem_diferenciar_maiusculas()
    {
        DescriptionRewriter rewriter = DescriptionRewriter.Create(
            [new("corrigir", "Foi corrigido"), new("Foi corrigido", "Errado")]);

        Assert.Equal("Foi corrigido o filtro.", rewriter.Rewrite("Corrigir o filtro."));
    }

    [Fact]
    public void Mantem_a_descricao_sem_troca()
    {
        Assert.Equal("Remover a rota.", DescriptionRewriter.Create(s_replaces).Rewrite("Remover a rota."));
    }

    [Fact]
    public void Diz_as_palavras_que_abrem_as_trocas()
    {
        Assert.Equal(["Adicionar", "Corrigir"], DescriptionRewriter.Create(s_replaces).LeadingWords);
    }

    [Fact]
    public void Recusa_a_troca_com_o_comeco_vazio()
    {
        AutoReleaseException error = Assert.Throws<AutoReleaseException>(
            () => DescriptionRewriter.Create([new(" ", "Algo")]));

        Assert.Contains("começo vazio", error.Problem, StringComparison.Ordinal);
    }
}
