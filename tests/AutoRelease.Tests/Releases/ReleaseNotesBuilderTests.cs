using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Releases;

namespace KempDec.AutoRelease.Tests.Releases;

public sealed class ReleaseNotesBuilderTests
{
    private static ReleaseCommit[] Commits(params string[] messages) =>
        [.. messages.Select((message, index) => ReleaseCommit.Create($"sha{index}", "Ana Lima", message))];

    [Fact]
    public void Agrupa_por_tipo_na_ordem_dos_tipos_com_os_commits_do_mais_antigo_ao_mais_novo()
    {
        string notes = ReleaseNotesBuilder.Build(
            Commits("fix: Corrigir a busca.", "feat: Adicionar o filtro.", "feat: Adicionar a lista.", "Sem tipo"),
            ReleaseRules.Default);

        Assert.Equal(
            "# Novidades\n\n- Adicionar o filtro.\n- Adicionar a lista.\n\n"
            + "# Correções\n\n- Corrigir a busca.\n\n"
            + "# Outros\n\n- Sem tipo\n",
            notes);
    }

    [Fact]
    public void Tira_os_ignorados_e_os_repetidos()
    {
        var rules = new ReleaseRules { Ignores = ["Merge"] };

        string notes = ReleaseNotesBuilder.Build(
            Commits("fix: Corrigir a busca.", "fix: Corrigir a busca.", "chore#: Atualizar.", "Merge branch 'x'"),
            rules);

        Assert.Equal("# Correções\n\n- Corrigir a busca.\n", notes);
    }

    [Fact]
    public void Troca_o_comeco_poe_o_autor_antes_do_corpo_e_o_escopo_na_frente()
    {
        var rules = new ReleaseRules
        {
            Rewriter = DescriptionRewriter.Create([new("Corrigir", "Foi corrigido")]),
            ShowAuthor = true
        };

        string notes = ReleaseNotesBuilder.Build(
            Commits("fix(api): Corrigir a rota.\n\nEla voltava 500.\nAgora volta 200."), rules);

        Assert.Equal("# Correções\n\n- **api:** Foi corrigido a rota. [Ana Lima]<br>Ela voltava 500.<br>Agora volta 200.\n",
            notes);
    }

    [Fact]
    public void O_operador_circunflexo_usa_so_o_corpo()
    {
        string notes = ReleaseNotesBuilder.Build(
            Commits("feat^: Ignorada.\n\nSó o corpo entra."), ReleaseRules.Default);

        Assert.Equal("# Novidades\n\n- Só o corpo entra.\n", notes);
    }

    [Fact]
    public void Sem_commits_que_entram_as_notas_ficam_vazias()
    {
        Assert.Equal(string.Empty, ReleaseNotesBuilder.Build(Commits("chore#: Interno."), ReleaseRules.Default));
    }
}
