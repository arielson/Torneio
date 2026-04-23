/// Configuracao global do app.
class AppConfig {
  static const String apiBaseUrl = 'https://torviaapi.ari.net.br';
  // static const String apiBaseUrl = 'http://192.168.1.4:5053';
  static const String mediaBasePath = '/media';

  /// Converte URLs relativas em absolutas.
  /// Exemplo: `fotos/equipes/x.jpg` -> `http://host/media/fotos/equipes/x.jpg`.
  static String? resolverUrl(String? url) {
    if (url == null || url.isEmpty) return null;
    if (url.startsWith('http://') || url.startsWith('https://')) return url;

    final normalizada = url.trimLeft().replaceFirst(RegExp(r'^/'), '');
    final relativa = normalizada.startsWith('media/')
        ? normalizada
        : '${mediaBasePath.replaceFirst(RegExp(r'^/'), '')}/$normalizada';

    return '${apiBaseUrl.trimRight()}/$relativa';
  }
}
