import '../flavor_config.dart';

// 0 = App (camera), 1 = App (galeria), null = Retaguarda
enum FonteFoto { camera, galeria }

class Captura {
  final String id;
  final String torneioId;
  final String itemId;
  final String nomeItem;
  final String membroId;
  final String nomeMembro;
  final String equipeId;
  final String nomeEquipe;
  final double tamanhoMedida;
  final double fatorMultiplicador;
  final double pontuacao;
  final String? fotoUrl;
  final DateTime dataHora;
  final bool pendenteSync;
  final bool invalidada;
  final String? motivoInvalidacao;

  const Captura({
    required this.id,
    required this.torneioId,
    required this.itemId,
    required this.nomeItem,
    required this.membroId,
    required this.nomeMembro,
    required this.equipeId,
    required this.nomeEquipe,
    required this.tamanhoMedida,
    required this.fatorMultiplicador,
    required this.pontuacao,
    this.fotoUrl,
    required this.dataHora,
    required this.pendenteSync,
    this.invalidada = false,
    this.motivoInvalidacao,
  });

  factory Captura.fromJson(Map<String, dynamic> json) => Captura(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        itemId: json['itemId'] as String,
        nomeItem: json['nomeItem'] as String,
        membroId: json['membroId'] as String,
        nomeMembro: json['nomeMembro'] as String,
        equipeId: json['equipeId'] as String,
        nomeEquipe: json['nomeEquipe'] as String,
        tamanhoMedida: (json['tamanhoMedida'] as num).toDouble(),
        fatorMultiplicador: (json['fatorMultiplicador'] as num).toDouble(),
        pontuacao: (json['pontuacao'] as num).toDouble(),
        fotoUrl: AppConfig.resolverUrl(json['fotoUrl'] as String?),
        dataHora: DateTime.parse(json['dataHora'] as String),
        pendenteSync: json['pendenteSync'] as bool? ?? false,
        invalidada: json['invalidada'] as bool? ?? false,
        motivoInvalidacao: json['motivoInvalidacao'] as String?,
      );
}

class RegistrarCapturaRequest {
  final String torneioId;
  final String itemId;
  final String membroId;
  final String equipeId;
  final double tamanhoMedida;
  final String fotoUrl;
  final DateTime dataHora;
  final bool pendenteSync;
  final int? fonteFoto; // 0 = Camera, 1 = Galeria, null = sem foto/retaguarda

  const RegistrarCapturaRequest({
    required this.torneioId,
    required this.itemId,
    required this.membroId,
    required this.equipeId,
    required this.tamanhoMedida,
    required this.fotoUrl,
    required this.dataHora,
    this.pendenteSync = false,
    this.fonteFoto,
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
        if (fonteFoto != null) 'fonteFoto': fonteFoto,
      };

  Map<String, dynamic> toDbMap(String localId) => {
        'id': localId,
        ...toJson(),
        'tamanhoMedida': tamanhoMedida,
        'pendenteSync': pendenteSync ? 1 : 0,
      };
}
