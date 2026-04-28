class Premio {
  final String id;
  final String torneioId;
  final int posicao;
  final String descricao;

  const Premio({
    required this.id,
    required this.torneioId,
    required this.posicao,
    required this.descricao,
  });

  factory Premio.fromJson(Map<String, dynamic> json) => Premio(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        posicao: json['posicao'] as int? ?? 0,
        descricao: json['descricao'] as String? ?? '',
      );
}
