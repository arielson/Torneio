# Roteiro de Gravacao Automatizada - Projeto Torneio

## Objetivo

Este documento organiza o passo a passo para produzir videos demonstrativos do projeto `Torneio` com execucao automatica em maquina Windows.

O foco sera:

- `App Android`
- `Projeto Web`
- perfis `Administrador do Torneio` e `Fiscal`
- sem incluir fluxos de `Administrador Geral`
- com `legenda`
- com `narracao em audio`
- com pipeline `automatico` como padrao

## Decisao Tecnica

Para este projeto, o padrao recomendado e:

- `Android Emulator` para gravacao do app
- `Appium` para automacao do app
- `Playwright` para automacao do projeto web
- `FFmpeg` para montagem final do video
- `Google Cloud Text-to-Speech` ou `ElevenLabs` para narracao
- arquivo `.srt` gerado diretamente do roteiro para legendas

## Por Que Emulador Em Vez de Espelhamento

Em Windows, o emulador e a melhor base para videos recorrentes porque oferece:

- resolucao estavel
- ambiente previsivel
- ausencia de notificacoes pessoais
- melhor reprodutibilidade
- automacao mais confiavel
- menos retrabalho em novas gravacoes

O espelhamento com aparelho real via `scrcpy` fica como opcao secundaria para cenarios em que seja necessario demonstrar hardware fisico, camera real ou comportamento especifico do aparelho.

## Escopo dos Videos

### Torneios de referencia

Usar os seguintes torneios como base:

1. `Torneio BTS Sport Fishing 2026`
   - `Modo de Sorteio = Nenhum`
2. `Amigos da Pesca 2026`
   - `Modo de Sorteio = Sorteio`
3. `Rei dos Mares 2026`
   - `Modo de Sorteio = Sorteio`

### Regras de demonstracao

- O torneio `BTS` deve mostrar os comportamentos sem sorteio.
- Os torneios `Amigos da Pesca` e `Rei dos Mares` podem ser intercalados para demonstrar os fluxos com sorteio.
- O menu e os comportamentos devem respeitar o perfil logado.
- A massa de dados precisa estar preparada antes da gravacao.

## O Que Precisamos Fazer a Partir de Agora

### Fase 1 - Fechar o roteiro operacional

Transformar o roteiro conceitual em roteiro de producao, cena por cena.

Cada cena deve conter:

- identificador da cena
- plataforma: `app` ou `web`
- perfil: `admin do torneio` ou `fiscal`
- torneio usado
- pre-condicoes
- acoes executadas
- resultado esperado em tela
- texto exato da narracao
- texto exato da legenda
- duracao estimada

### Fase 2 - Congelar a massa de dados

Preparar os dados para que toda execucao gere o mesmo video.

Precisamos validar:

- usuarios de `admin do torneio`
- usuarios de `fiscal`
- embarcacoes cadastradas
- pescadores cadastrados
- peixes cadastrados
- fiscais cadastrados
- capturas de exemplo
- relatorios com dados suficientes
- casos de reorganizacao emergencial
- casos de sorteio nao realizado
- casos de sorteio realizado
- status do torneio em estados esperados para demonstracao

### Fase 3 - Padronizar o ambiente Windows

Definir o ambiente onde a automacao sera executada:

- versao do Windows usada nas gravacoes
- Android Studio instalado
- Android Emulator configurado
- JDK configurado
- Node.js configurado
- Appium configurado
- FFmpeg configurado no `PATH`
- navegador e resolucao padronizados para web
- fonte e escala do Android fixas
- tema do Android fixo
- sem notificacoes e sem interferencias

### Fase 4 - Implementar a automacao

Construir o pipeline automatico:

- scripts para abrir ambiente
- scripts para preparar dados
- scripts para executar fluxos no app
- scripts para executar fluxos na web
- scripts para gravar tela
- scripts para gerar audio
- scripts para gerar legenda
- scripts para compor o video final

### Fase 5 - Gerar videos finais

Executar o pipeline para produzir:

- video do `app`
- video da `web`
- com narracao
- com legenda
- com estrutura reutilizavel para outros projetos

