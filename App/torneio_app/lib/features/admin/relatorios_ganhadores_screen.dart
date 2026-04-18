import 'package:flutter/material.dart';
import 'package:printing/printing.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/ganhador_relatorio.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class RelatoriosGanhadoresScreen extends StatefulWidget {
  const RelatoriosGanhadoresScreen({super.key});

  @override
  State<RelatoriosGanhadoresScreen> createState() => _RelatoriosGanhadoresScreenState();
}

class _RelatoriosGanhadoresScreenState extends State<RelatoriosGanhadoresScreen> {
  final ApiService _api = ApiService();
  late Future<List<GanhadorRelatorio>> _future;

  @override
  void initState() {
    super.initState();
    _future = _carregar();
  }

  Future<List<GanhadorRelatorio>> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return const [];

    final data = await _api.get(
      ApiConstants.relatoriosGanhadores(auth!.slug!),
      token: auth.token,
    );

    if (data is! List) return const [];
    return data
        .map((e) => GanhadorRelatorio.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<void> _abrirPdf(GanhadorRelatorio item, bool analitico) async {
    final auth = context.read<AuthProvider>().usuario;
    final slug = auth?.slug;
    final token = auth?.token;
    if (slug == null || token == null) return;

    final bytes = await _api.getBytes(
      ApiConstants.relatorioEquipePdf(slug, item.equipeId, analitico: analitico),
      token: token,
    );

    final tipo = analitico ? 'analitico' : 'sintetico';
    await Printing.layoutPdf(
      name: 'ganhador_${item.posicao}_$tipo.pdf',
      onLayout: (_) async => bytes,
    );
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelEquipe = config?.labelEquipe ?? 'Embarcação';
    final qtdGanhadores = config?.qtdGanhadores ?? 3;

    return Scaffold(
      appBar: AppBar(title: const Text('Relatórios dos Ganhadores')),
      body: FutureBuilder<List<GanhadorRelatorio>>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Text('Erro ao carregar ganhadores: ${snapshot.error}'),
              ),
            );
          }

          final itens = snapshot.data ?? const <GanhadorRelatorio>[];
          if (itens.isEmpty) {
            return const Center(
              child: Padding(
                padding: EdgeInsets.all(24),
                child: Text('Nenhum ganhador encontrado com base nas capturas atuais.'),
              ),
            );
          }

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(
                  'Top $qtdGanhadores $labelEquipe(s)',
                  style: Theme.of(context).textTheme.titleMedium,
                ),
              ),
              ...itens.map(
                (item) => Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '${item.posicao}º - ${item.nomeEquipe}',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                        const SizedBox(height: 4),
                        Text('Capitão: ${item.capitao}'),
                        Text('Pontos: ${item.totalPontos.toStringAsFixed(2)}'),
                        const SizedBox(height: 12),
                        Wrap(
                          spacing: 8,
                          runSpacing: 8,
                          children: [
                            OutlinedButton.icon(
                              onPressed: () => _abrirPdf(item, false),
                              icon: const Icon(Icons.picture_as_pdf_outlined),
                              label: const Text('Sintético'),
                            ),
                            FilledButton.icon(
                              onPressed: () => _abrirPdf(item, true),
                              icon: const Icon(Icons.picture_as_pdf),
                              label: const Text('Analítico'),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
