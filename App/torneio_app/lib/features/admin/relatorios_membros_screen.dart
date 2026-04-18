import 'package:flutter/material.dart';
import 'package:printing/printing.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/membro.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class RelatoriosMembrosScreen extends StatefulWidget {
  const RelatoriosMembrosScreen({super.key});

  @override
  State<RelatoriosMembrosScreen> createState() => _RelatoriosMembrosScreenState();
}

class _RelatoriosMembrosScreenState extends State<RelatoriosMembrosScreen> {
  final ApiService _api = ApiService();
  late Future<List<Membro>> _future;

  @override
  void initState() {
    super.initState();
    _future = _carregar();
  }

  Future<List<Membro>> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return const [];

    final data = await _api.get(
      ApiConstants.membros(auth!.slug!),
      token: auth.token,
    );

    if (data is! List) return const [];
    final lista = data.map((e) => Membro.fromJson(e as Map<String, dynamic>)).toList();
    lista.sort((a, b) => a.nome.compareTo(b.nome));
    return lista;
  }

  Future<void> _abrirPdf(Membro membro, bool analitico) async {
    final auth = context.read<AuthProvider>().usuario;
    final slug = auth?.slug;
    final token = auth?.token;
    if (slug == null || token == null) return;

    final bytes = await _api.getBytes(
      ApiConstants.relatorioMembroPdf(slug, membro.id, analitico: analitico),
      token: token,
    );

    final tipo = analitico ? 'analitico' : 'sintetico';
    await Printing.layoutPdf(
      name: 'membro_${membro.id}_$tipo.pdf',
      onLayout: (_) async => bytes,
    );
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final label = config?.labelMembro ?? 'Membro';
    final labelPlural = config?.labelMembroPlural ?? 'Membros';

    return Scaffold(
      appBar: AppBar(title: Text('Relatórios por $label')),
      body: FutureBuilder<List<Membro>>(
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

          final itens = snapshot.data ?? const <Membro>[];
          if (itens.isEmpty) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Text('Nenhum ${label.toLowerCase()} cadastrado.'),
              ),
            );
          }

          return ListView(
            padding: const EdgeInsets.all(16),
            children: itens
                .map(
                  (membro) => Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            membro.nome,
                            style: Theme.of(context).textTheme.titleMedium,
                          ),
                          const SizedBox(height: 12),
                          Wrap(
                            spacing: 8,
                            runSpacing: 8,
                            children: [
                              OutlinedButton.icon(
                                onPressed: () => _abrirPdf(membro, false),
                                icon: const Icon(Icons.picture_as_pdf_outlined),
                                label: const Text('Sintético'),
                              ),
                              FilledButton.icon(
                                onPressed: () => _abrirPdf(membro, true),
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