## Sequencia Recomendada

Seguir nesta ordem:

1. Criar o roteiro operacional do `app`.
2. Criar o roteiro operacional da `web`.
3. Criar a checklist da massa de dados.
4. Padronizar o ambiente Windows de execucao.
5. Definir a estrutura dos scripts de automacao.
6. Implementar a captura automatica do `app`.
7. Implementar a captura automatica da `web`.
8. Implementar a geracao de narracao.
9. Implementar a geracao de legendas.
10. Implementar a montagem final do video.

## Arquitetura Recomendada

### App Android

- execucao em `Android Emulator`
- automacao com `Appium`
- gravacao da tela pelo proprio emulador ou por ferramenta de captura controlada por script

### Web

- automacao com `Playwright`
- gravacao por captura de viewport ou por composicao orientada por cena

### Audio

- gerar audio a partir do texto aprovado do roteiro
- usar a mesma voz em todos os videos para manter padrao

### Legendas

- gerar `.srt` a partir do proprio roteiro
- evitar depender de transcricao automatica quando o objetivo e producao recorrente

### Edicao final

- unir video, audio e legenda via `FFmpeg`
- exportar versao final padrao para apresentacao e publicacao

## Estrutura de Pastas Sugerida

```text
docs/
  roteiro-gravacao-automatica-torneio.md
  roteiro-gravacao-template.md

artifacts/
  video/
    app/
    web/
  audio/
  subtitles/
  temp/

scripts/
  video/
    run.ps1
    manifests/
    app/
      run.ps1
      start-emulator.ps1
      record-emulator.ps1
    web/
      run.ps1
      record-web.ps1
    media/
      generate-script.ps1
      generate-subtitles.ps1
      generate-voice.ps1
      compose-video.ps1
    common/
      setup-tools.ps1
      prepare-output.ps1
      assert-tools.ps1
      helpers.ps1
      paths.ps1
```

## Scripts Criados Nesta Primeira Etapa

Ja existe uma base inicial para o pipeline:

- `scripts/video/run.ps1`
- `scripts/video/app/run.ps1`
- `scripts/video/web/run.ps1`
- `scripts/video/package.json`
- `scripts/video/app/start-emulator.ps1`
- `scripts/video/app/start-appium.ps1`
- `scripts/video/app/build-install-app.ps1`
- `scripts/video/app/record-emulator.ps1`
- `scripts/video/app/record-app.mjs`
- `scripts/video/web/record-web.ps1`
- `scripts/video/web/record-web.mjs`
- `scripts/video/web/start-web.ps1`
- `scripts/video/media/generate-script.ps1`
- `scripts/video/media/generate-subtitles.ps1`
- `scripts/video/media/generate-voice.ps1`
- `scripts/video/media/compose-video.ps1`
- `scripts/video/manifests/app-demo.json`
- `scripts/video/manifests/web-demo.json`
- `scripts/video/manifests/template-demo.json`

## O Que Essa Base Ja Faz

Nesta etapa inicial, o pipeline ja consegue:

- organizar as pastas de saida
- carregar manifests JSON para web e app
- gerar um roteiro Markdown a partir do manifesto
- gerar legenda `.srt` a partir das cenas
- gerar um arquivo texto com a narracao base
- validar ferramentas obrigatorias
- iniciar o emulador Android, quando solicitado
- instalar dependencias Node do pipeline
- instalar o browser Chromium do Playwright
- instalar o driver `UiAutomator2` do Appium
- executar uma gravacao web bruta baseada em cenas e URLs do manifesto
- executar uma gravacao Android bruta baseada em `adb + Appium + UiAutomator2`
- subir a retaguarda web automaticamente
- instalar o APK debug do app no emulador

## O Que Ainda Falta Conectar

As proximas etapas de implementacao sao:

- executar o fluxo real do `Appium` no app
- executar o fluxo real do `Playwright` na web
- amadurecer os seletores e passos reais de cada cena do app
- integrar um provedor real de `TTS`
- montar o `.mp4` final com `FFmpeg`
- preparar a massa de dados automaticamente antes da gravacao

## Comandos Planejados

