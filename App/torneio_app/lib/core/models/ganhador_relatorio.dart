class GanhadorRelatorio {
  final int posicao;
  final String equipeId;
  final String nomeEquipe;
  final String capitao;
  final double totalPontos;

  const GanhadorRelatorio({
    required this.posicao,
    required this.equipeId,
    required this.nomeEquipe,
    required this.capitao,
    required this.totalPontos,
  });

  factory GanhadorRelatorio.fromJson(Map<String, dynamic> json) =>
      GanhadorRelatorio(
        posicao: json['posicao'] as int,
        equipeId: json['equipeId'] as String,
        nomeEquipe: json['nomeEquipe'] as String,
        capitao: json['capitao'] as String? ?? '',
        totalPontos: (json['totalPontos'] as num).toDouble(),
      );
}
