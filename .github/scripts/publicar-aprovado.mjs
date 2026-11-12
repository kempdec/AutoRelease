// Publica no Live o pedido de aprovação que o dono aprovou no Studio (PLANO-PUBLICACAO.md, D135). Roda no workflow
// "Publicar aprovado" de cada repositório (.github/workflows/publicar-aprovado.yml), de 5 em 5 minutos, no runner da
// máquina do Live, e é o mesmo arquivo nos três repositórios (o Studio, o FynanceApp e o markdown): os modelos estão em
// implantar/publicar-aprovado/.
//
// O que ele faz, sem guardar nada entre uma execução e outra (o estado mora no Studio e no próprio git):
//
//   1. Pergunta ao Studio, com a chave de publicação do repositório, pelos pedidos aprovados dele.
//   2. Para o pedido mais antigo, confere de novo, do lado de cá: o repositório é este, o commit tem os 40 caracteres, a
//      decisão foi de quem foi escolhido, pela web ou pelo aplicativo.
//   3. Confere o commit: ele existe e está no develop. Junta na main com um merge que leva o identificador do pedido na
//      mensagem, empurra a main e dispara os workflows da publicação (o push com o GITHUB_TOKEN não dispara workflow
//      nenhum, de propósito do GitHub; o disparo pelo botão dispara).
//   4. Não espera a publicação ali: o runner do Live é o mesmo que a publicação usa, e esperar travaria os dois. Nas
//      execuções seguintes, ele acha a junção pelo identificador na mensagem e lê como os workflows terminaram: todos
//      deram certo, marca o pedido como feito; um falhou, escreve a falha no pedido (que continua aprovado).
//
// Nunca sobe a ponta do develop: sobe o commit pedido, e só ele. O que não confere não sobe, e a razão vai para o pedido.
//
// Variáveis:
//   STUDIO_URL                    o endereço do Studio (https://studio.kempdec.com)
//   STUDIO_CHAVE_DE_PUBLICACAO    a chave de publicação deste repositório (o segredo)
//   GITHUB_REPOSITORY             dono/nome, que o Actions põe
//   GITHUB_TOKEN                  o token do workflow, com contents: write e actions: write
//   GITHUB_API_URL                a API do GitHub, que o Actions põe (https://api.github.com)
//   PUBLICACOES                   os workflows da publicação, separados por espaço, cada um com as entradas depois de
//                                 ":" (como "release.yml:lojas=auto:mobile implantar-linux.yml:distribution=live").
//                                 O valor "auto:<pasta>" vira "true" quando a pasta mudou desde a última tag.
//   RAMO_DE_DESENVOLVIMENTO       o ramo de onde o commit vem (develop)
//   RAMO_DO_LIVE                  o ramo do Live (main)
//   STUDIO_APROVADORES            opcional: os identificadores de quem pode aprovar, separados por vírgula
//   ENSAIO                        "true": diz o que faria, sem juntar, empurrar, disparar nem marcar nada
//
// Uso: node tools/publicar-aprovado.mjs   (na pasta do repositório, com a main obtida)

import { execFileSync } from 'node:child_process'
import crypto from 'node:crypto'

/** A forma do commit pedido: os 40 caracteres de um SHA-1, e nunca o começo dele. */
const formaDoCommit = /^[0-9a-f]{40}$/

/**
 * Lê a configuração do ambiente.
 *
 * @param env O ambiente.
 * @returns A configuração, ou o motivo de não dar para seguir.
 */
export function configuracao(env) {
  const faltam = ['STUDIO_URL', 'STUDIO_CHAVE_DE_PUBLICACAO', 'GITHUB_REPOSITORY', 'GITHUB_TOKEN', 'PUBLICACOES'].filter((v) => !env[v])

  if (faltam.length > 0) {
    return { erro: `Faltam as variáveis ${faltam.join(', ')}.` }
  }

  return {
    studio: env.STUDIO_URL.replace(/\/+$/, ''),
    chave: env.STUDIO_CHAVE_DE_PUBLICACAO,
    repositorio: env.GITHUB_REPOSITORY.toLowerCase(),
    token: env.GITHUB_TOKEN,
    github: (env.GITHUB_API_URL || 'https://api.github.com').replace(/\/+$/, ''),
    publicacoes: lerPublicacoes(env.PUBLICACOES),
    desenvolvimento: env.RAMO_DE_DESENVOLVIMENTO || 'develop',
    live: env.RAMO_DO_LIVE || 'main',
    aprovadores: (env.STUDIO_APROVADORES || '').split(',').map((a) => a.trim().toLowerCase()).filter((a) => a.length > 0),
    ensaio: env.ENSAIO === 'true',
  }
}

