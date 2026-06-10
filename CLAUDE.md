# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão geral

Auto Release é uma ferramenta CLI .NET (empacotada como .NET global tool `autorelease`) que gera releases no GitHub automaticamente a partir das mensagens de commit de um repositório, seguindo o padrão Conventional Commits (com variações próprias). A mesma ferramenta é consumida pela GitHub Action `kempdec/autorelease-action`.

A interface do usuário (CLI, documentação, comentários XML e mensagens) é em **português do Brasil**. Mantenha esse idioma ao adicionar mensagens, descrições de comandos/opções e comentários `/// <summary>`.

## Comandos

```powershell
dotnet build                                  # Compilar a solução
dotnet run --project src/AutoRelease -- note  # Executar localmente (passe args após --)
dotnet pack                                    # Gerar o pacote NuGet em ./published
dotnet tool install -g autorelease            # Instalar a ferramenta publicada
```

Não há projeto de testes na solução. O `launchSettings.json` define dois profiles de execução: `AutoRelease Note` (args `note`) e `AutoRelease Create` (args `create`).

Para executar localmente é necessário fornecer configuração (token/repo) — via `autorelease.config.json` na pasta de trabalho ou variáveis de ambiente com prefixo `AutoRelease_`.

## Arquitetura

`Program.cs` apenas delega para `AutoReleaseCommand.InvokeAsync(args)`. O `AutoReleaseCommand` (um `RootCommand` do `System.CommandLine`) registra três subcomandos: `create`, `note` e `version`.

### Os três subcomandos e como se encadeiam

- **`version`** (`VersionSubCommand`): calcula a próxima versão SemVer. Busca o último release no GitHub, lê os commits desde então e incrementa major (se há `IsBreakingChange`), minor (se há um commit do tipo `Feat`) ou patch (caso contrário). Sem release anterior, usa `FirstVersion`.
- **`note`** (`NoteSubCommand`): gera as notas do release em Markdown. Lê os commits desde o release anterior, converte cada um em `CommitMessage`, filtra ignorados, deduplica por `ReleaseDescription`, agrupa por tipo e ordena por `ICommitMessageType.Ordering`.
- **`create`** (`CreateSubCommand`): orquestra os outros dois — chama `note` para o corpo e `version` para a tag (a menos que `--version` seja informada), depois cria o release via Octokit. A saída (`--output-type`) pode ser `Id`, `UploadUrl` ou `Version`.

`create` reusa `note` e `version` instanciando-os diretamente e chamando `HandleResultAsync(inputs)` — os inputs compartilhados são possíveis porque `ICreateSubCommandInputs` herda das interfaces de note e version.

### O padrão SubCommand (Binder / Inputs / Config)

Cada subcomando segue um conjunto paralelo de tipos. Ao adicionar/alterar um subcomando ou opção, atualize todos os elos da cadeia:

- **`Options/*Option.cs`** — uma classe por opção CLI, derivada de `System.CommandLine.Option<T>` (nome, descrição em pt-BR, parsing). `TypesOption`, `ReplacesOption` e `IgnoresOption` fazem parsing customizado de listas/dicionários.
- **`SubCommands/Binders/*Binder.cs`** — derivam de `GlobalBinder`, declaram as opções como propriedades e implementam `GetBoundValue` para materializar os `Inputs`. `GlobalBinder.AddOptionsTo` adiciona via reflection toda propriedade do tipo `Option`.
- **`SubCommands/Inputs/*Inputs.cs` + `I*Inputs.cs`** — o objeto fortemente tipado entregue ao handler. As interfaces formam uma hierarquia (`IGlobalInputs` → note/version → create) que permite o reuso entre subcomandos.
- **`SubCommands/SubCommandBase.cs`** — base genérica `SubCommandBase<TBinder, TInputs, TIInputs>`. Registra o binder, faz o bind do handler e converte `SubCommandResult` em saída de console (verde/sucesso, vermelho/erro). Implemente `HandleResultAsync` na subclasse.
- **`Configurations/Commands/*Config.cs`** — espelham as opções para permitir configuração via arquivo/env. Propriedades decoradas com `[ConfigOption<TOption>]` (e `[ConfigCommand<TCommand>]` no nível raiz).

### Camada de configuração (arquivo + env → args)

`AutoReleaseConfig.Instance` carrega `autorelease.config.json` (opcional) e variáveis `AutoRelease_*` via `KempDec.StarterDotNet.Console`. Antes do parsing, `AutoReleaseCommand.InvokeAsync` chama `config.SetArgs(args)`: por reflection, ele lê as propriedades decoradas com os atributos `ConfigCommand`/`ConfigOption` e **injeta os valores configurados como argumentos CLI** — mas só quando a opção/comando ainda não foi passado explicitamente na linha de comando. Ou seja, args explícitos sempre têm precedência sobre arquivo/env. Aninhamento em env usa `__` (ex.: `AutoRelease_CreateCommand__Types`).

### Mensagens de commit (`Commits/`)

`CommitMessage` é o parser central. `SplitMessage` separa `tipo: descrição` da primeira linha e junta o restante como corpo (linhas unidas por `<br>`). Operadores no final do tipo, antes do `:`:

- `!` → `IsBreakingChange` (bump major)
- `#` → `Ignore` (omite das notas)
- `^` → usa o corpo como descrição do release (ignora a descrição)

Os tipos de commit têm duas origens: tipos embutidos (`Commits/Types/*CommitMessageType.cs` — feat/fix/chore/default, descobertos por reflection via `AssemblyHelper.GetAllClassesWithInterface`) ou tipos definidos pelo usuário via opção `--types`/config (formato `key=Label`, e a ordem define a ordenação no release). Os labels embutidos vêm do recurso localizado `Resources/CommitTypesResource.resx`. A ordenação dos grupos segue o enum `CommitMessageTypeOrder` (Feat=0, Fix, Chore, Default=255).

## CI/CD

`.github/workflows/publish.yml` roda no push para `main` (em `src/**`) ou manualmente. Usa a própria action `kempdec/autorelease-action@v2` para criar o release, depois `dotnet pack` e publica o `.nupkg` no NuGet.org e no GitHub Packages. A versão do release alimenta `dotnet pack -p:PackageVersion=`.

## Convenções

- Target: **.NET 8.0**, `Nullable` e `ImplicitUsings` habilitados, namespace raiz `KempDec.AutoRelease`.
- Estilo definido em `.editorconfig` (regras detalhadas de C#). Comentários de documentação XML são esperados (`GenerateDocumentationFile` ativo).
- Dependências principais: `Octokit` (API do GitHub), `Semver` (versionamento), `System.CommandLine` (CLI, beta), `KempDec.StarterDotNet.*` (configuração e reflection).
