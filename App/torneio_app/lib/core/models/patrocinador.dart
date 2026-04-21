import '../flavor_config.dart';

class Patrocinador {
  final String id;
  final String nome;
  final String fotoUrl;
  final String? instagram;
  final String? site;
  final String? zap;

  const Patrocinador({
    required this.id,
    required this.nome,
    required this.fotoUrl,
    this.instagram,
    this.site,
    this.zap,
  });

  factory Patrocinador.fromJson(Map<String, dynamic> json) => Patrocinador(
    id: json['id'] as String,
    nome: json['nome'] as String,
    fotoUrl: AppConfig.resolverUrl(json['fotoUrl'] as String?) ?? '',
    instagram: json['instagram'] as String?,
    site: json['site'] as String?,
    zap: json['zap'] as String?,
  );
}