/**
 * Os workflows da publicação, com as entradas de cada um.
 *
 * @param texto O valor de PUBLICACOES.
 * @returns Os workflows.
 */
export function lerPublicacoes(texto) {
  return texto
    .split(/\s+/)
    .filter((p) => p.length > 0)
    .map((p) => {
      const [workflow, ...entradas] = p.split(':')
      const juntas = []

      // "lojas=auto:mobile" foi partido no ":" de dentro: o que não tem "=" é o resto da entrada de antes.
      for (const e of entradas) {
        if (!e.includes('=') && juntas.length > 0) {
          juntas[juntas.length - 1] += `:${e}`
        } else {
          juntas.push(e)
        }
      }

      return { workflow, entradas: Object.fromEntries(juntas.map((e) => [e.slice(0, e.indexOf('=')), e.slice(e.indexOf('=') + 1)])) }
    })
}

/**
 * Confere, do lado de cá, que o pedido vale para subir (regra 1 do plano): o Studio já confere, e isto é a segunda guarda.
 *
 * @param pedido O pedido, como o Studio o devolve.
 * @param cfg A configuração.
 * @returns O motivo de não valer, ou `null`.
 */
export function motivoParaNaoSubir(pedido, cfg) {
  if (typeof pedido?.id !== 'string' || !/^[0-9a-f-]{36}$/.test(pedido.id)) {
    return 'o pedido veio sem identificador'
  }

  if (String(pedido.repositorio).toLowerCase() !== cfg.repositorio) {
    return `o pedido é de ${pedido.repositorio}, e este repositório é ${cfg.repositorio}`
  }

  if (typeof pedido.commit !== 'string' || !formaDoCommit.test(pedido.commit)) {
    return 'o pedido não traz o commit inteiro'
  }

  if (pedido.canal !== 'web' && pedido.canal !== 'aplicativo') {
    return 'a aprovação não foi feita na tela'
  }

  if (pedido.aprovador?.id === undefined || pedido.decididaPor?.id !== pedido.aprovador.id) {
    return 'quem decidiu não é quem foi escolhido para aprovar'
  }

  if (cfg.aprovadores.length > 0 && !cfg.aprovadores.includes(String(pedido.aprovador.id).toLowerCase())) {
    return 'quem aprovou não está entre quem pode aprovar a publicação deste repositório'
  }

  return null
}

/** Os 7 primeiros caracteres do commit. */
const curto = (commit) => commit.slice(0, 7)

/** A chave de idempotência de uma marca: a mesma para o mesmo pedido e o mesmo desfecho, em qualquer execução. */
export function chaveDaMarca(pedido, desfecho) {
  const h = crypto.createHash('sha256').update(`${pedido}:${desfecho}`).digest('hex')

  return `${h.slice(0, 8)}-${h.slice(8, 12)}-4${h.slice(13, 16)}-8${h.slice(17, 20)}-${h.slice(20, 32)}`
}

/** A marca que a junção leva na mensagem: é por ela que a execução seguinte acha o que esta juntou. */
export const marcaDaJuncao = (pedido) => `Pedido de aprovação ${pedido}`

/** Quando, em São Paulo, como a diretoria lê: "05/11 às 16h12". */
export function quando(instante) {
  const partes = Object.fromEntries(
    new Intl.DateTimeFormat('pt-BR', { timeZone: 'America/Sao_Paulo', day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false })
      .formatToParts(instante)
      .map((p) => [p.type, p.value]),
  )

  return `${partes.day}/${partes.month} às ${partes.hour}h${partes.minute}`
}

