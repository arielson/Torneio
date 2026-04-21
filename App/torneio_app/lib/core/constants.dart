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
  static String patrocinadores(String slug) => '$_base/api/$slug/patrocinadores';
  static String capturas(String slug) => '$_base/api/$slug/capturas';
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
