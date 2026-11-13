using KempDec.AutoRelease.Configuration;
using KempDec.AutoRelease.Errors;
using Microsoft.Extensions.Configuration;

namespace KempDec.AutoRelease.Tests.Configuration;

public sealed class SettingsReaderTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("autorelease-").FullName;

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private SettingsReader Load(string? json, Dictionary<string, string?>? environment = null,
        params string[] sections)
    {
        if (json is not null)
        {
            File.WriteAllText(Path.Combine(_directory, ConfigurationLoader.DefaultFileName), json);
        }

        (IConfiguration configuration, _) = ConfigurationLoader.Load(_directory, null, environment ?? []);

        return new SettingsReader(configuration, sections);
    }

    [Fact]
    public void A_linha_de_comando_vale_sobre_a_secao_que_vale_sobre_a_raiz()
    {
        SettingsReader reader = Load("""
            {
              // Comentário aceito.
              "ProjectName": "Raiz",
              "CreateCommand": { "ProjectName": "Seção", },
            }
            """, null, "CreateCommand");

        Assert.Equal("Linha", reader.GetString("ProjectName", "Linha"));
        Assert.Equal("Seção", reader.GetString("ProjectName"));
        Assert.Equal("Raiz", Load(null, null, "NoteCommand").GetString("ProjectName"));
    }

    [Fact]
    public void A_variavel_de_ambiente_vale_sobre_o_arquivo()
    {
        SettingsReader reader = Load(
            """{ "CreateCommand": { "Types": ["feat=Do arquivo"] } }""",
            new() { ["AutoRelease_CreateCommand__Types"] = "feat=\"Da variável\" fix=Correções" },
            "CreateCommand");

        Assert.Equal(["feat=Da variável", "fix=Correções"], reader.GetList("Types"));
    }

    [Fact]
    public void Le_a_lista_do_arquivo_na_ordem()
    {
        SettingsReader reader = Load("""{ "Ignores": ["Merge", "Refatorar.", "Atualizar a versão"] }""");

        Assert.Equal(["Merge", "Refatorar.", "Atualizar a versão"], reader.GetList("Ignores"));
    }

    [Fact]
    public void Le_as_trocas_mantendo_o_espaco_do_fim_das_chaves()
    {
        SettingsReader reader = Load("""{ "Replaces": { "Adicionar a ": "Foi adicionada a ", "Corrigir": "Foi corrigido" } }""");

        IReadOnlyList<KeyValuePair<string, string>> pairs = reader.GetPairs("Replaces");

        Assert.Contains(new KeyValuePair<string, string>("Adicionar a ", "Foi adicionada a "), pairs);
        Assert.Contains(new KeyValuePair<string, string>("Corrigir", "Foi corrigido"), pairs);
    }

    [Fact]
    public void Le_a_versao_com_ou_sem_o_prefixo()
    {
        SettingsReader reader = Load(null, new() { ["AutoRelease_Version"] = "v1.2.3" });

        Assert.Equal("1.2.3", reader.GetVersion("Version")?.ToString());
        Assert.Equal("2.0.0", reader.GetVersion("Version", "2.0.0")?.ToString());
    }

    [Fact]
    public void Recusa_a_versao_e_o_sinalizador_invalidos_em_duas_partes()
    {
        SettingsReader reader = Load("""{ "Version": "um", "Draft": "talvez" }""");

        AutoReleaseException version = Assert.Throws<AutoReleaseException>(() => reader.GetVersion("Version"));
        AutoReleaseException draft = Assert.Throws<AutoReleaseException>(() => reader.GetBool("Draft"));

        Assert.Contains("não é uma versão semântica", version.Problem, StringComparison.Ordinal);
        Assert.Contains("semver.org", version.NextStep, StringComparison.Ordinal);
        Assert.Contains("true ou false", draft.Problem, StringComparison.Ordinal);
    }

    [Fact]
    public void O_arquivo_invalido_vira_erro_em_duas_partes()
    {
        AutoReleaseException error = Assert.Throws<AutoReleaseException>(() => Load("{ \"Types\": [ "));

        Assert.Contains("não é um JSON válido", error.Problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Separa_os_itens_como_a_linha_de_comando()
    {
        Assert.Equal(
            ["feat=Novidades desta versão", "fix=Correções", ""],
            SettingsReader.SplitArguments("  feat=\"Novidades desta versão\"   fix=Correções \"\" "));
    }
}
