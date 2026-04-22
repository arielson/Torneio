import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/indicadores_financeiros.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';

class FinanceiroAdminScreen extends StatefulWidget {
  const FinanceiroAdminScreen({super.key});

  @override
  State<FinanceiroAdminScreen> createState() => _FinanceiroAdminScreenState();
}

class _FinanceiroAdminScreenState extends State<FinanceiroAdminScreen> {
  final _api = ApiService();
  bool _carregando = true;
  String? _erro;
  IndicadoresFinanceiros? _indicadores;

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
      final indicadoresData = await _api.get(
        ApiConstants.financeiroIndicadores(auth!.slug!),
        token: auth.token,
      );
      final indicadores = IndicadoresFinanceiros.fromJson(indicadoresData as Map<String, dynamic>);

      if (!mounted) return;
      setState(() {
        _indicadores = indicadores;
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar dados financeiros.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final moeda = NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$');

    return Scaffold(
      appBar: AppBar(title: const Text('Visão geral')),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  if (_erro != null)
                    Padding(
                      padding: const EdgeInsets.only(bottom: 16),
                      child: Text(_erro!, textAlign: TextAlign.center),
                    ),
                  if (_indicadores != null)
                    Wrap(
                      spacing: 12,
                      runSpacing: 12,
                      children: [
                        _IndicadorCard(titulo: 'Pescadores', valor: '${_indicadores!.quantidadeMembros}'),
                        _IndicadorCard(titulo: 'Embarcações', valor: '${_indicadores!.quantidadeEquipes}'),
                        _IndicadorCard(
                          titulo: 'Arrecadação prevista',
                          valor: moeda.format(_indicadores!.arrecadacaoPrevista),
                        ),
                        _IndicadorCard(
                          titulo: 'Custo total',
                          valor: moeda.format(_indicadores!.custoTotalTorneio),
                        ),
                        _IndicadorCard(
                          titulo: 'Taxa de inscrição',
                          valor: moeda.format(_indicadores!.taxaInscricaoValor),
                        ),
                        _IndicadorCard(
                          titulo: 'Saldo projetado',
                          valor: moeda.format(_indicadores!.saldoProjetado),
                          destaque: _indicadores!.saldoProjetado >= 0 ? Colors.green : Colors.red,
                        ),
                        _IndicadorCard(
                          titulo: 'Cobranças inadimplentes',
                          valor: '${_indicadores!.parcelasInadimplentes}',
                          destaque: Colors.red,
                        ),
                        _IndicadorCard(
                          titulo: 'Valor em aberto',
                          valor: moeda.format(_indicadores!.valorEmAberto),
                          destaque: Colors.orange,
                        ),
                        _IndicadorCard(
                          titulo: 'Custos lançados',
                          valor: '${_indicadores!.quantidadeCustosLancados}',
                        ),
                        _IndicadorCard(
                          titulo: 'Produtos extras',
                          valor: '${_indicadores!.quantidadeProdutosExtras}',
                        ),
                        _IndicadorCard(
                          titulo: 'Receita extras',
                          valor: moeda.format(_indicadores!.receitaExtrasPrevista),
                        ),
                        _IndicadorCard(
                          titulo: 'Doações',
                          valor: '${_indicadores!.quantidadeDoacoesPatrocinadores}',
                        ),
                        _IndicadorCard(
                          titulo: 'Receita doações',
                          valor: moeda.format(_indicadores!.receitaDoacoesPatrocinadores),
                        ),
                      ],
                    ),
                  const SizedBox(height: 16),
                  Card(
                    child: Column(
                      children: [
                        ListTile(
                          leading: const Icon(Icons.tune_outlined),
                          title: const Text('Configuração'),
                          subtitle: const Text('Defina valores, taxa de inscrição e quantidade de parcelas.'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.pushNamed(context, '/admin/financeiro/configuracao'),
                        ),
                        const Divider(height: 1),
                        ListTile(
                          leading: const Icon(Icons.receipt_long_outlined),
                          title: const Text('Cobranças'),
                          subtitle: const Text('Gerencie vencimentos, pagamentos, comprovantes e inadimplência.'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.pushNamed(context, '/admin/financeiro/cobrancas'),
                        ),
                        const Divider(height: 1),
                        ListTile(
                          leading: const Icon(Icons.payments_outlined),
                          title: const Text('Custos'),
                          subtitle: const Text('Lance custos do torneio e acompanhe os custos das embarcações.'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.pushNamed(context, '/admin/financeiro/custos'),
                        ),
                        const Divider(height: 1),
                        ListTile(
                          leading: const Icon(Icons.shopping_bag_outlined),
                          title: const Text('Produtos extras'),
                          subtitle: const Text('Cadastre produtos opcionais e registre vendas por pescador.'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.pushNamed(context, '/admin/financeiro/extras'),
                        ),
                        const Divider(height: 1),
                        ListTile(
                          leading: const Icon(Icons.card_giftcard_outlined),
                          title: const Text('Doações'),
                          subtitle: const Text('Registre doações de patrocinadores em dinheiro ou produtos.'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.pushNamed(context, '/admin/financeiro/doacoes'),
                        ),
                        const Divider(height: 1),
                        ListTile(
                          leading: const Icon(Icons.checklist_outlined),
                          title: const Text('Checklist'),
                          subtitle: const Text('Acompanhe as pendencias operacionais do torneio.'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.pushNamed(context, '/admin/financeiro/checklist'),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
    );
  }

}

class _IndicadorCard extends StatelessWidget {
  final String titulo;
  final String valor;
  final Color? destaque;

  const _IndicadorCard({
    required this.titulo,
    required this.valor,
    this.destaque,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 170,
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(titulo, style: Theme.of(context).textTheme.bodySmall),
              const SizedBox(height: 8),
              Text(
                valor,
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                      color: destaque,
                    ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
