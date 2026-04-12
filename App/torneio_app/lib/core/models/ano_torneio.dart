class AnoTorneio {
  final String id;
  final String torneioId;
  final int ano;
  final String status; // "Aberto" | "Liberado" | "Finalizado"

  const AnoTorneio({
    required this.id,
    required this.torneioId,
    required this.ano,
    required this.status,
  });

  factory AnoTorneio.fromJson(Map<String, dynamic> json) => AnoTorneio(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        ano: json['ano'] as int,
        status: json['status'] as String,
      );

  bool get isLiberado => status == 'Liberado';
}
