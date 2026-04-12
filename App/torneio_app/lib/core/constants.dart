class ApiConstants {
  // Altere para o endereço real do backend em produção
  static const String baseUrl = 'https://torneio.ari.net.br';

  static String config(String slug) => '$baseUrl/api/$slug/config';
  static String login(String slug) => '$baseUrl/api/$slug/auth/login';
  static String loginAdminGeral() => '$baseUrl/api/auth/login';
  static String equipes(String slug) => '$baseUrl/api/$slug/equipes';
  static String membros(String slug) => '$baseUrl/api/$slug/membros';
  static String itens(String slug) => '$baseUrl/api/$slug/itens';
  static String capturas(String slug) => '$baseUrl/api/$slug/capturas';
  static String sync(String slug) => '$baseUrl/api/$slug/sync';
  static String sorteio(String slug) => '$baseUrl/api/$slug/sorteio';
  static String anos(String slug) => '$baseUrl/api/$slug/anos';
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
