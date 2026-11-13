using KempDec.AutoRelease.Cli;
using KempDec.AutoRelease.Configuration;
using KempDec.AutoRelease.Tests.Fakes;

namespace KempDec.AutoRelease.Tests.Compatibility;

/// <summary>
/// Quem atualiza para a versão 2 sem mexer em nada tem a mesma versão e as mesmas notas que a versão 0 publicou. Cada
/// caso é uma release de verdade do KempDec Studio: a configuração dele como estava, os commits da tag anterior até a
/// da release, e as notas como a versão 0 (a Action v2) as publicou no GitHub.
/// </summary>
public sealed class PublishedReleasesTests : IDisposable
{
    private static readonly string Fixtures = Path.Combine(AppContext.BaseDirectory, "Compatibility", "Fixtures");

    private readonly string _directory = Directory.CreateTempSubdirectory("autorelease-compat-").FullName;

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Theory]
    [InlineData("v1.5.2", "v1.6.0")]
    [InlineData("v1.7.1", "v1.8.0")]
    public async Task A_versao_2_com_a_configuracao_de_hoje_repete_a_release_publicada(string anterior, string publicada)
    {
        File.Copy(Path.Combine(Fixtures, "kempdec-studio.config.json"),
            Path.Combine(_directory, ConfigurationLoader.DefaultFileName));

        var repository = new FakeReleaseRepository();

        repository.Commit("feat: Commit anterior à release.");
        repository.Tag(anterior);

        foreach (string message in File.ReadAllLines(Path.Combine(Fixtures, $"kempdec-studio-{publicada}.commits.txt")))
        {
            if (message.Length > 0)
            {
                repository.Commit(message);
            }
        }

        using var output = new StringWriter();
        using var error = new StringWriter();

        var context = new CliContext
        {
            Output = output,
            Error = error,
            Environment = new Dictionary<string, string?>
            {
                ["AutoRelease_Token"] = "token-de-teste",
                ["AutoRelease_Repo"] = "kempdec/studio"
            },
            WorkingDirectory = _directory,
            CreateRepository = (_, _, _, _) => repository,
            Clock = TimeProvider.System,
            RunGit = (_, _) => Task.FromResult(string.Empty)
        };

        int exitCode = await new AutoReleaseCli(context).InvokeAsync(["create"], TestContext.Current.CancellationToken);

        Assert.True(exitCode == 0, error.ToString());

        var created = Assert.Single(repository.Created);
        string expected = File.ReadAllText(Path.Combine(Fixtures, $"kempdec-studio-{publicada}.notes.md"));

        Assert.Equal(publicada, created.TagName);
        Assert.Equal(expected.ReplaceLineEndings("\n").TrimEnd(), created.Body.ReplaceLineEndings("\n").TrimEnd());
    }
}
