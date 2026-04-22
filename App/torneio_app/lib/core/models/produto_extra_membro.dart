class ProdutoExtraMembro {
  final String id;
  final String torneioId;
  final String produtoExtraTorneioId;
  final String membroId;
  final String nomeMembro;
  final double quantidade;
  final double valorCobrado;
  final String? observacao;
  final bool ativo;
  final String? parcelaId;
  final bool pago;
  final DateTime? dataPagamento;
  final bool inadimplente;

  const ProdutoExtraMembro({
    required this.id,
    required this.torneioId,
    required this.produtoExtraTorneioId,
    required this.membroId,
    required this.nomeMembro,
    required this.quantidade,
    required this.valorCobrado,
    this.observacao,
    required this.ativo,
    this.parcelaId,
    required this.pago,
    this.dataPagamento,
    required this.inadimplente,
  });

  factory ProdutoExtraMembro.fromJson(Map<String, dynamic> json) => ProdutoExtraMembro(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        produtoExtraTorneioId: json['produtoExtraTorneioId'] as String,
        membroId: json['membroId'] as String,
        nomeMembro: json['nomeMembro'] as String? ?? '',
        quantidade: (json['quantidade'] as num?)?.toDouble() ?? 1,
        valorCobrado: (json['valorCobrado'] as num?)?.toDouble() ?? 0,
        observacao: json['observacao'] as String?,
        ativo: json['ativo'] as bool? ?? true,
        parcelaId: json['parcelaId'] as String?,
        pago: json['pago'] as bool? ?? false,
        dataPagamento: json['dataPagamento'] != null
            ? DateTime.tryParse(json['dataPagamento'] as String)
            : null,
        inadimplente: json['inadimplente'] as bool? ?? false,
      );
}
