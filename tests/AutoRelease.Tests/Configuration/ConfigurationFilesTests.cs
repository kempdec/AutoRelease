using KempDec.AutoRelease.Commits;
using KempDec.AutoRelease.Configuration;
using Microsoft.Extensions.Configuration;

namespace KempDec.AutoRelease.Tests.Configuration;

/// <summary>
/// Confere que os arquivos de configuração do repositório carregam pelo código de verdade: o modelo do README e a
/// configuração do próprio Auto Release, que o fluxo de publicação usa.
/// </summary>
public sealed class ConfigurationFilesTests
{
    [Theory]
    [InlineData("autorelease.config.json")]
    [InlineData("src/AutoRelease/autorelease.config.json")]
    public void O_arquivo_carrega_com_tipos_trocas_e_lint_validos(string relativePath)
    {
        string path = Path.Combine(FindRepositoryRoot(), relativePath);
        (IConfiguration configuration, bool fileHasToken) = ConfigurationLoader.Load(
            Path.GetDirectoryName(path)!, path, new Dictionary<string, string?>());
        var create = new SettingsReader(configuration, ["CreateCommand"]);
        var lint = new SettingsReader(configuration, ["LintCommand", "CreateCommand"]);

        Assert.False(fileHasToken);
        Assert.True(CommitTypeCatalog.Parse(create.GetList("Types")).IsConfigured);
        Assert.NotEmpty(DescriptionRewriter.Create(create.GetPairs("Replaces")).LeadingWords);
        Assert.NotNull(create.GetVersion("FirstVersion"));
        _ = lint.GetBool("RequirePeriod");
    }

    /// <summary>
    /// Procura a raiz do repositório, a pasta da solução, subindo a partir da pasta dos testes.
    /// </summary>
    /// <returns>O caminho da raiz.</returns>
    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AutoRelease.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("A raiz do repositório não foi encontrada.");
    }
}
