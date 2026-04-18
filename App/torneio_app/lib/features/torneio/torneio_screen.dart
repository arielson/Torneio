import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/config_provider.dart';

class TorneioScreen extends StatefulWidget {
  const TorneioScreen({super.key});

  @override
  State<TorneioScreen> createState() => _TorneioScreenState();
}

class _TorneioScreenState extends State<TorneioScreen> {
  String? _slug;
  bool _inicializado = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (!_inicializado) {
      _slug = ModalRoute.of(context)?.settings.arguments as String?;
      if (_slug != null) {
        WidgetsBinding.instance.addPostFrameCallback((_) {
          context.read<ConfigProvider>().carregarConfig(_slug!);
        });
      }
      _inicializado = true;
    }
  }

  Color _corStatus(String status) => switch (status) {
        'Liberado' => Colors.green,
        'Finalizado' => Colors.grey,
        _ => Colors.orange,
      };

  @override
  Widget build(BuildContext context) {
    final configProv = context.watch<ConfigProvider>();
    final config = configProv.config;

    if (configProv.carregando) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (config == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Torneio')),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.error_outline, size: 64, color: Colors.grey),
              const SizedBox(height: 16),
              const Text('Torneio nao encontrado.'),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Voltar'),
              ),
            ],
          ),
        ),
      );
    }

    final cor = _corStatus(config.status);

    return Scaffold(
      appBar: AppBar(
        title: Text(config.nomeTorneio),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Logo
            if (config.logoUrl != null)
              Center(
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(12),
                  child: Image.network(config.logoUrl!, height: 120, fit: BoxFit.contain,
                      errorBuilder: (context, error, stackTrace) => const SizedBox.shrink()),
                ),
              ),
            const SizedBox(height: 16),

            // Info card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            config.nomeTorneio,
                            style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                          ),
                        ),
                        Chip(
                          label: Text(config.status, style: TextStyle(color: cor, fontSize: 12)),
                          backgroundColor: cor.withAlpha(30),
                          side: BorderSide(color: cor.withAlpha(80)),
                        ),
                      ],
                    ),
                    const Divider(height: 24),
                    _InfoRow(icon: Icons.straighten, label: 'Medida', value: config.medidaCaptura),
                    if (config.permitirCapturaOffline)
                      const _InfoRow(icon: Icons.wifi_off, label: 'Offline', value: 'Suportado'),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),

            // Status message
            if (config.status == 'Aberto')
              _StatusBanner(
                icon: Icons.lock_clock,
                message: 'Este torneio ainda nao esta aberto ao publico.',
                color: Colors.orange,
              )
            else if (config.status == 'Finalizado')
              _StatusBanner(
                icon: Icons.check_circle,
                message: 'Este torneio foi encerrado.',
                color: Colors.grey,
              ),

            const SizedBox(height: 16),

            // Login button
            FilledButton.icon(
              icon: const Icon(Icons.login),
              label: const Text('Fiscal/Administração'),
              onPressed: () => Navigator.pushNamed(context, '/login'),
            ),
          ],
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
      padding: const EdgeInsets.symmetric(vertical: 3),
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

class _StatusBanner extends StatelessWidget {
  final IconData icon;
  final String message;
  final Color color;
  const _StatusBanner({required this.icon, required this.message, required this.color});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: color.withAlpha(30),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withAlpha(80)),
      ),
      child: Row(
        children: [
          Icon(icon, color: color),
          const SizedBox(width: 8),
          Expanded(child: Text(message, style: TextStyle(color: color))),
        ],
      ),
    );
  }
}
