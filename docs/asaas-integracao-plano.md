# Plano de Integração Asaas — Torneio

> Gerado em: 2026-05-27  
> Status: Diagnóstico inicial (Etapa 1)

---

## 1. Estrutura do Projeto Atual

```
D:\Source\Torneio\Retaguarda
├── Torneio.Domain        (net10.0) — entidades, enums, interfaces
├── Torneio.Application   (net10.0) — DTOs, serviços, FluentValidation 11.9.2
├── Torneio.Infrastructure(net10.0) — EF Core 9.0.1 + Npgsql, repositórios, seed
├── Torneio.API           (net10.0) — WebAPI, JWT Bearer 10.0.2
└── Torneio.Web           (net10.0) — ASP.NET MVC, Razor Views, cookie auth
```

**Frontend:** Razor Views (MVC) — não há React/Blazor. Toda UI de cobrança deve ser em Razor.  
**ORM:** Entity Framework Core 9.0.1 com Npgsql + snake_case naming convention.  
**Autenticação Web:** Cookie (`AdminGeral` / `AdminTorneio`).  
**Autenticação API:** JWT Bearer com claims `sub`, `torneio_id`, `perfil`, `slug`.

---

## 2. Mapeamento do Módulo Asaas Existente (D:\Source\PescaPro\Web\Sistema\Sistema.Asaas)

### O que existe

| Componente | Arquivo | Descrição |
|---|---|---|
| `AsaasHttpClient` | `Configuration/AsaasHttpClient.cs` | HttpClient tipado. Header `access_token`. Suporte a redirects manuais. |
| `AsaasConfig` | `Configuration/AsaasConfig.cs` | POCO com `ApiKey`, `BaseUrl`, `IsSandbox`, `TimeoutSeconds`, `ApiPath` |
| `AsaasClient` | `AsaasClient.cs` | Fachada principal — expõe `Payments`, `Customers`, `Webhooks`, `MyAccount`, etc. |
| `CustomerService` | `Services/CustomerService.cs` | CRUD Customer. Busca por `cpfCnpj`, `externalReference`. |
| `PaymentService` | `Services/PaymentService.cs` | Criar, listar, status, refund, PIX QR Code, simular. |
| `WebhookService` | `Services/WebhookService.cs` | Registrar/listar/atualizar webhook na conta Asaas. |
| `ApplicationAsaasService` | (PescaPro.Application) | Orquestrador do PescaPro — cria customer+payment, processa webhook, solicita saque. |
| DTOs | `Models/Customers/`, `Models/Payments/`, `Models/Webhooks/` | CustomerRequest/Response, PaymentRequest/Response, WebhookRequest/Response |
| `AsaasListResponse<T>` | `Models/Common/` | Wrapper de paginação padrão Asaas |
| `AsaasException` | `Configuration/AsaasException.cs` | Exceção com `StatusCode` e `ResponseContent` |
| Enums | `Models/Enums/` | `BillingType` (PIX, CREDIT_CARD, UNDEFINED, …), `PaymentStatus` |

### Limitação crítica para reuso

O `AsaasHttpClient` recebe `ApiKey` **no construtor** via `AsaasConfig` — é um singleton por chave. Para o Torneio cada torneio tem sua própria chave de API. **Solução:** criar `IAsaasClientFactory` que instancia `AsaasClient` dinamicamente com a chave do torneio em cada operação. Não injetar `AsaasClient` direto no DI como singleton.

---

## 3. Mapeamento do Projeto Atual

### 3.1 TorneioEntity

**Arquivo:** `Torneio.Domain/Entities/TorneioEntity.cs`

| Campo relevante | Tipo | Situação |
|---|---|---|
| `Id` | Guid | ✅ Existe |
| `Slug` | string | ✅ Existe — único, usado no roteamento |
| `ValorPorMembro` | decimal | ✅ Existe — valor total por membro |
| `QuantidadeParcelas` | int | ✅ Existe — número de parcelas |
| `DataPrimeiroVencimento` | DateTime? | ✅ Existe |
| `ExibirModuloFinanceiro` | bool | ✅ Existe |

