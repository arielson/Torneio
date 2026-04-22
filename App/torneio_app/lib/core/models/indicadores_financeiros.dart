class IndicadoresFinanceiros {
  final int quantidadeMembros;
  final int quantidadeEquipes;
  final int quantidadeAdministradores;
  final double custoTotalTorneio;
  final double valorPorMembro;
  final double taxaInscricaoValor;
  final int quantidadeParcelas;
  final double arrecadacaoPrevista;
  final double receitaPrevista;
  final double saldoProjetado;
  final int parcelasInadimplentes;
  final double valorEmAberto;
  final int embarcacoesConfirmadas;
  final int quantidadeCustosLancados;
  final int quantidadeProdutosExtras;
  final double receitaExtrasPrevista;
  final int quantidadeDoacoesPatrocinadores;
  final double receitaDoacoesPatrocinadores;

  const IndicadoresFinanceiros({
    required this.quantidadeMembros,
    required this.quantidadeEquipes,
    required this.quantidadeAdministradores,
    required this.custoTotalTorneio,
    required this.valorPorMembro,
    required this.taxaInscricaoValor,
    required this.quantidadeParcelas,
    required this.arrecadacaoPrevista,
    required this.receitaPrevista,
    required this.saldoProjetado,
    required this.parcelasInadimplentes,
    required this.valorEmAberto,
    required this.embarcacoesConfirmadas,
    required this.quantidadeCustosLancados,
    required this.quantidadeProdutosExtras,
    required this.receitaExtrasPrevista,
    required this.quantidadeDoacoesPatrocinadores,
    required this.receitaDoacoesPatrocinadores,
  });

  factory IndicadoresFinanceiros.fromJson(Map<String, dynamic> json) => IndicadoresFinanceiros(
        quantidadeMembros: json['quantidadeMembros'] as int? ?? 0,
        quantidadeEquipes: json['quantidadeEquipes'] as int? ?? 0,
        quantidadeAdministradores: json['quantidadeAdministradores'] as int? ?? 0,
        custoTotalTorneio: (json['custoTotalTorneio'] as num?)?.toDouble() ?? 0,
        valorPorMembro: (json['valorPorMembro'] as num?)?.toDouble() ?? 0,
        taxaInscricaoValor: (json['taxaInscricaoValor'] as num?)?.toDouble() ?? 0,
        quantidadeParcelas: json['quantidadeParcelas'] as int? ?? 0,
        arrecadacaoPrevista: (json['arrecadacaoPrevista'] as num?)?.toDouble() ?? 0,
        receitaPrevista: (json['receitaPrevista'] as num?)?.toDouble() ?? 0,
        saldoProjetado: (json['saldoProjetado'] as num?)?.toDouble() ?? 0,
        parcelasInadimplentes: json['parcelasInadimplentes'] as int? ?? 0,
        valorEmAberto: (json['valorEmAberto'] as num?)?.toDouble() ?? 0,
        embarcacoesConfirmadas: json['embarcacoesConfirmadas'] as int? ?? 0,
        quantidadeCustosLancados: json['quantidadeCustosLancados'] as int? ?? 0,
        quantidadeProdutosExtras: json['quantidadeProdutosExtras'] as int? ?? 0,
        receitaExtrasPrevista: (json['receitaExtrasPrevista'] as num?)?.toDouble() ?? 0,
        quantidadeDoacoesPatrocinadores: json['quantidadeDoacoesPatrocinadores'] as int? ?? 0,
        receitaDoacoesPatrocinadores: (json['receitaDoacoesPatrocinadores'] as num?)?.toDouble() ?? 0,
      );
}
