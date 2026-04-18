/// Configuração global do app.
class AppConfig {
  //static const String apiBaseUrl = 'https://torneioapi.ari.net.br';
  static const String apiBaseUrl = 'http://192.168.1.4:5053';

  /// Converte URLs relativas (ex: /media/logos/x.jpg) em absolutas.
  /// A API serve os mesmos arquivos estáticos em /media, então apiBaseUrl é suficiente.
  static String? resolverUrl(String? url) {
    if (url == null || url.isEmpty) return null;
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return '${apiBaseUrl.trimRight()}/${url.trimLeft().replaceFirst(RegExp(r'^/'), '')}';
  }
}
