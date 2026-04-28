import '../flavor_config.dart';

class EspeciePeixe {
  final String id;
  final String nome;
  final String? nomeCientifico;
  final String? fotoUrl;

  const EspeciePeixe({
    required this.id,
    required this.nome,
    this.nomeCientifico,
    this.fotoUrl,
  });

  factory EspeciePeixe.fromJson(Map<String, dynamic> json) => EspeciePeixe(
        id: json['id'] as String,
        nome: json['nome'] as String,
        nomeCientifico: json['nomeCientifico'] as String?,
        fotoUrl: AppConfig.resolverUrl(json['fotoUrl'] as String?),
      );
}
