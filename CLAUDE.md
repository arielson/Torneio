# CLAUDE.md — Projeto Torneio

## Visão geral
Plataforma multi-tenant de gerenciamento de torneios. Cada torneio é um tenant com seu próprio slug, configurações, cadastros e dados — isolados no mesmo banco. Nome, terminologia e comportamentos são configuráveis por tenant via banco de dados.
Exemplo de URL: torneio.ari.net.br/amigosdapesca

## Estrutura de pastas
```
D:\Source\Torneio
├── Retaguarda
│   ├── Torneio.slnx
│   ├── Torneio.Domain
│   ├── Torneio.Application
│   ├── Torneio.Infrastructure
│   ├── Torneio.Web
│   ├── Torneio.API
│   ├── Torneio.Asaas            → biblioteca de integração com a API Asaas (sem deps do projeto)
│   └── Torneio.Asaas.Tests      → xUnit + Moq, testa calculadoras + processador de webhook
├── App\                 → Flutter/Dart
└── CLAUDE.md
```

## Stack
- Backend: .NET C# em camadas (Domain → Application → Infrastructure → Presentation)
- Web: ASP.NET MVC (Razor Views, autenticação por cookie)
- API: ASP.NET WebAPI com OAuth2/JWT
- App: Flutter/Dart (Android/iOS)
- ORM: Entity Framework Core
- Banco: PostgreSQL (Npgsql)
- Senhas: BCrypt
- Relatórios: PDF gerado no backend

## Roteamento por tenant
**Web:**
```
/                              → home da plataforma
/admin                         → painel AdminGeral
/{slug}                        → home do torneio
/{slug}/admin                  → painel AdminTorneio
/{slug}/capturas
/{slug}/sorteio
/{slug}/relatorios
```

**API:**
```
/api/{slug}/config             → público, sem auth
/api/{slug}/capturas
/api/{slug}/sorteio
/api/{slug}/relatorios
/api/{slug}/sync
/api/webhook/asaas             → webhook Asaas (AllowAnonymous, valida header asaas-access-token)
```

**Web — rotas públicas (sem login):**
```
/{slug}/cobranca/{celular}           → tela de acesso (CPF como senha)
/{slug}/cobranca/{celular}/cobrancas → minhas cobranças (session-based)
/{slug}/cobranca/{celular}/qrcode/{parcelaId} → QR Code PIX via AJAX
```

**Web — rotas Asaas (AdminGeral):**
```
/admin/asaas                          → lista de torneios e status da integração
/admin/asaas/{torneioId}/configurar   → configurar chave de API e formas de pagamento
/admin/asaas/{torneioId}/registrar-webhook → registrar/atualizar webhook no Asaas
```

**Web — rotas Asaas (AdminTorneio):**
```
/{slug}/financeiro/asaas/{parcelaId}/gerar   → gerar cobrança PIX ou cartão
/{slug}/financeiro/asaas/{parcelaId}/qrcode  → QR Code PIX (AJAX, AdminTorneio)
/{slug}/financeiro/asaas/{parcelaId}/cancelar → cancelar cobrança
```

## Perfis de usuário
- **AdminGeral**: acessa tudo na plataforma, cria/edita/desativa torneios, gerencia AdminsTorneio, pode agir como AdminTorneio em qualquer torneio
- **AdminTorneio**: acessa somente seu(s) torneio(s), CRUD completo, sorteio, relatórios, liberar/finalizar ano. Um usuário pode ser admin em N torneios.
- **Fiscal**: login próprio por torneio, acessa somente sua equipe, registra capturas, sync offline, relatório da sua equipe
- **Telespectador**: sem login, lê dados públicos do torneio pelo slug

## Entidades

**Torneio (tenant):**
- Id (Guid), Slug, NomeTorneio, LogoUrl?, Ativo
- Terminologia: LabelEquipe, LabelMembro, LabelSupervisor, LabelItem, LabelCaptura
- Regras: UsarFatorMultiplicador (bool), MedidaCaptura (string), PermitirCapturaOffline (bool)
- Sorteio: ModoSorteio (enum), PermitirEscolhaManual (bool)

**AdminGeral:** Id, Nome, Usuario, SenhaHash

**AdminTorneio (N:N):** Id, UsuarioId, TorneioId, Nome, Usuario, SenhaHash

**Fiscal:** Id, TorneioId, AnoTorneioId, Nome, FotoUrl?, Usuario, SenhaHash

**AnoTorneio:** Id, TorneioId, Ano (int), Status (enum), replicável do ano anterior

**Equipe:** Id, TorneioId, AnoTorneioId, Nome, FotoUrl?, Capitao, FotoCapitaoUrl?, FiscalId, QtdVagas

