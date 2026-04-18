class Captura {
  final String id;
  final String torneioId;
  final String anoTorneioId;
  final String itemId;
  final String nomeItem;
  final String membroId;
  final String nomeMembro;
  final String equipeId;
  final String nomeEquipe;
  final double tamanhoMedida;
  final double fatorMultiplicador;
  final double pontuacao;
  final String fotoUrl;
  final DateTime dataHora;
  final bool pendenteSync;

  const Captura({
    required this.id,
    required this.torneioId,
    required this.anoTorneioId,
    required this.itemId,
    required this.nomeItem,
    required this.membroId,
    required this.nomeMembro,
    required this.equipeId,
    required this.nomeEquipe,
    required this.tamanhoMedida,
    required this.fatorMultiplicador,
    required this.pontuacao,
    required this.fotoUrl,
    required this.dataHora,
    required this.pendenteSync,
  });

  factory Captura.fromJson(Map<String, dynamic> json) => Captura(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        anoTorneioId: json['anoTorneioId'] as String,
        itemId: json['itemId'] as String,
        nomeItem: json['nomeItem'] as String,
        membroId: json['membroId'] as String,
        nomeMembro: json['nomeMembro'] as String,
        equipeId: json['equipeId'] as String,
        nomeEquipe: json['nomeEquipe'] as String,
        tamanhoMedida: (json['tamanhoMedida'] as num).toDouble(),
        fatorMultiplicador: (json['fatorMultiplicador'] as num).toDouble(),
        pontuacao: (json['pontuacao'] as num).toDouble(),
        fotoUrl: json['fotoUrl'] as String? ?? '',
        dataHora: DateTime.parse(json['dataHora'] as String),
        pendenteSync: json['pendenteSync'] as bool? ?? false,
      );
}

/// Usado para registrar uma nova captura (online ou offline)
class RegistrarCapturaRequest {
  final String torneioId;
  final String itemId;
  final String membroId;
  final String equipeId;
  final double tamanhoMedida;
  final String fotoUrl;
  final DateTime dataHora;
  final bool pendenteSync;

  const RegistrarCapturaRequest({
    required this.torneioId,
    required this.itemId,
    required this.membroId,
    required this.equipeId,
    required this.tamanhoMedida,
    required this.fotoUrl,
    required this.dataHora,
    this.pendenteSync = false,
  });

  Map<String, dynamic> toJson() => {
        'torneioId': torneioId,
        'itemId': itemId,
        'membroId': membroId,
        'equipeId': equipeId,
        'tamanhoMedida': tamanhoMedida,
        'fotoUrl': fotoUrl,
        'dataHora': dataHora.toIso8601String(),
        'pendenteSync': pendenteSync,
      };

  Map<String, dynamic> toDbMap(String localId) => {
        'id': localId,
        ...toJson(),
        'tamanhoMedida': tamanhoMedida,
        'pendenteSync': pendenteSync ? 1 : 0,
      };
}
