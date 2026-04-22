import 'dart:math' as math;
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../core/constants.dart';
import '../../core/models/relatorio_financeiro.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';

class FinanceiroRelatoriosScreen extends StatefulWidget {
  const FinanceiroRelatoriosScreen({super.key});

  @override
  State<FinanceiroRelatoriosScreen> createState() => _FinanceiroRelatoriosScreenState();
}

class _FinanceiroRelatoriosScreenState extends State<FinanceiroRelatoriosScreen> {
  final _api = ApiService();
  bool _carregando = true;
  String? _erro;
  RelatorioFinanceiro? _relatorio;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final data = await _api.get(ApiConstants.financeiroRelatorios(auth!.slug!), token: auth.token);
      if (!mounted) return;
      setState(() {
        _relatorio = RelatorioFinanceiro.fromJson(data as Map<String, dynamic>);
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar relatórios financeiros.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final moeda = NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$');

    return Scaffold(
      appBar: AppBar(title: const Text('Relatórios financeiros')),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  if (_erro != null)
                    Padding(
                      padding: const EdgeInsets.only(top: 24),
                      child: Text(_erro!, textAlign: TextAlign.center),
                    ),
                  if (_erro == null && _relatorio != null) ...[
                    Card(
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text('Fluxo projetado', style: Theme.of(context).textTheme.titleMedium),
                            const SizedBox(height: 8),
                            Text(
                              'Azul: recebimentos previstos. Vermelho: pagamentos previstos. Verde: saldo acumulado.',
                              style: Theme.of(context).textTheme.bodySmall,
                            ),
                            const SizedBox(height: 16),
                            SizedBox(
                              height: 240,
                              child: _relatorio!.fluxoCaixaProjetado.isEmpty
                                  ? const Center(
                                      child: Text('Ainda não há datas suficientes para montar a linha do tempo financeira.'),
                                    )
                                  : CustomPaint(
                                      painter: _FluxoFinanceiroPainter(_relatorio!.fluxoCaixaProjetado),
                                      child: const SizedBox.expand(),
                                    ),
                            ),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(height: 12),
                    _SecaoTabela(
                      titulo: 'Receitas por tipo',
                      child: Column(
                        children: _relatorio!.receitasPorTipo
                            .map(
                              (item) => ListTile(
                                contentPadding: EdgeInsets.zero,
                                title: Text(item.label),
                                subtitle: Text('${item.quantidade} cobrança(s)'),
                                trailing: Column(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  crossAxisAlignment: CrossAxisAlignment.end,
                                  children: [
                                    Text(moeda.format(item.total), style: const TextStyle(fontWeight: FontWeight.w700)),
                                    Text('Pago ${moeda.format(item.pago)}', style: Theme.of(context).textTheme.bodySmall),
                                    Text('Aberto ${moeda.format(item.emAberto)}', style: Theme.of(context).textTheme.bodySmall),
                                  ],
                                ),
                              ),
                            )
                            .toList(),
                      ),
                    ),
                    const SizedBox(height: 12),
                    _SecaoTabela(
                      titulo: 'Custos por categoria',
                      child: Column(
                        children: _relatorio!.custosPorCategoria
                            .map(
                              (item) => ListTile(
                                contentPadding: EdgeInsets.zero,
                                title: Text(item.label),
                                subtitle: Text('${item.quantidade} lançamento(s)'),
                                trailing: Text(
                                  moeda.format(item.total),
                                  style: const TextStyle(fontWeight: FontWeight.w700),
                                ),
                              ),
                            )
                            .toList(),
                      ),
                    ),
                    const SizedBox(height: 12),
                    _SecaoTabela(
                      titulo: 'Próximos recebimentos pendentes',
                      vazio: _relatorio!.proximosRecebimentosPendentes.isEmpty
                          ? 'Nenhuma cobrança pendente encontrada.'
                          : null,
                      child: Column(
                        children: _relatorio!.proximosRecebimentosPendentes
                            .map(
                              (item) => ListTile(
                                contentPadding: EdgeInsets.zero,
                                title: Text(item.nomeMembro),
                                subtitle: Text('${item.descricao}\nVencimento: ${DateFormat('dd/MM/yyyy').format(item.vencimento)}'),
                                isThreeLine: true,
                                trailing: Text(
                                  moeda.format(item.valor),
                                  style: const TextStyle(fontWeight: FontWeight.w700),
                                ),
                              ),
                            )
                            .toList(),
                      ),
                    ),
                    const SizedBox(height: 12),
                    _SecaoTabela(
                      titulo: 'Próximos pagamentos previstos',
                      vazio: _relatorio!.proximosPagamentosPendentes.isEmpty
                          ? 'Nenhum pagamento com vencimento informado.'
                          : null,
                      child: Column(
                        children: _relatorio!.proximosPagamentosPendentes
                            .map(
                              (item) => ListTile(
                                contentPadding: EdgeInsets.zero,
                                title: Text(item.categoriaLabel),
                                subtitle: Text(
                                  '${item.descricao}\nVencimento: ${item.vencimento == null ? '-' : DateFormat('dd/MM/yyyy').format(item.vencimento!)}',
                                ),
                                isThreeLine: true,
                                trailing: Text(
                                  moeda.format(item.valorTotal),
                                  style: const TextStyle(fontWeight: FontWeight.w700),
                                ),
                              ),
                            )
                            .toList(),
                      ),
                    ),
                  ],
                ],
              ),
            ),
    );
  }
}