/**
 * O que o repositório faz por esta execução: o git e os pedidos à API do GitHub e do Studio. Tudo o que muda alguma coisa
 * passa por aqui, e o ensaio troca o que muda por uma linha no registro.
 */
export class Publicacao {
  constructor(cfg, { git = rodarGit, buscar = fetch, registrar = console.log, agora = () => new Date() } = {}) {
    this.cfg = cfg
    this.git = git
    this.buscar = buscar
    this.registrar = registrar
    this.agora = agora
  }

  async #pedir(url, opcoes = {}) {
    const resposta = await this.buscar(url, opcoes)
    const texto = await resposta.text()
    let corpo = null

    try {
      corpo = texto.length === 0 ? null : JSON.parse(texto)
    } catch {
      corpo = texto
    }

    return { situacao: resposta.status, corpo }
  }

  /** Os pedidos aprovados deste repositório, do mais antigo ao mais novo. */
  async pedidos() {
    const r = await this.#pedir(`${this.cfg.studio}/api/publicacao/aprovacoes`, { headers: { authorization: `Bearer ${this.cfg.chave}`, accept: 'application/json' } })

    if (r.situacao !== 200 || !Array.isArray(r.corpo)) {
      throw new Error(`O Studio não respondeu a pergunta (situação ${r.situacao}${r.corpo?.titulo ? `: ${r.corpo.titulo}` : ''}).`)
    }

    return r.corpo
  }

  /** Marca o pedido como feito, ou registra que falhou. */
  async marcar(pedido, { falhou, resultado }) {
    if (this.cfg.ensaio) {
      this.registrar(`ENSAIO: marcaria o pedido ${pedido.id} como ${falhou ? 'falhou' : 'feito'}: ${resultado}`)

      return
    }

    const r = await this.#pedir(`${this.cfg.studio}/api/publicacao/aprovacoes/${pedido.id}/executar`, {
      method: 'POST',
      headers: {
        authorization: `Bearer ${this.cfg.chave}`,
        'content-type': 'application/json',
        'idempotency-key': chaveDaMarca(pedido.id, falhou ? `falhou:${resultado}` : 'feito'),
      },
      body: JSON.stringify({ resultado, commit: pedido.commit, ...(falhou ? { falhou: true } : {}) }),
    })

    if (r.situacao !== 200) {
      throw new Error(`O Studio recusou a marca do pedido ${pedido.id} (situação ${r.situacao}${r.corpo?.titulo ? `: ${r.corpo.titulo}` : ''}).`)
    }

    this.registrar(`Pedido ${pedido.id}: ${falhou ? 'a falha foi escrita no pedido' : 'marcado como feito'}. ${resultado}`)
  }

  #github(caminho, opcoes = {}) {
    return this.#pedir(`${this.cfg.github}/repos/${this.cfg.repositorio}${caminho}`, {
      ...opcoes,
      headers: { authorization: `Bearer ${this.cfg.token}`, accept: 'application/vnd.github+json', 'x-github-api-version': '2022-11-28', ...(opcoes.headers ?? {}) },
    })
  }

  /** As execuções disparadas pelo botão de um workflow na main, da mais nova para a mais velha. */
  async execucoes(workflow) {
    const r = await this.#github(`/actions/workflows/${encodeURIComponent(workflow)}/runs?branch=${encodeURIComponent(this.cfg.live)}&event=workflow_dispatch&per_page=20`)

    if (r.situacao !== 200 || !Array.isArray(r.corpo?.workflow_runs)) {
      throw new Error(`O GitHub não listou as execuções de ${workflow} (situação ${r.situacao}).`)
    }

    return r.corpo.workflow_runs
  }

  /** Dispara um workflow na main, com as entradas. */
  async disparar(workflow, entradas) {
    if (this.cfg.ensaio) {
      this.registrar(`ENSAIO: dispararia ${workflow} na ${this.cfg.live} com ${JSON.stringify(entradas)}`)

      return
    }

    const r = await this.#github(`/actions/workflows/${encodeURIComponent(workflow)}/dispatches`, {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ ref: this.cfg.live, inputs: entradas }),
    })

    if (r.situacao !== 204) {
      throw new Error(`O GitHub não disparou ${workflow} (situação ${r.situacao}).`)
    }

    this.registrar(`Disparado: ${workflow} na ${this.cfg.live}.`)
  }

  /** Se o commit está no ramo (ou é ele). */
  contem(ramo, commit) {
    try {
      this.git(['merge-base', '--is-ancestor', commit, ramo])

      return true
    } catch {
      return false
    }
  }

  /** A junção que esta publicação fez para o pedido, na main, ou `null`. */
  juncaoDe(pedido) {
    const achada = this.git(['log', `origin/${this.cfg.live}`, '--merges', '--fixed-strings', `--grep=${marcaDaJuncao(pedido.id)}`, '--format=%H', '-1']).trim()

    return achada.length === 0 ? null : achada
  }

  /** As entradas de um workflow, com o "auto:<pasta>" resolvido: "true" quando a pasta mudou desde a última tag. */
  entradasDe(publicacao) {
    return Object.fromEntries(
      Object.entries(publicacao.entradas).map(([nome, valor]) => {
        if (!valor.startsWith('auto:')) {
          return [nome, valor]
        }

        let ultima = ''

        try {
          ultima = this.git(['describe', '--tags', '--abbrev=0', `origin/${this.cfg.live}^`]).trim()
        } catch {
          // Sem tag nenhuma, tudo mudou.
        }

        if (ultima.length === 0) {
          return [nome, 'true']
        }

        try {
          this.git(['diff', '--quiet', ultima, `origin/${this.cfg.live}`, '--', valor.slice('auto:'.length)])

          return [nome, 'false']
        } catch {
          return [nome, 'true']
        }
      }),
    )
  }

  /**
   * Como estão os workflows da publicação de uma junção: a execução mais nova de cada um disparada na main a partir dela
   * (o commit dela ou um depois dela).
   *
   * @returns `faltam` (os que ainda não têm execução), `andando`, e as que terminaram, com a conclusão.
   */
  async situacaoDa(juncao) {
    const faltam = []
    const andando = []
    const terminadas = []

    for (const publicacao of this.cfg.publicacoes) {
      const execucao = (await this.execucoes(publicacao.workflow)).find((e) => typeof e.head_sha === 'string' && this.contem(e.head_sha, juncao))

      if (execucao === undefined) {
        faltam.push(publicacao)
      } else if (execucao.status !== 'completed') {
        andando.push({ publicacao, execucao })
      } else {
        terminadas.push({ publicacao, execucao })
      }
    }

    return { faltam, andando, terminadas }
  }

  /**
   * Junta o commit pedido na main e empurra. Recusa, sem juntar nada, o commit que não existe ou não está no develop, e a
   * junção com conflito.
   *
   * @returns A junção, ou o motivo da recusa.
   */
  juntar(pedido) {
    const { desenvolvimento, live } = this.cfg

    try {
      this.git(['cat-file', '-e', `${pedido.commit}^{commit}`])
    } catch {
      return { recusa: `O commit ${curto(pedido.commit)} não existe neste repositório: nada subiu.` }
    }

    if (!this.contem(`origin/${desenvolvimento}`, pedido.commit)) {
      return { recusa: `O commit ${curto(pedido.commit)} não está no ${desenvolvimento}: nada subiu.` }
    }

    if (this.contem(`origin/${live}`, pedido.commit)) {
      return { recusa: `O commit ${curto(pedido.commit)} já está na ${live} sem ter passado por esta publicação: nada subiu de novo. Confira o Live e marque pelo conector.` }
    }

    if (this.cfg.ensaio) {
      this.registrar(`ENSAIO: juntaria ${pedido.commit} na ${live} (pedido ${pedido.id}, "${pedido.titulo}"), e empurraria a ${live}.`)

      return { juncao: null }
    }

    this.git(['checkout', '-B', live, `origin/${live}`])

    try {
      this.git(['merge', '--no-ff', '--no-edit', '-m', `Merge do commit ${curto(pedido.commit)} na ${live}, aprovado no Studio`, '-m', marcaDaJuncao(pedido.id), pedido.commit])
    } catch {
      try {
        this.git(['merge', '--abort'])
      } catch {
        // Sem junção em andamento, nada a desfazer.
      }

      return { recusa: `A junção do commit ${curto(pedido.commit)} com a ${live} tem conflito: nada subiu. Leve a ${live} para o ${desenvolvimento} e peça de novo.` }
    }

    this.git(['push', 'origin', `HEAD:refs/heads/${live}`])

    return { juncao: this.git(['rev-parse', 'HEAD']).trim() }
  }

  /**
   * Uma volta: cuida do pedido mais antigo e para no primeiro que ainda está subindo. Um de cada vez, na ordem em que foram
   * aprovados.
   *
   * @returns O que aconteceu, para o registro e os testes.
   */
  async volta() {
    const pedidos = await this.pedidos()
    const feito = []

    if (pedidos.length === 0) {
      this.registrar('Nenhum pedido aprovado para este repositório.')

      return feito
    }

    this.git(['fetch', '--no-tags', 'origin', this.cfg.desenvolvimento, this.cfg.live])
    this.git(['fetch', '--tags', '--force', 'origin'])

    for (const pedido of pedidos) {
      const motivo = motivoParaNaoSubir(pedido, this.cfg)

      if (motivo !== null) {
        // O Studio só devolve o que vale: o que não confere aqui é defeito, e não sobe nem é marcado.
        this.registrar(`::error::O pedido ${pedido?.id ?? '?'} não sobe: ${motivo}.`)
        feito.push({ pedido: pedido?.id, desfecho: 'recusado-aqui', motivo })
        continue
      }

      const juncao = this.juncaoDe(pedido)

      if (juncao === null) {
        const junta = this.juntar(pedido)

        if (junta.recusa !== undefined) {
          await this.marcar(pedido, { falhou: true, resultado: junta.recusa })
          feito.push({ pedido: pedido.id, desfecho: 'recusado', motivo: junta.recusa })
          continue
        }

        for (const publicacao of this.cfg.publicacoes) {
          await this.disparar(publicacao.workflow, this.entradasDe(publicacao))
        }

        feito.push({ pedido: pedido.id, desfecho: 'disparado', juncao: junta.juncao })

        return feito
      }

      const { faltam, andando, terminadas } = await this.situacaoDa(juncao)
      const falhas = terminadas.filter((t) => t.execucao.conclusion !== 'success' && t.execucao.conclusion !== 'skipped')

      if (falhas.length > 0) {
        const lista = falhas.map((f) => `${f.publicacao.workflow} (${f.execucao.html_url ?? f.execucao.id})`).join(', ')

        await this.marcar(pedido, { falhou: true, resultado: `A publicação do commit ${curto(pedido.commit)} falhou em ${lista}. O commit já está na ${this.cfg.live}.` })
        feito.push({ pedido: pedido.id, desfecho: 'falhou' })
        continue
      }

      if (faltam.length > 0) {
        for (const publicacao of faltam) {
          await this.disparar(publicacao.workflow, this.entradasDe(publicacao))
        }

        feito.push({ pedido: pedido.id, desfecho: 'disparado', juncao })

        return feito
      }

      if (andando.length > 0) {
        this.registrar(`Pedido ${pedido.id}: a publicação está rodando (${andando.map((a) => a.publicacao.workflow).join(', ')}).`)
        feito.push({ pedido: pedido.id, desfecho: 'andando' })

        return feito
      }

      await this.marcar(pedido, { falhou: false, resultado: `Subiu ao Live em ${quando(this.agora())}, no commit ${curto(pedido.commit)}.` })
      feito.push({ pedido: pedido.id, desfecho: 'feito' })
    }

    return feito
  }
}

/** Roda o git na pasta de agora e devolve a saída; o que falha lança. */
function rodarGit(args) {
  return execFileSync('git', args, { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] })
}

if (import.meta.url === `file://${process.argv[1]}`) {
  const cfg = configuracao(process.env)

  if (cfg.erro !== undefined) {
    console.log(`::error::${cfg.erro}`)
    process.exit(1)
  }

  try {
    await new Publicacao(cfg).volta()
  } catch (erro) {
    console.log(`::error::${erro instanceof Error ? erro.message : String(erro)}`)
    process.exit(1)
  }
}
