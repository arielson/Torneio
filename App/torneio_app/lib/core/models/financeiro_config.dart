class FinanceiroConfig {
  final String torneioId;
  final double valorPorMembro;
  final int quantidadeParcelas;
  final DateTime? dataPrimeiroVencimento;
  final double taxaInscricaoValor;
  final DateTime? dataVencimentoTaxaInscricao;
  final bool possuiConfiguracaoAnterior;

  const FinanceiroConfig({
    required this.torneioId,
    required this.valorPorMembro,
    required this.quantidadeParcelas,
    this.dataPrimeiroVencimento,
    this.taxaInscricaoValor = 0,
    this.dataVencimentoTaxaInscricao,
    this.possuiConfiguracaoAnterior = false,
  });

  factory FinanceiroConfig.fromJson(Map<String, dynamic> json) => FinanceiroConfig(
        torneioId: json['torneioId'] as String,
        valorPorMembro: (json['valorPorMembro'] as num?)?.toDouble() ?? 0,
        quantidadeParcelas: json['quantidadeParcelas'] as int? ?? 0,
        dataPrimeiroVencimento: json['dataPrimeiroVencimento'] != null
            ? DateTime.tryParse(json['dataPrimeiroVencimento'] as String)
            : null,
        taxaInscricaoValor: (json['taxaInscricaoValor'] as num?)?.toDouble() ?? 0,
        dataVencimentoTaxaInscricao: json['dataVencimentoTaxaInscricao'] != null
            ? DateTime.tryParse(json['dataVencimentoTaxaInscricao'] as String)
            : null,
        possuiConfiguracaoAnterior: json['possuiConfiguracaoAnterior'] as bool? ?? false,
      );
}
