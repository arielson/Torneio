import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/parcela_torneio.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class CobrancasMembroScreen extends StatefulWidget {
  const CobrancasMembroScreen({super.key});

  @override
  State<CobrancasMembroScreen> createState() => _CobrancasMembroScreenState();
}

class _CobrancasMembroScreenState extends State<CobrancasMembroScreen> {
  final _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<ParcelaTorneio> _cobrancas = const [];

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
      final data = await _api.get(
        ApiConstants.membroCobrancas(auth!.slug!),
        token: auth.token,
      );

      if (!mounted) return;
      setState(() {
        _cobrancas = data is List
            ? data.map((e) => ParcelaTorneio.fromJson(e as Map<String, dynamic>)).toList()
            : <ParcelaTorneio>[];
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar cobrancas.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _logout() async {
    await context.read<AuthProvider>().logout();
    if (!mounted) return;
    Navigator.pushNamedAndRemoveUntil(context, '/home', (_) => false);
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>().usuario;
    final config = context.watch<ConfigProvider>().config;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Minhas cobrancas'),
        actions: [
          IconButton(
            onPressed: _logout,
            icon: const Icon(Icons.logout),
            tooltip: 'Sair',
          ),
        ],
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  if (auth != null)
                    Card(
                      child: ListTile(
                        title: Text(auth.nome),
                        subtitle: Text(config?.nomeTorneio ?? ''),
                        leading: const CircleAvatar(child: Icon(Icons.person)),
                      ),
                    ),
                  if (_erro != null)
                    Padding(
                      padding: const EdgeInsets.only(top: 24),
                      child: Text(_erro!, textAlign: TextAlign.center),
                    )
                  else if (_cobrancas.isEmpty)
                    const Padding(
                      padding: EdgeInsets.only(top: 24),
                      child: Text('Nenhuma cobranca encontrada.', textAlign: TextAlign.center),
                    )
                  else
                    ...(_cobrancas.toList()..sort((a, b) => a.vencimento.compareTo(b.vencimento))).map(
                      (cobranca) => Card(
                        margin: const EdgeInsets.only(top: 12),
                        child: Padding(
                          padding: const EdgeInsets.all(16),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  Expanded(
                                    child: Text(
                                      cobranca.descricao,
                                      style: Theme.of(context).textTheme.titleMedium,
                                    ),
                                  ),
                                  _StatusChip(cobranca: cobranca),
                                ],
                              ),
                              const SizedBox(height: 8),
                              Text('Tipo: ${_tipoLabel(cobranca.tipoParcela)}'),
                              Text('Valor: ${cobranca.valorFormatado}'),
                              Text('Vencimento: ${DateFormat('dd/MM/yyyy').format(cobranca.vencimento)}'),
                              if (cobranca.dataPagamento != null)
                                Text('Data de pagamento: ${DateFormat('dd/MM/yyyy').format(cobranca.dataPagamento!)}'),
                              if ((cobranca.observacao ?? '').trim().isNotEmpty) ...[
                                const SizedBox(height: 6),
                                Text('Observacao: ${cobranca.observacao}'),
                              ],
                            ],
                          ),
                        ),
                      ),
                    ),
                ],
              ),
            ),
    );
  }

  String _tipoLabel(String tipoParcela) => switch (tipoParcela) {
        'Mensalidade' => 'Parcela',
        'TaxaInscricao' => 'Inscricao',
        'ProdutoExtra' => 'Produto extra',
        _ => tipoParcela,
      };
}

class _StatusChip extends StatelessWidget {
  final ParcelaTorneio cobranca;

  const _StatusChip({required this.cobranca});

  @override
  Widget build(BuildContext context) {
    if (cobranca.pago) {
      return const Chip(label: Text('Pago'), backgroundColor: Color(0xFFDFF5E1));
    }
    if (cobranca.inadimplente) {
      return const Chip(label: Text('Inadimplente'), backgroundColor: Color(0xFFFDE2E1));
    }
    return const Chip(label: Text('Em aberto'), backgroundColor: Color(0xFFFFF1D6));
  }
}