Os comandos-base do pipeline ficaram assim:

```powershell
.\scripts\video\run.ps1 -Target web -GenerateOnly
.\scripts\video\run.ps1 -Target app -GenerateOnly
.\scripts\video\run.ps1 -Target app -StartEmulator
.\scripts\video\common\setup-tools.ps1 -InstallNodeDependencies -InstallPlaywrightBrowsers
.\scripts\video\run.ps1 -Target web -InstallDependencies
.\scripts\video\common\setup-tools.ps1 -InstallNodeDependencies -InstallAppiumDriver
.\scripts\video\run.ps1 -Target web -InstallDependencies -StartWeb
.\scripts\video\run.ps1 -Target app -InstallDependencies -StartEmulator -BuildAndInstallApp -StartAppium
```

O modo `GenerateOnly` serve para validar manifesto, roteiro, legenda e texto de narracao sem ainda executar a gravacao automatica.

## Credenciais Por Variavel de Ambiente

Para evitar editar os manifests a cada execucao, o pipeline aceita credenciais por variaveis de ambiente.
Na `web`, cada torneio deve usar o `admin do torneio` do respectivo `slug`.
No `app`, as cenas podem alternar entre `Admin do Torneio` e `Fiscal`, porque os menus sao diferentes.

### Web

```powershell
.\scripts\video\set-video-env.ps1
.\scripts\video\run.ps1 -Target web -InstallDependencies -StartWeb
```

### App

```powershell
.\scripts\video\set-video-env.ps1
.\scripts\video\run.ps1 -Target app -InstallDependencies -StartEmulator -BuildAndInstallApp -StartAppium
```

Edite o arquivo abaixo para informar os usuarios e senhas reais:

- `scripts/video/set-video-env.ps1`

Nesse mesmo arquivo voce tambem pode ajustar:

- `VIDEO_WEB_BASE_URL`
- `VIDEO_SLUG_AMIGOS`
- `VIDEO_SLUG_REI`
- `VIDEO_SLUG_BTS`

Essas variaveis sobrescrevem os slugs e a `baseUrl` definidos nos manifests.

## Saidas Finais Esperadas

Quando a execucao completa termina com sucesso, os arquivos finais ficam em:

- `artifacts/video/web/web-demo-final.mp4`
- `artifacts/video/app/app-demo-final.mp4`

Artefatos intermediarios:

- `artifacts/video/audio/`
- `artifacts/video/subtitles/`
- `artifacts/video/web/raw/`
- `artifacts/video/app/raw/`
- `artifacts/video/web/screens/`
- `artifacts/video/app/screens/`

## Ferramentas Recomendadas

### Obrigatorias

- `Android Studio`
- `Android Emulator`
- `Node.js`
- `Appium`
- `Playwright`
- `FFmpeg`

### Opcionais

- `scrcpy` para testes rapidos em aparelho real
- `OBS Studio` para capturas manuais assistidas
- `Google Cloud Text-to-Speech`
- `ElevenLabs`

## Fontes de referencia

- Playwright videos: https://playwright.dev/docs/videos
- Appium drivers: https://appium.io/docs/en/latest/ecosystem/drivers/
- Appium Android UiAutomator2: https://appium.io/docs/en/2.1/quickstart/uiauto2-driver/
- Android Emulator gravacao: https://developer.android.com/studio/run/emulator-record-screen
- scrcpy: https://github.com/Genymobile/scrcpy
- FFmpeg: https://ffmpeg.org/ffmpeg-doc.html
- FFmpeg filtros e subtitles: https://www.ffmpeg.org/ffmpeg-filters.html
- Google Cloud Text-to-Speech: https://cloud.google.com/text-to-speech/docs
- Gemini TTS: https://docs.cloud.google.com/text-to-speech/docs/gemini-tts
- ElevenLabs TTS: https://elevenlabs.io/text-to-speech-api/
- OBS Studio overview: https://obsproject.com/eu/wiki/obs-studio-overview

## Proximo passo

O proximo passo recomendado e criar o `roteiro operacional do app`, cena por cena, ja no formato que depois alimentara a automacao.
