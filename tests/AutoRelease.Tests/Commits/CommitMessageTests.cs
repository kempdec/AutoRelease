using KempDec.AutoRelease.Commits;

namespace KempDec.AutoRelease.Tests.Commits;

public sealed class CommitMessageTests
{
    [Fact]
    public void Le_o_tipo_e_a_descricao()
    {
        var message = CommitMessage.Parse("feat: Adicionar a busca.");

        Assert.Equal("feat", message.Type);
        Assert.Equal("Adicionar a busca.", message.Description);
        Assert.Null(message.Scope);
        Assert.Empty(message.Body);
        Assert.False(message.IsBreakingChange);
    }

    [Fact]
    public void Mantem_os_dois_pontos_da_descricao()
    {
        var message = CommitMessage.Parse("fix: Corrigir o horário: ele mostrava o dia seguinte.");

        Assert.Equal("Corrigir o horário: ele mostrava o dia seguinte.", message.Description);
    }

    [Fact]
    public void Le_o_escopo_e_o_tipo_em_minusculas()
    {
        var message = CommitMessage.Parse("Feat(api): Adicionar a rota.");

        Assert.Equal("feat", message.Type);
        Assert.Equal("api", message.Scope);
        Assert.Equal("Adicionar a rota.", message.Description);
    }

    [Theory]
    [InlineData("feat!: Remover a rota antiga.", true, false, false)]
    [InlineData("chore#: Atualizar os pacotes.", false, true, false)]
    [InlineData("fix^: Ignorada.", false, false, true)]
    [InlineData("feat(api)!#: Mudar e esconder.", true, true, false)]
    public void Le_os_operadores(string text, bool breaking, bool hidden, bool bodyOnly)
    {
        var message = CommitMessage.Parse(text);

        Assert.Equal(breaking, message.IsBreakingChange);
        Assert.Equal(hidden, message.IsHidden);
        Assert.Equal(bodyOnly, message.UsesBodyAsDescription);
        Assert.True(message.IsConventional);
    }

    [Fact]
    public void Mensagem_fora_do_formato_fica_sem_tipo_com_o_titulo_inteiro()
    {
        var message = CommitMessage.Parse("Merge branch 'develop' into main");

        Assert.Null(message.Type);
        Assert.False(message.IsConventional);
        Assert.Equal("Merge branch 'develop' into main", message.Description);
    }

    [Fact]
    public void Separa_o_corpo_das_linhas_em_branco_das_pontas()
    {
        var message = CommitMessage.Parse("fix: Corrigir a busca.\r\n\r\nLinha um.\r\nLinha dois.\r\n\r\n");

        Assert.Equal(["Linha um.", "Linha dois."], message.Body);
    }

    [Fact]
    public void Tira_os_rodapes_do_git_do_corpo()
    {
        var message = CommitMessage.Parse(
            "fix: Corrigir a busca.\n\nO filtro voltava vazio.\n\nCo-Authored-By: Alguém <a@b.c>\nSigned-off-by: Ana <a@b.c>");

        Assert.Equal(["O filtro voltava vazio."], message.Body);
    }

    [Fact]
    public void Mantem_no_corpo_uma_linha_de_uma_palavra_com_dois_pontos()
    {
        var message = CommitMessage.Parse("fix: Corrigir a busca.\n\nNota: só vale no celular.");

        Assert.Equal(["Nota: só vale no celular."], message.Body);
    }

    [Theory]
    [InlineData("feat: Trocar a API.\n\nBREAKING CHANGE: a rota antiga saiu.")]
    [InlineData("feat: Trocar a API.\n\nBREAKING-CHANGE: a rota antiga saiu.")]
    public void O_rodape_breaking_change_marca_a_quebra(string text)
    {
        var message = CommitMessage.Parse(text);

        Assert.True(message.IsBreakingChange);
        Assert.Empty(message.Body);
    }
}
