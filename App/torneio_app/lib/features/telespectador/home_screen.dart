import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/models/ano_torneio.dart';

class HomeTelespectadorScreen extends StatelessWidget {
  const HomeTelespectadorScreen({super.key});

  Color _corStatus(String status) {
    return switch (status) {
      'Liberado' => Colors.green,
      'Finalizado' => Colors.grey,
      _ => Colors.orange,
    };
  }

  @override
  Widget build(BuildContext context) {
    final configProv = context.watch<ConfigProvider>();
    final config = configProv.config;

    if (configProv.carregando) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (config == null) {
      return Scaffold(
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Text('Torneio não encontrado.'),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () => Navigator.pushReplacementNamed(context, '/'),
                child: const Text('Voltar'),
              ),
            ],
          ),
        ),
      );
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(config.nomeTorneio),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => Navigator.pushReplacementNamed(context, '/'),
        ),
        actions: [
          TextButton.icon(
            icon: const Icon(Icons.login, color: Colors.white),
            label: const Text('Entrar', style: TextStyle(color: Colors.white)),
            onPressed: () => Navigator.pushReplacementNamed(context, '/login'),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () => configProv.carregarConfig(config.slug),
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Info do torneio
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        config.nomeTorneio,
                        style: Theme.of(context).textTheme.titleLarge?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                      const SizedBox(height: 8),
                      _InfoRow(icon: Icons.sports, label: 'Modo', value: config.modoSorteio),
                      _InfoRow(icon: Icons.straighten, label: 'Medida', value: config.medidaCaptura),
                      if (config.permitirCapturaOffline)
                        const _InfoRow(
                          icon: Icons.wifi_off,
                          label: 'Offline',
                          value: 'Suportado',
                        ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Text(
                'Edições',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 8),
              if (configProv.anos.isEmpty)
                const Center(child: Text('Nenhuma edição disponível.'))
              else
                ...configProv.anos.map((a) => _AnoTile(ano: a, corStatus: _corStatus)),
            ],
          ),
        ),
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;

  const _InfoRow({required this.icon, required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        children: [
          Icon(icon, size: 16, color: Colors.grey),
          const SizedBox(width: 6),
          Text('$label: ', style: const TextStyle(color: Colors.grey)),
          Text(value),
        ],
      ),
    );
  }
}

class _AnoTile extends StatelessWidget {
  final AnoTorneio ano;
  final Color Function(String) corStatus;

  const _AnoTile({required this.ano, required this.corStatus});

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: corStatus(ano.status).withAlpha(30),
          child: Text(
            '${ano.ano}',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              color: corStatus(ano.status),
            ),
          ),
        ),
        title: Text('${ano.ano}'),
        trailing: Chip(
          label: Text(ano.status),
          backgroundColor: corStatus(ano.status).withAlpha(40),
          labelStyle: TextStyle(color: corStatus(ano.status), fontSize: 12),
        ),
      ),
    );
  }
}
