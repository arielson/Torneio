import 'package:intl/intl.dart';

class CustoTorneio {
  final String id;
  final String torneioId;
  final String categoria;
  final String descricao;
  final double quantidade;
  final double valorUnitario;
  final double valorTotal;
  final String categoriaLabel;
  final DateTime? vencimento;
  final String? responsavel;
  final String? observacao;
  final bool derivadoDaEmbarcacao;
  final String? equipeId;

  const CustoTorneio({
    required this.id,
    required this.torneioId,
    required this.categoria,
    required this.descricao,
    required this.quantidade,
    required this.valorUnitario,
    required this.valorTotal,
    required this.categoriaLabel,
    this.vencimento,
    this.responsavel,
    this.observacao,
    required this.derivadoDaEmbarcacao,
    this.equipeId,
  });

  factory CustoTorneio.fromJson(Map<String, dynamic> json) => CustoTorneio(
        id: json['id'] as String? ?? '',
        torneioId: json['torneioId'] as String,
        categoria: json['categoria'] as String? ?? 'Outros',
        descricao: json['descricao'] as String? ?? '',
        quantidade: (json['quantidade'] as num?)?.toDouble() ?? 0,
        valorUnitario: (json['valorUnitario'] as num?)?.toDouble() ?? 0,
        valorTotal: (json['valorTotal'] as num?)?.toDouble() ?? 0,
        categoriaLabel: json['categoriaLabel'] as String? ?? _labelCategoria(json['categoria'] as String?),
        vencimento: json['vencimento'] == null ? null : DateTime.tryParse(json['vencimento'].toString()),
        responsavel: json['responsavel'] as String?,
        observacao: json['observacao'] as String?,
        derivadoDaEmbarcacao: json['derivadoDaEmbarcacao'] as bool? ?? false,
        equipeId: json['equipeId'] as String?,
      );

  String get valorTotalFormatado => NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$').format(valorTotal);

  static String _labelCategoria(String? categoria) => switch (categoria) {
        'Embarcacao' => 'Embarcação',
        'Camisas' => 'Camisas',
        'Alimentacao' => 'Alimentação',
        'Combustivel' => 'Combustível',
        'Premiacoes' => 'Premiações',
        _ => 'Outros',
      };
}
