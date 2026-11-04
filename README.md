# Auto Release

[![Conferir](https://github.com/kempdec/AutoRelease/actions/workflows/ci.yml/badge.svg)](https://github.com/kempdec/AutoRelease/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/AutoRelease)](https://www.nuget.org/packages/AutoRelease)

Gere as releases do seu projeto no GitHub a partir das mensagens de commit: a versão sai dos tipos dos commits
([SemVer](https://semver.org)), e as notas saem das descrições, agrupadas e escritas para quem lê.

```text
feat: Adicionar a busca de demandas.          →   # Novidades
fix: Corrigir o filtro que voltava vazio.          - Foi adicionada a busca de demandas.
                                                  # Correções
                                                   - Foi corrigido o filtro que voltava vazio.
```

O que ele garante:

- **A release sai do commit exato que disparou o fluxo**, e não da ponta do branch no momento em que roda.
- **Os commits contam a partir da tag da versão anterior**, e não pela data: um commit refeito com data antiga não some.
- **Quando algo falha, o comando falha**: o código de saída é 1 e a mensagem diz o que houve e o que fazer.
- **O resultado e as mensagens ficam separados**: `$(autorelease create)` guarda só o resultado.
- **As mensagens fora da regra são barradas antes**, no commit e na CI, pelo comando `lint`.

## Índice

- [GitHub Actions](#github-actions)
- [Linha de comando](#linha-de-comando)
- [Escrevendo commits](#escrevendo-commits)
- [Como a versão e as notas são calculadas](#como-a-versão-e-as-notas-são-calculadas)
- [Configuração](#configuração)
- [Conferindo as mensagens de commit](#conferindo-as-mensagens-de-commit)
- [Saída e códigos de saída](#saída-e-códigos-de-saída)
- [Migrando da versão 1](#migrando-da-versão-1)
- [Segurança](#segurança)

## GitHub Actions

Crie o arquivo `.github/workflows/release.yml` no seu repositório:

```yml
name: Criar release

on:
  push:
    branches:
      - main

jobs:
  release:
    name: Criar release
    runs-on: ubuntu-latest
    permissions:
      contents: write # Criar a tag e a release; nada além disso.

    steps:
      - name: Criar release
        uses: kempdec/autorelease-action@v3
        env:
          AutoRelease_Token: ${{ secrets.GITHUB_TOKEN }}
```

A cada push no `main`, o Auto Release lê os commits desde a última tag de versão, calcula a próxima e cria a release no
commit do push. O repositório e o commit vêm do GitHub Actions; o token é o que o próprio GitHub Actions fornece.

Para criar a release quando você publica uma tag de versão, informe a versão da tag:

```yml
on:
  push:
    tags:
      - "v*"

# ...
        env:
          AutoRelease_Token: ${{ secrets.GITHUB_TOKEN }}
          AutoRelease_Version: ${{ github.ref_name }}
```

## Linha de comando

Instale a ferramenta com o SDK do .NET 10:

```sh
dotnet tool install -g autorelease
```

| Comando              | O que faz |
| -------------------- | --------- |
| `autorelease create` | Calcula a versão, escreve as notas e cria a release. |
| `autorelease note`   | Escreve as notas da próxima release, sem criar nada. |
| `autorelease version`| Calcula a versão da próxima release, sem criar nada. |
| `autorelease lint`   | Confere as mensagens de commit contra as regras do projeto. |

Antes de criar a primeira release, veja o que sairia, sem criar nada:

```sh
export AutoRelease_Token="<seu token>"
autorelease create --repo kempdec/MeuProjeto --dry-run
```

```text
Ensaio: nada foi criado.
Título: MeuProjeto v1.4.0 (2026-11-04)
Tag: v1.4.0, no commit 8f2c1a9
Versão anterior: v1.3.2
Commits desde ela: 12

# Novidades
...
```

Use `autorelease <comando> --help` para ver todas as opções de um comando.

## Escrevendo commits

O Auto Release lê as mensagens no formato do [Conventional Commits](https://www.conventionalcommits.org):

```text
<tipo>(<escopo opcional>)<operadores opcionais>: <descrição>

[corpo opcional]

[rodapés opcionais]
```

- O **escopo** aparece em negrito na frente do item: `fix(api): Corrigir a rota.` vira `**api:** Corrigir a rota.`
- O **corpo** entra no mesmo item, depois da descrição.
- Os **rodapés do Git** (`Co-Authored-By`, `Signed-off-by`, `Refs #12`) ficam fora das notas.
- O rodapé **`BREAKING CHANGE:`** marca a quebra de compatibilidade, como o operador `!`.

Os operadores ficam colados ao tipo, antes dos dois-pontos, e podem ser combinados:

| Operador | Efeito |
| -------- | ------ |
| `!`      | Quebra a compatibilidade: sobe a versão `major`. Ex.: `feat!: Remover a rota antiga.` |
| `#`      | Fica fora das notas e da conta da versão. Ex.: `chore#: Atualizar a CI.` |
| `^`      | Usa só o corpo nas notas, no lugar da descrição. Ex.: `feat^: Ignorada.` |

## Como a versão e as notas são calculadas

**A versão anterior** é a maior tag com o prefixo do projeto (`v`, por padrão) que é uma versão semântica. **Os
commits** são os que o commit da release tem e o da tag não. Sem tag, vale a história inteira e a versão
`FirstVersion`.

**O salto** vem dos commits que contam (os ignorados não contam):

| Commit | Salto |
| ------ | ----- |
| Algum com `!` ou `BREAKING CHANGE:` | `major` (1.4.2 → 2.0.0) |
| Algum do **primeiro tipo** de `Types` (`feat`, por padrão) | `minor` (1.4.2 → 1.5.0) |
| Qualquer outro | `patch` (1.4.2 → 1.4.3) |

Depois de uma pre-release (como `2.0.0-beta.1`), a próxima versão é a estável dela (`2.0.0`).

**As notas** têm um grupo por tipo, na ordem de `Types`, com um item por commit, do mais antigo ao mais novo. Um texto
repetido entra uma vez. O commit de um tipo fora da lista vai para o grupo "Outros", no fim.

## Configuração

Configure pelo arquivo `autorelease.config.json`, na raiz do projeto, ou pelas variáveis de ambiente com o prefixo
`AutoRelease_`. O [modelo com todas as opções](src/AutoRelease/autorelease.config.json) explica cada uma.

```jsonc
// autorelease.config.json
{
  "CreateCommand": {
    "ProjectName": "Meu Projeto",
    "Types": ["feat=Novidades", "fix=Correções", "chore=Notas técnicas"],
    "Replaces": {
      "Adicionar": "Foi adicionado",
      "Adicionar a ": "Foi adicionada a ",
      "Corrigir": "Foi corrigido"
    },
    "Ignores": ["Merge"]
  }
}
```

**O token não vai no arquivo**, que costuma ir para o repositório. Passe-o em `AutoRelease_Token`, guardado como
segredo. Se o Auto Release achar um token no arquivo, ele avisa.

Cada chave vale na raiz (para todos os comandos) ou na seção de um comando (`CreateCommand`, `NoteCommand`,
`VersionCommand`, `LintCommand`). `note`, `version` e `lint` também leem a seção `CreateCommand`. A precedência é:

1. a opção da linha de comando (`--types`);
2. a variável de ambiente (`AutoRelease_CreateCommand__Types`, com `__` separando a seção);
3. a seção do comando no arquivo;
4. a raiz do arquivo;
5. o padrão.

Numa variável de ambiente, uma lista é um texto com os itens separados por espaço, e um item com espaço vai entre
aspas: `AutoRelease_CreateCommand__Types='feat="Novidades desta versão" fix=Correções'`.

| Chave | Opção | Padrão | O que faz |
| ----- | ----- | ------ | --------- |
| `Token` | `--token` | `GITHUB_TOKEN` | O token de acesso ao GitHub. Use a variável de ambiente. |
| `Repo` | `--repo` | `GITHUB_REPOSITORY` | O repositório, como `dono/nome`. |
| `Commit` | `--commit` | `GITHUB_SHA` | O commit exato da release. |
| `Branch` | `--branch` | o branch padrão | O branch da release, quando ele não é o do GitHub Actions. |
| `Version` | `--version` | calculada | A versão da release, quando ela não deve ser calculada. |
| `Types` | `--types` | `feat`, `fix`, `chore` | Os tipos, na ordem das notas, como `chave=Título`. O primeiro sobe a `minor`. |
| `Replaces` | `--replaces` | nenhuma | As trocas do começo das descrições. Vale a de começo mais longo que casa, uma vez. |
| `Ignores` | `--ignores` | nenhum | Os começos das descrições que ficam fora das notas e da versão. |
| `ShowAuthor` | `--show-author` | `false` | Mostra o autor em cada item das notas. |
| `FirstVersion` | `--first-version` | `1.0.0` | A versão da primeira release. |
| `VersionPrefix` | `--version-prefix` | `v` | O prefixo das tags de versão. |
| `ProjectName` | `--project-name` | o nome do repositório | O nome do projeto no título da release. |
| `Draft` | `--draft` | `false` | Cria a release como rascunho. |
| `PreRelease` | `--pre-release` | `false` | Marca a release como pre-release. |
| `OutputType` | `--output-type` | `Id` | O que o `create` escreve: `Id`, `UploadUrl`, `Version`, `Tag` ou `HtmlUrl`. |
| `DryRun` | `--dry-run` | `false` | Mostra a release que seria criada, sem criar nada. |
| `SkipEmpty` | `--skip-empty` | `false` | Não cria a release quando todos os commits desde a anterior são ignorados. |

Com a variável `GITHUB_API_URL` apontando para o seu GitHub Enterprise Server (o GitHub Actions a preenche sozinho), o
Auto Release usa esse endereço.

## Conferindo as mensagens de commit

O `lint` confere as mensagens contra as mesmas regras que escrevem as notas: o formato, o tipo dentro de `Types` e o
começo dentro de `Replaces`. Uma mensagem fora da regra é barrada antes de virar uma nota errada.

```text
✗ 3f9a2c1 fix: Ajustar o filtro.
  A descrição começa por "Ajustar", que não está entre os começos do projeto. Comece por um destes: Adicionar, Corrigir.
1 mensagem está fora das regras do projeto.
```

No commit, pelo gancho `commit-msg` do Git (`.git/hooks/commit-msg`):

```sh
#!/bin/sh
exec autorelease lint --file "$1"
```

Na CI, para os commits de um pull request (com `fetch-depth: 0` no checkout):

```sh
autorelease lint --from "$BASE_SHA" --to "$HEAD_SHA"
```

As regras opcionais ficam na seção `LintCommand`:

| Chave | Padrão | O que faz |
| ----- | ------ | --------- |
| `RequireLeadingWord` | `true` | A descrição começa por uma das trocas de `Replaces` (quando há trocas). |
| `RequirePeriod` | `false` | O título termina em ponto final. |
| `MaxHeaderLength` | sem limite | O tamanho máximo do título. |
| `ForbiddenTrailers` | nenhum | Os rodapés recusados, como `Co-Authored-By`. |

Sem `Types`, valem os tipos do Conventional Commits. As mensagens que o Git escreve (`Merge`, `Revert`, `fixup!`) e as
que começam por um item de `Ignores` não passam pela conferência.

## Saída e códigos de saída

- **A saída padrão** recebe só o resultado: a versão, as notas ou o que `OutputType` pede.
- **A saída de erros** recebe os avisos e os erros, em duas partes: o que houve e o que fazer.

| Código | Quando |
| ------ | ------ |
| `0` | Deu certo, inclusive quando `SkipEmpty` não criou a release por não haver nada novo. |
| `1` | Algo falhou: o token foi recusado, o repositório não existe, a release já existe, uma mensagem está fora da regra. |

## Migrando da versão 1

A versão 2 roda no .NET 10 e é usada pela `kempdec/autorelease-action@v3`. A `@v2` continua instalando a versão 0, sem
mudar nada para quem a usa. A configuração da versão 1 continua valendo, com estas diferenças:

- A release sai do commit que disparou o fluxo (`GITHUB_SHA`), e não da ponta do branch.
- Os commits contam a partir da tag da versão anterior, e não da data da última release.
- O comando sai com o código 1 quando falha. Antes, ele saía com 0, e o fluxo passava como se tivesse dado certo.
- O primeiro tipo de `Types` sobe a `minor`, como esta documentação sempre disse; antes, só `feat` subia.
- Os commits ignorados (`#` e `Ignores`) não contam para a versão.
- As trocas de `Replaces` valem pela de começo mais longo, e não pela ordem no arquivo.
- A descrição não é mais cortada no segundo `: `, e os rodapés do Git ficam fora das notas.
- A tag anterior precisa ter o prefixo de `VersionPrefix`.

## Segurança

Para relatar uma vulnerabilidade, siga a [política de segurança](SECURITY.md), e não abra uma issue pública.

## Autores

- [KempDec](https://kempdec.com), mantenedora do projeto de código aberto.
- [Vinícius Lima](https://github.com/viniciusxdl).

## Licença

[MIT](LICENSE.txt)
