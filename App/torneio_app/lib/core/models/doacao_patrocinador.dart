class DoacaoPatrocinador {
  final String id;
  final String torneioId;
  final String? patrocinadorId;
  final String nomePatrocinador;
  final String tipo;
  final String descricao;
  final double? quantidade;
  final double? valor;
  final String? observacao;
  final DateTime dataDoacao;

  const DoacaoPatrocinador({
    required this.id,
    required this.torneioId,
    this.patrocinadorId,
    required this.nomePatrocinador,
    required this.tipo,
    required this.descricao,
    this.quantidade,
    this.valor,
    this.observacao,
    required this.dataDoacao,
  });

  bool get geraReceita => tipo.toLowerCase() == 'dinheiro' && (valor ?? 0) > 0;

  factory DoacaoPatrocinador.fromJson(Map<String, dynamic> json) => DoacaoPatrocinador(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        patrocinadorId: json['patrocinadorId'] as String?,
        nomePatrocinador: json['nomePatrocinador'] as String? ?? '',
        tipo: json['tipo'] as String? ?? 'Dinheiro',
        descricao: json['descricao'] as String? ?? '',
        quantidade: (json['quantidade'] as num?)?.toDouble(),
        valor: (json['valor'] as num?)?.toDouble(),
        observacao: json['observacao'] as String?,
        dataDoacao: DateTime.tryParse(json['dataDoacao'] as String? ?? '') ?? DateTime.now(),
      );
}
