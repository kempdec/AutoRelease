using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Errors;

namespace KempDec.AutoRelease.Tests.Commits;

public sealed class CommitTypeCatalogTests
{
    [Fact]
    public void Sem_configuracao_valem_feat_fix_e_chore()
    {
        CommitTypeCatalog catalog = CommitTypeCatalog.Parse([]);

        Assert.False(catalog.IsConfigured);
        Assert.Equal(["feat", "fix", "chore"], catalog.Types.Select(type => type.Key));
        Assert.Equal("feat", catalog.MinorType.Key);
    }

    [Fact]
    public void Le_os_tipos_na_ordem_e_o_primeiro_e_o_das_novidades()
    {
        CommitTypeCatalog catalog = CommitTypeCatalog.Parse(["novidade=Novidades", "fix=Correções = e mais"]);

        Assert.True(catalog.IsConfigured);
        Assert.Equal("novidade", catalog.MinorType.Key);
        Assert.Equal("Correções = e mais", catalog.Types[1].Label);
        Assert.Equal(1, catalog.Types[1].Order);
    }

    [Fact]
    public void O_tipo_desconhecido_vai_para_outros()
    {
        CommitTypeCatalog catalog = CommitTypeCatalog.Parse(["feat=Novidades"]);

        Assert.Equal(CommitTypeCatalog.Other, catalog.Resolve(CommitMessage.Parse("infra: Corrigir o servidor.")));
        Assert.Equal(CommitTypeCatalog.Other, catalog.Resolve(CommitMessage.Parse("Atualizar sem tipo.")));
    }

    [Theory]
    [InlineData("feat")]
    [InlineData("=Novidades")]
    [InlineData("feat=")]
    public void Recusa_o_tipo_fora_do_formato(string entry)
    {
        AutoReleaseException error = Assert.Throws<AutoReleaseException>(() => CommitTypeCatalog.Parse([entry]));

        Assert.Contains("chave=Título", error.Problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Recusa_o_tipo_repetido()
    {
        Assert.Throws<AutoReleaseException>(() => CommitTypeCatalog.Parse(["feat=A", "FEAT=B"]));
    }
}
