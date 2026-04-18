import '../flavor_config.dart';

class TorneioResumo {
  final String id;
  final String slug;
  final String nomeTorneio;
  final String? logoUrl;
  final String status;
  final bool ativo;
  final DateTime criadoEm;

  const TorneioResumo({
    required this.id,
    required this.slug,
    required this.nomeTorneio,
    this.logoUrl,
    required this.status,
    required this.ativo,
    required this.criadoEm,
  });

  factory TorneioResumo.fromJson(Map<String, dynamic> json) => TorneioResumo(
        id: json['id'] as String,
        slug: json['slug'] as String,
        nomeTorneio: json['nomeTorneio'] as String,
        logoUrl: AppConfig.resolverUrl(json['logoUrl'] as String?),
        status: json['status'] as String? ?? 'Aberto',
        ativo: json['ativo'] as bool? ?? true,
        criadoEm: DateTime.tryParse(json['criadoEm'] as String? ?? '') ?? DateTime.now(),
      );
}
