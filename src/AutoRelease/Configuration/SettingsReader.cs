using System.Text;
using KempDec.AutoRelease.Errors;
using Microsoft.Extensions.Configuration;
using Semver;

namespace KempDec.AutoRelease.Configuration;

/// <summary>
/// Responsável por ler as configurações do Auto Release de um comando, na ordem de precedência: a linha de comando,
/// a seção do comando (como <c>CreateCommand</c>), a raiz e o padrão.
/// </summary>
/// <remarks>Inicializa uma nova instância de <see cref="SettingsReader"/>.</remarks>
/// <param name="configuration">A configuração do arquivo e das variáveis de ambiente.</param>
/// <param name="sections">As seções do comando, da mais específica para a mais geral; a raiz vem por último, sem
/// precisar ser informada.</param>
internal sealed class SettingsReader(IConfiguration configuration, IReadOnlyList<string> sections)
{
    /// <summary>
    /// Lê um texto.
    /// </summary>
    /// <param name="key">A chave, como <c>ProjectName</c>.</param>
    /// <param name="cliValue">O valor da linha de comando, quando informado.</param>
    /// <returns>O texto, ou <see langword="null"/> quando não há em lugar nenhum.</returns>
    public string? GetString(string key, string? cliValue = null)
    {
        if (!string.IsNullOrWhiteSpace(cliValue))
        {
            return cliValue.Trim();
        }

        string? value = Find(key)?.Value;

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Lê um sinalizador.
    /// </summary>
    /// <param name="key">A chave, como <c>Draft</c>.</param>
    /// <param name="cliValue">O valor da linha de comando, quando informado.</param>
    /// <param name="defaultValue">O valor quando não há em lugar nenhum.</param>
    /// <returns>O sinalizador.</returns>
    /// <exception cref="AutoReleaseException">Quando o valor não é <c>true</c> nem <c>false</c>.</exception>
    public bool GetBool(string key, bool? cliValue = null, bool defaultValue = false)
    {
        if (cliValue is bool value)
        {
            return value;
        }

        string? text = GetString(key);

        if (text is null)
        {
            return defaultValue;
        }

        return bool.TryParse(text, out bool parsed)
            ? parsed
            : throw new AutoReleaseException(
                $"O valor de {key} é \"{text}\", e ele precisa ser true ou false.",
                $"Corrija {key} no autorelease.config.json ou na variável AutoRelease_{key}.");
    }

    /// <summary>
    /// Lê uma versão semântica.
    /// </summary>
    /// <param name="key">A chave, como <c>FirstVersion</c>.</param>
    /// <param name="cliValue">O valor da linha de comando, quando informado.</param>
    /// <param name="prefix">O prefixo das versões, que é tirado da frente quando presente, como em <c>v1.2.0</c>.</param>
    /// <returns>A versão, ou <see langword="null"/> quando não há em lugar nenhum.</returns>
    /// <exception cref="AutoReleaseException">Quando o valor não é uma versão semântica.</exception>
    public SemVersion? GetVersion(string key, string? cliValue = null, string prefix = "v")
    {
        string? text = GetString(key, cliValue);

        if (text is null)
        {
            return null;
        }

        string bare = prefix.Length > 0 && text.StartsWith(prefix, StringComparison.Ordinal) ? text[prefix.Length..] : text;

        return SemVersion.TryParse(bare, SemVersionStyles.Strict, out SemVersion? version)
            ? version
            : throw new AutoReleaseException(
                $"O valor de {key} é \"{text}\", que não é uma versão semântica.",
                "Escreva a versão como 1.2.0 (ou 1.2.0-beta.1), conforme semver.org.");
    }

    /// <summary>
    /// Lê uma lista.
    /// </summary>
    /// <remarks>
    /// No arquivo, a lista é um vetor. Numa variável de ambiente, é um texto com os itens separados por espaço, e um
    /// item com espaço vai entre aspas, como em <c>feat="Novidades desta versão" fix=Correções</c>.
    /// </remarks>
    /// <param name="key">A chave, como <c>Types</c>.</param>
    /// <param name="cliValues">Os valores da linha de comando, quando informados.</param>
    /// <returns>A lista, vazia quando não há em lugar nenhum.</returns>
    public IReadOnlyList<string> GetList(string key, IReadOnlyList<string>? cliValues = null)
    {
        if (cliValues is { Count: > 0 })
        {
            return cliValues;
        }

        IConfigurationSection? section = Find(key);

        if (section is null)
        {
            return [];
        }

        if (section.Value is not null)
        {
            return SplitArguments(section.Value);
        }

        return [.. section.GetChildren()
            .OrderBy(child => int.TryParse(child.Key, out int index) ? index : int.MaxValue)
            .Select(child => child.Value)
            .OfType<string>()];
    }

    /// <summary>
    /// Lê um dicionário, mantendo os espaços das chaves, como em <c>"Adicionar a ": "Foi adicionada a "</c>.
    /// </summary>
    /// <param name="key">A chave, como <c>Replaces</c>.</param>
    /// <param name="cliValues">Os valores da linha de comando, no formato <c>chave=valor</c>, quando informados.</param>
    /// <returns>Os pares, vazios quando não há em lugar nenhum.</returns>
    public IReadOnlyList<KeyValuePair<string, string>> GetPairs(string key, IReadOnlyList<string>? cliValues = null)
    {
        IReadOnlyList<string> entries = cliValues is { Count: > 0 } ? cliValues : [];
        IConfigurationSection? section = entries.Count > 0 ? null : Find(key);

        if (section?.Value is string text)
        {
            entries = SplitArguments(text);
        }

        if (entries.Count > 0)
        {
            return [.. entries.Select(entry => ParsePair(key, entry))];
        }

        return section is null
            ? []
            : [.. section.GetChildren()
                .Where(child => child.Value is not null)
                .Select(child => new KeyValuePair<string, string>(child.Key, child.Value!))];
    }

    /// <summary>
    /// Separa um texto em itens, como a linha de comando: por espaço, com as aspas juntando os espaços de um item.
    /// </summary>
    /// <param name="text">O texto.</param>
    /// <returns>Os itens.</returns>
    public static IReadOnlyList<string> SplitArguments(string text)
    {
        var items = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        bool hasItem = false;

        foreach (char character in text)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
                hasItem = true;
            }
            else if (char.IsWhiteSpace(character) && !inQuotes)
            {
                if (hasItem)
                {
                    items.Add(current.ToString());
                    current.Clear();
                    hasItem = false;
                }
            }
            else
            {
                current.Append(character);
                hasItem = true;
            }
        }

