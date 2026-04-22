$ErrorActionPreference = 'Stop'

function Get-BrowserPath {
    $candidates = @(
        'C:\Program Files\Google\Chrome\Application\chrome.exe',
        'C:\Program Files (x86)\Google\Chrome\Application\chrome.exe',
        'C:\Program Files\Microsoft\Edge\Application\msedge.exe',
        'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw 'Nao foi possivel localizar Chrome ou Edge em caminhos padrao.'
}

function New-HtmlDocument {
    param(
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][string]$Subtitle,
        [Parameter(Mandatory = $true)][string]$Body
    )

    $generatedAt = Get-Date -Format 'dd/MM/yyyy HH:mm'

    return @"
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8" />
  <title>$Title</title>
  <style>
    @@page {
      size: A4;
      margin: 18mm 16mm;
    }
    body {
      font-family: "Segoe UI", Arial, sans-serif;
      color: #1f2937;
      font-size: 12px;
      line-height: 1.5;
      margin: 0;
    }
    h1 {
      font-size: 24px;
      margin: 0 0 4px 0;
      color: #0f172a;
    }
    h2 {
      font-size: 16px;
      margin: 20px 0 8px 0;
      color: #0f172a;
      border-bottom: 1px solid #cbd5e1;
      padding-bottom: 4px;
    }
    h3 {
      font-size: 13px;
      margin: 16px 0 6px 0;
      color: #1d4ed8;
    }
    p {
      margin: 6px 0;
    }
    ul, ol {
      margin: 6px 0 10px 22px;
      padding: 0;
    }
    li {
      margin: 4px 0;
    }
    .cover {
      border: 1px solid #cbd5e1;
      border-radius: 14px;
      padding: 18px;
      background: linear-gradient(180deg, #f8fafc 0%, #eef2ff 100%);
      margin-bottom: 18px;
    }
    .subtitle {
      color: #475569;
      font-size: 13px;
      margin-bottom: 8px;
    }
    .meta {
      font-size: 11px;
      color: #64748b;
    }
    .box {
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      padding: 10px 12px;
      margin: 8px 0 12px 0;
    }
    .tag {
      display: inline-block;
      background: #dbeafe;
      color: #1d4ed8;
      border-radius: 999px;
      padding: 2px 8px;
      font-size: 10px;
      font-weight: 600;
      margin-right: 6px;
    }
    .small {
      font-size: 11px;
      color: #475569;
    }
    .page-break {
      page-break-before: always;
    }
  </style>
</head>
<body>
  <div class="cover">
    <div class="tag">Roteiro de Gravacao</div>
    <h1>$Title</h1>
    <div class="subtitle">$Subtitle</div>
    <div class="meta">Gerado automaticamente em $generatedAt</div>
  </div>
  $Body
</body>
</html>
"@
}

function Save-PdfFromHtml {
    param(
        [Parameter(Mandatory = $true)][string]$BrowserPath,
        [Parameter(Mandatory = $true)][string]$HtmlPath,
        [Parameter(Mandatory = $true)][string]$PdfPath
    )

    $uri = 'file:///' + ($HtmlPath -replace '\\', '/')
    $arguments = @(
        '--headless=new'
        '--disable-gpu'
        '--allow-file-access-from-files'
        '--no-pdf-header-footer'
        "--print-to-pdf=$PdfPath"
        $uri
    )

    $process = Start-Process -FilePath $BrowserPath -ArgumentList $arguments -PassThru -Wait -WindowStyle Hidden
    if ($process.ExitCode -ne 0) {
        throw "Falha ao gerar PDF '$PdfPath'. Codigo de saida: $($process.ExitCode)"
    }
}

$downloadDir = 'C:\Users\ariel\Downloads'
$tempDir = Join-Path $downloadDir '_roteiros_temp'
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

$browserPath = Get-BrowserPath

$roteiros = @(
    [PSCustomObject]@{
        FileName = '1_Roteiro_Admin_Geral.pdf'
        Title = 'Roteiro Completo do Administrador Geral'
        Subtitle = 'Fluxo focado na retaguarda web, cobrindo governança da plataforma, torneios, acessos, banners e auditoria.'
        Body = @'
<h2>Objetivo do video</h2>
<p>Apresentar todas as responsabilidades do Administrador Geral, que controla a plataforma como um todo, sem entrar nos detalhes operacionais internos de cada torneio.</p>

<h2>Cenario recomendado</h2>
<div class="box">
  <ul>
  <li>Usar pelo menos tres torneios ja cadastrados: <strong>Torneio BTS Sport Fishing 2026</strong>, <strong>Amigos da Pesca 2026</strong> e <strong>Rei dos Mares 2026</strong>.</li>
  <li>Garantir que pelo menos um torneio esteja ativo e outro inativo para demonstrar os dois estados.</li>
  <li>Manter ao menos um admin geral e um admin de torneio cadastrados para demonstrar a gestao de acessos.</li>
  <li>Deixar pelo menos um torneio com registro publico de pescador habilitado para mostrar a governanca da nova funcionalidade.</li>
  </ul>
</div>

<h2>Sequencia sugerida de gravacao</h2>
<ol>
  <li><strong>Abertura e login</strong>: acessar a tela inicial da web, entrar no login do Administrador Geral e autenticar com sucesso.</li>
  <li><strong>Painel Geral</strong>: mostrar os indicadores de quantidade de torneios, ativos e inativos, e explicar que este painel centraliza toda a operacao da plataforma.</li>
  <li><strong>Lista de torneios</strong>: percorrer a grade/lista principal, destacando nome, slug, status e modo de sorteio de cada torneio.</li>
  <li><strong>Criar novo torneio</strong>: entrar no cadastro de torneio e mostrar os principais parametros:
    <ul>
      <li>nome, slug e logo;</li>
      <li>tipo de torneio e nomenclaturas customizaveis;</li>
      <li>modo de sorteio;</li>
      <li>fator multiplicador;</li>
      <li>quantidade de ganhadores;</li>
      <li>premiacao por embarcacao e/ou pescador;</li>
      <li>flag para exibir ou nao o modulo financeiro;</li>
      <li>flag para permitir ou nao o registro publico de pescador.</li>
    </ul>
  </li>
  <li><strong>Editar torneio existente</strong>: abrir um torneio ja criado e mostrar que os parametros podem ser ajustados sem precisar recriar o evento.</li>
  <li><strong>Ativar e desativar torneio</strong>: demonstrar os botoes de ativacao e desativacao para controle de disponibilidade da operacao.</li>
  <li><strong>Excluir torneio</strong>: apenas mostrar a acao e o alerta de exclusao permanente, sem confirmar se a base estiver em uso.</li>
  <li><strong>Acessar painel de um torneio</strong>: usar o atalho direto para entrar no painel administrativo interno de um dos torneios.</li>
  <li><strong>Reorganizacao emergencial pelo painel geral</strong>: mostrar que o Admin Geral tambem possui acesso ao atalho de reorganizacao emergencial para torneios com sorteio habilitado.</li>
  <li><strong>Admins Gerais</strong>: abrir o modulo de Administradores Gerais, listar usuarios existentes, criar um novo admin e demonstrar a remocao quando aplicavel.</li>
  <li><strong>Admins do Torneio</strong>: a partir de um torneio especifico, abrir a tela de admins do torneio, listar os usuarios vinculados, criar um novo admin e mostrar a remocao.</li>
  <li><strong>Banners</strong>: acessar o modulo de banners e demonstrar:
    <ul>
      <li>cadastro com imagem;</li>
      <li>associacao a um torneio;</li>
      <li>destinos suportados, como torneio, site, WhatsApp, Instagram e e-mail;</li>
      <li>ativacao, desativacao e exclusao.</li>
    </ul>
  </li>
  <li><strong>Validacao publica</strong>: abrir a pagina publica de um torneio e mostrar que as configuracoes definidas pelo Admin Geral se refletem no ambiente do evento, inclusive login e registro do pescador quando habilitados.</li>
  <li><strong>Logs de auditoria</strong>: abrir a tela de logs, aplicar filtros por torneio, categoria, perfil, periodo e busca livre.</li>
  <li><strong>Limpeza de logs</strong>: mostrar o botao de limpar todos e explicar que, apos a limpeza, um novo log e gravado registrando a acao com os dados relevantes.</li>
  <li><strong>Encerramento</strong>: reforcar que o Admin Geral e o perfil de governanca da plataforma e finalizar com logout.</li>
</ol>

<h2>Pontos de narracao importantes</h2>
<ul>
  <li>O Administrador Geral administra a plataforma e os torneios, mas a operacao diaria de cada evento acontece no perfil de Admin do Torneio.</li>
  <li>Os slugs permitem separar ambientes de torneio sem mistura de dados.</li>
  <li>As flags de configuracao do torneio determinam o que aparece para administradores, fiscais, pescadores e publico.</li>
  <li>Os logs de auditoria sustentam rastreabilidade administrativa e operacional.</li>
</ul>

<h2>Resultado esperado do video</h2>
<p>Ao final, o espectador deve entender como novos torneios sao criados, configurados, ativados, auditados e delegados para administradores locais.</p>
'@
    },
    [PSCustomObject]@{
        FileName = '2_Roteiro_Admin_Torneio.pdf'
        Title = 'Roteiro Completo do Administrador do Torneio'
        Subtitle = 'Fluxo principal da retaguarda web, cobrindo configuração, operação, financeiro, sorteio, relatórios e exceções.'
        Body = @'
<h2>Objetivo do video</h2>
<p>Demonstrar todas as funcionalidades disponiveis ao Administrador do Torneio na retaguarda web, desde a configuracao inicial ate o encerramento operacional do evento.</p>

<h2>Cenario recomendado</h2>
<div class="box">
  <ul>
    <li>Usar o <strong>Torneio BTS Sport Fishing 2026</strong> para mostrar regras sem sorteio.</li>
    <li>Usar <strong>Amigos da Pesca 2026</strong> e <strong>Rei dos Mares 2026</strong> para mostrar os fluxos com sorteio.</li>
    <li>Manter dados de exemplo para embarcacoes, pescadores, peixes, fiscais, patrocinadores, capturas, premios e modulos financeiros.</li>
  </ul>
</div>

<h2>Sequencia sugerida de gravacao</h2>
<ol>
  <li><strong>Login do Admin do Torneio</strong>: acessar a URL do torneio, entrar com usuario local e chegar ao painel interno.</li>
  <li><strong>Painel do torneio</strong>: apresentar o nome do evento, status atual, patrocinadores exibidos na tela inicial e os atalhos principais.</li>
  <li><strong>Status do torneio</strong>: mostrar as transicoes entre <strong>Aberto</strong>, <strong>Liberado</strong> e <strong>Finalizado</strong>, incluindo o retorno para aberto quando necessario.</li>
  <li><strong>Clonagem de torneio</strong>: abrir a tela de clonagem e explicar que ela acelera a criacao de novas edicoes com base em configuracoes anteriores.</li>
  <li><strong>Configuracao publica do torneio</strong>: abrir a edicao do torneio e destacar as flags que controlam a tela publica, incluindo modulo financeiro, premiacao por embarcacao ou pescador e registro publico de pescador.</li>
  <li><strong>Cadastro de fiscais</strong>: listar, criar, editar e remover fiscais; destacar foto, usuario, senha e vinculacao de multiplas embarcacoes ao fiscal.</li>
  <li><strong>Cadastro de embarcacoes</strong>: listar, criar, editar e remover; mostrar foto da embarcacao, foto do capitao, capitao, vagas e, se o financeiro estiver habilitado, custo e status financeiro.</li>
  <li><strong>Gerenciamento de pescadores por embarcacao</strong>: quando o modo de sorteio exigir distribuicao, abrir a tela interna de membros da embarcacao para vincular e remover pescadores.</li>
  <li><strong>Caso especial do modo Nenhum</strong>: mostrar que, nesse modo, nao ha menu de sorteio e nao ha exibicao de quantidade/lista de pescadores por embarcacao.</li>
  <li><strong>Cadastro de pescadores</strong>: listar, criar, editar e remover; destacar nome, foto, celular formatado, tamanho da camisa quando o financeiro estiver ativo e usuario/senha opcionais para acesso do pescador.</li>
  <li><strong>Cadastro de peixes</strong>: listar, criar, editar e remover; mostrar imagem, fator multiplicador e comprimento minimo opcional.</li>
  <li><strong>Cadastro de patrocinadores</strong>: criar patrocinadores com imagem e pelo menos um destino opcional; mostrar flags de exibicao na tela inicial e nos relatorios.</li>
  <li><strong>Tela publica do torneio</strong>: abrir a pagina inicial publica do torneio e mostrar patrocinadores, login do admin, login do pescador, botao de registro publico e recuperacao de senha do pescador quando a funcionalidade estiver habilitada.</li>
  <li><strong>Capturas</strong>: acessar a lista de capturas e demonstrar:
    <ul>
      <li>visualizacao das fotos;</li>
      <li>alteracao do tamanho da captura com log;</li>
      <li>invalidacao e revalidacao;</li>
      <li>remocao quando necessario.</li>
    </ul>
  </li>
  <li><strong>Reorganizacao emergencial</strong>: abrir a tela, explicar o caracter excepcional da operacao, preencher motivo, digitar a confirmacao exigida e concluir a troca de pescador entre embarcacoes.</li>
  <li><strong>Sorteio</strong>: nos torneios com sorteio habilitado, abrir o modulo, exibir as pre-condicoes, realizar o calculo, mostrar a animacao/resultado, confirmar o sorteio, ajustar posicoes se necessario e tambem demonstrar a limpeza do resultado.</li>
  <li><strong>Premios</strong>: cadastrar premios por posicao e explicar que a premiacao pode contemplar embarcacoes e/ou pescadores de acordo com a configuracao do torneio.</li>
  <li><strong>Financeiro - visao geral</strong>: abrir o dashboard e apresentar os indicadores calculados no backend.</li>
  <li><strong>Financeiro - configuracao</strong>: definir valor por pescador, quantidade de parcelas, data de vencimento e taxa de inscricao opcional; destacar a confirmacao para substituir configuracao anterior.</li>
  <li><strong>Financeiro - gerenciamento de parcelas</strong>: abrir a tela especifica e demonstrar gerar para novos pescadores, regerar parcelas de pescadores selecionados e regerar parcelas gerais com cautela.</li>
  <li><strong>Financeiro - cobrancas</strong>: filtrar por pescador, tipo, nao pagas e inadimplentes; mostrar edicao de vencimento, observacao, marcacao de pagamento, alteracao manual com log e envio de comprovante.</li>
  <li><strong>Financeiro - custos</strong>: cadastrar custos gerais com categoria em portugues, quantidade, valor, vencimento e explicar que custos de embarcacoes compoem o total quando o modulo financeiro esta ativo.</li>
  <li><strong>Financeiro - produtos extras</strong>: cadastrar produtos opcionais e registrar vendas por pescador, lembrando que toda venda gera cobranca e o pagamento e controlado na tela de cobrancas.</li>
  <li><strong>Financeiro - doacoes</strong>: registrar doacoes de patrocinadores em dinheiro ou produto e explicar o impacto das doacoes em dinheiro na receita prevista.</li>
  <li><strong>Financeiro - checklist</strong>: cadastrar itens operacionais, definir responsavel entre admins do torneio e marcar a conclusao.</li>
  <li><strong>Financeiro - relatorios</strong>: apresentar os relatorios financeiros e o grafico de linha para comparar previsao de recebimentos e pagamentos com base nos vencimentos.</li>
  <li><strong>Relatorios</strong>: gerar os relatorios por embarcacao, por pescador, de ganhadores e de maiores capturas, destacando a presenca da secao de patrocinadores quando houver exibicao habilitada.</li>
  <li><strong>Encerramento</strong>: retornar ao painel e finalizar mostrando que toda a operacao do torneio pode ser administrada sem apoio de planilhas externas.</li>
</ol>

<h2>Pontos de narracao importantes</h2>
<ul>
  <li>O menu Sorteio desaparece automaticamente quando o modo de sorteio do torneio e <strong>Nenhum</strong>.</li>
  <li>A reorganizacao emergencial exige cautela, motivo e confirmacao explicita, alem de gerar log.</li>
  <li>O modulo financeiro pode ser ocultado por configuracao do torneio.</li>
  <li>As imagens de embarcacoes, capitaes, pescadores, peixes, fiscais e capturas ajudam na conferencia operacional.</li>
  <li>O acesso do pescador depende de credenciais opcionais e a recuperacao de senha pode ser feita com validacao por SMS quando o torneio permitir.</li>
</ul>

<h2>Resultado esperado do video</h2>
<p>Ao final, o espectador deve entender como o Admin do Torneio conduz o evento completo na retaguarda, incluindo cadastros, operacao, controle financeiro, sorteio, premiacao e relatorios.</p>
'@
    },
    [PSCustomObject]@{
        FileName = '3_Roteiro_Fiscal_App.pdf'
        Title = 'Roteiro Completo do Fiscal pelo App'
        Subtitle = 'Fluxo mobile focado em preparação offline, registro de capturas, lista histórica e sincronização controlada.'
        Body = @'
<h2>Objetivo do video</h2>
<p>Mostrar a rotina do fiscal no aplicativo, desde a entrada no torneio ate o envio e a sincronizacao das capturas, destacando o comportamento offline.</p>

<h2>Cenario recomendado</h2>
<div class="box">
  <ul>
    <li>Usar um torneio <strong>Liberado</strong> com fiscais, embarcacoes, pescadores e peixes ja cadastrados.</li>
    <li>Manter fotos em embarcacoes, pescadores e peixes para evidenciar a identificacao visual.</li>
    <li>Preparar pelo menos uma captura pendente de sincronizacao para demonstrar o fluxo manual.</li>
  </ul>
</div>

<h2>Sequencia sugerida de gravacao</h2>
<ol>
  <li><strong>Tela inicial do app</strong>: abrir o aplicativo e mostrar a lista de torneios disponiveis para acesso.</li>
  <li><strong>Detalhes do torneio</strong>: abrir um torneio especifico, apresentar nome, status, medida da captura e botao de login administrativo/fiscal.</li>
  <li><strong>Login do fiscal</strong>: autenticar como fiscal e entrar no painel principal do app.</li>
  <li><strong>Carregamento inicial dos dados</strong>: explicar que, logo apos o login, o app carrega embarcacoes, pescadores, peixes e imagens necessarias para continuar operando mesmo em area sem internet.</li>
  <li><strong>Painel do fiscal</strong>: apresentar:
    <ul>
      <li>saudacao e identificacao do perfil;</li>
      <li>patrocinadores visiveis;</li>
      <li>embarcacoes sob responsabilidade do fiscal;</li>
      <li>indicador de capturas pendentes de sincronizacao.</li>
    </ul>
  </li>
  <li><strong>Registrar captura</strong>: abrir a tela de registro e mostrar:
    <ul>
      <li>selecao pesquisavel de embarcacao;</li>
      <li>selecao pesquisavel de pescador;</li>
      <li>selecao pesquisavel de peixe;</li>
      <li>visualizacao ampliada das imagens para conferencia;</li>
      <li>preenchimento de medida e foto da captura.</li>
    </ul>
  </li>
  <li><strong>Sincronizar depois</strong>: demonstrar uma captura marcada para sincronizacao posterior e explicar que ela nao sera enviada automaticamente se essa opcao tiver sido escolhida.</li>
  <li><strong>Registrar agora</strong>: mostrar uma captura sendo enviada imediatamente quando houver conectividade e a opcao de envio direto estiver selecionada.</li>
  <li><strong>Lista de capturas registradas</strong>: abrir o historico local do fiscal e conferir as capturas ja registradas.</li>
  <li><strong>Tela de sincronizacao</strong>: abrir a tela especifica de sincronizacao e mostrar que as capturas pendentes podem ser enviadas manualmente pelo botao <strong>Sincronizar agora</strong>.</li>
  <li><strong>Consulta visual</strong>: abrir imagens em tela cheia em embarcacoes, pescadores, peixes ou capturas para reforcar a conferencia em campo.</li>
  <li><strong>Teste de contingencia</strong>: reforcar em narracao que o app suporta operacao offline porque os dados criticos sao baixados logo apos o login.</li>
  <li><strong>Atualizacao da tela inicial</strong>: retornar ao painel do fiscal para mostrar a mudanca no contador de pendencias.</li>
  <li><strong>Logout</strong>: encerrar a sessao para fechar o fluxo operacional do fiscal.</li>
</ol>

<h2>Pontos de narracao importantes</h2>
<ul>
  <li>Quando o torneio estiver em modo de sorteio <strong>Nenhum</strong>, o painel do fiscal nao deve destacar a contagem do tipo <strong>0/6 pescadores</strong> por embarcacao.</li>
  <li>As imagens pequenas podem ser ampliadas para facilitar a conferencia em campo.</li>
  <li>O fluxo manual de sincronizacao e importante para torneios que nao desejam atualizacao em tempo real.</li>
</ul>

<h2>Resultado esperado do video</h2>
<p>Ao final, o espectador deve entender que o fiscal consegue registrar capturas com seguranca, inclusive em cenarios com internet instavel, sem perder rastreabilidade nem controle de envio.</p>
'@
    },
    [PSCustomObject]@{
        FileName = '4_Roteiro_Pescador.pdf'
        Title = 'Roteiro Completo do Pescador'
        Subtitle = 'Fluxo publico de registro, autenticacao, recuperacao de senha e consulta das proprias cobrancas.'
        Body = @'
<h2>Objetivo do video</h2>
<p>Apresentar a jornada do pescador dentro da plataforma, desde a tela publica do torneio ate o acesso as proprias cobrancas, usando os novos recursos de registro e recuperacao por SMS.</p>

<h2>Cenario recomendado</h2>
<div class="box">
  <ul>
    <li>Usar um torneio com <strong>registro publico de pescador habilitado</strong>.</li>
    <li>Manter configuracao financeira ativa para que o video mostre cobrancas reais sendo geradas.</li>
    <li>Preparar um pescador sem credenciais e outro com credenciais ja definidas para cobrir os dois cenarios.</li>
  </ul>
</div>

<h2>Sequencia sugerida de gravacao</h2>
<ol>
  <li><strong>Tela inicial publica do torneio</strong>: abrir a pagina inicial do torneio e destacar patrocinadores, informacoes do evento e os botoes de acesso do pescador.</li>
  <li><strong>Registro publico</strong>: entrar no fluxo de registro do pescador, preencher os dados solicitados, informar celular e mostrar o recebimento e a digitacao do codigo enviado por SMS.</li>
  <li><strong>Definicao de credenciais</strong>: ainda no registro, definir usuario e senha opcionais para acesso futuro do pescador.</li>
  <li><strong>Resultado do registro</strong>: concluir o cadastro e explicar que, se houver configuracao financeira ativa, as parcelas do pescador passam a gerar cobrancas automaticamente.</li>
  <li><strong>Login do pescador</strong>: retornar a tela publica, entrar no login do pescador e autenticar com usuario e senha.</li>
  <li><strong>Minhas cobrancas</strong>: apresentar a listagem das cobrancas do pescador, incluindo tipo, vencimento, situacao, valor e comprovantes quando existirem.</li>
  <li><strong>Recuperacao de senha</strong>: sair da conta, abrir o fluxo de recuperacao, informar usuario e celular, validar novo codigo por SMS e cadastrar uma nova senha.</li>
  <li><strong>Login apos recuperacao</strong>: autenticar novamente com a senha redefinida e confirmar que o acesso foi restabelecido.</li>
  <li><strong>Cenario com cadastro interno previo</strong>: explicar que um pescador cadastrado pela retaguarda pode ganhar credenciais depois, usando o proprio fluxo publico quando permitido pelo torneio.</li>
  <li><strong>Encerramento</strong>: finalizar reforcando autonomia do pescador para acompanhar suas proprias cobrancas sem depender do administrador.</li>
</ol>

<h2>Pontos de narracao importantes</h2>
<ul>
  <li>O registro publico so aparece quando a configuracao do torneio permitir.</li>
  <li>Usuario e senha do pescador sao opcionais no cadastro interno, mas passam a ser necessarios para o acesso individual as cobrancas.</li>
  <li>O SMS funciona como segunda etapa de validacao para registro publico e recuperacao de senha.</li>
  <li>As cobrancas so aparecem quando existir configuracao financeira aplicavel ao torneio e ao pescador.</li>
</ul>

<h2>Resultado esperado do video</h2>
<p>Ao final, o espectador deve entender que o pescador pode se registrar, recuperar senha e acompanhar suas cobrancas com seguranca, de forma independente e validada por celular.</p>
'@
    },
    [PSCustomObject]@{
        FileName = '5_Roteiro_Espectador.pdf'
        Title = 'Roteiro do Espectador'
        Subtitle = 'Fluxo publico de descoberta de torneios, banners, consulta de informacoes e entrada no ambiente do evento.'
        Body = @'
<h2>Objetivo do video</h2>
<p>Apresentar a experiencia publica para quem acompanha a plataforma como espectador, sem permissao administrativa, focando na descoberta dos torneios e no acesso as informacoes essenciais.</p>

<h2>Escopo recomendado</h2>
<div class="box">
  <p>Este roteiro pode ser gravado principalmente no app, pois hoje a experiencia publica do espectador esta mais clara no aplicativo. Se desejar, voce pode complementar com a tela inicial publica da web, que lista torneios ativos.</p>
</div>

<h2>Cenario recomendado</h2>
<div class="box">
  <ul>
    <li>Manter banners ativos com diferentes destinos configurados.</li>
  <li>Usar os tres torneios de exemplo para mostrar busca e comparacao de status.</li>
  <li>Garantir que ao menos um torneio esteja Aberto, outro Liberado e outro Finalizado para demonstrar mensagens diferentes.</li>
  <li>Manter patrocinadores ativos para evidenciar a lista sempre visivel na tela inicial do torneio.</li>
  </ul>
</div>

<h2>Sequencia sugerida de gravacao</h2>
<ol>
  <li><strong>Abertura do app publico</strong>: mostrar a lista inicial de torneios disponiveis.</li>
  <li><strong>Busca por nome ou slug</strong>: usar o campo de pesquisa para localizar um torneio especifico.</li>
  <li><strong>Banners da plataforma</strong>: navegar pelos banners e demonstrar que eles podem abrir um torneio, site, WhatsApp, Instagram ou e-mail, conforme a configuracao.</li>
  <li><strong>Lista de torneios</strong>: destacar nome, logo, slug e status visual de cada torneio.</li>
  <li><strong>Abrir o torneio 1</strong>: entrar em <strong>Torneio BTS Sport Fishing 2026</strong> e mostrar a tela de detalhes com logo, medida da captura e status.</li>
  <li><strong>Patrocinadores do torneio</strong>: destacar a lista de patrocinadores sempre visivel e demonstrar a navegacao pelos destinos configurados quando houver link, Instagram, WhatsApp ou site.</li>
  <li><strong>Mensagem por status</strong>: demonstrar como a aplicacao informa quando o torneio ainda nao esta publico ou quando ja foi encerrado.</li>
  <li><strong>Abrir um torneio liberado</strong>: entrar em um torneio liberado para mostrar o mesmo detalhe em contexto ativo.</li>
  <li><strong>Botoes publicos</strong>: explicar que a mesma tela publica oferece caminhos para login administrativo e, quando configurado, para login, registro e recuperacao de senha do pescador, sem expor menus internos ao espectador.</li>
  <li><strong>Comparacao entre torneios</strong>: voltar para a lista, abrir outro torneio e mostrar como o slug e a identidade visual ajudam a separar as edicoes.</li>
  <li><strong>Encerramento</strong>: concluir que a plataforma tambem atende a consulta publica basica, organizando torneios e destaque visual de campanhas.</li>
</ol>

<h2>Pontos de narracao importantes</h2>
<ul>
  <li>O espectador nao acessa menus administrativos nem dados sensiveis.</li>
  <li>Os banners ajudam na divulgacao e no redirecionamento para canais oficiais do evento.</li>
  <li>O status do torneio comunica rapidamente se o evento esta aberto, liberado ou finalizado.</li>
</ul>

<h2>Resultado esperado do video</h2>
<p>Ao final, o espectador deve perceber que a plataforma tambem oferece uma camada publica organizada, simples e adequada para descoberta e consulta de torneios.</p>
'@
    }
)

foreach ($roteiro in $roteiros) {
    $html = New-HtmlDocument -Title $roteiro.Title -Subtitle $roteiro.Subtitle -Body $roteiro.Body
    $htmlPath = Join-Path $tempDir ($roteiro.FileName -replace '\.pdf$', '.html')
    $pdfPath = Join-Path $downloadDir $roteiro.FileName

    Set-Content -Path $htmlPath -Value $html -Encoding UTF8
    Save-PdfFromHtml -BrowserPath $browserPath -HtmlPath $htmlPath -PdfPath $pdfPath
}

Write-Host 'PDFs gerados com sucesso em:' -ForegroundColor Green
$roteiros | ForEach-Object {
    Write-Host (Join-Path $downloadDir $_.FileName)
}
