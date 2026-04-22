import '../flavor_config.dart';

class Membro {
  final String id;
  final String nome;
  final String? fotoUrl;
  final String? celular;
  final String? tamanhoCamisa;

  const Membro({
    required this.id,
    required this.nome,
    this.fotoUrl,
    this.celular,
    this.tamanhoCamisa,
  });

  factory Membro.fromJson(Map<String, dynamic> json) => Membro(
    id: json['id'] as String,
    nome: json['nome'] as String,
    fotoUrl: AppConfig.resolverUrl(json['fotoUrl'] as String?),
    celular: json['celular'] as String?,
    tamanhoCamisa: json['tamanhoCamisa'] as String?,
  );
}
