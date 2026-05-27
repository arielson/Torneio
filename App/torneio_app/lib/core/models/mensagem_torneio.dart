class MensagemTorneio {
  final String id;
  final String torneioId;
  final String titulo;
  final String corpo;
  final String criadoPor;
  final DateTime criadoEm;

  const MensagemTorneio({
    required this.id,
    required this.torneioId,
    required this.titulo,
    required this.corpo,
    required this.criadoPor,
    required this.criadoEm,
  });

  factory MensagemTorneio.fromJson(Map<String, dynamic> json) => MensagemTorneio(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        titulo: json['titulo'] as String,
        corpo: json['corpo'] as String,
        criadoPor: json['criadoPor'] as String,
        criadoEm: DateTime.tryParse(json['criadoEm'] as String? ?? '')?.toLocal() ?? DateTime.now(),
      );
}
