# CLAUDE.md

Orientação para o Claude Code neste repositório.

## O que é

O Auto Release é uma ferramenta de linha de comando em .NET 10 (a .NET tool `autorelease`) que cria as releases de um
repositório do GitHub a partir das mensagens de commit: calcula a versão (SemVer) e escreve as notas. A Action
`kempdec/autorelease-action` a instala e roda no GitHub Actions. O repositório é **público** e representa a KempDec:
código, mensagens e documentação seguem o padrão dela.

A interface (mensagens, ajuda, README) e os comentários de documentação são em **português do Brasil**.

## Comandos

```sh
dotnet build                                   # Compilar (estilo e avisos quebram o build)
dotnet test                                    # Testar (Microsoft.Testing.Platform, pelo global.json)
dotnet run --project src/AutoRelease -- note   # Rodar localmente; os argumentos vêm depois do --
dotnet pack src/AutoRelease -p:Version=2.0.0   # Gerar o pacote em ./published
```

## Arquitetura

O núcleo não sabe do GitHub nem da linha de comando, então quase tudo se testa sem rede:

| Pasta | O que tem |
| ----- | --------- |
| `Commits/` | A leitura da mensagem (`CommitMessage`), os tipos (`CommitTypeCatalog`) e as trocas do começo (`DescriptionRewriter`). |
| `Releases/` | A versão (`VersionCalculator`), as notas (`ReleaseNotesBuilder`) e o plano da release (`ReleasePlanner`), funções puras sobre os commits. |
| `GitHub/` | A fronteira com o GitHub: a interface `IReleaseRepository` e a implementação com o Octokit, que traduz cada falha num `AutoReleaseException`. |
| `Configuration/` | O arquivo `autorelease.config.json` e as variáveis `AutoRelease_*` (`ConfigurationLoader`) e a precedência (`SettingsReader`). |
| `Lint/` | A conferência das mensagens (`CommitLinter`), com as mesmas regras das notas. |
| `Cli/` | Os comandos (`AutoReleaseCli`) e o contexto injetável (`CliContext`): saídas, ambiente, GitHub, relógio e Git. |

Regras que não se quebram:

- **Erro esperado é `AutoReleaseException`, em duas partes**: o que houve e o próximo passo, sem pilha nem nome de
  classe. A linha de comando o escreve na saída de erros e sai com 1.
- **A saída padrão leva só o resultado.** Aviso, erro e informação vão para a saída de erros, porque os fluxos fazem
  `$(autorelease create)`.
- **A release sai de um commit lido uma vez**, no começo (`ReleasePlanner`), e os commits contam pela comparação com a
  tag anterior, nunca pela data.
- **A configuração de quem já usa continua valendo**: mudar o nome de uma chave, de uma opção ou o formato das notas é
  quebra de compatibilidade (`feat!`), e entra na seção "Migrando" do README.
- **O Octokit se confere contra o GitHub de verdade.** O `GetSha1` dele devolve texto vazio no lugar do commit; por isso
  o commit é lido com `Commit.Get`.

## Testes

`tests/AutoRelease.Tests`, com xUnit v3. O `FakeReleaseRepository` simula um repositório com história linear, e os
testes de `AutoReleaseCliTests` rodam a ferramenta inteira por ele (códigos de saída, saídas, ensaio, `lint`). Toda
mudança de comportamento leva um teste que falha no código de antes.

## Commits e releases

As mensagens seguem a configuração do próprio repositório (`autorelease.config.json`), que o fluxo "Conferir" aplica
com o `lint` a cada push e pull request:

- **Tipos:** `feat`, `fix`, `docs`, `chore`, `refactor`. `!` marca a quebra de compatibilidade e `#` tira o commit das
  notas.
- **A descrição começa por um verbo da lista**, no infinitivo: Adicionar, Alterar, Atualizar, Remover, Refatorar,
  Corrigir ou Aprimorar, e termina em ponto final.
- **Só o título**, e sem `Co-Authored-By`: a autoria fica nos metadados do commit.

O trabalho vai para o `develop`. O push no `main` (com mudança em `src/`) publica: o fluxo "Publicar" testa, calcula a
versão com a ferramenta deste commit, envia o pacote ao NuGet e ao GitHub Packages e só então cria a release. Publicar
é decisão do dono.
