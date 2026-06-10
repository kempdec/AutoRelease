using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Commits.Types;
using Xunit;

namespace AutoRelease.Tests.Commits;

/// <summary>
/// Testes para o parser <see cref="CommitMessage"/>.
/// </summary>
public class CommitMessageTests
{
    [Theory]
    [InlineData("feat: Adicionar X.", "feat", "Adicionar X.")]
    [InlineData("fix: Corrigir Y.", "fix", "Corrigir Y.")]
    [InlineData("chore: Tarefa Z.", "chore", "Tarefa Z.")]
    public void Construtor_ComTipoConhecido_DefineTipoEDescricao(string message, string expectedKey,
        string expectedDescription)
    {
        var commit = new CommitMessage(message);

        Assert.Equal(expectedKey, commit.Type.Key);
        Assert.Equal(expectedDescription, commit.Description);
        Assert.Equal(expectedDescription, commit.ReleaseDescription);
        Assert.False(commit.Ignore);
        Assert.False(commit.IsBreakingChange);
    }

    [Fact]
    public void Construtor_SemTipo_UsaTipoPadrao()
    {
        var commit = new CommitMessage("Mensagem sem tipo");

        Assert.IsType<DefaultCommitMessageType>(commit.Type);
        Assert.Equal("Mensagem sem tipo", commit.Description);
    }

    [Fact]
    public void Construtor_TipoDesconhecido_UsaTipoPadrao()
    {
        var commit = new CommitMessage("desconhecido: Algo qualquer.");

        Assert.IsType<DefaultCommitMessageType>(commit.Type);
        Assert.Equal("Algo qualquer.", commit.Description);
    }

    [Fact]
    public void Construtor_OperadorQuebra_DefineIsBreakingChange()
    {
        var commit = new CommitMessage("feat!: Mudança incompatível.");

        Assert.True(commit.IsBreakingChange);
        Assert.Equal("feat", commit.Type.Key);
        Assert.Equal("Mudança incompatível.", commit.Description);
    }

    [Fact]
    public void Construtor_OperadorIgnorar_DefineIgnore()
    {
        var commit = new CommitMessage("chore#: Atualizar os pacotes.");

        Assert.True(commit.Ignore);
        Assert.Equal("chore", commit.Type.Key);
    }

    [Fact]
    public void Construtor_OperadorCorpoComoDescricao_UsaCorpoNaDescricaoDoRelease()
    {
        var commit = new CommitMessage("feat^: Descrição ignorada.\nCorpo usado.");

        Assert.Equal("Descrição ignorada.", commit.Description);
        Assert.Equal("Corpo usado.", commit.ReleaseDescription);
    }

    [Fact]
    public void Construtor_ComCorpo_ConcatenaComQuebraDeLinhaHtml()
    {
        var commit = new CommitMessage("feat: Descrição.\nCorpo da mensagem.");

        Assert.Equal("Corpo da mensagem.", commit.Body);
        Assert.Equal("Descrição.<br>Corpo da mensagem.", commit.ReleaseDescription);
    }

    [Fact]
    public void Construtor_DescricaoNoIgnores_DefineIgnore()
    {
        var commit = new CommitMessage("chore: Atualizar a versão do pacote.", ignores: ["Atualizar a versão"]);

        Assert.True(commit.Ignore);
    }

    [Fact]
    public void Construtor_ComReplaces_SubstituiInicioDaDescricaoDoRelease()
    {
        var replaces = new List<(string OldValue, string NewValue)> { ("Adicionar a ", "Foi adicionada a ") };

        var commit = new CommitMessage("feat: Adicionar a página de cadastro.", replaces: replaces);

        Assert.Equal("Foi adicionada a página de cadastro.", commit.ReleaseDescription);
    }

    [Fact]
    public void Construtor_ComTiposPersonalizados_UsaTipoCorrespondente()
    {
        var types = new List<CommitMessageType>
        {
            new() { Key = "feat", Label = "Novidades", Ordering = 0 }
        };

        var commit = new CommitMessage("feat: Algo novo.", types: types);

        Assert.Equal("feat", commit.Type.Key);
        Assert.Equal("Novidades", commit.Type.Label);
    }

    [Fact]
    public void Construtor_ComAutor_AdicionaNomeNoFinalDaDescricaoDoRelease()
    {
        var commit = new CommitMessage("feat: Algo novo.", authorName: "Fulano");

        Assert.Equal("Algo novo. [Fulano]", commit.ReleaseDescription);
    }
}
