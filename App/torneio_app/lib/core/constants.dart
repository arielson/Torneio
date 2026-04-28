import 'flavor_config.dart';

class ApiConstants {
  static const String _base = AppConfig.apiBaseUrl;

  static String config(String slug) => '$_base/api/$slug/config';
  static String login(String slug) => '$_base/api/$slug/auth/login';
  static String loginAdminGeral() => '$_base/api/auth/login';
  static String equipes(String slug) => '$_base/api/$slug/equipes';
  static String fiscais(String slug) => '$_base/api/$slug/fiscais';
  static String membros(String slug) => '$_base/api/$slug/membros';
  static String itens(String slug) => '$_base/api/$slug/itens';
  static String especiesPeixe() => '$_base/api/especies';
  static String patrocinadores(String slug) => '$_base/api/$slug/patrocinadores';
  static String premios(String slug) => '$_base/api/$slug/premios';
  static String participantes(String slug) => '$_base/api/$slug/participantes';
  static String registroPescadorSolicitarCodigo(String slug) => '$_base/api/$slug/registro-pescador/solicitar-codigo';
  static String registroPescadorConfirmar(String slug) => '$_base/api/$slug/registro-pescador/confirmar';
  static String recuperarSenhaPescadorSolicitarCodigo(String slug) => '$_base/api/$slug/pescador/recuperar-senha/solicitar-codigo';
  static String recuperarSenhaPescadorConfirmar(String slug) => '$_base/api/$slug/pescador/recuperar-senha/confirmar';
  static String membroCobrancas(String slug) => '$_base/api/$slug/membro/financeiro/cobrancas';
  static String adminsTorneio(String slug) => '$_base/api/$slug/admins-torneio';
  static String capturas(String slug) => '$_base/api/$slug/capturas';
  static String financeiroConfig(String slug) => '$_base/api/$slug/financeiro/config';
  static String financeiroIndicadores(String slug) => '$_base/api/$slug/financeiro/indicadores';
  static String financeiroRelatorios(String slug) => '$_base/api/$slug/financeiro/relatorios';
  static String financeiroGerarParcelas(String slug) => '$_base/api/$slug/financeiro/cobrancas/gerar';
  static String financeiroCobrancas(
    String slug, {
    String? membroId,
    bool inadimplentes = false,
    bool naoPagas = false,
    String? tipo,
  }) {
    final params = <String>[
      if (membroId != null && membroId.isNotEmpty) 'membroId=$membroId',
      if (tipo != null && tipo.isNotEmpty) 'tipo=${Uri.encodeComponent(tipo)}',
      if (naoPagas) 'naoPagas=true',
      if (inadimplentes) 'inadimplentes=true',
    ];
    final query = params.isEmpty ? '' : '?${params.join('&')}';
    return '$_base/api/$slug/financeiro/cobrancas$query';
  }
  static String financeiroCobranca(String slug, String cobrancaId) => '$_base/api/$slug/financeiro/cobrancas/$cobrancaId';
  static String financeiroPagamentoCobranca(String slug, String cobrancaId) => '$_base/api/$slug/financeiro/cobrancas/$cobrancaId/pagamento';
  static String financeiroComprovanteCobranca(String slug, String cobrancaId) => '$_base/api/$slug/financeiro/cobrancas/$cobrancaId/comprovante';
  static String financeiroExtras(String slug) => '$_base/api/$slug/financeiro/extras';
  static String financeiroExtraMembros(String slug, String produtoId) => '$_base/api/$slug/financeiro/extras/$produtoId/membros';
  static String financeiroRemoverExtraMembro(String slug, String adesaoId) => '$_base/api/$slug/financeiro/extras/membros/$adesaoId';
  static String financeiroDoacoes(String slug) => '$_base/api/$slug/financeiro/doacoes';
  static String financeiroCustos(String slug) => '$_base/api/$slug/financeiro/custos';
  static String financeiroChecklist(String slug) => '$_base/api/$slug/financeiro/checklist';
  static String rankingPublico(String slug) => '$_base/api/$slug/ranking';
  static String relatoriosGanhadores(String slug) => '$_base/api/$slug/relatorios/ganhadores';
  static String relatorioMaioresCapturasPdf(String slug, int quantidade) =>
      '$_base/api/$slug/relatorios/maiores-capturas?quantidade=$quantidade';
  static String relatorioEquipePdf(String slug, String equipeId, {bool analitico = false}) =>
      '$_base/api/$slug/relatorios/equipe/$equipeId?analitico=$analitico';
  static String relatorioMembroPdf(String slug, String membroId, {bool analitico = false}) =>
      '$_base/api/$slug/relatorios/membro/$membroId?analitico=$analitico';
  static String sync(String slug) => '$_base/api/$slug/sync';
  static String sorteio(String slug) => '$_base/api/$slug/sorteio';
  static String sorteioConfirmar(String slug) => '$_base/api/$slug/sorteio/confirmar';
  static String sorteioPreCondicoes(String slug) => '$_base/api/$slug/sorteio/pre-condicoes';
  static String grupos(String slug) => '$_base/api/$slug/grupos';
  static String grupoMembros(String slug, String grupoId) => '$_base/api/$slug/grupos/$grupoId/membros';
  static String reorganizacaoEmergencialEquipe(String slug) => '$_base/api/$slug/equipes/reorganizacao-emergencial';
  static String torneioLiberar(String slug) => '$_base/api/$slug/admin/liberar';
  static String torneioFinalizar(String slug) => '$_base/api/$slug/admin/finalizar';
  static String torneioReabrir(String slug) => '$_base/api/$slug/admin/reabrir';
  static String torneioConfiguracaoAdmin(String slug) => '$_base/api/$slug/admin/configuracao';
  static String sorteioGrupo(String slug) => '$_base/api/$slug/sorteio-grupo';
  static String sorteioGrupoConfirmar(String slug) => '$_base/api/$slug/sorteio-grupo/confirmar';
  static String sorteioGrupoPreCondicoes(String slug) => '$_base/api/$slug/sorteio-grupo/pre-condicoes';
  static String anos(String slug) => '$_base/api/$slug/anos';
  static String torneiosRecentes({int limite = 5}) => '$_base/api/torneios/recentes?limite=$limite';
  static String torneiosBuscar(String q) => '$_base/api/torneios/buscar?q=${Uri.encodeComponent(q)}';
  static String banners() => '$_base/api/banners';
}

class StorageKeys {
  static const String token = 'auth_token';
  static const String perfil = 'auth_perfil';
  static const String slug = 'auth_slug';
  static const String userId = 'auth_user_id';
  static const String userName = 'auth_user_name';
  static const String torneioId = 'auth_torneio_id';
  static const String ultimoSlug = 'ultimo_slug';
}
