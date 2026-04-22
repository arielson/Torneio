class ProdutoExtraTorneio {
  final String id;
  final String torneioId;
  final String nome;
  final String? descricao;
  final double valor;
  final bool ativo;
  final int quantidadeAderidos;

  const ProdutoExtraTorneio({
    required this.id,
    required this.torneioId,
    required this.nome,
    this.descricao,
    required this.valor,
    required this.ativo,
    required this.quantidadeAderidos,
  });

  factory ProdutoExtraTorneio.fromJson(Map<String, dynamic> json) => ProdutoExtraTorneio(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        nome: json['nome'] as String? ?? '',
        descricao: json['descricao'] as String?,
        valor: (json['valor'] as num?)?.toDouble() ?? 0,
        ativo: json['ativo'] as bool? ?? true,
        quantidadeAderidos: json['quantidadeAderidos'] as int? ?? 0,
      );
}
