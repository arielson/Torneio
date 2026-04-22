import '../flavor_config.dart';

class Equipe {
  final String id;
  final String torneioId;
  final String nome;
  final String? fotoUrl;
  final String capitao;
  final String? fotoCapitaoUrl;
  final int qtdVagas;
  final double custo;
  final String statusFinanceiro;
  final DateTime? dataVencimentoCusto;
  final int qtdMembros;
  final List<String> membroIds;
  final List<String> fiscalIds;

  const Equipe({
    required this.id,
    required this.torneioId,
    required this.nome,
    this.fotoUrl,
    required this.capitao,
    this.fotoCapitaoUrl,
    required this.qtdVagas,
    this.custo = 0,
    this.statusFinanceiro = 'Pendente',
    this.dataVencimentoCusto,
    required this.qtdMembros,
    this.membroIds = const [],
    this.fiscalIds = const [],
  });

  factory Equipe.fromJson(Map<String, dynamic> json) => Equipe(
    id: json['id'] as String,
    torneioId: json['torneioId'] as String,
    nome: json['nome'] as String,
    fotoUrl: AppConfig.resolverUrl(json['fotoUrl'] as String?),
    capitao: json['capitao'] as String,
    fotoCapitaoUrl: AppConfig.resolverUrl(json['fotoCapitaoUrl'] as String?),
    qtdVagas: json['qtdVagas'] as int,
    custo: (json['custo'] as num?)?.toDouble() ?? 0,
    statusFinanceiro: _parseStatusFinanceiro(json['statusFinanceiro']),
    dataVencimentoCusto: json['dataVencimentoCusto'] == null ? null : DateTime.tryParse(json['dataVencimentoCusto'].toString()),
    qtdMembros: json['qtdMembros'] as int,
    membroIds: (json['membroIds'] as List<dynamic>? ?? []).map((e) => e as String).toList(),
    fiscalIds: (json['fiscalIds'] as List<dynamic>? ?? []).map((e) => e as String).toList(),
  );

  static String _parseStatusFinanceiro(dynamic value) {
    if (value is String && value.isNotEmpty) return value;
    if (value is int) {
      return switch (value) {
        1 => 'Confirmada',
        2 => 'Cancelada',
        _ => 'Pendente',
      };
    }
    return 'Pendente';
  }
}
