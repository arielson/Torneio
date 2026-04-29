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
  Future<GanhadoresResponse>? _future;
  int _quantidadeEquipes = 3;
  int _quantidadeMembrosPontuacao = 3;
  int _quantidadeMembrosMaiorCaptura = 3;

  Future<void> _selecionarFiltros() async {
    final config = context.read<ConfigProvider>().config;
    final equipesController = TextEditingController(text: _quantidadeEquipes.toString());
    final membrosController = TextEditingController(text: _quantidadeMembrosPontuacao.toString());
    final maiorCapturaController = TextEditingController(text: _quantidadeMembrosMaiorCaptura.toString());
    final exibirMaiorCaptura = config?.tipoTorneio == 'Pesca';

    final confirmado = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Ganhadores'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text(
                'Informe quantas embarcacoes, pescadores por pontuacao e pescadores por maior captura devem entrar no relatorio.',
              ),
              const SizedBox(height: 12),
              TextField(
                controller: equipesController,
                keyboardType: TextInputType.number,
                autofocus: true,
                decoration: const InputDecoration(
                  labelText: 'Embarcacoes ganhadoras',
                  helperText: 'Informe de 0 a 999.',
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: membrosController,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(
                  labelText: 'Pescadores por pontuacao',
                  helperText: 'Informe de 0 a 999.',
                ),
              ),
              if (exibirMaiorCaptura) ...[
                const SizedBox(height: 12),
                TextField(
                  controller: maiorCapturaController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: 'Pescadores por maior captura',
                    helperText: 'Informe de 0 a 999.',
                  ),
                ),
              ],
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              final qtdEquipes = int.tryParse(equipesController.text.trim());
              final qtdMembros = int.tryParse(membrosController.text.trim());
              final qtdMaiorCaptura = int.tryParse(maiorCapturaController.text.trim());
              final valoresValidos =
                  qtdEquipes != null &&
                  qtdMembros != null &&
                  (!exibirMaiorCaptura || qtdMaiorCaptura != null) &&
                  qtdEquipes >= 0 &&
                  qtdEquipes <= 999 &&
                  qtdMembros >= 0 &&
                  qtdMembros <= 999 &&
                  (!exibirMaiorCaptura || (qtdMaiorCaptura! >= 0 && qtdMaiorCaptura <= 999));
              if (!valoresValidos) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Informe quantidades entre 0 e 999.')),
                );
                return;
              }
              setState(() {
                _quantidadeEquipes = qtdEquipes;
                _quantidadeMembrosPontuacao = qtdMembros;
                _quantidadeMembrosMaiorCaptura = exibirMaiorCaptura ? qtdMaiorCaptura! : 0;
              });
              Navigator.of(dialogContext).pop(true);
            },
            child: const Text('Aplicar'),
          ),
        ],
      ),
    );

    if (confirmado == true) {
      setState(() {
        _future = _carregar();
      });
    }
  }

  Future<GanhadoresResponse> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) {
      return const GanhadoresResponse(
        quantidadeEquipes: 0,
        quantidadeMembrosPontuacao: 0,
        quantidadeMembrosMaiorCaptura: 0,
        exibirMaiorCaptura: false,
        equipesGanhadoras: [],
        membrosGanhadores: [],
        membrosMaiorCaptura: [],
      );
    }

    final data = await _api.get(
      ApiConstants.relatoriosGanhadores(
        auth!.slug!,
        quantidadeEquipes: _quantidadeEquipes,
        quantidadeMembrosPontuacao: _quantidadeMembrosPontuacao,
        quantidadeMembrosMaiorCaptura: _quantidadeMembrosMaiorCaptura,
      ),
      token: auth.token,
    );

    if (data is! Map<String, dynamic>) {
      return const GanhadoresResponse(
        quantidadeEquipes: 0,
        quantidadeMembrosPontuacao: 0,
        quantidadeMembrosMaiorCaptura: 0,
        exibirMaiorCaptura: false,
        equipesGanhadoras: [],
        membrosGanhadores: [],
        membrosMaiorCaptura: [],
      );
    }

    return GanhadoresResponse.fromJson(data);
  }

  Future<void> _abrirPdfGanhadores({required bool analitico}) async {
    final auth = context.read<AuthProvider>().usuario;
    final slug = auth?.slug;
    final token = auth?.token;
    if (slug == null || token == null) return;

    final bytes = await _api.getBytes(
      ApiConstants.relatorioGanhadoresPdf(
        slug,
        quantidadeEquipes: _quantidadeEquipes,
        quantidadeMembrosPontuacao: _quantidadeMembrosPontuacao,
        quantidadeMembrosMaiorCaptura: _quantidadeMembrosMaiorCaptura,
        analitico: analitico,
      ),
      token: token,
    );

    final tipo = analitico ? 'analitico' : 'sintetico';
    await Printing.layoutPdf(
      name: 'ganhadores_$tipo.pdf',
      onLayout: (_) async => bytes,
    );
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelEquipePlural = config?.labelEquipePlural ?? 'Embarcacoes';
    final labelMembroPlural = config?.labelMembroPlural ?? 'Pescadores';

    return Scaffold(
      appBar: AppBar(
        title: const Text('Relatorios dos Ganhadores'),
        actions: [
          IconButton(
            onPressed: _selecionarFiltros,
            icon: const Icon(Icons.tune),
            tooltip: 'Escolher quantidades',
          ),
        ],
      ),
      body: _future == null
          ? Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.emoji_events_outlined, size: 64, color: Colors.amber),
                    const SizedBox(height: 16),
                    const Text(
                      'Defina quantas embarcacoes e pescadores deseja considerar em cada categoria do relatorio.',
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16),
                    FilledButton.icon(
                      onPressed: _selecionarFiltros,
                      icon: const Icon(Icons.tune),
                      label: const Text('Escolher quantidades'),
                    ),
                  ],
                ),
              ),
            )
          : FutureBuilder<GanhadoresResponse>(
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
                    (resp.equipesGanhadoras.isEmpty &&
                        resp.membrosGanhadores.isEmpty &&
                        resp.membrosMaiorCaptura.isEmpty);

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
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: [
                        OutlinedButton.icon(
                          onPressed: () => _abrirPdfGanhadores(analitico: false),
                          icon: const Icon(Icons.picture_as_pdf_outlined),
                          label: const Text('PDF sintetico'),
                        ),
                        FilledButton.icon(
                          onPressed: () => _abrirPdfGanhadores(analitico: true),
                          icon: const Icon(Icons.picture_as_pdf),
                          label: const Text('PDF analitico'),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    if (resp.equipesGanhadoras.isNotEmpty) ...[
                      Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: Text(
                          'Top ${resp.quantidadeEquipes} $labelEquipePlural',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                      ),
                      ...resp.equipesGanhadoras.map((item) => _CardGanhadorEquipe(item: item)),
                      const SizedBox(height: 16),
                    ],
                    if (resp.membrosGanhadores.isNotEmpty) ...[
                      Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: Text(
                          'Top ${resp.quantidadeMembrosPontuacao} $labelMembroPlural por pontuacao',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                      ),
                      ...resp.membrosGanhadores.map(
                        (item) => _CardGanhadorMembro(
                          item: item,
                          destaque: 'Pontos: ${item.totalPontos.toStringAsFixed(2)}',
                        ),
                      ),
                      const SizedBox(height: 16),
                    ],
                    if (resp.exibirMaiorCaptura && resp.membrosMaiorCaptura.isNotEmpty) ...[
                      Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: Text(
                          'Top ${resp.quantidadeMembrosMaiorCaptura} $labelMembroPlural por maior captura',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                      ),
                      ...resp.membrosMaiorCaptura.map(
                        (item) => _CardGanhadorMembro(
                          item: item,
                          destaque:
                              'Maior captura: ${item.maiorCaptura?.toStringAsFixed(1) ?? "0.0"} ${config?.medidaCaptura ?? "cm"}'
                              '${(item.nomeItemMaiorCaptura?.isNotEmpty ?? false) ? ' - ${item.nomeItemMaiorCaptura}' : ''}',
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

  const _CardGanhadorEquipe({required this.item});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('${item.posicao}o - ${item.nomeEquipe}', style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 4),
            Text('Capitao: ${item.capitao}'),
            Text('Pontos: ${item.totalPontos.toStringAsFixed(2)}'),
          ],
        ),
      ),
    );
  }
}

class _CardGanhadorMembro extends StatelessWidget {
  final GanhadorMembro item;
  final String destaque;

  const _CardGanhadorMembro({
    required this.item,
    required this.destaque,
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
            Text('${item.posicao}o - ${item.nomeMembro}', style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 4),
            Text(destaque),
          ],
        ),
      ),
    );
  }
}
