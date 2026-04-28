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
  late Future<GanhadoresResponse> _future;

  @override
  void initState() {
    super.initState();
    _future = _carregar();
  }

  Future<GanhadoresResponse> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) {
      return GanhadoresResponse(
        premiacaoPorEquipe: false,
        premiacaoPorMembro: false,
        equipesGanhadoras: const [],
        membrosGanhadores: const [],
      );
    }

    final data = await _api.get(
      ApiConstants.relatoriosGanhadores(auth!.slug!),
      token: auth.token,
    );

    if (data is! Map<String, dynamic>) {
      return GanhadoresResponse(
        premiacaoPorEquipe: false,
        premiacaoPorMembro: false,
        equipesGanhadoras: const [],
        membrosGanhadores: const [],
      );
    }
    return GanhadoresResponse.fromJson(data);
  }

  Future<void> _abrirPdfEquipe(GanhadorEquipe item, bool analitico) async {
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
      name: 'ganhador_equipe_${item.posicao}_$tipo.pdf',
      onLayout: (_) async => bytes,
    );
  }

  Future<void> _abrirPdfMembro(GanhadorMembro item, bool analitico) async {
    final auth = context.read<AuthProvider>().usuario;
    final slug = auth?.slug;
    final token = auth?.token;
    if (slug == null || token == null) return;

    final bytes = await _api.getBytes(
      ApiConstants.relatorioMembroPdf(slug, item.membroId, analitico: analitico),
      token: token,
    );

    final tipo = analitico ? 'analitico' : 'sintetico';
    await Printing.layoutPdf(
      name: 'ganhador_membro_${item.posicao}_$tipo.pdf',
      onLayout: (_) async => bytes,
    );
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelEquipe = config?.labelEquipe ?? 'Embarcacao';
    final labelEquipePlural = config?.labelEquipePlural ?? 'Embarcacoes';
    final labelMembro = config?.labelMembro ?? 'Pescador';
    final labelMembroPlural = config?.labelMembroPlural ?? 'Pescadores';
    final qtdGanhadores = config?.qtdGanhadores ?? 3;
    final exibirQtdGanhadores = config?.modoSorteio == 'Nenhum';

    return Scaffold(
      appBar: AppBar(title: const Text('Relatórios dos Ganhadores')),
      body: FutureBuilder<GanhadoresResponse>(
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

          final resp = snapshot.data;
          final semDados = resp == null ||
              (!resp.premiacaoPorEquipe && !resp.premiacaoPorMembro) ||
              (resp.equipesGanhadoras.isEmpty && resp.membrosGanhadores.isEmpty);

          if (semDados) {
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
              if (resp.premiacaoPorEquipe) ...[
                Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: Text(
                    exibirQtdGanhadores
                        ? 'Top $qtdGanhadores $labelEquipePlural'
                        : 'Ganhadores por $labelEquipe',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
                if (resp.equipesGanhadoras.isEmpty)
                  const Padding(
                    padding: EdgeInsets.only(bottom: 16),
                    child: Text('Nenhuma captura registrada ainda.', style: TextStyle(color: Colors.grey)),
                  )
                else
                  ...resp.equipesGanhadoras.map(
                    (item) => _CardGanhadorEquipe(
                      item: item,
                      onSintetico: () => _abrirPdfEquipe(item, false),
                      onAnalitico: () => _abrirPdfEquipe(item, true),
                    ),
                  ),
                const SizedBox(height: 16),
              ],
              if (resp.premiacaoPorMembro) ...[
                Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: Text(
                    exibirQtdGanhadores
                        ? 'Top $qtdGanhadores $labelMembroPlural'
                        : 'Ganhadores por $labelMembro',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
                if (resp.membrosGanhadores.isEmpty)
                  const Padding(
                    padding: EdgeInsets.only(bottom: 16),
                    child: Text('Nenhuma captura registrada ainda.', style: TextStyle(color: Colors.grey)),
                  )
                else
                  ...resp.membrosGanhadores.map(
                    (item) => _CardGanhadorMembro(
                      item: item,
                      onSintetico: () => _abrirPdfMembro(item, false),
                      onAnalitico: () => _abrirPdfMembro(item, true),
                    ),
                  ),
              ],
            ],
          );
        },
      ),
    );
  }
}

class _CardGanhadorEquipe extends StatelessWidget {
  final GanhadorEquipe item;
  final VoidCallback onSintetico;
  final VoidCallback onAnalitico;

  const _CardGanhadorEquipe({
    required this.item,
    required this.onSintetico,
    required this.onAnalitico,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              '${item.posicao}o - ${item.nomeEquipe}',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 4),
            Text('Capitao: ${item.capitao}'),
            Text('Pontos: ${item.totalPontos.toStringAsFixed(2)}'),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                OutlinedButton.icon(
                  onPressed: onSintetico,
                  icon: const Icon(Icons.picture_as_pdf_outlined),
                  label: const Text('Sintetico'),
                ),
                FilledButton.icon(
                  onPressed: onAnalitico,
                  icon: const Icon(Icons.picture_as_pdf),
                  label: const Text('Analitico'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _CardGanhadorMembro extends StatelessWidget {
  final GanhadorMembro item;
  final VoidCallback onSintetico;
  final VoidCallback onAnalitico;

  const _CardGanhadorMembro({
    required this.item,
    required this.onSintetico,
    required this.onAnalitico,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              '${item.posicao}o - ${item.nomeMembro}',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 4),
            Text('Pontos: ${item.totalPontos.toStringAsFixed(2)}'),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                OutlinedButton.icon(
                  onPressed: onSintetico,
                  icon: const Icon(Icons.picture_as_pdf_outlined),
                  label: const Text('Sintetico'),
                ),
                FilledButton.icon(
                  onPressed: onAnalitico,
                  icon: const Icon(Icons.picture_as_pdf),
                  label: const Text('Analitico'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
