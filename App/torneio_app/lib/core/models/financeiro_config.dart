class FinanceiroConfig {
  final String torneioId;
  final double valorPorMembro;
  final int quantidadeParcelas;
  final DateTime? dataPrimeiroVencimento;
  final double taxaInscricaoValor;
  final DateTime? dataVencimentoTaxaInscricao;
  final bool possuiConfiguracaoAnterior;
  final List<ValorParcelaFinanceira> valoresParcelas;

  const FinanceiroConfig({
    required this.torneioId,
    required this.valorPorMembro,
    required this.quantidadeParcelas,
    this.dataPrimeiroVencimento,
    this.taxaInscricaoValor = 0,
    this.dataVencimentoTaxaInscricao,
    this.possuiConfiguracaoAnterior = false,
    this.valoresParcelas = const [],
  });

  factory FinanceiroConfig.fromJson(Map<String, dynamic> json) => FinanceiroConfig(
        torneioId: json['torneioId'] as String,
        valorPorMembro: (json['valorPorMembro'] as num?)?.toDouble() ?? 0,
        quantidadeParcelas: json['quantidadeParcelas'] as int? ?? 0,
        dataPrimeiroVencimento: _parseDate(json['dataPrimeiroVencimento']),
        taxaInscricaoValor: (json['taxaInscricaoValor'] as num?)?.toDouble() ?? 0,
        dataVencimentoTaxaInscricao: _parseDate(json['dataVencimentoTaxaInscricao']),
        possuiConfiguracaoAnterior: json['possuiConfiguracaoAnterior'] as bool? ?? false,
        valoresParcelas: (json['valoresParcelas'] as List<dynamic>? ?? const [])
            .map((e) => ValorParcelaFinanceira.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

/// Parseia um valor de data vindo do JSON (String ISO 8601 ou null).
/// Aceita formatos com ou sem timezone, retornando sempre DateTime local.
DateTime? _parseDate(dynamic value) {
  if (value == null) return null;
  final raw = value as String;
  final dt = DateTime.tryParse(raw);
  if (dt != null) return dt.toLocal();
  // Fallback: tenta apenas a parte da data (yyyy-MM-dd)
  if (raw.length >= 10) return DateTime.tryParse(raw.substring(0, 10))?.toLocal();
  return null;
}

class ValorParcelaFinanceira {
  final int numeroParcela;
  final double valor;

  const ValorParcelaFinanceira({
    required this.numeroParcela,
    required this.valor,
  });

  factory ValorParcelaFinanceira.fromJson(Map<String, dynamic> json) => ValorParcelaFinanceira(
        numeroParcela: json['numeroParcela'] as int? ?? 0,
        valor: (json['valor'] as num?)?.toDouble() ?? 0,
      );
}
