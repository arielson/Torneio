import 'package:flutter/material.dart';
import 'package:printing/printing.dart';
import 'package:provider/provider.dart';

import '../../core/constants.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class RelatoriosAdminScreen extends StatelessWidget {
  const RelatoriosAdminScreen({super.key});

  static final ApiService _api = ApiService();

  Future<void> _emitirMaioresCapturas(BuildContext context) async {
    final controller = TextEditingController(text: '1');
    final quantidade = await showDialog<int>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Maiores capturas'),
        content: TextField(
          controller: controller,
          keyboardType: TextInputType.number,
          autofocus: true,
          decoration: const InputDecoration(
            labelText: 'Quantidade',
            helperText: 'Informe de 1 a 999.',
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              final valor = int.tryParse(controller.text.trim());
              if (valor == null || valor < 1 || valor > 999) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Informe uma quantidade entre 1 e 999.')),
                );
                return;
              }
              Navigator.of(dialogContext).pop(valor);
            },
            child: const Text('Emitir'),
          ),
        ],
      ),
    );

    if (quantidade == null || !context.mounted) return;

    final auth = context.read<AuthProvider>().usuario;
    final slug = auth?.slug;
    final token = auth?.token;
    if (slug == null || token == null) return;

    try {
      final bytes = await _api.getBytes(
        ApiConstants.relatorioMaioresCapturasPdf(slug, quantidade),
        token: token,
      );

      await Printing.layoutPdf(
        name: 'maiores_capturas_$quantidade.pdf',
        onLayout: (_) async => bytes,
      );
    } catch (e) {
      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Erro ao emitir relatório: $e')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelEquipe = config?.labelEquipe ?? 'Embarcação';
    final labelMembro = config?.labelMembro ?? 'Pescador';
    final exibirMaioresCapturas = config?.tipoTorneio == 'Pesca';

    return Scaffold(
      appBar: AppBar(title: const Text('Relatórios')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            child: ListTile(
              leading: const Icon(Icons.groups_outlined),
              title: Text('Por $labelEquipe'),
              subtitle: Text(
                'Selecione uma ${labelEquipe.toLowerCase()} para gerar o relatório sintético ou analítico.',
              ),
              trailing: const Icon(Icons.chevron_right),
              onTap: () => Navigator.pushNamed(context, '/admin/relatorios/equipes'),
            ),
          ),
          Card(
            child: ListTile(
              leading: const Icon(Icons.person_outline),
              title: Text('Por $labelMembro'),
              subtitle: Text(
                'Selecione um ${labelMembro.toLowerCase()} para gerar o relatório sintético ou analítico.',
              ),
              trailing: const Icon(Icons.chevron_right),
              onTap: () => Navigator.pushNamed(context, '/admin/relatorios/membros'),
            ),
          ),
          Card(
            child: ListTile(
              leading: const Icon(Icons.emoji_events_outlined),
              title: const Text('Ganhadores'),
              subtitle: const Text(
                'Defina quantas embarcações ganhadoras, quantos pescadores por pontuação e quantos pescadores por maior captura deseja considerar. Depois disso, emita um PDF sintético ou analítico consolidado.',
              ),
              trailing: const Icon(Icons.chevron_right),
              onTap: () => Navigator.pushNamed(context, '/admin/relatorios/ganhadores'),
            ),
          ),
          if (exibirMaioresCapturas)
            Card(
              child: ListTile(
                leading: const Icon(Icons.straighten_outlined),
                title: const Text('Maiores capturas'),
                subtitle: const Text(
                  'Informe quantas maiores capturas deseja incluir no ranking do torneio.',
                ),
                trailing: const Icon(Icons.picture_as_pdf_outlined),
                onTap: () => _emitirMaioresCapturas(context),
              ),
            ),
        ],
      ),
    );
  }
}