**Conclusão:** `Slug` já existe, nenhuma migration necessária para TorneioEntity.

### 3.2 Membro (o "Pescador" neste projeto)

**Arquivo:** `Torneio.Domain/Entities/Membro.cs`

| Campo | Tipo | Situação |
|---|---|---|
| `Id` | Guid | ✅ |
| `TorneioId` | Guid | ✅ |
| `Nome` | string | ✅ |
| `Celular` | string? | ✅ — usado como chave na URL pública |
| `Cpf` | — | ❌ **AUSENTE** — **bloqueador crítico** |

> **⚠️ BLOQUEADOR:** A entidade `Membro` não possui campo `Cpf`.
> Isso bloqueia duas funcionalidades:
> 1. Criação de Customer no Asaas (`cpfCnpj` é campo obrigatório na API deles)
> 2. Confirmação de identidade na URL pública `/{slug}/cobranca/{telefone}` (validação por CPF)
>
> **Decisão necessária (ver seção 6):** O CPF deve ser adicionado à entidade `Membro`. Por ser um campo sensível mas opcional para torneios sem Asaas, adicionar como `string?` (nullable). Obrigatório apenas quando `ConfiguracaoAsaasTorneio.Habilitada = true`.

### 3.3 Financeiro — ParcelaTorneio

**Arquivo:** `Torneio.Domain/Entities/ParcelaTorneio.cs`

Esta é a entidade financeira central. Representa parcelas de membros.

| Campo | Tipo | Situação |
|---|---|---|
| `Id` | Guid | ✅ |
| `TorneioId` | Guid | ✅ |
| `MembroId` | Guid | ✅ |
| `TipoParcela` | enum (Mensalidade, TaxaInscricao, ProdutosExtra) | ✅ |
| `NumeroParcela` | int | ✅ |
| `Valor` | decimal | ✅ |
| `Vencimento` | DateTime | ✅ |
| `Pago` | bool | ✅ |
| `DataPagamento` | DateTime? | ✅ |
| `Bonificada` | bool | ✅ |
| Campos de comprovante | múltiplos | ✅ |
| `AsaasPaymentId` | — | ❌ **AUSENTE** |
| `DataPrevisaoCredito` | — | ❌ **AUSENTE** |
| `DataCreditoEfetivo` | — | ❌ **AUSENTE** |

**Conclusão:** `ParcelaTorneio` é o equivalente ao "lançamento financeiro de receita" do plano. O `CobrancaAsaas` deve ter FK para `ParcelaTorneio` (campo `ParcelaTorneioId`), não um genérico `LancamentoFinanceiroId`. Quando o webhook confirmar pagamento, o handler chama `ParcelaTorneio.MarcarComoPago()`.

Não há entidade separada de "previsão de caixa" no projeto — `DataPrevisaoCredito` e `DataCreditoEfetivo` ficarão em `CobrancaAsaas` mesmo, e o admin visualiza pelo relatório financeiro existente.

### 3.4 CustoTorneio (referência)

**Arquivo:** `Torneio.Domain/Entities/CustoTorneio.cs`

Entidade de custos (despesas do torneio). Não é alterada pela integração Asaas.

### 3.5 DbContext

**Arquivo:** `Torneio.Infrastructure/Data/TorneioDbContext.cs`

DbSets existentes relevantes: `Torneiros`, `Membros`, `ParcelasTorneio`, `ValoresParcelas`, `CustosTorneio`.

Novos DbSets a adicionar: `ConfiguracoesAsaasTorneio`, `CobrancasAsaas`, `WebhookEventosAsaas`.

---

## 4. O Que Já Existe e Pode Ser Reutilizado