**Membro:** Id, TorneioId, AnoTorneioId, Nome, FotoUrl?, Cpf?, Celular?, TamanhoCamisa?, Usuario?, SenhaHash?
- `Cpf` não é obrigatório para gerar cobrança; é obrigatório para o membro acessar suas cobranças no link público (usado como "senha")
- `Celular` é usado como identificador na URL pública: `/{slug}/cobranca/{celular}`

**Item:** Id, TorneioId, Nome, FotoUrl?, Comprimento (decimal), FatorMultiplicador (decimal, default 1.0)

**Captura:** Id, TorneioId, AnoTorneioId, ItemId, MembroId, EquipeId, TamanhoMedida (decimal), FotoUrl, DataHora, PendenteSync (bool)

**SorteioEquipe:** Id, TorneioId, AnoTorneioId, EquipeId, MembroId, Posicao

**ConfiguracaoAsaasTorneio:** Id, TorneioId (unique), ChaveApiAsaas?, StatusChave (enum), AsaasAccountId?, AceitarPix, AceitarCartaoCredito, DataAtivacao?
- Uma por torneio; gerenciada pelo AdminGeral em `/admin/asaas`
- `StatusChave`: NaoConfigurada | Ativa | Inativa

**CobrancaAsaas:** Id, TorneioId, MembroId, ParcelaTorneioId (unique), AsaasPaymentId (unique), AsaasCustomerId?, AsaasInvoiceUrl?, Status (enum), FormaPagamento? (enum), ValorOriginal, TaxaAsaas?, Vencimento, DataPrevisaoCredito?, DataCreditoEfetivo?, CriadoEm, AtualizadoEm
- `Status`: Pendente | Confirmado | Recebido | Vencido | Estornado | Excluido | RecusadoCartao
- `FormaPagamento`: Pix | CartaoCredito
- Uma CobrancaAsaas por ParcelaTorneio; gera nova cobrança se a anterior foi cancelada/excluída
- `AsaasCustomerId` é cacheado localmente para evitar chamadas repetidas à API

**WebhookEventoAsaas:** Id, EventoId (unique), TipoEvento, AsaasPaymentId?, PayloadJson, Processado, ErroProcessamento?, RecebidoEm, ProcessadoEm?
- Tabela **global** — sem TorneioId, sem query filter no EF Core
- Garante idempotência: se `EventoId` já existe, ignora o evento

## Regras de negócio
- Pontuação: soma(TamanhoMedida × FatorMultiplicador) por captura
- FatorMultiplicador exibido nos relatórios somente quando > 1.0
- Replicação de ano: clona Fiscais, Equipes, Membros e Itens com novo AnoTorneioId — nunca replica Capturas nem Sorteios
- Capturas podem ser offline no app → sync automático ao reconectar ou manual
- Sorteio somente online; modos: Sorteio (aleatório animado) | Escolha (manual) | Hibrido (sorteia sugestão, admin ajusta)
- Fiscal acessa apenas sua Equipe; AdminTorneio acessa tudo do seu tenant; AdminGeral acessa tudo da plataforma
- Query filter global no EF Core por TorneioId em todas as entidades filhas
- TorneioId sempre obrigatório nos repositórios — nunca query cross-tenant

## Isolamento multi-tenant (EF Core)
Todas as entidades filhas de Torneio têm HasQueryFilter por TorneioId:
```csharp
modelBuilder.Entity<Equipe>()
    .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId);
```
`ITenantContext` é injetado no DbContext via DI.
O slug da rota é resolvido por `ITenantResolver` no início de cada request.
O TorneioId é carregado no claim do JWT para a API.

## JWT Claims
- `sub`: usuarioId
- `torneio_id`: TorneioId (Guid)
- `perfil`: "AdminGeral" | "AdminTorneio" | "Fiscal"
- `slug`: "amigosdapesca"

