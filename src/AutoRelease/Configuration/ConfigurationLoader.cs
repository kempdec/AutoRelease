using System.Text.Json;
using KempDec.AutoRelease.Errors;
using Microsoft.Extensions.Configuration;

namespace KempDec.AutoRelease.Configuration;

/// <summary>
/// Classe com métodos auxiliares para carregar a configuração do Auto Release: o arquivo
/// <c>autorelease.config.json</c> e as variáveis de ambiente com o prefixo <c>AutoRelease_</c>.
/// </summary>
internal static class ConfigurationLoader
{
    /// <summary>
    /// O nome do arquivo de configuração, procurado na pasta em que o comando roda.
    /// </summary>
    public const string DefaultFileName = "autorelease.config.json";

    /// <summary>
    /// O prefixo das variáveis de ambiente. Um <c>__</c> no nome separa as seções, como em
    /// <c>AutoRelease_CreateCommand__Types</c>.
    /// </summary>
    public const string EnvironmentPrefix = "AutoRelease_";

    /// <summary>
    /// Carrega a configuração. As variáveis de ambiente valem sobre o arquivo.
    /// </summary>
    /// <param name="workingDirectory">A pasta em que o comando roda.</param>
    /// <param name="filePath">O caminho do arquivo, quando informado; sem ele, vale o
    /// <see cref="DefaultFileName"/> da pasta, se existir.</param>
    /// <param name="environment">As variáveis de ambiente.</param>
    /// <returns>A configuração e um sinalizador indicando se o arquivo tem o token.</returns>
    /// <exception cref="AutoReleaseException">Quando o arquivo informado não existe ou não é um JSON válido.</exception>
    public static (IConfiguration Configuration, bool FileHasToken) Load(string workingDirectory, string? filePath,
        IReadOnlyDictionary<string, string?> environment)
    {
        string path = Path.GetFullPath(filePath ?? DefaultFileName, workingDirectory);

        if (filePath is not null && !File.Exists(path))
        {
            throw new AutoReleaseException(
                $"O arquivo de configuração {path} não existe.",
                "Confira o caminho em --config, ou tire a opção para usar o autorelease.config.json da pasta.");
        }

        var builder = new ConfigurationBuilder();
        bool fileHasToken = false;

        if (File.Exists(path))
        {
            fileHasToken = HasToken(path);
            builder.AddJsonFile(path, optional: false, reloadOnChange: false);
        }

        builder.AddInMemoryCollection(environment
            .Where(variable => variable.Key.StartsWith(EnvironmentPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(variable => new KeyValuePair<string, string?>(
                variable.Key[EnvironmentPrefix.Length..].Replace("__", ConfigurationPath.KeyDelimiter,
                    StringComparison.Ordinal),
                variable.Value)));

        try
        {
            return (builder.Build(), fileHasToken);
        }
        catch (InvalidDataException exception)
        {
            throw new AutoReleaseException(
                $"O arquivo {path} não é um JSON válido: {exception.InnerException?.Message ?? exception.Message}",
                "Corrija o arquivo; os comentários com // são aceitos.",
                exception);
        }
    }

    /// <summary>
    /// Diz se o arquivo de configuração tem um token preenchido.
    /// </summary>
    /// <param name="path">O caminho do arquivo.</param>
    /// <returns><see langword="true"/> quando a raiz do arquivo tem a chave <c>Token</c> com um texto.</returns>
    private static bool HasToken(string path)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            using var document = JsonDocument.Parse(stream, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.EnumerateObject().Any(property =>
                    property.NameEquals("Token") && property.Value.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(property.Value.GetString()));
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
