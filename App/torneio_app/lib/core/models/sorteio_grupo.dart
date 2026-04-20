class SorteioGrupo {
  final String id;
  final String grupoId;
  final String nomeGrupo;
  final String equipeId;
  final String nomeEquipe;
  final int posicao;
  final List<String> nomesMembros;

  const SorteioGrupo({
    required this.id,
    required this.grupoId,
    required this.nomeGrupo,
    required this.equipeId,
    required this.nomeEquipe,
    required this.posicao,
    required this.nomesMembros,
  });

  factory SorteioGrupo.fromJson(Map<String, dynamic> json) => SorteioGrupo(
        id: json['id'] as String,
        grupoId: json['grupoId'] as String,
        nomeGrupo: json['nomeGrupo'] as String? ?? '',
        equipeId: json['equipeId'] as String,
        nomeEquipe: json['nomeEquipe'] as String? ?? '',
        posicao: json['posicao'] as int,
        nomesMembros: (json['nomesMembros'] as List<dynamic>? ?? [])
            .map((n) => n as String)
            .toList(),
      );
}