## appsettings.json (apenas infraestrutura)
```json
{
  "ConnectionStrings": {
    "Default": "Host=...;Database=torneio;Username=...;Password=..."
  },
  "Jwt": {
    "SecretKey": "...",
    "ExpiracaoHoras": 8
  },
  "Storage": {
    "BasePath": "D:\\Storage\\Torneio"
  },
  "Plataforma": {
    "NomePlataforma": "Torneio",
    "UrlBase": "https://torneio.ari.net.br"
  },
  "Asaas": {
    "Ambiente": "Sandbox",
    "BaseUrlSandbox": "https://sandbox.asaas.com",
    "BaseUrlProducao": "https://api.asaas.com",
    "WebhookAuthToken": "token-secreto-webhook",
    "WebhookEmail": "admin@torneio.ari.net.br",
    "PrazoCreditoCartaoDias": 32,
    "Taxas": {
      "Pix": 1.99,
      "CartaoFixo": 0.49,
      "CartaoPercentual": 2.99,
      "PromocaoAtiva": false,
      "CartaoPercentualPromocional": 1.99
    },
    "Webhook": {
      "EventosAssinados": [
        "PAYMENT_CONFIRMED",
        "PAYMENT_RECEIVED",
        "PAYMENT_OVERDUE",
        "PAYMENT_REFUNDED",
        "PAYMENT_DELETED",
        "PAYMENT_CREDIT_CARD_CAPTURE_REFUSED"
      ]
    }
  }
}
```
- `Plataforma:UrlBase` é usada para construir a URL do webhook: `{UrlBase}/api/webhook/asaas`
- `Asaas:WebhookAuthToken` é validado no header `asaas-access-token` de cada POST no webhook
- A chave de API de cada torneio fica em `ConfiguracaoAsaasTorneio.ChaveApiAsaas` (banco), **não** no appsettings

## Relatórios PDF (gerados no backend)
Cabeçalho: NomeTorneio + Ano
1. Equipe sintético: total pontos + lista (item, membro, medida, fator se >1)
2. Equipe analítico: igual + fotos com data/hora
3. Membro sintético: total pontos + lista (item, medida, fator se >1)
4. Membro analítico: igual + fotos com data/hora

Fiscal: somente sua Equipe. AdminTorneio/AdminGeral: qualquer Equipe ou Membro.

## Endpoint público da API
`GET /api/{slug}/config` → retorna configuração do torneio (sem dados sensíveis)
O app consome ao inicializar para adaptar terminologia e modo de sorteio.

## Fases de desenvolvimento
- **Fase 1:** Domain — entidades, enums, interfaces, value objects, ITenantContext
- **Fase 2:** Application — DTOs, serviços, casos de uso, FluentValidation, TenantResolver
- **Fase 3:** Infrastructure — DbContext (Npgsql/EF Core), query filters, repositórios, migrations, FileStorage, seed AdminGeral
- **Fase 4:** API — JWT, middleware de tenant, controllers, /config, sync offline
- **Fase 5:** Web — roteamento por slug, TorneioBaseController, CRUDs, sorteio, painel AdminGeral
- **Fase 6:** App Flutter — estrutura, telas por perfil, offline/sync, relatórios
- **Fase 7:** Relatórios PDF — geração no backend, download via API e Web

**Integração Asaas (Etapas 3–11):**
- **Etapa 3:** Domain + Infrastructure — entidades, enums, migrations, configuração EF Core
- **Etapa 4:** Torneio.Asaas — calculadoras de taxa e previsão de crédito, testes unitários
- **Etapa 5:** Configuração AdminGeral — CRUD de chave de API por torneio, views de gestão
- **Etapa 6:** Geração de cobrança — CobrancaAsaasServico, lookup de customer, integração PIX/cartão, tela EditarParcela atualizada com QR Code AJAX
- **Etapa 7:** Webhook handler — WebhookAsaasProcessador, AsaasWebhookController, idempotência, mapeamento de eventos para status de cobrança e parcela
- **Etapa 8:** Página pública do membro — `/{slug}/cobranca/{celular}`, auth por CPF via session, QR Code on-demand
- **Etapa 9:** Registro automático de webhook no Asaas — ao salvar configuração + botão manual
- **Etapa 10:** Testes — 47 testes (xUnit + Moq): calculadoras, entidades de domínio, processador de webhook
- **Etapa 11:** Documentação (este arquivo)

## Integração Asaas

### Biblioteca Torneio.Asaas
Projeto separado, zero dependência de Domain/Application/Infrastructure. Contém:
- `AsaasClient` — ponto de entrada; expõe `Payments`, `Customers`, `Webhooks`, `MyAccount`
- `IAsaasClientFactory` / `AsaasClientFactory` — cria um `AsaasClient` por chamada com a chave do torneio
- `CalculadoraTaxaAsaas` — PIX: taxa flat; Cartão: fixo + percentual (com suporte a promoção)
- `CalculadoraPrevisaoCredito` — PIX: próximo dia útil; Cartão: T + prazoDias corridos
- Os dois calculadores são registrados como **Singleton** em DI, instanciados com `AsaasOptions`

### Fluxo de cobrança (AdminTorneio)
1. Admin abre `/{slug}/financeiro/{parcelaId}` e clica "Gerar cobrança PIX" ou "Cartão"
2. `CobrancaAsaasServico.GerarCobranca` valida config, resolve/cria customer no Asaas, cria payment
3. `CobrancaAsaas` é salva no banco; para PIX, o QR Code **não** é gerado aqui
4. QR Code é gerado on-demand quando o admin ou o membro clica no botão — via `GET .../qrcode/{parcelaId}`

