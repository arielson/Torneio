import '../flavor_config.dart';

class Equipe {
  final String id;
  final String torneioId;
  final String nome;
  final String? fotoUrl;
  final String capitao;
  final String? fotoCapitaoUrl;
  final String fiscalId;
  final int qtdVagas;
  final int qtdMembros;
  final List<String> membroIds;

  const Equipe({
    required this.id,
    required this.torneioId,
    required this.nome,
    this.fotoUrl,
    required this.capitao,
    this.fotoCapitaoUrl,
    required this.fiscalId,
    required this.qtdVagas,
    required this.qtdMembros,
    this.membroIds = const [],
  });

  factory Equipe.fromJson(Map<String, dynamic> json) => Equipe(
    id: json['id'] as String,
    torneioId: json['torneioId'] as String,
    nome: json['nome'] as String,
    fotoUrl: AppConfig.resolverUrl(json['fotoUrl'] as String?),
    capitao: json['capitao'] as String,
    fotoCapitaoUrl: AppConfig.resolverUrl(json['fotoCapitaoUrl'] as String?),
    fiscalId: json['fiscalId'] as String,
    qtdVagas: json['qtdVagas'] as int,
    qtdMembros: json['qtdMembros'] as int,
    membroIds:
        (json['membroIds'] as List<dynamic>? ?? [])
            .map((e) => e as String)
            .toList(),
  );
}
