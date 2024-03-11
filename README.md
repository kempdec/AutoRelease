# Auto Release

Gere automaticamente releases do seu projeto no GitHub usando o Auto Release.

## Começando

Use o sumário abaixo para escolher como quer aprender a usar o Auto Release.

- [**GitHub Actions**](#github-actions) - Automatize a criação dos seus releases com o CI/CD do GitHub.
- [**CLI (Interface de Linha de Comando)**](#cli-interface-de-linha-de-comando) - Crie os seus releases a partir do seu terminal.

Depois, veja como escrever as suas mensagens de commit para usar todo o potencial do Auto Release [**clicando aqui**](#escrevendo-commits).

## Índice

- [Auto Release](#auto-release)
  - [Começando](#começando)
  - [Índice](#índice)
  - [GitHub Actions](#github-actions)
    - [Criando um fluxo de trabalho simples no GitHub Actions](#criando-um-fluxo-de-trabalho-simples-no-github-actions)
    - [Personalizando o fluxo de trabalho](#personalizando-o-fluxo-de-trabalho)
  - [CLI (Interface de Linha de Comando)](#cli-interface-de-linha-de-comando)
    - [Instalação](#instalação)
  - [Escrevendo commits](#escrevendo-commits)
  - [Configurações](#configurações)
    - [Arquivo de configuração](#arquivo-de-configuração)
    - [Variáveis de ambiente](#variáveis-de-ambiente)
    - [Propriedades de configuração](#propriedades-de-configuração)
      - [CreateComand](#createcomand)
      - [CreateComand e NoteCommand](#createcomand-e-notecommand)
      - [CreateComand e VersionCommand](#createcomand-e-versioncommand)
  - [Autores](#autores)
  - [Licença](#licença)

## GitHub Actions

Para começar a usar o Auto Release no GitHub Actions é muito fácil, siga os passos abaixo e diga adeus a escrever
releases manualmente.

### Criando um fluxo de trabalho simples no GitHub Actions

Acesse o seu repositório no GitHub e no menu suspenso "**Add file**", vá em "**Create new file**".
![Botão "Create new file" no GitHub](assets/readme/create-new-file.png)

---

Em "**Name your file...**" digite o seguinte: `.github/workflows/main.yml`.

![Campo "Name your file..." no GitHub](assets/readme/name-your-file.png)

---

Ficará semelhante a imagem abaixo:

![Campo "Name your file..." com o caminho ".github/workflows/main.yml" no GitHub](assets/readme/name-your-file-main-yml.png)

---

Copie o código abaixo e cole no corpo do arquivo no GitHub.

``` yml
# .github/workflows/main.yml

name: Criar Release

on:
  push:
    branches:
      - main

jobs:
  create-release:
    name: Criar Release
    runs-on: ubuntu-latest
    permissions: write-all

    steps:
      - name: Criar Release
        uses: kempdec/autorelease-action@v1
        env:
          AutoRelease_Token: ${{ secrets.GITHUB_TOKEN }} # Este token é fornecido pelo GitHub Actions, você não precisa criar seu próprio token.
```

---

Clique no botão "**Commit changes**".

![Botão "Commit changes" no GitHub](assets/readme/commit-changes-btn.png)

---

Escreva a sua mensagem de commit e clique mais uma vez em "**Commit changes**".

![Janela "Commit changes" no GitHub](assets/readme/commit-changes.png)

**Pronto! Isso já é o suficiente para que o Auto Release comece a gerar automaticamente os releases do seu repositório no GitHub. Toda vez que um `push` for feito para o branch `main`, um release será criado.**

Veja como configurar e costumizar o Auto Release para que atenda melhor as suas necessidades e da sua equipe **[clicando aqui](#configurações)**.

### Personalizando o fluxo de trabalho

Caso você queira disparar a ação de criação da release manualmente, troque o conteúdo de `on: push` para `on: workflow_dispatch`.

``` yml
# .github/workflows/main.yml

name: Criar Release

on:
  workflow_dispatch:

jobs:
  create-release:
    name: Criar Release
    runs-on: ubuntu-latest
    permissions: write-all

    steps:
      - name: Criar Release
        uses: kempdec/autorelease-action@v1
        env:
          AutoRelease_Token: ${{ secrets.GITHUB_TOKEN }} # Este token é fornecido pelo GitHub Actions, você não precisa criar seu próprio token.
```

Ou se preferir, o release pode ser criado quando você fizer o `push` de uma `tag` de versão para o repositório. Nesse caso troque o conteúdo de `on: push: branches` para `on: push: tags` e adicione a variável de ambiente `AutoRelease_Version`.

``` yml
# .github/workflows/main.yml

name: Criar Release

on:
  push:
    tags:
      - "v*" # Tags que iniciarem com v, como v1.0.0, v2.3.5-beta.1.

jobs:
  create-release:
    name: Criar Release
    runs-on: ubuntu-latest
    permissions: write-all

    steps:
      - name: Criar Release
        uses: kempdec/autorelease-action@v1
        env:
          AutoRelease_Token: ${{ secrets.GITHUB_TOKEN }} # Este token é fornecido pelo GitHub Actions, você não precisa criar seu próprio token.
          AutoRelease_Version: ${{ github.ref_name }}
```

## CLI (Interface de Linha de Comando)

### Instalação

Instale a ferramenta a partir do SDK do .NET.

``` powershell
dotnet tool install -g autorelease
```

Digite o comando `autorelease --help` em seu Terminal para que seja impresso a ajuda.

![Opção "--help" na CLI do Auto Release](assets/readme/cli-help.png)

Para imprimir a ajuda de algum comando, informe ele e em seguida a opção `--help`.

Exemplo: `autorelease create --help`.

![Opção "--help" para o comando "create" na CLI do Auto Release](assets/readme/cli-create-help.png)

**A CLI também funciona com um arquivo de configuração do Auto Release.** Nos exemplos acima, o arquivo deveria estar em `C:\Users\ceo`, que é o caminho em que o terminal se encontra.

Veja como usar um arquivo de configuração [clicando aqui.](#configurações)

## Escrevendo commits

O Auto Release usa o padrão do [**Conventional Commits**](https://www.conventionalcommits.org) (com poucas diferenças)
para ler os commits e gerar o release automaticamente.

Ou seja, a mensagem do commit deve ser estruturada da seguinte forma:

> **Obs.: O Auto Release ainda não oferece suporte a escopos opcionais, mas em breve será adicionado o recurso.**

``` txt
<tipo>: <descrição>

[corpo opcional]

[rodapé(s) opcional(is)]
```

Os tipos `feat` e `fix`, operador `!`, e o texto `BREAKING CHANGE:` funcionam como está descrito em [**Conventional Commits**](https://www.conventionalcommits.org), com as seguintes diferenças:

- Caso a propriedade `Types` (arquivo) ou `types` (CLI) seja especificada, o primeiro tipo terá o funcionamento de `feat`,
de incrementar a versão `minor` e todos os demais tipos incrementarão a versão `patch`.
- O texto `BREAKING CHANGE:` pode ser colocado em qualquer lugar do corpo ou rodapé, não sendo necessário estar no início do texto. Apenas se atente ao uso de `:` (2 pontos), o `BREAKING CHANGE:` sem os 2 pontos, não será considerado.
- Caso o corpo ou rodapé da mensagem de commit for especificado, somente eles serão utilizados nas notas geradas pelo Auto Release.
  
  Exemplo da mensagem de commit:
  ``` text
  fix: Corrigir a criação de artigos.

  Corrigido um problema em que quando a data de publicação era inferior a data de criação, o artigo não era criado.

  Obs.: Somente de exemplo.
  ```

  As notas geradas serão semelhantes a abaixo:
  
  ``` text
  # My Project v1.0.0 (2024-01-01)

  # Correções
  
  - Corrigido um problema em que quando a data de publicação era inferior a data de criação, o artigo não era criado.
    
    Obs.: Somente de exemplo.
  ```

## Configurações

### Arquivo de configuração

Uma das maneiras de configurar o Auto Release, é adicionando um arquivo com o nome `autorelease.config.json` na raiz do seu projeto.

> Exemplo: Se o seu projeto está em `D:/MeuProjeto`, é nela que o arquivo deverá ser criado.

**Um modelo atualizado com todas as opções do `autorelease.config.json` pode ser encontrado [clicando aqui](https://github.com/kempdec/AutoRelease/blob/main/src/AutoRelease/autorelease.config.json).**

**Observações:**
- Todas as propriedades de configuração são **OPCIONAIS**.
- A configuração abaixo é **apenas um exemplo** e **NÃO deve** ser usada em seu projeto, a menos que entenda o que está fazendo.
- Caso você esteja utilizando o Auto Release no GitHub Actions, as propriedades `NoteCommand` e `VersionCommand` devem ser ignoradas.

``` jsonc
// autorelease.config.json

{
  // O token para acesso ao repositório no GitHub.
  "Token": "ghp_1DF8350tVc1Ry0IBuWH3uh8cTRpofs0FY0Iv",
  // O repositório que contém os commits no GitHub.
  "Repo": "kempdec/MyRepository",
  // O branch do repositório no GitHub.
  "Branch": "main",

  // As configurações do subcomando de criação automática de um release.
  "CreateCommand": {
    // A versão do release.
    // No exemplo abaixo, o release seria criado na v1.3.0. O uso dela é recomendado somente de forma dinâmica, como em
    // uma variável de ambiente definida pelo GitHub Actions.
    "Version": "v1.3.0",
    // O nome do projeto.
    // Ele pode ser utilizado em alguns lugares, como parte do título do release.
    "ProjectName": "Meu Repositório",
    // Um sinalizador indicando se o release é um rascunho.
    "Draft": false,
    // Um sinalizador indicando se o release é um pre-release.
    "PreRelease": false,
    // O tipo de saída da criação automática do release.
    // Algumas opções são: Id, UploadUrl e Version.
    "OutputType": "Id",

    // Os tipos das mensagens de commit.
    "Types": [
      "feat=Novidades desta versão",
      "fix=Principais correções desta versão",
      "chore=Notas técnicas"
    ],
    // As substituições do início das mensagens de commit.
    "Replaces": {
      "Adicionar": "Foi adicionado",
      "Adicionar o ": "Foi adicionado o ",
      "Adicionar a ": "Foi adicionada a ",
      "Corrigir": "Corrigido um problema em que"
    },
    // Os inícios das mensagens de commit que serão ignoradas.
    "Ignores": [
      "Refatorar."
    ],

    // A primeira versão do repositório.
    "FirstVersion": "0.1.0",
    // O prefixo das versões do repositório.
    "VersionPrefix": "v"
  },

  // As configurações do subcomando de geração automática das notas do release.
  "NoteCommand": {
    // Os tipos das mensagens de commit.
    "Types": [
      "feat=Novidades desta versão",
      "fix=Principais correções desta versão"
    ],
    // As substituições do início das mensagens de commit.
    "Replaces": null,
    // Os inícios das mensagens de commit que serão ignoradas.
    "Ignores": [
      "Refatorar."
    ]
  },

  // As configurações do subcomando de geração automática da versão do release.
  "VersionCommand": {
    // A primeira versão do repositório.
    "FirstVersion": "0.1.0",
    // O prefixo das versões do repositório.
    "VersionPrefix": null
  }
}
```

### Variáveis de ambiente

Você pode definir variáveis de ambiente com o prefixo `AutoRelease_` para configurar o Auto Release.

Para configurar o repositório, por exemplo, o nome da variável de ambiente seria `AutoRelease_Repo`.

``` jsonc
// Essa variável de ambiente:
AutoRelease_Repo="kempdec/MyRepository"

// É equivalente a essa propriedade no arquivo de configuração:
{
  "Repo": "kempdec/MyRepository"
}
```

Use `__` (dois `_`) no nome da variável de ambiente para definir uma propriedade de um objeto.

``` jsonc
// Essa variável de ambiente:
AutoRelease_CreateCommand__Types="feat=\"Novidades\" fix=\"Correções\" chore=\"Notas técnicas\""

// É equivalente a essa propriedade no arquivo de configuração:
{
  "CreateCommand": {
    "Types": [
        "types=Novidades",
        "fix=Correções",
        "chore=Notas técnicas"
    ]
  }
}
```

### Propriedades de configuração

| **Arquivo** | **CLI**         | **Tipo**  | **Padrão** | **Sumário** |
| ----------- | --------------- | --------- | ---------- | ----------- |
| `Token`     | `token`         | `string`  | `null!`    | `[OBRIGATÓRIA NO CLI]`<br>O token para acesso ao repositório no GitHub.<br>Exemplo: `ghp_1DF8350tVc1Ry0IBuWH3uh8cTRpofs0FY0Iv`. |
| `Repo`      | `repo`          | `string`  | `null!`    | `[OBRIGATÓRIA NO CLI]`<br>O repositório que contém os commits no GitHub.<br>Exemplo: `kempdec/MyRepository`. |
| `Branch`    | `branch`        | `string?` | `null`     | O branch do repositório no GitHub.<br>Exemplo: `main`. |

#### CreateComand

| **Arquivo**   | **CLI**         | **Tipo**   | **Padrão** | **Sumário** |
| ------------- | --------------- | ---------- | ---------- | ----------- |
| `Version`     | `version`       | `string?`  | `null`     | A versão do release [**(SemVer 2.0)**](https://semver.org).<br>Quando especificada, a versão do release não será gerada automaticamente. O uso dessa propriedade é recomendado somente de forma dinâmica, como em uma variável de ambiente definida através do GitHub Actions.<br>Exemplo: `1.3.0`. |
| `ProjectName` | `project-name`  | `string?`  | `null`     | O nome do projeto.<br>Ele pode ser utilizado em alguns lugares, como parte do título do release.<br>Exemplo: `Meu Repositório`. |
| `Draft`       | `draft`         | `boolean`  | `false`    | Um sinalizador indicando se o release é um rascunho.<br>Exemplo: `false`. |
| `PreRelease`  | `pre-release`   | `boolean`  | `false`    | Um sinalizador indicando se o release é um pre-release.<br>Exemplo: `false`. |
| `OutputType`  | `output-type`   | `string`   | `Id`       | O tipo de saída da criação automática do release.<br>Os valores aceitos são: `Id`, `UploadUrl` ou `Version` |

#### CreateComand e NoteCommand

| **Arquivo**   | **CLI**         | **Tipo**   | **Padrão** | **Sumário** |
| ------------- | --------------- | ---------- | ---------- | ----------- |
| `Types`       | `types`         | `string[]` | `null`     | Os tipos das mensagens de commit.<br><br>Exemplo arquivo:<br>`["feat=Novidades", "fix=Correções", "chore=Notas técnicas"]`.<br><br>Exemplo CLI:<br>`feat="Novidades" fix="Correções" chore="Notas técnicas"`.<br><br>**A ordem dos tipos definirá também a ordem que são escritos nas notas do release.** Neste exemplo, as `Novidades` serão listadas primeiro. |
| `Replaces`    | `replaces`      | `object`   | `null`     | As substituições do início das mensagens de commit.<br><br>Exemplo arquivo:<br>`{"Adicionar a ": "Foi adicionada a ", "Corrigir": "Foi corrigido"}`.<br><br>Exemplo CLI:<br>`"Adicionar a "="Foi adicionada a " "Corrigir"="Foi corrigido"`.<br><br>Usando a seguinte mensagem de commit como exemplo:<br>`feat: Adicionar a página de cadastro de usuários.`<br><br>Na geração das notas do release, ela seria substituída por:<br>`Foi adicionada a página de cadastro de usuários.` |
| `Ignores`     | `ignores`       | `string[]` | `null`     | Os inícios das mensagens de commit que serão ignoradas.<br><br>Exemplo arquivo:<br>`["Refatorar.", "Atualizar a versão"]`.<br><br>Exemplo CLI:<br>`"Refatorar" "Atualizar a versão"`.<br><br>Usando a seguinte mensagem de commit como exemplo:<br>`chore: Atualizar a versão do pacote.`<br><br>Ela seria ignorada, por começar com `Atualizar a versão`.<br><br>**Você também pode ignorar uma mensagem de commit usando o operador `#`.**<br><br>No exemplo abaixo, a mensagem de commit será ignorada, independente de estar ou não em `Ignores`:<br>`chore#: Atualizar a versão do pacote.` |
| `ShowAuthor`  | `show-author`   | `boolean`  | `false`    | Um sinalizador indicando se o autor deve ser exibido no final das mensagens de commit.<br>Exemplo: `false`. |

#### CreateComand e VersionCommand

| **Arquivo**     | **CLI**          | **Tipo**   | **Padrão** | **Sumário** |
| --------------- | ---------------- | ---------- | ---------- | ----------- |
| `FirstVersion`  | `first-version`  | `string`   | `1.0.0`    | A primeira versão do repositório [**(SemVer 2.0)**](https://semver.org).<br>Essa propriedade é utilizada na criação do primeiro release.<br>Exemplo: `0.1.0`. |
| `VersionPrefix` | `version-prefix` | `string`   | `v`        | O prefixo das versões do repositório.<br>Exemplo: `v`. |

## Autores

- [KempDec](https://kempdec.com) - Mantedora do projeto de código aberto.
- [Vinícius Lima](https://github.com/viniciusxdl) - Desenvolvedor .NET C#.

## Licença

[MIT](https://github.com/kempdec/AutoRelease/blob/main/LICENSE.txt)