| Item | Origem | Como reutilizar |
|---|---|---|
| `AsaasHttpClient` | `Sistema.Asaas` | Copiar. Adaptar para aceitar `ApiKey` como parâmetro na instanciação (já faz isso via `AsaasConfig`). |
| `AsaasClient` | `Sistema.Asaas` | Copiar. Criar `IAsaasClientFactory` que instancia com a chave do torneio. |
| `CustomerService` | `Sistema.Asaas` | Copiar sem modificação. |
| `PaymentService` | `Sistema.Asaas` | Copiar sem modificação. |
| `WebhookService` | `Sistema.Asaas` | Copiar sem modificação. |
| Todos os DTOs (`Models/`) | `Sistema.Asaas` | Copiar sem modificação (CustomerRequest/Response, PaymentRequest/Response, etc.). |
| `AsaasException` | `Sistema.Asaas` | Copiar sem modificação. |
| `AsaasListResponse<T>` | `Sistema.Asaas` | Copiar sem modificação. |
| `TorneioEntity.Slug` | Projeto atual | Já existe — sem migration. |
| `ParcelaTorneio` | Projeto atual | Reuse como "lançamento financeiro". Webhook chama `MarcarComoPago()`. |
| `ValorParcelaTorneio` | Projeto atual | Valores configurados por parcela já existem — usar na geração das cobranças. |

**Não reutilizar:** `ApplicationAsaasService` do PescaPro — está muito acoplado ao domínio PescaPro (PescadorSaida, Carteira, Saque). Criar serviço novo seguindo os mesmos padrões.

---

## 5. O Que Precisa Ser Criado

### 5.1 Camada Domain

| Artefato | Tipo | Observações |
|---|---|---|
| `ConfiguracaoAsaasTorneio` | Entidade | 1:1 com TorneioEntity. Chave API criptografada. |
| `CobrancaAsaas` | Entidade | Linka para `ParcelaTorneio` e `Membro`. |
| `WebhookEventoAsaas` | Entidade | Auditoria e idempotência de webhooks. |
| `FormaPagamentoAsaas` | Enum | PIX, CARTAO_CREDITO, INDEFINIDA |
| `StatusCobrancaAsaas` | Enum | PENDENTE, AGUARDANDO, CONFIRMADA, RECEBIDA, VENCIDA, ESTORNADA, CANCELADA, RECUSADA |
| `StatusChaveAsaas` | Enum | ATIVA, DESABILITADA, EXPIRADA |

### 5.2 Camada Infrastructure

| Artefato | Observações |
|---|---|
| Pasta `Torneio.Infrastructure/Asaas/` | Módulo copiado de `Sistema.Asaas` |
| `AsaasOptions` | Binding de `appsettings.json` via `IOptions<T>` |
| `IAsaasClientFactory` + `AsaasClientFactory` | Cria `AsaasClient` com chave por torneio |
| Configurações EF Core das 3 novas entidades | Migrations |
| `ICobrancaAsaasRepository` + impl | Repositório padrão do projeto |
| `IConfiguracaoAsaasTorneioRepository` + impl | Repositório padrão |

### 5.3 Camada Application

| Artefato | Observações |
|---|---|
| `CalculadoraTaxaAsaas` | Lê `AsaasOptions.Taxas`. Respeita `PromocaoAtiva`. |
| `CalculadoraPrevisaoCredito` | PIX=same day. Cartão=D+32 (configurável). |
| `IGerarCobrancasTorneioServico` + impl | Orquestra criação de customer+cobranças por lote. |
| `IWebhookAsaasServico` + impl | Handlers por tipo de evento. |
| `IConfiguracaoAsaasTorneioServico` + impl | CRUD da configuração, validação da chave. |
| DTOs específicos do domínio | `CobrancaAsaasDto`, `ConfiguracaoAsaasDto`, etc. |

### 5.4 Camada API/Web

| Artefato | Tipo | Observações |
|---|---|---|
| `POST /webhooks/asaas` | API Controller | Validação token, idempotência, despacho para handlers |
| `GET /admin/torneios/{id}/configuracao-asaas` | Web Controller | Restrito a AdminGeral |
| `GET/{POST} /{slug}/cobranca/{telefone}` | Web Controller | Público. Confirmação por CPF, rate limit. |
| `POST /api/{slug}/financeiro/cobranças-asaas` | API Controller | Restrito a AdminTorneio. Gera cobranças. |

