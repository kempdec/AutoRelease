# Política de segurança

## Versões que recebem correções

| Versão | Recebe correções de segurança |
| ------ | ----------------------------- |
| 2.x    | Sim |
| 1.x e 0.x | Não: atualize para a 2.x |

## Como relatar uma vulnerabilidade

Não abra uma issue pública. Use o [relato privado de vulnerabilidades do GitHub](https://github.com/kempdec/AutoRelease/security/advisories/new)
e diga:

- o que acontece e o que poderia ser feito com isso;
- a versão do Auto Release e como ele roda (GitHub Actions ou linha de comando);
- os passos para reproduzir, sem tokens nem dados reais.

A KempDec responde em até 5 dias úteis com a avaliação e o prazo da correção, e avisa quando ela for publicada.

## O token

O Auto Release precisa de um token com permissão de escrita no repositório para criar a tag e a release. Para reduzir o
risco:

- no GitHub Actions, use o `GITHUB_TOKEN` com `permissions: contents: write`, e não um token pessoal;
- nunca ponha o token no `autorelease.config.json`: passe-o na variável `AutoRelease_Token`, guardada como segredo;
- se um token já foi para o repositório, revogue-o e crie outro, porque ele continua na história do Git.
