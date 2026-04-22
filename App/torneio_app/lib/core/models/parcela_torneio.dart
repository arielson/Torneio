import 'package:intl/intl.dart';

class ParcelaTorneio {
  final String id;
  final String torneioId;
  final String membroId;
  final String nomeMembro;
  final String tipoParcela;
  final String descricao;
  final int numeroParcela;
  final double valor;
  final DateTime vencimento;
  final bool vencimentoEditadoManual;
  final bool pago;
  final DateTime? dataPagamento;
  final String? observacao;
  final bool inadimplente;
  final String? comprovanteNomeArquivo;
  final DateTime? comprovanteDataUpload;
  final String? comprovanteUsuarioNome;
  final String? comprovanteUrl;
  final String? comprovanteContentType;

  const ParcelaTorneio({
    required this.id,
    required this.torneioId,
    required this.membroId,
    required this.nomeMembro,
    required this.tipoParcela,
    required this.descricao,
    required this.numeroParcela,
    required this.valor,
    required this.vencimento,
    required this.vencimentoEditadoManual,
    required this.pago,
    this.dataPagamento,
    this.observacao,
    required this.inadimplente,
    this.comprovanteNomeArquivo,
    this.comprovanteDataUpload,
    this.comprovanteUsuarioNome,
    this.comprovanteUrl,
    this.comprovanteContentType,
  });

  factory ParcelaTorneio.fromJson(Map<String, dynamic> json) => ParcelaTorneio(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        membroId: json['membroId'] as String,
        nomeMembro: json['nomeMembro'] as String? ?? '',
        tipoParcela: json['tipoParcela'] as String? ?? '',
        descricao: json['descricao'] as String? ?? '',
        numeroParcela: json['numeroParcela'] as int? ?? 0,
        valor: (json['valor'] as num?)?.toDouble() ?? 0,
        vencimento: DateTime.parse(json['vencimento'] as String),
        vencimentoEditadoManual: json['vencimentoEditadoManual'] as bool? ?? false,
        pago: json['pago'] as bool? ?? false,
        dataPagamento: json['dataPagamento'] != null
            ? DateTime.tryParse(json['dataPagamento'] as String)
            : null,
        observacao: json['observacao'] as String?,
        inadimplente: json['inadimplente'] as bool? ?? false,
        comprovanteNomeArquivo: json['comprovanteNomeArquivo'] as String?,
        comprovanteDataUpload: json['comprovanteDataUpload'] != null
            ? DateTime.tryParse(json['comprovanteDataUpload'] as String)
            : null,
        comprovanteUsuarioNome: json['comprovanteUsuarioNome'] as String?,
        comprovanteUrl: json['comprovanteUrl'] as String?,
        comprovanteContentType: json['comprovanteContentType'] as String?,
      );

  String get valorFormatado => NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$').format(valor);
}