        if (hasItem)
        {
            items.Add(current.ToString());
        }

        return items;
    }

    /// <summary>
    /// Procura uma chave nas seções do comando e depois na raiz.
    /// </summary>
    /// <param name="key">A chave.</param>
    /// <returns>A seção da chave, ou <see langword="null"/> quando ela não existe em lugar nenhum.</returns>
    private IConfigurationSection? Find(string key)
    {
        foreach (string prefix in sections.Append(string.Empty))
        {
            IConfigurationSection section = configuration.GetSection(prefix.Length > 0 ? $"{prefix}:{key}" : key);

            if (section.Value is not null || section.GetChildren().Any())
            {
                return section;
            }
        }

        return null;
    }

    /// <summary>
    /// Lê um par no formato <c>chave=valor</c>.
    /// </summary>
    /// <param name="key">A chave da configuração, para a mensagem de erro.</param>
    /// <param name="entry">O par.</param>
    /// <returns>O par lido.</returns>
    /// <exception cref="AutoReleaseException">Quando o par não tem o sinal de igual.</exception>
    private static KeyValuePair<string, string> ParsePair(string key, string entry)
    {
        string[] parts = entry.Split('=', count: 2);

        return parts.Length == 2
            ? new KeyValuePair<string, string>(parts[0], parts[1])
            : throw new AutoReleaseException(
                $"O item \"{entry}\" de {key} não está no formato começo=troca.",
                $"Escreva cada item de {key} como \"Adicionar\"=\"Foi adicionado\".");
    }
}
