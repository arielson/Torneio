import '../flavor_config.dart';

class BannerApp {
  final String id;
  final String imagemUrl;
  final String torneioSlug;
  final String torneioNome;
  final int ordem;
  final String tipoDestino;
  final String? destino;

  const BannerApp({
    required this.id,
    required this.imagemUrl,
    required this.torneioSlug,
    required this.torneioNome,
    required this.ordem,
    required this.tipoDestino,
    this.destino,
  });

  factory BannerApp.fromJson(Map<String, dynamic> json) => BannerApp(
        id: json['id'] as String,
        imagemUrl: AppConfig.resolverUrl(json['imagemUrl'] as String) ?? '',
        torneioSlug: json['torneioSlug'] as String? ?? '',
        torneioNome: json['torneioNome'] as String? ?? '',
        ordem: json['ordem'] as int? ?? 0,
        tipoDestino: json['tipoDestino'] as String? ?? 'Torneio',
        destino: json['destino'] as String?,
      );
}