class _SecaoTabela extends StatelessWidget {
  final String titulo;
  final Widget child;
  final String? vazio;

  const _SecaoTabela({
    required this.titulo,
    required this.child,
    this.vazio,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(titulo, style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 12),
            if (vazio != null) Text(vazio!) else child,
          ],
        ),
      ),
    );
  }
}

class _FluxoFinanceiroPainter extends CustomPainter {
  final List<FluxoFinanceiroLinha> pontos;

  _FluxoFinanceiroPainter(this.pontos);

  @override
  void paint(Canvas canvas, Size size) {
    if (pontos.isEmpty) return;

    const margemEsquerda = 28.0;
    const margemTopo = 12.0;
    const margemDireita = 12.0;
    const margemBase = 28.0;
    final largura = size.width - margemEsquerda - margemDireita;
    final altura = size.height - margemTopo - margemBase;
    final valores = pontos
        .expand((e) => [e.recebimentosPrevistos, e.pagamentosPrevistos, e.saldoAcumulado])
        .toList();
    final minValor = math.min(0, valores.reduce(math.min));
    final maxValor = math.max(1, valores.reduce(math.max));
    final faixa = (maxValor - minValor).abs() < 0.01 ? 1.0 : (maxValor - minValor);

    final eixoPaint = Paint()
      ..color = const Color(0xFFCBD5E1)
      ..strokeWidth = 1;
    canvas.drawLine(
      Offset(margemEsquerda, margemTopo + altura),
      Offset(margemEsquerda + largura, margemTopo + altura),
      eixoPaint,
    );
    canvas.drawLine(
      const Offset(margemEsquerda, margemTopo),
      Offset(margemEsquerda, margemTopo + altura),
      eixoPaint,
    );

    Path montarPath(double Function(FluxoFinanceiroLinha item) valor) {
      final path = Path();
      for (var i = 0; i < pontos.length; i++) {
        final x = pontos.length == 1 ? margemEsquerda + largura / 2 : margemEsquerda + (largura * i / (pontos.length - 1));
        final normalizado = (valor(pontos[i]) - minValor) / faixa;
        final y = margemTopo + altura - (normalizado * altura);
        if (i == 0) {
          path.moveTo(x, y);
        } else {
          path.lineTo(x, y);
        }
      }
      return path;
    }

    canvas.drawPath(
      montarPath((item) => item.recebimentosPrevistos),
      Paint()
        ..color = const Color(0xFF2563EB)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3,
    );
    canvas.drawPath(
      montarPath((item) => item.pagamentosPrevistos),
      Paint()
        ..color = const Color(0xFFDC2626)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3,
    );
    canvas.drawPath(
      montarPath((item) => item.saldoAcumulado),
      Paint()
        ..color = const Color(0xFF16A34A)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3,
    );

    final labelStyle = const TextStyle(color: Color(0xFF475569), fontSize: 10);
    for (var i = 0; i < pontos.length; i++) {
      final x = pontos.length == 1 ? margemEsquerda + largura / 2 : margemEsquerda + (largura * i / (pontos.length - 1));
      final texto = TextPainter(
        text: TextSpan(text: DateFormat('dd/MM').format(pontos[i].data), style: labelStyle),
        textDirection: ui.TextDirection.ltr,
      )..layout();
      texto.paint(canvas, Offset(x - (texto.width / 2), margemTopo + altura + 6));
    }
  }

  @override
  bool shouldRepaint(covariant _FluxoFinanceiroPainter oldDelegate) => oldDelegate.pontos != pontos;
}
