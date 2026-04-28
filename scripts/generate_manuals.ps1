$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root 'artifacts\manuals'
$pdfDir = $root

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$browserCandidates = @(
    'C:\Program Files\Google\Chrome\Application\chrome.exe',
    'C:\Program Files (x86)\Google\Chrome\Application\chrome.exe',
    'C:\Program Files\Microsoft\Edge\Application\msedge.exe',
    'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
)

$browser = $browserCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $browser) {
    throw 'Chrome ou Edge nao encontrado para gerar os PDFs.'
}

$publicPdfName = 'Informa' + [char]231 + [char]245 + 'es para Espectador (acesso p' + [char]250 + 'blico).pdf'
$publicPdfTempName = 'Informacoes para Espectador (acesso publico).pdf'

$style = @"
<style>
  * { box-sizing: border-box; }
  body {
    font-family: "Segoe UI", Arial, sans-serif;
    color: #1f2937;
    margin: 0;
    padding: 28px 34px 40px;
    line-height: 1.52;
    font-size: 12px;
    background: #ffffff;
  }
  h1, h2, h3, h4 { margin: 0; color: #0f4f49; }
  h1 { font-size: 28px; border-bottom: 3px solid #106962; padding-bottom: 10px; margin-bottom: 18px; }
  h2 { font-size: 18px; margin: 18px 0 10px; }
  h3 { font-size: 14px; margin: 12px 0 8px; }
  h4 { font-size: 12.5px; margin: 0 0 6px; }
  p { margin: 0 0 10px; }
  ul { margin: 0 0 10px 18px; padding: 0; }
  li { margin: 0 0 5px; }
  .lead {
    background: #ecfdf5;
    border: 1px solid #a7f3d0;
    border-left: 5px solid #106962;
    border-radius: 10px;
    padding: 12px 14px;
    margin-bottom: 18px;
  }
  .note, .warn {
    border-radius: 10px;
    padding: 10px 12px;
    margin: 12px 0;
  }
  .note { background: #eff6ff; border: 1px solid #93c5fd; }
  .warn { background: #fff7ed; border: 1px solid #fdba74; }
  .screen {
    border: 1px solid #d1d5db;
    border-radius: 12px;
    padding: 12px 14px;
    margin: 12px 0;
    background: #f9fafb;
    page-break-inside: avoid;
  }
  .screen-title { font-size: 15px; font-weight: 700; color: #106962; margin-bottom: 8px; }
  .tag {
    display: inline-block;
    padding: 3px 8px;
    border-radius: 999px;
    background: #e0f2fe;
    color: #075985;
    font-size: 10.5px;
    font-weight: 700;
    margin-bottom: 8px;
  }
  .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
  .block {
    background: #ffffff;
    border: 1px solid #e5e7eb;
    border-radius: 10px;
    padding: 10px;
  }
  .small { font-size: 11px; color: #4b5563; }
  @media print {
    body { print-color-adjust: exact; -webkit-print-color-adjust: exact; }
  }
</style>
"@

$adminGeralBody = @"
<div class='lead'>
  Este manual cobre o perfil <strong>Administrador Geral</strong> no projeto web. O foco &eacute; explicar cada tela para quem nunca teve contato com a plataforma, mostrando objetivo, campos principais e uso pr&aacute;tico.
</div>

<h2>O que faz o Admin Geral</h2>
<p>O Administrador Geral cuida da estrutura da plataforma. Ele cria torneios, define regras iniciais, controla acessos administrativos, mant&eacute;m cat&aacute;logos globais e consulta a auditoria do sistema.</p>

<div class='screen'>
  <div class='screen-title'>Tela: Login administrativo</div>
  <div class='tag'>Entrada do ambiente estrutural</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Garantir que apenas pessoas autorizadas entrem na &aacute;rea administrativa principal.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Usu&aacute;rio</strong>: identifica quem est&aacute; tentando acessar.</li>
        <li><strong>Senha</strong>: credencial de seguran&ccedil;a do acesso.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Painel</div>
  <div class='tag'>Menu lateral: Geral &gt; Painel</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Ser o ponto inicial da gest&atilde;o da plataforma, reunindo os torneios cadastrados.</p>
    </div>
    <div class='block'>
      <h4>Uso pr&aacute;tico</h4>
      <ul>
        <li>Localizar um torneio existente.</li>
        <li>Abrir o contexto de um torneio.</li>
        <li>Iniciar um novo cadastro.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Novo Torneio</div>
  <div class='tag'>Menu lateral: Torneios &gt; Novo Torneio</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Criar a estrutura de um novo torneio. O que for definido aqui afeta web, app e a &aacute;rea p&uacute;blica.</p>
    </div>
    <div class='block'>
      <h4>Campos mais importantes</h4>
      <ul>
        <li><strong>Slug</strong>: trecho da URL do torneio.</li>
        <li><strong>Nome</strong>: t&iacute;tulo principal do evento.</li>
        <li><strong>Data</strong>: data oficial do torneio.</li>
        <li><strong>Descri&ccedil;&atilde;o</strong>: texto p&uacute;blico do evento.</li>
        <li><strong>Observa&ccedil;&otilde;es internas</strong>: informa&ccedil;&otilde;es administrativas n&atilde;o p&uacute;blicas.</li>
        <li><strong>Cor prim&aacute;ria</strong>: identidade visual do torneio.</li>
      </ul>
    </div>
  </div>
  <div class='grid'>
    <div class='block'>
      <h4>Configura&ccedil;&otilde;es relevantes</h4>
      <ul>
        <li><strong>Tipo de torneio</strong>: define a natureza do evento.</li>
        <li><strong>Modo de sorteio</strong>: determina a l&oacute;gica de distribui&ccedil;&atilde;o operacional.</li>
        <li><strong>Terminologias</strong>: nomes como embarca&ccedil;&atilde;o, pescador, fiscal, peixe e captura.</li>
      </ul>
    </div>
    <div class='block'>
      <h4>Flags importantes</h4>
      <ul>
        <li><strong>Captura offline</strong>: permite operar sem internet.</li>
        <li><strong>M&oacute;dulo financeiro</strong>: habilita cobran&ccedil;as, custos e indicadores.</li>
        <li><strong>Cadastro p&uacute;blico de membro com SMS</strong>: habilita registro e acesso do membro na &aacute;rea p&uacute;blica.</li>
        <li><strong>Exibi&ccedil;&otilde;es p&uacute;blicas</strong>: controlam lista inicial, pesquisa e participantes.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Editar Torneio</div>
  <div class='tag'>A&ccedil;&atilde;o a partir da lista de torneios</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Atualizar um torneio existente sem recriar toda a estrutura.</p>
    </div>
    <div class='block'>
      <h4>Quando usar</h4>
      <ul>
        <li>Para corrigir identidade visual.</li>
        <li>Para ajustar visibilidade p&uacute;blica.</li>
        <li>Para revisar nomenclaturas e regras iniciais.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Clonar Torneio</div>
  <div class='tag'>Reaproveitamento de estrutura</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Reaproveitar a base de um torneio anterior para criar uma nova edi&ccedil;&atilde;o mais rapidamente.</p>
    </div>
    <div class='block'>
      <h4>Campos importantes</h4>
      <ul>
        <li><strong>Nome do novo torneio</strong>: identifica a nova edi&ccedil;&atilde;o.</li>
        <li><strong>Slug</strong>: URL do torneio clonado.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Banners</div>
  <div class='tag'>Menu lateral: Torneios &gt; Banners</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Controlar os banners visuais da plataforma para divulga&ccedil;&atilde;o e destaque.</p>
    </div>
    <div class='block'>
      <h4>Atributos importantes</h4>
      <ul>
        <li><strong>Imagem</strong>: arte exibida.</li>
        <li><strong>Destino</strong>: link aberto ao clicar no banner.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Esp&eacute;cies de Peixe</div>
  <div class='tag'>Menu lateral: Torneios &gt; Esp&eacute;cies de Peixe</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Manter o cat&aacute;logo global de esp&eacute;cies usado pelos torneios do tipo pesca.</p>
    </div>
    <div class='block'>
      <h4>Atributos importantes</h4>
      <ul>
        <li><strong>Nome da esp&eacute;cie</strong>: padr&atilde;o usado em todos os torneios.</li>
        <li><strong>Imagem</strong>: apoio visual para identifica&ccedil;&atilde;o no web e no app.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Admins Gerais</div>
  <div class='tag'>Menu lateral: Acesso &gt; Admins Gerais</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Controlar quem possui o maior n&iacute;vel de acesso da plataforma.</p>
    </div>
    <div class='block'>
      <h4>Campos importantes</h4>
      <ul>
        <li><strong>Nome</strong>: identifica o respons&aacute;vel.</li>
        <li><strong>Usu&aacute;rio</strong>: login estrutural.</li>
        <li><strong>Senha</strong>: credencial que deve respeitar a pol&iacute;tica da plataforma.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Admins do Torneio</div>
  <div class='tag'>Dentro do contexto de um torneio</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Definir quem pode operar um torneio espec&iacute;fico sem receber acesso estrutural total.</p>
    </div>
    <div class='block'>
      <h4>Campos importantes</h4>
      <ul>
        <li><strong>Nome</strong>: identifica o administrador do torneio.</li>
        <li><strong>Usu&aacute;rio</strong>: login daquele torneio.</li>
        <li><strong>Troca obrigat&oacute;ria de senha</strong>: usada ap&oacute;s redefini&ccedil;&atilde;o.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Logs</div>
  <div class='tag'>Menu lateral: Auditoria &gt; Logs</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Rastrear o que aconteceu no sistema, quem executou a a&ccedil;&atilde;o e em qual contexto.</p>
    </div>
    <div class='block'>
      <h4>Quando consultar</h4>
      <ul>
        <li>Para auditar altera&ccedil;&otilde;es.</li>
        <li>Para entender quem modificou acesso, status ou cadastros.</li>
        <li>Para revisar opera&ccedil;&otilde;es sens&iacute;veis.</li>
      </ul>
    </div>
  </div>
</div>
"@

$adminTorneioBody = @"
<div class='lead'>
  Este manual cobre o perfil <strong>Administrador do Torneio</strong> no web e no app. O foco &eacute; apresentar cada tela para quem ainda n&atilde;o conhece a plataforma, explicando objetivo, campos e situa&ccedil;&otilde;es em que uma tela pode nem estar habilitada.
</div>

<h2>O que faz o Admin do Torneio</h2>
<p>O Admin do Torneio &eacute; quem conduz a opera&ccedil;&atilde;o do evento no sistema. Ele configura o torneio, monta cadastros, acompanha capturas, controla sorteio e pr&ecirc;mios, emite relat&oacute;rios e, quando o torneio usa financeiro, cuida tamb&eacute;m de cobran&ccedil;as, custos e indicadores.</p>

<div class='screen'>
  <div class='screen-title'>Tela: Painel do Admin do Torneio</div>
  <div class='tag'>Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Ser o centro da opera&ccedil;&atilde;o do torneio, reunindo status, atalhos e a&ccedil;&otilde;es principais.</p>
    </div>
    <div class='block'>
      <h4>Como ler o status</h4>
      <ul>
        <li><strong>Aberto</strong>: fase de prepara&ccedil;&atilde;o e cadastro.</li>
        <li><strong>Liberado</strong>: torneio ativo para opera&ccedil;&atilde;o e divulga&ccedil;&atilde;o.</li>
        <li><strong>Finalizado</strong>: evento encerrado, mantendo hist&oacute;rico e relat&oacute;rios.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Dados do Torneio</div>
  <div class='tag'>Se&ccedil;&atilde;o Torneio | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Atualizar as caracter&iacute;sticas principais do evento sem depender do Admin Geral.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Nome</strong>: t&iacute;tulo oficial do torneio.</li>
        <li><strong>Logo</strong>: imagem principal do evento.</li>
        <li><strong>Descri&ccedil;&atilde;o</strong>: texto p&uacute;blico logo abaixo do nome.</li>
        <li><strong>Observa&ccedil;&otilde;es internas</strong>: informa&ccedil;&otilde;es administrativas n&atilde;o p&uacute;blicas.</li>
        <li><strong>Cor prim&aacute;ria</strong>: identidade visual do torneio.</li>
      </ul>
    </div>
  </div>
  <div class='grid'>
    <div class='block'>
      <h4>Flags importantes</h4>
      <ul>
        <li><strong>Fator multiplicador</strong>: altera o c&aacute;lculo do ranking quando essa regra existir.</li>
        <li><strong>Captura offline</strong>: permite ao fiscal operar sem internet.</li>
        <li><strong>Exibir m&oacute;dulo financeiro</strong>: mostra ou esconde toda a &aacute;rea financeira.</li>
        <li><strong>Exibir participantes</strong>: mostra ou esconde a se&ccedil;&atilde;o de participantes na tela p&uacute;blica.</li>
      </ul>
    </div>
    <div class='block'>
      <h4>Condi&ccedil;&otilde;es</h4>
      <ul>
        <li>A quantidade de ganhadores s&oacute; faz sentido quando o modo de sorteio &eacute; Nenhum.</li>
        <li>Alguns campos podem aparecer ou desaparecer conforme a configura&ccedil;&atilde;o do torneio.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Embarca&ccedil;&otilde;es</div>
  <div class='tag'>Cadastros | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Registrar as embarca&ccedil;&otilde;es do torneio e centralizar informa&ccedil;&otilde;es de campo e, quando ativo, de custo.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Nome</strong>: identifica a embarca&ccedil;&atilde;o.</li>
        <li><strong>Capit&atilde;o</strong>: respons&aacute;vel principal da embarca&ccedil;&atilde;o.</li>
        <li><strong>Vagas</strong>: capacidade prevista, quando esse controle existir.</li>
        <li><strong>Fotos</strong>: apoio visual da embarca&ccedil;&atilde;o e do capit&atilde;o.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    Campos como custo, vencimento e status financeiro podem n&atilde;o estar vis&iacute;veis. Eles s&oacute; aparecem quando o m&oacute;dulo financeiro est&aacute; habilitado.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Fiscais</div>
  <div class='tag'>Cadastros | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Cadastrar os operadores de campo que usar&atilde;o o app para registrar capturas.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Nome</strong>: identifica o fiscal.</li>
        <li><strong>Usu&aacute;rio e senha</strong>: credenciais do app.</li>
        <li><strong>Foto</strong>: ajuda no reconhecimento interno.</li>
        <li><strong>Embarca&ccedil;&otilde;es vinculadas</strong>: define o universo de trabalho do fiscal.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Pescadores</div>
  <div class='tag'>Cadastros | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Manter o cadastro dos participantes do torneio para as partes esportiva, financeira e p&uacute;blica, quando aplic&aacute;vel.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Foto</strong>: identifica visualmente o participante.</li>
        <li><strong>Celular</strong>: contato opcional.</li>
        <li><strong>Tamanho da camisa</strong>: apoio log&iacute;stico e financeiro.</li>
        <li><strong>Usu&aacute;rio e senha</strong>: acesso do membro &agrave; &aacute;rea de cobran&ccedil;as.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    Usu&aacute;rio e senha do membro podem n&atilde;o estar habilitados. Eles s&oacute; fazem sentido quando o torneio permite cadastro p&uacute;blico do membro com SMS.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Grupos</div>
  <div class='tag'>Cadastros | Web e App | Modo GrupoEquipe</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Organizar grupos de pescadores previamente definidos quando o sorteio usa equipes montadas antes da distribui&ccedil;&atilde;o.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Nome do grupo</strong>: identifica a equipe pr&eacute;-montada.</li>
        <li><strong>Membros do grupo</strong>: define quem comp&otilde;e aquele grupo.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    Esta tela pode n&atilde;o estar habilitada. Ela s&oacute; aparece quando o modo de sorteio do torneio &eacute; GrupoEquipe.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Peixes / Itens</div>
  <div class='tag'>Cadastros | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Definir quais esp&eacute;cies ou itens s&atilde;o v&aacute;lidos para as capturas do torneio.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Esp&eacute;cie</strong>: item selecionado do cat&aacute;logo global.</li>
        <li><strong>Comprimento m&iacute;nimo</strong>: regra opcional de valida&ccedil;&atilde;o.</li>
        <li><strong>Fator multiplicador</strong>: regra opcional de pontua&ccedil;&atilde;o.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Patrocinadores</div>
  <div class='tag'>Cadastros | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Registrar os parceiros do evento e decidir como eles aparecer&atilde;o na tela p&uacute;blica e nos relat&oacute;rios.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Imagem</strong>: principal forma de apresenta&ccedil;&atilde;o do patrocinador.</li>
        <li><strong>Instagram, Facebook, site e WhatsApp</strong>: destinos do clique do p&uacute;blico.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    A se&ccedil;&atilde;o p&uacute;blica de patrocinadores pode n&atilde;o aparecer. Isso acontece quando n&atilde;o houver patrocinadores cadastrados ou quando a exibi&ccedil;&atilde;o p&uacute;blica n&atilde;o estiver habilitada.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Capturas</div>
  <div class='tag'>Opera&ccedil;&otilde;es | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Conferir tudo o que foi registrado pelos fiscais e permitir corre&ccedil;&otilde;es administrativas quando necess&aacute;rias.</p>
    </div>
    <div class='block'>
      <h4>A&ccedil;&otilde;es principais</h4>
      <ul>
        <li>Visualizar capturas e imagens.</li>
        <li>Invalidar captura.</li>
        <li>Editar o tamanho da captura.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Sorteio</div>
  <div class='tag'>Opera&ccedil;&otilde;es | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Executar a distribui&ccedil;&atilde;o aleat&oacute;ria prevista na regra do torneio.</p>
    </div>
    <div class='block'>
      <h4>Fluxo esperado</h4>
      <ul>
        <li>Conferir eleg&iacute;veis.</li>
        <li>Executar o sorteio.</li>
        <li>Ver a anima&ccedil;&atilde;o e o resultado.</li>
        <li>Confirmar ou limpar o sorteio.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    Esta tela pode n&atilde;o estar habilitada. Quando o modo de sorteio do torneio for Nenhum, o menu de Sorteio desaparece no web e no app.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Pr&ecirc;mios</div>
  <div class='tag'>Opera&ccedil;&otilde;es | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Registrar a premia&ccedil;&atilde;o do evento para que ela apare&ccedil;a organizada no sistema e na comunica&ccedil;&atilde;o p&uacute;blica.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Posi&ccedil;&atilde;o</strong>: coloca&ccedil;&atilde;o premiada.</li>
        <li><strong>Descri&ccedil;&atilde;o</strong>: o que ser&aacute; entregue como pr&ecirc;mio.</li>
        <li><strong>Destino</strong>: se o pr&ecirc;mio vale para embarca&ccedil;&atilde;o, pescador ou ambos.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Relat&oacute;rios</div>
  <div class='tag'>Opera&ccedil;&otilde;es | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Emitir documentos de acompanhamento, divulga&ccedil;&atilde;o e fechamento do torneio.</p>
    </div>
    <div class='block'>
      <h4>Quando usar</h4>
      <ul>
        <li>Para acompanhar o andamento do evento.</li>
        <li>Para divulgar resultados.</li>
        <li>Para formalizar sa&iacute;das para equipe, patrocinadores ou p&uacute;blico.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Reorganiza&ccedil;&atilde;o Emergencial</div>
  <div class='tag'>Opera&ccedil;&otilde;es | Web e App | Uso excepcional</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Permitir um ajuste manual excepcional da distribui&ccedil;&atilde;o de pescadores entre embarca&ccedil;&otilde;es quando houver um problema operacional grave.</p>
    </div>
    <div class='block'>
      <h4>Como interpretar</h4>
      <ul>
        <li>N&atilde;o &eacute; uma tela de uso rotineiro.</li>
        <li>Exige confirma&ccedil;&atilde;o administrativa.</li>
        <li>Gera log para auditoria.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    Esta tela pode n&atilde;o estar habilitada. Quando o modo de sorteio do torneio for Nenhum, ela n&atilde;o aparece no web nem no app.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Vis&atilde;o geral financeira</div>
  <div class='tag'>Financeiro | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Dar uma leitura r&aacute;pida da sa&uacute;de financeira do torneio.</p>
    </div>
    <div class='block'>
      <h4>Indicadores e significado</h4>
      <ul>
        <li><strong>Arrecada&ccedil;&atilde;o prevista</strong>: expectativa total de receitas.</li>
        <li><strong>Saldo projetado</strong>: previs&atilde;o do resultado financeiro.</li>
        <li><strong>Inadimpl&ecirc;ncia</strong>: cobran&ccedil;as vencidas e n&atilde;o pagas.</li>
        <li><strong>Valor em aberto</strong>: tudo o que ainda n&atilde;o foi recebido.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    Toda a se&ccedil;&atilde;o Financeiro pode n&atilde;o estar habilitada. Se a flag Exibir m&oacute;dulo financeiro estiver desligada no torneio, as telas financeiras n&atilde;o aparecem no web e no app.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Configura&ccedil;&atilde;o financeira</div>
  <div class='tag'>Financeiro | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Definir a regra b&aacute;sica de cobran&ccedil;a dos participantes do torneio.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Valor por pescador</strong>: total esperado por participante.</li>
        <li><strong>Quantidade de parcelas</strong>: em quantas partes o valor ser&aacute; dividido.</li>
        <li><strong>Taxa de inscri&ccedil;&atilde;o</strong>: cobran&ccedil;a opcional adicional.</li>
        <li><strong>Primeiro vencimento</strong>: data base para as parcelas.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Cobran&ccedil;as</div>
  <div class='tag'>Financeiro | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Controlar tudo o que o torneio tem a receber dos participantes.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Tipo</strong>: origem da cobran&ccedil;a, como inscri&ccedil;&atilde;o, parcela ou produto extra.</li>
        <li><strong>Vencimento</strong>: data esperada de pagamento.</li>
        <li><strong>Comprovante</strong>: arquivo que registra a comprova&ccedil;&atilde;o do pagamento.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Custos</div>
  <div class='tag'>Financeiro | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Registrar os gastos do torneio para acompanhar se a previs&atilde;o de caixa &eacute; vi&aacute;vel.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Categoria</strong>: tipo do gasto.</li>
        <li><strong>Quantidade</strong> e <strong>valor unit&aacute;rio</strong>: comp&otilde;em o valor total.</li>
        <li><strong>Vencimento</strong>: data em que o gasto deve ser pago.</li>
        <li><strong>Respons&aacute;vel</strong>: pessoa associada ao custo.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Produtos extras</div>
  <div class='tag'>Financeiro | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Criar itens opcionais vendidos aos participantes para gerar receita adicional.</p>
    </div>
    <div class='block'>
      <h4>Como funciona</h4>
      <ul>
        <li>Primeiro cadastra-se o produto.</li>
        <li>Depois registra-se a venda para um pescador.</li>
        <li>Essa venda gera uma cobran&ccedil;a.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Doa&ccedil;&otilde;es</div>
  <div class='tag'>Financeiro | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Registrar contribui&ccedil;&otilde;es em dinheiro ou produtos recebidas pelo torneio.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Patrocinador</strong>: parceiro j&aacute; cadastrado, quando existir.</li>
        <li><strong>Doador</strong>: identifica a origem quando n&atilde;o h&aacute; patrocinador selecionado.</li>
        <li><strong>Tipo de doa&ccedil;&atilde;o</strong>: define se &eacute; receita em dinheiro ou recebimento em produto.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Checklist</div>
  <div class='tag'>Financeiro | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Acompanhar tarefas operacionais que precisam ser conclu&iacute;das durante a prepara&ccedil;&atilde;o e a execu&ccedil;&atilde;o do torneio.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Item</strong>: tarefa a ser feita.</li>
        <li><strong>Data</strong>: prazo ou refer&ecirc;ncia temporal.</li>
        <li><strong>Respons&aacute;vel</strong>: admin encarregado de acompanhar a execu&ccedil;&atilde;o.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Relat&oacute;rios financeiros</div>
  <div class='tag'>Financeiro | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Dar uma vis&atilde;o anal&iacute;tica da situa&ccedil;&atilde;o financeira, complementando o dashboard.</p>
    </div>
    <div class='block'>
      <h4>Quando usar</h4>
      <ul>
        <li>Para avaliar o fluxo de recebimentos e pagamentos.</li>
        <li>Para revisar inadimpl&ecirc;ncia.</li>
        <li>Para comparar custos e receitas.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: P&aacute;gina p&uacute;blica do torneio</div>
  <div class='tag'>Confer&ecirc;ncia obrigat&oacute;ria | Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Ser a vitrine do torneio para p&uacute;blico, participantes e patrocinadores.</p>
    </div>
    <div class='block'>
      <h4>O que o admin deve validar</h4>
      <ul>
        <li>Se a descri&ccedil;&atilde;o est&aacute; correta.</li>
        <li>Se ranking, premia&ccedil;&otilde;es, participantes e patrocinadores est&atilde;o coerentes.</li>
        <li>Se o acesso p&uacute;blico do membro aparece apenas quando deve aparecer.</li>
      </ul>
    </div>
  </div>
</div>
"@

$fiscalBody = @"
<div class='lead'>
  Este manual cobre o perfil <strong>Fiscal</strong> no aplicativo. O foco &eacute; orientar quem nunca usou a plataforma, explicando objetivo das telas, significado dos campos e comportamentos importantes para a opera&ccedil;&atilde;o de campo.
</div>

<h2>O que faz o Fiscal</h2>
<p>O fiscal registra capturas, revisa dados e sincroniza informa&ccedil;&otilde;es com o sistema. Esse perfil n&atilde;o altera regras do torneio, n&atilde;o executa sorteio e n&atilde;o opera o financeiro.</p>

<div class='screen'>
  <div class='screen-title'>Tela: Lista inicial de torneios</div>
  <div class='tag'>App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Apresentar os torneios dispon&iacute;veis para entrada no app.</p>
    </div>
    <div class='block'>
      <h4>Como usar</h4>
      <ul>
        <li>Localizar o torneio correto.</li>
        <li>Conferir nome e identidade visual.</li>
        <li>Abrir a p&aacute;gina p&uacute;blica do torneio.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: P&aacute;gina p&uacute;blica do torneio</div>
  <div class='tag'>App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Exibir a apresenta&ccedil;&atilde;o do torneio e oferecer o ponto de entrada para o perfil Fiscal.</p>
    </div>
    <div class='block'>
      <h4>O que aparece</h4>
      <ul>
        <li>Logo, nome, descri&ccedil;&atilde;o e status.</li>
        <li>Ranking, premia&ccedil;&otilde;es, participantes e patrocinadores, quando habilitados.</li>
        <li>Bot&atilde;o de acesso para Fiscal ou Administra&ccedil;&atilde;o.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Login do Fiscal</div>
  <div class='tag'>App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Garantir que o operador de campo entre com o perfil correto.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Usu&aacute;rio</strong>: identifica o fiscal.</li>
        <li><strong>Senha</strong>: credencial de acesso ao app.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    Se o torneio permitir captura offline, o app precisa carregar listas de embarca&ccedil;&otilde;es, pescadores e itens antes de o fiscal ir para uma &aacute;rea sem internet.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Home do Fiscal</div>
  <div class='tag'>App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Concentrar as informa&ccedil;&otilde;es operacionais principais e servir como menu do fiscal.</p>
    </div>
    <div class='block'>
      <h4>O que a tela exibe</h4>
      <ul>
        <li>Nome do torneio.</li>
        <li>Nome do fiscal.</li>
        <li>Embarca&ccedil;&otilde;es vinculadas.</li>
        <li>Capit&atilde;o da embarca&ccedil;&atilde;o.</li>
        <li>Pend&ecirc;ncias de sincroniza&ccedil;&atilde;o, quando existirem.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    A quantidade de pescadores por embarca&ccedil;&atilde;o pode n&atilde;o aparecer. Quando o modo de sorteio do torneio &eacute; Nenhum, essa informa&ccedil;&atilde;o n&atilde;o faz sentido operacional.
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Registrar Captura</div>
  <div class='tag'>App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Registrar uma captura com dados suficientes para valida&ccedil;&atilde;o administrativa e composi&ccedil;&atilde;o do ranking.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Embarca&ccedil;&atilde;o</strong>: contexto da captura.</li>
        <li><strong>Pescador</strong>: participante autor da captura.</li>
        <li><strong>Peixe ou item</strong>: esp&eacute;cie ou item v&aacute;lido.</li>
        <li><strong>Tamanho</strong>: medida usada na regra do torneio.</li>
        <li><strong>Foto</strong>: evid&ecirc;ncia visual da captura.</li>
      </ul>
    </div>
  </div>
  <div class='grid'>
    <div class='block'>
      <h4>Recursos da tela</h4>
      <ul>
        <li>Busca para encontrar embarca&ccedil;&atilde;o, pescador e peixe.</li>
        <li>Visualiza&ccedil;&atilde;o ampliada de imagens.</li>
        <li>Op&ccedil;&atilde;o de enviar agora ou sincronizar depois.</li>
      </ul>
    </div>
    <div class='block'>
      <h4>Condi&ccedil;&atilde;o importante</h4>
      <p>Se a captura for marcada para sincronizar depois, o envio deve ser feito manualmente na tela de sincroniza&ccedil;&atilde;o.</p>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Capturas Registradas</div>
  <div class='tag'>App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Permitir que o fiscal revise o que j&aacute; foi salvo antes de concluir sua rotina.</p>
    </div>
    <div class='block'>
      <h4>Quando consultar</h4>
      <ul>
        <li>Para confirmar se uma captura foi registrada.</li>
        <li>Para revisar dados e imagens.</li>
        <li>Para conferir o hist&oacute;rico antes da sincroniza&ccedil;&atilde;o.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Sincronizar</div>
  <div class='tag'>App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Centralizar o envio das capturas pendentes quando o registro n&atilde;o foi transmitido em tempo real.</p>
    </div>
    <div class='block'>
      <h4>Como interpretar</h4>
      <ul>
        <li>Se h&aacute; itens na fila, eles ainda n&atilde;o chegaram ao servidor.</li>
        <li>Ao tocar em <strong>sincronizar agora</strong>, o app tenta enviar as pend&ecirc;ncias.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Cen&aacute;rio: Opera&ccedil;&atilde;o offline</div>
  <div class='tag'>App | Condicional</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo do recurso</h4>
      <p>Permitir continuidade da opera&ccedil;&atilde;o em &aacute;reas com sinal ruim ou inexistente.</p>
    </div>
    <div class='block'>
      <h4>Condi&ccedil;&atilde;o de habilita&ccedil;&atilde;o</h4>
      <p>Esse fluxo s&oacute; faz sentido quando o torneio est&aacute; configurado para permitir captura offline.</p>
    </div>
  </div>
</div>
"@

$espectadorBody = @"
<div class='lead'>
  Este documento apresenta o acesso <strong>publico</strong> da plataforma no web e no app. O objetivo &eacute; explicar ao primeiro uso o que o espectador enxerga e o que pode ser disponibilizado ao membro participante.
</div>

<h2>O que &eacute; o acesso p&uacute;blico</h2>
<p>O acesso p&uacute;blico funciona como a vitrine do torneio. &Eacute; onde o p&uacute;blico acompanha a apresenta&ccedil;&atilde;o do evento, o ranking, as premia&ccedil;&otilde;es, os patrocinadores e, quando permitido, os participantes. Em alguns torneios, essa &aacute;rea tamb&eacute;m oferece autoatendimento ao membro.</p>

<div class='screen'>
  <div class='screen-title'>Tela: Lista p&uacute;blica de torneios</div>
  <div class='tag'>Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Apresentar os torneios que a organiza&ccedil;&atilde;o decidiu exibir publicamente.</p>
    </div>
    <div class='block'>
      <h4>Condi&ccedil;&atilde;o de exibi&ccedil;&atilde;o</h4>
      <p>Somente aparecem aqui os torneios marcados para a lista inicial p&uacute;blica.</p>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: Pesquisa p&uacute;blica</div>
  <div class='tag'>Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Permitir localizar um torneio espec&iacute;fico sem depender de ele estar em destaque na lista.</p>
    </div>
    <div class='block'>
      <h4>Condi&ccedil;&atilde;o de exibi&ccedil;&atilde;o</h4>
      <p>Um torneio s&oacute; aparece na busca se estiver marcado para exibi&ccedil;&atilde;o na pesquisa p&uacute;blica.</p>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Tela: P&aacute;gina p&uacute;blica do torneio</div>
  <div class='tag'>Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da tela</h4>
      <p>Concentrar as informa&ccedil;&otilde;es oficiais do torneio para espectadores e interessados.</p>
    </div>
    <div class='block'>
      <h4>Campos e significado</h4>
      <ul>
        <li><strong>Logo</strong>: identidade visual do evento.</li>
        <li><strong>Nome</strong>: t&iacute;tulo principal.</li>
        <li><strong>Descri&ccedil;&atilde;o</strong>: apresenta o torneio ao p&uacute;blico.</li>
        <li><strong>Status</strong>: informa a fase do evento.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Se&ccedil;&atilde;o: Ranking</div>
  <div class='tag'>Web e App</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da se&ccedil;&atilde;o</h4>
      <p>Mostrar a classifica&ccedil;&atilde;o atual do torneio.</p>
    </div>
    <div class='block'>
      <h4>O que o p&uacute;blico v&ecirc;</h4>
      <ul>
        <li>Coloca&ccedil;&atilde;o por embarca&ccedil;&atilde;o e/ou pescador.</li>
        <li>Pontua&ccedil;&atilde;o atual.</li>
        <li>Imagens e detalhes visuais quando o torneio exibe esse conte&uacute;do.</li>
      </ul>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Se&ccedil;&atilde;o: Premia&ccedil;&otilde;es</div>
  <div class='tag'>Web e App | Ap&oacute;s o ranking</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da se&ccedil;&atilde;o</h4>
      <p>Mostrar ao p&uacute;blico o que est&aacute; em disputa no torneio.</p>
    </div>
    <div class='block'>
      <h4>Condi&ccedil;&atilde;o de exibi&ccedil;&atilde;o</h4>
      <p>Essa se&ccedil;&atilde;o s&oacute; aparece quando o torneio est&aacute; liberado.</p>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Se&ccedil;&atilde;o: Participantes</div>
  <div class='tag'>Web e App | Opcional</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da se&ccedil;&atilde;o</h4>
      <p>Apresentar quem est&aacute; participando do torneio.</p>
    </div>
    <div class='block'>
      <h4>Condi&ccedil;&atilde;o de exibi&ccedil;&atilde;o</h4>
      <p>Essa se&ccedil;&atilde;o s&oacute; aparece quando o torneio est&aacute; configurado para exibir participantes na tela p&uacute;blica.</p>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Se&ccedil;&atilde;o: Patrocinadores</div>
  <div class='tag'>Web e App | Opcional</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da se&ccedil;&atilde;o</h4>
      <p>Dar visibilidade aos apoiadores do torneio e facilitar o acesso do p&uacute;blico aos seus canais.</p>
    </div>
    <div class='block'>
      <h4>Condi&ccedil;&atilde;o de exibi&ccedil;&atilde;o</h4>
      <p>Essa se&ccedil;&atilde;o s&oacute; aparece quando houver patrocinadores cadastrados com exibi&ccedil;&atilde;o p&uacute;blica habilitada.</p>
    </div>
  </div>
</div>

<div class='screen'>
  <div class='screen-title'>Acesso p&uacute;blico do membro participante</div>
  <div class='tag'>Web e App | Condicional</div>
  <div class='grid'>
    <div class='block'>
      <h4>Objetivo da &aacute;rea</h4>
      <p>Permitir que o pr&oacute;prio membro se identifique e acesse recursos pessoais quando o torneio oferece esse servi&ccedil;o.</p>
    </div>
    <div class='block'>
      <h4>Funcionalidades poss&iacute;veis</h4>
      <ul>
        <li>Entrar como membro.</li>
        <li>Registrar membro.</li>
        <li>Recuperar senha do membro.</li>
        <li>Consultar cobran&ccedil;as pr&oacute;prias.</li>
      </ul>
    </div>
  </div>
  <div class='note'>
    Essa &aacute;rea pode n&atilde;o estar habilitada. Ela s&oacute; aparece quando o torneio permite cadastro p&uacute;blico do membro com SMS.
  </div>
</div>
"@

$manuals = @(
    @{ FileName = 'manual_admin_geral.html'; PdfName = 'Manual de Admin Geral.pdf'; Title = 'Manual de Admin Geral'; Body = $adminGeralBody },
    @{ FileName = 'manual_admin_torneio.html'; PdfName = 'Manual de Admin Torneio.pdf'; Title = 'Manual de Admin Torneio'; Body = $adminTorneioBody },
    @{ FileName = 'manual_fiscal_app.html'; PdfName = 'Manual de Fiscal (App).pdf'; Title = 'Manual de Fiscal (App)'; Body = $fiscalBody },
    @{ FileName = 'informacoes_espectador.html'; PdfName = $publicPdfTempName; Title = 'Informacoes para Espectador (acesso publico)'; Body = $espectadorBody }
)

foreach ($manual in $manuals) {
    $htmlPath = Join-Path $outDir $manual.FileName
    $pdfPath = Join-Path $pdfDir $manual.PdfName

    if (Test-Path $pdfPath) {
        Remove-Item -LiteralPath $pdfPath -Force
    }

    $html = @"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
  <meta charset='utf-8'>
  <title>$($manual.Title)</title>
  $style
</head>
<body>
  <h1>$($manual.Title)</h1>
  $($manual.Body)
</body>
</html>
"@

    [System.IO.File]::WriteAllText($htmlPath, $html, [System.Text.UTF8Encoding]::new($false))

    & $browser `
        --headless=new `
        --disable-gpu `
        --print-to-pdf="$pdfPath" `
        --no-pdf-header-footer `
        --run-all-compositor-stages-before-draw `
        "$htmlPath" | Out-Null
}

$publicPdfTempPath = Join-Path $pdfDir $publicPdfTempName
$publicPdfFinalPath = Join-Path $pdfDir $publicPdfName
if (Test-Path $publicPdfFinalPath) {
    Remove-Item -LiteralPath $publicPdfFinalPath -Force
}
if (Test-Path $publicPdfTempPath) {
    Move-Item -LiteralPath $publicPdfTempPath -Destination $publicPdfFinalPath -Force
}

Write-Host "PDFs gerados em $pdfDir"