### 5.5 Testes

| Artefato | Observações |
|---|---|
| `CalculadoraTaxaAsaasTests` | xUnit. 5 cenários de taxa conforme plano. |
| `CalculadoraPrevisaoCreditoTests` | xUnit. 3 cenários de data conforme plano. |
| `WebhookAsaasHandlerTests` | 7 eventos simulados. |

---

## 6. O Que Precisa Ser Alterado (com paths)

| Arquivo | Alteração | Impacto |
|---|---|---|
| `Torneio.Domain/Entities/Membro.cs` | Adicionar `Cpf` (string?, nullable) + `AtualizarCpf()` | Migration obrigatória |
| `Torneio.Infrastructure/Data/TorneioDbContext.cs` | Adicionar 3 novos DbSets + configurações EF | — |
| `Torneio.API/appsettings.json` | Adicionar seção `Asaas` completa | — |
| `Torneio.API/appsettings.Development.json` | Credenciais sandbox | — |
| `Torneio.Infrastructure/DependencyInjection.cs` (ou equivalente) | Registrar factory Asaas, options, services | — |
| `Torneio.Web/` (Program.cs ou equivalente) | Registrar serviços Asaas para Web também | — |

---

## 7. Riscos Identificados

| # | Risco | Severidade | Mitigação |
|---|---|---|---|
| R1 | `Membro` sem CPF — bloqueador para Customer Asaas e confirmação pública | **CRÍTICO** | Migration para adicionar `Cpf` nullable. Ver pergunta aberta P1. |
| R2 | `AsaasHttpClient` é singleton por chave — precisa de factory | **ALTO** | `IAsaasClientFactory` cria instância sob demanda com chave do torneio. |
| R3 | Race condition: webhook chega antes da resposta HTTP de criação do payment | **MÉDIO** | `CobrancaAsaas` criada com status `PENDENTE` antes de chamar Asaas. Webhook `PAYMENT_CREATED` atualiza se a linha já existir (upsert por `AsaasPaymentId`). |
| R4 | Criptografia da chave Asaas: rotação de chave do Data Protection pode tornar chaves existentes ilegíveis | **MÉDIO** | Configurar key ring persistente em disco (não em memória). Documentar procedimento de rotação. |
| R5 | Limite de 10 chaves ativas por conta Asaas — sem UI para revogar | **MÉDIO** | Criar ação "Desativar e revogar chave" na tela admin que marca `StatusChave=EXPIRADA` e orienta o admin a revogar no painel Asaas. |
| R6 | `ACCESS_TOKEN_DISABLED`/`EXPIRED` nos webhooks: como identificar qual torneio pertence a qual chave? | **MÉDIO** | Guardar hash da chave em `ConfiguracaoAsaasTorneio.ChaveApiHash`. No webhook, buscar pelo hash recebido no payload. Verificar formato exato do payload Asaas para este evento. |
| R7 | Sem infraestrutura de notificações (email/push) para alertar AdminGeral sobre chave desabilitada | **BAIXO** | Registrar em log estruturado e exibir banner de alerta no painel AdminGeral. Email pode ser adicionado depois (Twilio SendGrid já tem config no projeto). |
| R8 | `ParcelaTorneio.Pago` é bool — sem estado intermediário (ex: "aguardando crédito do cartão") | **BAIXO** | `Pago=true` marcado em `PAYMENT_CONFIRMED` (não esperar `PAYMENT_RECEIVED`). Estado "crédito previsto" é exibido pela `CobrancaAsaas.DataPrevisaoCredito`, sem alterar `ParcelaTorneio`. |

---

## 8. Decisões Arquiteturais (respondidas em 2026-05-27)

