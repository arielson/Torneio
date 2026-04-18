import 'package:flutter/material.dart';
import 'package:printing/printing.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/equipe.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class RelatoriosEquipesScreen extends StatefulWidget {
  const RelatoriosEquipesScreen({super.key});

  @override
  State<RelatoriosEquipesScreen> createState() => _RelatoriosEquipesScreenState();
}

class _RelatoriosEquipesScreenState extends State<RelatoriosEquipesScreen> {
  final ApiService _api = ApiService();
  late Future<List<Equipe>> _future;

  @override
  void initState() {
    super.initState();
    _future = _carregar();
  }

  Future<List<Equipe>> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return const [];

    final data = await _api.get(
      ApiConstants.equipes(auth!.slug!),
      token: auth.token,
    );

    if (data is! List) return const [];
    final lista = data.map((e) => Equipe.fromJson(e as Map<String, dynamic>)).toList();
    lista.sort((a, b) => a.nome.compareTo(b.nome));
    return lista;
  }

  Future<void> _abrirPdf(Equipe equipe, bool analitico) async {
    final auth = context.read<AuthProvider>().usuario;
    final slug = auth?.slug;
    final token = auth?.token;
    if (slug == null || token == null) return;

    final bytes = await _api.getBytes(
      ApiConstants.relatorioEquipePdf(slug, equipe.id, analitico: analitico),
      token: token,
    );

    final tipo = analitico ? 'analitico' : 'sintetico';
    await Printing.layoutPdf(
      name: 'equipe_${equipe.id}_$tipo.pdf',
      onLayout: (_) async => bytes,
    );
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final label = config?.labelEquipe ?? 'Equipe';
    final labelPlural = config?.labelEquipePlural ?? 'Equipes';

    return Scaffold(
      appBar: AppBar(title: Text('Relatórios por $label')),
      body: FutureBuilder<List<Equipe>>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Text('Erro ao carregar $labelPlural: ${snapshot.error}'),
              ),
            );
          }

          final itens = snapshot.data ?? const <Equipe>[];
          if (itens.isEmpty) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Text('Nenhuma ${label.toLowerCase()} cadastrada.'),
              ),
            );
          }

          return ListView(
            padding: const EdgeInsets.all(16),
            children: itens
                .map(
                  (equipe) => Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            equipe.nome,
                            style: Theme.of(context).textTheme.titleMedium,
                          ),
                          const SizedBox(height: 4),
                          Text('Capitão: ${equipe.capitao}'),
                          const SizedBox(height: 12),
                          Wrap(
                            spacing: 8,
                            runSpacing: 8,
                            children: [
                              OutlinedButton.icon(
                                onPressed: () => _abrirPdf(equipe, false),
                                icon: const Icon(Icons.picture_as_pdf_outlined),
                                label: const Text('Sintético'),
                              ),
                              FilledButton.icon(
                                onPressed: () => _abrirPdf(equipe, true),
                                icon: const Icon(Icons.picture_as_pdf),
                                label: const Text('Analítico'),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                )
                .toList(),
          );
        },
      ),
    );
  }
}
