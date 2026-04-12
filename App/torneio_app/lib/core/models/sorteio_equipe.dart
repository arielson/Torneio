class SorteioEquipe {
  final String id;
  final String equipeId;
  final String nomeEquipe;
  final String membroId;
  final String nomeMembro;
  final int posicao;

  const SorteioEquipe({
    required this.id,
    required this.equipeId,
    required this.nomeEquipe,
    required this.membroId,
    required this.nomeMembro,
    required this.posicao,
  });

  factory SorteioEquipe.fromJson(Map<String, dynamic> json) => SorteioEquipe(
        id: json['id'] as String,
        equipeId: json['equipeId'] as String,
        nomeEquipe: json['nomeEquipe'] as String,
        membroId: json['membroId'] as String,
        nomeMembro: json['nomeMembro'] as String,
        posicao: json['posicao'] as int,
      );
}
