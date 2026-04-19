class GanhadorEquipe {
  final int posicao;
  final String equipeId;
  final String nomeEquipe;
  final String capitao;
  final double totalPontos;

  const GanhadorEquipe({
    required this.posicao,
    required this.equipeId,
    required this.nomeEquipe,
    required this.capitao,
    required this.totalPontos,
  });

  factory GanhadorEquipe.fromJson(Map<String, dynamic> json) => GanhadorEquipe(
        posicao: json['posicao'] as int,
        equipeId: json['equipeId'] as String,
        nomeEquipe: json['nomeEquipe'] as String,
        capitao: json['capitao'] as String? ?? '',
        totalPontos: (json['totalPontos'] as num).toDouble(),
      );
}

class GanhadorMembro {
  final int posicao;
  final String membroId;
  final String nomeMembro;
  final double totalPontos;

  const GanhadorMembro({
    required this.posicao,
    required this.membroId,
    required this.nomeMembro,
    required this.totalPontos,
  });

  factory GanhadorMembro.fromJson(Map<String, dynamic> json) => GanhadorMembro(
        posicao: json['posicao'] as int,
        membroId: json['membroId'] as String,
        nomeMembro: json['nomeMembro'] as String,
        totalPontos: (json['totalPontos'] as num).toDouble(),
      );
}

class GanhadoresResponse {
  final bool premiacaoPorEquipe;
  final bool premiacaoPorMembro;
  final List<GanhadorEquipe> equipesGanhadoras;
  final List<GanhadorMembro> membrosGanhadores;

  const GanhadoresResponse({
    required this.premiacaoPorEquipe,
    required this.premiacaoPorMembro,
    required this.equipesGanhadoras,
    required this.membrosGanhadores,
  });

  factory GanhadoresResponse.fromJson(Map<String, dynamic> json) => GanhadoresResponse(
        premiacaoPorEquipe: json['premiacaoPorEquipe'] as bool? ?? true,
        premiacaoPorMembro: json['premiacaoPorMembro'] as bool? ?? false,
        equipesGanhadoras: (json['equipesGanhadoras'] as List? ?? [])
            .map((e) => GanhadorEquipe.fromJson(e as Map<String, dynamic>))
            .toList(),
        membrosGanhadores: (json['membrosGanhadores'] as List? ?? [])
            .map((e) => GanhadorMembro.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

// Alias para compatibilidade de código existente (usa GanhadorEquipe)
typedef GanhadorRelatorio = GanhadorEquipe;