### Fluxo de cobrança pública (Membro)
1. Link enviado ao membro: `/{slug}/cobranca/{celular}`
2. Membro digita CPF → validado contra `Membro.Cpf` (normalizado, apenas dígitos)
3. Se válido, `membroId` salvo em session key `cobranca_{slug}_{celularNormalizado}`
4. Membro vê suas cobranças e pode gerar QR Code PIX on-demand

### Fluxo de webhook
```
POST /api/webhook/asaas
  → valida header asaas-access-token
  → lê body como string
  → WebhookAsaasProcessador.ProcessarAsync(payloadJson)
      → TenantContext.DefinirAdminGeral()          ← bypassa query filters EF Core
      → verifica idempotência por EventoId
      → salva WebhookEventoAsaas (Processado=false)
      → switch(tipoEvento):
          PAYMENT_CONFIRMED → cobrança.Status=Confirmado + parcela.MarcarComoPago()
          PAYMENT_RECEIVED  → cobrança.Status=Recebido + DataCreditoEfetivo
          PAYMENT_OVERDUE   → cobrança.Status=Vencido
          PAYMENT_REFUNDED  → cobrança.Status=Estornado + parcela.DesmarcarPagamento()
          PAYMENT_DELETED   → cobrança.Status=Excluido
          PAYMENT_CREDIT_CARD_CAPTURE_REFUSED → cobrança.Status=RecusadoCartao
      → MarcarProcessado() ou MarcarErro(ex.Message)
      → salva WebhookEventoAsaas (estado final)
```

### Lookup de customer Asaas
Para evitar duplicatas no Asaas, a ordem de resolução do `customerId` é:
1. Checar tabela `cobrancas_asaas` local por `AsaasCustomerId` do membro
2. Buscar no Asaas por `externalReference = membroId`
3. Criar novo customer (com `NotificationDisabled = true`)

### Registro de webhook
- Disparado automaticamente ao salvar configuração Asaas (falha silenciosa com aviso)
- Também disponível via botão "Registrar / Atualizar Webhook" na tela de configuração
- Estratégia: lista webhooks do torneio no Asaas → atualiza se encontrar pela URL → cria se não encontrar
- URL registrada: `{Plataforma:UrlBase}/api/webhook/asaas`

### Observações importantes
- A chave de API de cada torneio fica em banco (`ConfiguracaoAsaasTorneio.ChaveApiAsaas`), nunca no appsettings
- `IAsaasClientFactory` é Singleton; cria `AsaasClient` por chamada — **não** reutiliza instâncias entre torneios
- `WebhookEventoAsaas` não tem `TorneioId` — é tabela global; `CobrancaAsaasRepositorio.ObterPorAsaasPaymentId` usa `IgnoreQueryFilters()`
- CPF não é obrigatório para gerar cobrança; é obrigatório apenas para o membro acessar o link público

## Migrations EF Core
Sempre que criar uma migration manualmente (quando o `dotnet ef migrations add` não puder ser executado por DLLs bloqueadas em uso), **três arquivos são obrigatórios**:
1. `<timestamp>_<Nome>.cs` — a migration em si (Up/Down)
2. `<timestamp>_<Nome>.Designer.cs` — partial class com atributos `[DbContext]` e `[Migration]`
3. `TorneioDbContextModelSnapshot.cs` — snapshot atualizado com as novas propriedades

O `.Designer.cs` segue este padrão mínimo:
```csharp
// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Torneio.Infrastructure.Data;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    [DbContext(typeof(TorneioDbContext))]
    [Migration("<timestamp>_<Nome>")]
    partial class <Nome>
    {
    }
}
```

Preferir sempre rodar `dotnet ef migrations add` quando possível (parar a aplicação primeiro se necessário). Criar migration manualmente só como último recurso.

## Convenções de código
- Nomes de domínio: **português** (entidades, propriedades, métodos de negócio)
- Padrões técnicos: **inglês** (interfaces com I, GetById, Create, Update, Delete)
- Entidades: construtor privado + factory method estático `Criar(...)`
- Propriedades: `set` privado
- Controllers: finos — recebem, delegam ao Application, retornam resposta
- Validações: FluentValidation na camada Application
- Senhas: BCrypt
- Fotos: salvas em disco, path relativo no banco, servidas via URL autenticada
- Ids: Guid em todas as entidades
- Banco: PostgreSQL com Npgsql — usar snake_case nos nomes de tabela e coluna
  (convenção Npgsql: `UseSnakeCaseNamingConvention()`)
