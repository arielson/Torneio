# Template Generico - Gravacao Automatizada de Produto

## Objetivo

Este documento serve como base reutilizavel para planejar videos automatizados de qualquer sistema, aplicativo ou plataforma.

Ele foi pensado para:

- ambiente Windows
- gravacao recorrente
- pipeline automatizado
- videos com narracao
- videos com legenda
- reutilizacao entre projetos

## Decisao Tecnica Base

Padrao recomendado:

- `Emulador` ou `simulador` para apps, quando possivel
- `automacao de UI` para execucao dos fluxos
- `TTS` para narracao
- `SRT` gerado do roteiro para legenda
- `FFmpeg` para composicao final

## Critério Para Escolher Emulador ou Espelhamento

Escolha `emulador/simulador` quando precisar de:

- repetibilidade
- qualidade visual previsivel
- automacao robusta
- ambiente controlado
- pouca variacao entre gravacoes

Escolha `espelhamento de aparelho real` apenas quando precisar de:

- demonstrar hardware fisico
- mostrar recursos dependentes do dispositivo
- gravar uso real fora do ambiente controlado

## Estrutura de Planejamento

### 1. Escopo

Definir:

- produto
- publico-alvo
- plataformas
- perfis de usuario
- funcionalidades que entram
- funcionalidades que ficam de fora

### 2. Roteiro operacional

Cada cena deve conter:

- id
- plataforma
- perfil
- contexto
- pre-condicoes
- acoes
- resultado esperado
- texto da narracao
- texto da legenda
- duracao estimada

### 3. Massa de dados

Definir:

- usuarios
- senhas
- dados de exemplo
- cenarios especiais
- estados do sistema necessarios para cada cena

### 4. Ambiente de captura

Padronizar:

- resolucao
- zoom
- escala de fonte
- idioma
- tema
- notificacoes
- navegador
- dispositivo virtual

### 5. Pipeline de automacao

Separar em etapas:

1. preparar ambiente
2. restaurar ou popular dados
3. executar automacao
4. gravar tela
5. gerar narracao
6. gerar legenda
7. montar video final

## Estrutura de Pastas Sugerida

```text
docs/
  roteiro-projeto.md
  roteiro-template.md

artifacts/
  video/
  audio/
  subtitles/
  temp/

scripts/
  video/
    app/
    web/
    media/
```

## Pilha Tecnologica Sugerida

### Web

- `Playwright`

### Mobile

- `Appium`
- `Android Emulator`
- `iOS Simulator`, quando aplicavel

### Gravacao

- gravador nativo do emulador, simulador ou ferramenta dedicada

### Audio

- `Google Cloud Text-to-Speech`
- `ElevenLabs`

### Edicao e composicao

- `FFmpeg`

## Checklist Inicial

- roteiro aprovado
- massa de dados pronta
- ambiente configurado
- automacao validada
- audios gerados
- legendas geradas
- exportacao final validada

## Checklist de Qualidade

- sem notificacoes indevidas
- sem dados sensiveis
- sem travamentos visuais
- sem textos cortados
- sem transicoes bruscas nao planejadas
- narracao alinhada com a imagem
- legenda sincronizada
- resolucao final correta

## Fontes de referencia

- Playwright videos: https://playwright.dev/docs/videos
- Appium drivers: https://appium.io/docs/en/latest/ecosystem/drivers/
- Android Emulator gravacao: https://developer.android.com/studio/run/emulator-record-screen
- scrcpy: https://github.com/Genymobile/scrcpy
- FFmpeg: https://ffmpeg.org/ffmpeg-doc.html
- Google Cloud Text-to-Speech: https://cloud.google.com/text-to-speech/docs
- ElevenLabs TTS: https://elevenlabs.io/text-to-speech-api/

## Proximo passo padrao

Apos criar este documento, o primeiro trabalho concreto deve ser montar o roteiro operacional da plataforma alvo.
