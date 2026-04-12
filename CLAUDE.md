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
│   └── Torneio.API
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

**Membro:** Id, TorneioId, AnoTorneioId, Nome, FotoUrl?

**Item:** Id, TorneioId, Nome, FotoUrl?, Comprimento (decimal), FatorMultiplicador (decimal, default 1.0)

**Captura:** Id, TorneioId, AnoTorneioId, ItemId, MembroId, EquipeId, TamanhoMedida (decimal), FotoUrl, DataHora, PendenteSync (bool)

**SorteioEquipe:** Id, TorneioId, AnoTorneioId, EquipeId, MembroId, Posicao

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
  }
}
```

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
