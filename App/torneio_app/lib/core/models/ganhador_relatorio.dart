class GanhadorEquipe {
  final int posicao;
  final String equipeId;
  final String nomeEquipe;
  final String capitao;
  final double totalPontos;
  final List<String> pescadores;

  const GanhadorEquipe({
    required this.posicao,
    required this.equipeId,
    required this.nomeEquipe,
    required this.capitao,
    required this.totalPontos,
    required this.pescadores,
  });

  factory GanhadorEquipe.fromJson(Map<String, dynamic> json) => GanhadorEquipe(
        posicao: json['posicao'] as int,
        equipeId: json['equipeId'] as String,
        nomeEquipe: json['nomeEquipe'] as String,
        capitao: json['capitao'] as String? ?? '',
        totalPontos: (json['totalPontos'] as num).toDouble(),
        pescadores: (json['pescadores'] as List? ?? []).map((e) => e.toString()).toList(),
      );
}

class GanhadorMembro {
  final int posicao;
  final String membroId;
  final String nomeMembro;
  final double totalPontos;
  final double? maiorCaptura;
  final String? nomeItemMaiorCaptura;

  const GanhadorMembro({
    required this.posicao,
    required this.membroId,
    required this.nomeMembro,
    required this.totalPontos,
    this.maiorCaptura,
    this.nomeItemMaiorCaptura,
  });

  factory GanhadorMembro.fromJson(Map<String, dynamic> json) => GanhadorMembro(
        posicao: json['posicao'] as int,
        membroId: json['membroId'] as String,
        nomeMembro: json['nomeMembro'] as String,
        totalPontos: (json['totalPontos'] as num?)?.toDouble() ?? 0,
        maiorCaptura: (json['maiorCaptura'] as num?)?.toDouble(),
        nomeItemMaiorCaptura: json['nomeItemMaiorCaptura'] as String?,
      );
}

class GanhadoresResponse {
  final int quantidadeEquipes;
  final int quantidadeMembrosPontuacao;
  final int quantidadeMembrosMaiorCaptura;
  final bool exibirPescadoresDasEmbarcacoes;
  final bool exibirMaiorCaptura;
  final List<GanhadorEquipe> equipesGanhadoras;
  final List<GanhadorMembro> membrosGanhadores;
  final List<GanhadorMembro> membrosMaiorCaptura;

  const GanhadoresResponse({
    required this.quantidadeEquipes,
    required this.quantidadeMembrosPontuacao,
    required this.quantidadeMembrosMaiorCaptura,
    required this.exibirPescadoresDasEmbarcacoes,
    required this.exibirMaiorCaptura,
    required this.equipesGanhadoras,
    required this.membrosGanhadores,
    required this.membrosMaiorCaptura,
  });

  factory GanhadoresResponse.fromJson(Map<String, dynamic> json) => GanhadoresResponse(
        quantidadeEquipes: json['quantidadeEquipes'] as int? ?? 0,
        quantidadeMembrosPontuacao: json['quantidadeMembrosPontuacao'] as int? ?? 0,
        quantidadeMembrosMaiorCaptura: json['quantidadeMembrosMaiorCaptura'] as int? ?? 0,
        exibirPescadoresDasEmbarcacoes: json['exibirPescadoresDasEmbarcacoes'] as bool? ?? false,
        exibirMaiorCaptura: json['exibirMaiorCaptura'] as bool? ?? false,
        equipesGanhadoras: (json['equipesGanhadoras'] as List? ?? [])
            .map((e) => GanhadorEquipe.fromJson(e as Map<String, dynamic>))
            .toList(),
        membrosGanhadores: (json['membrosGanhadores'] as List? ?? [])
            .map((e) => GanhadorMembro.fromJson(e as Map<String, dynamic>))
            .toList(),
        membrosMaiorCaptura: (json['membrosMaiorCaptura'] as List? ?? [])
            .map((e) => GanhadorMembro.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

// Alias para compatibilidade de código existente (usa GanhadorEquipe)
typedef GanhadorRelatorio = GanhadorEquipe;