| # | Pergunta | Decisão |
|---|---|---|
| P1 | CPF obrigatório? | **Nullable em `Membro`.** CPF só é exigido quando o membro tenta acessar o link público de cobranças (funciona como "senha"). Não é necessário para gerar a cobrança no sistema. |
| P2 | Membros sem CPF ao habilitar Asaas? | Idem P1 — cobranças são geradas para todos os membros. Membro sem CPF simplesmente não consegue acessar o link público até que o admin cadastre o CPF dele. |
| P3 | Juros/multa por atraso? | **Não há.** `Interest` e `Fine` não serão enviados ao Asaas. |
| P4 | Vencimentos das parcelas 2 e 3? | **Já configurado em `ParcelaTorneio.Vencimento`** — usar diretamente. Não há cálculo extra. |
| P5 | Reenvio de link para cobrança vencida? | **Não há reenvio.** O QR Code / Copia-e-Cola é gerado **on-demand** quando o membro clica na cobrança. Cobranças vencidas também geram na hora (Asaas permite pagamento de vencidos). |
| P6 | URL pública quando Asaas desabilitado? | **404 padrão** do sistema. |

### Mudança arquitetural importante (decorrente de P4/P5)

O plano original previa geração em batch com `linkPagamento` armazenado. As respostas revelam um fluxo diferente:

**Fluxo correto:**
1. Admin aciona "Gerar Cobranças" → cria registros `CobrancaAsaas` locais + cria payment no Asaas (obtém `AsaasPaymentId`). **Não armazena QR Code.**
2. Membro acessa `/{slug}/cobranca/{telefone}` → confirma CPF → vê lista de parcelas.
3. Membro clica em uma parcela pendente → sistema chama `GetPixQrCodeAsync(asaasPaymentId)` **na hora** → exibe QR Code e Copia-e-Cola.
4. Para Cartão: redireciona para `invoiceUrl` (esta sim armazenada no `CobrancaAsaas`).
5. Webhook confirma pagamento → `MarcarComoPago()` na `ParcelaTorneio`.

**Impacto no modelo:** `CobrancaAsaas` **não precisa** de campos `PixQrCodeImage` nem `PixPayload`. Apenas `AsaasPaymentId` e `InvoiceUrl` (para cartão). O QR Code é sempre buscado ao vivo.

---

## 9. Sequência de Dependências entre Etapas

```
Etapa 2 (config/options) 
    → Etapa 3 (modelo de dados + migrations)
        → P1/P2 DEVE ser respondida antes de migrar Membro
    → Etapa 4 (calculadoras) — independente, pode ir em paralelo após Etapa 2
Etapa 3 
    → Etapa 5 (tela admin — UI da ConfiguracaoAsaas)
    → Etapa 6 (geração de cobranças)
    → Etapa 7 (webhook)
Etapa 6 + Etapa 7 
    → Etapa 8 (URL pública do pescador)
Etapa 8 
    → Etapa 9 (configuração de webhook no Asaas — precisa da URL pública)
    → Etapa 10 (testes end-to-end)
```

---

## 10. Estado por Etapa (atualizado ao longo do desenvolvimento)

| Etapa | Status | Observações |
|---|---|---|
| Etapa 1 — Diagnóstico | ✅ Concluída | Este documento |
| Etapa 2 — Config/infraestrutura | ⏳ Aguardando | Aguarda respostas P1–P6 |
| Etapa 3 — Modelo de dados | ⏳ Aguardando | Depende de P1, P2, P4 |
| Etapa 4 — Calculadoras | ⏳ Aguardando | — |
| Etapa 5 — Tela admin | ⏳ Aguardando | — |
| Etapa 6 — Geração de cobranças | ⏳ Aguardando | — |
| Etapa 7 — Webhook | ⏳ Aguardando | — |
| Etapa 8 — URL pública | ⏳ Aguardando | — |
| Etapa 9 — Registrar webhook | ⏳ Aguardando | — |
| Etapa 10 — Testes E2E | ⏳ Aguardando | — |
| Etapa 11 — Documentação final | ⏳ Aguardando | — |
