# Auto Release

Gere automaticamente releases do seu projeto no GitHub usando o Auto Release.

## Começando

Use o sumário abaixo para escolher como quer aprender a usar o Auto Release.

- [**GitHub Actions**](#github-actions) - Automatize a criação dos seus releases com o CI/CD do GitHub.
- [**CLI (Interface de Linha de Comando)**](#cli) - Crie os seus releases a partir do seu terminal. `No momento a documentação para a CLI está ausente e será adicionada em breve, enquanto isso, veja como usar o Auto Release no GitHub Actions.`

## GitHub Actions

Para começar a usar o Auto Release no GitHub Actions é muito fácil, siga os passos abaixo e diga adeus a escrever
releases manualmente.

### 1. Criando um fluxo de trabalho simples

Acesse o seu repositório no GitHub e no menu suspenso "**Add file**", vá em "**Create new file**".
![Botão "Create new file" no GitHub](assets/readme/create-new-file.png)

Em "**Name your file...**" digite o seguinte: `.github/workflows/main.yml`.

![Campo "Name your file..." no GitHub](assets/readme/name-your-file.png)

Ficará semelhante a imagem abaixo:

![Campo "Name your file..." com o caminho ".github/workflows/main.yml" no GitHub](assets/readme/name-your-file-main-yml.png)

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

Clique no botão "**Commit changes**".

![Botão "Commit changes" no GitHub](assets/readme/commit-changes-btn.png)

Escreva a sua mensagem de commit e clique mais uma vez em "**Commit changes**".

![Janela "Commit changes" no GitHub](assets/readme/commit-changes.png)

**Pronto! Isso já é o suficiente para que o Auto Release comece a gerar automaticamente os releases do seu repositório no GitHub.**

Veja a seguir como configurar e costumizar o Auto Release para que atenda melhor as suas necessidades e da sua equipe.

## Autores

- [KempDec](https://kempdec.com) - Mantedora do projeto de código aberto.
- [Vinícius Lima](https://github.com/viniciusxdl) - Desenvolvedor .NET C#.

## Licença

[MIT](https://github.com/kempdec/AutoRelease/blob/main/LICENSE.txt)