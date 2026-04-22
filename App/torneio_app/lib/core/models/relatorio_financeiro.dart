import 'custo_torneio.dart';
import 'indicadores_financeiros.dart';
import 'parcela_torneio.dart';

class RelatorioFinanceiro {
  final IndicadoresFinanceiros indicadores;
  final List<FluxoFinanceiroLinha> fluxoCaixaProjetado;
  final List<ResumoFinanceiroPorTipo> receitasPorTipo;
  final List<ResumoFinanceiroPorCategoria> custosPorCategoria;
  final List<ParcelaTorneio> proximosRecebimentosPendentes;
  final List<CustoTorneio> proximosPagamentosPendentes;

  const RelatorioFinanceiro({
    required this.indicadores,
    required this.fluxoCaixaProjetado,
    required this.receitasPorTipo,
    required this.custosPorCategoria,
    required this.proximosRecebimentosPendentes,
    required this.proximosPagamentosPendentes,
  });

  factory RelatorioFinanceiro.fromJson(Map<String, dynamic> json) => RelatorioFinanceiro(
        indicadores: IndicadoresFinanceiros.fromJson(json['indicadores'] as Map<String, dynamic>? ?? const {}),
        fluxoCaixaProjetado: (json['fluxoCaixaProjetado'] as List<dynamic>? ?? [])
            .map((e) => FluxoFinanceiroLinha.fromJson(e as Map<String, dynamic>))
            .toList(),
        receitasPorTipo: (json['receitasPorTipo'] as List<dynamic>? ?? [])
            .map((e) => ResumoFinanceiroPorTipo.fromJson(e as Map<String, dynamic>))
            .toList(),
        custosPorCategoria: (json['custosPorCategoria'] as List<dynamic>? ?? [])
            .map((e) => ResumoFinanceiroPorCategoria.fromJson(e as Map<String, dynamic>))
            .toList(),
        proximosRecebimentosPendentes: (json['proximosRecebimentosPendentes'] as List<dynamic>? ?? [])
            .map((e) => ParcelaTorneio.fromJson(e as Map<String, dynamic>))
            .toList(),
        proximosPagamentosPendentes: (json['proximosPagamentosPendentes'] as List<dynamic>? ?? [])
            .map((e) => CustoTorneio.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class FluxoFinanceiroLinha {
  final DateTime data;
  final double recebimentosPrevistos;
  final double pagamentosPrevistos;
  final double saldoDiario;
  final double saldoAcumulado;

  const FluxoFinanceiroLinha({
    required this.data,
    required this.recebimentosPrevistos,
    required this.pagamentosPrevistos,
    required this.saldoDiario,
    required this.saldoAcumulado,
  });

  factory FluxoFinanceiroLinha.fromJson(Map<String, dynamic> json) => FluxoFinanceiroLinha(
        data: DateTime.tryParse(json['data']?.toString() ?? '') ?? DateTime.now(),
        recebimentosPrevistos: (json['recebimentosPrevistos'] as num?)?.toDouble() ?? 0,
        pagamentosPrevistos: (json['pagamentosPrevistos'] as num?)?.toDouble() ?? 0,
        saldoDiario: (json['saldoDiario'] as num?)?.toDouble() ?? 0,
        saldoAcumulado: (json['saldoAcumulado'] as num?)?.toDouble() ?? 0,
      );
}

class ResumoFinanceiroPorTipo {
  final String chave;
  final String label;
  final int quantidade;
  final double total;
  final double pago;
  final double emAberto;

  const ResumoFinanceiroPorTipo({
    required this.chave,
    required this.label,
    required this.quantidade,
    required this.total,
    required this.pago,
    required this.emAberto,
  });

  factory ResumoFinanceiroPorTipo.fromJson(Map<String, dynamic> json) => ResumoFinanceiroPorTipo(
        chave: json['chave'] as String? ?? '',
        label: json['label'] as String? ?? '',
        quantidade: json['quantidade'] as int? ?? 0,
        total: (json['total'] as num?)?.toDouble() ?? 0,
        pago: (json['pago'] as num?)?.toDouble() ?? 0,
        emAberto: (json['emAberto'] as num?)?.toDouble() ?? 0,
      );
}

class ResumoFinanceiroPorCategoria {
  final String chave;
  final String label;
  final int quantidade;
  final double total;

  const ResumoFinanceiroPorCategoria({
    required this.chave,
    required this.label,
    required this.quantidade,
    required this.total,
  });

  factory ResumoFinanceiroPorCategoria.fromJson(Map<String, dynamic> json) => ResumoFinanceiroPorCategoria(
        chave: json['chave'] as String? ?? '',
        label: json['label'] as String? ?? '',
        quantidade: json['quantidade'] as int? ?? 0,
        total: (json['total'] as num?)?.toDouble() ?? 0,
      );
}
