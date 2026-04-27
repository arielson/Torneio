import '../flavor_config.dart';

class Item {
  final String id;
  final String especiePeixeId;
  final String nome;
  final String? nomeCientifico;
  final double? comprimento;
  final double fatorMultiplicador;
  final String? fotoUrl;

  const Item({
    required this.id,
    required this.especiePeixeId,
    required this.nome,
    this.nomeCientifico,
    required this.comprimento,
    required this.fatorMultiplicador,
    this.fotoUrl,
  });

  factory Item.fromJson(Map<String, dynamic> json) => Item(
    id: json['id'] as String,
    especiePeixeId: json['especiePeixeId'] as String,
    nome: json['nome'] as String,
    nomeCientifico: json['nomeCientifico'] as String?,
    comprimento: (json['comprimento'] as num?)?.toDouble(),
    fatorMultiplicador: (json['fatorMultiplicador'] as num).toDouble(),
    fotoUrl: AppConfig.resolverUrl(json['fotoUrl'] as String?),
  );
}
