import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/config_provider.dart';

class RelatoriosAdminScreen extends StatelessWidget {
  const RelatoriosAdminScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelEquipe = config?.labelEquipe ?? 'Embarcação';
    final labelMembro = config?.labelMembro ?? 'Pescador';
    final qtdGanhadores = config?.qtdGanhadores ?? 3;

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
              subtitle: Text(
                'Relatórios dos $qtdGanhadores ganhadores com base na classificação atual por $labelEquipe.',
              ),
              trailing: const Icon(Icons.chevron_right),
              onTap: () => Navigator.pushNamed(context, '/admin/relatorios/ganhadores'),
            ),
          ),
        ],
      ),
    );
  }
}
