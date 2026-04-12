import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/captura_provider.dart';
import '../../core/providers/config_provider.dart';

class SyncScreen extends StatelessWidget {
  const SyncScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final capProv = context.watch<CapturaProvider>();
    final auth = context.watch<AuthProvider>();
    final config = context.watch<ConfigProvider>().config;
    final pendentes = capProv.pendentesSync;

    return Scaffold(
      appBar: AppBar(title: const Text('Sincronização')),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Icon(
              pendentes > 0 ? Icons.sync_problem : Icons.cloud_done,
              size: 80,
              color: pendentes > 0 ? Colors.orange : Colors.green,
            ),
            const SizedBox(height: 16),
            Text(
              pendentes > 0
                  ? '$pendentes captura(s) aguardando sincronização'
                  : 'Tudo sincronizado!',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 8),
            Text(
              pendentes > 0
                  ? 'As capturas abaixo foram registradas offline e precisam ser enviadas ao servidor.'
                  : 'Não há capturas pendentes de sincronização.',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.grey),
            ),
            const SizedBox(height: 32),
            if (capProv.mensagemSync != null)
              Container(
                padding: const EdgeInsets.all(12),
                margin: const EdgeInsets.only(bottom: 16),
                decoration: BoxDecoration(
                  color: Colors.green.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.green.shade200),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.check_circle, color: Colors.green),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        capProv.mensagemSync!,
                        style: const TextStyle(color: Colors.green),
                      ),
                    ),
                  ],
                ),
              ),
            if (capProv.erro != null)
              Container(
                padding: const EdgeInsets.all(12),
                margin: const EdgeInsets.only(bottom: 16),
                decoration: BoxDecoration(
                  color: Colors.red.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.red.shade200),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.error, color: Colors.red),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        capProv.erro!,
                        style: const TextStyle(color: Colors.red),
                      ),
                    ),
                  ],
                ),
              ),
            FilledButton.icon(
              icon: capProv.sincronizando
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : const Icon(Icons.sync),
              label: Text(capProv.sincronizando ? 'Sincronizando...' : 'Sincronizar agora'),
              onPressed: capProv.sincronizando || auth.usuario == null || config == null
                  ? null
                  : () => capProv.sincronizar(config.slug, auth.usuario!.token),
            ),
            const SizedBox(height: 12),
            Text(
              'A sincronização automática ocorre quando o dispositivo reconecta à internet.',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Colors.grey),
            ),
          ],
        ),
      ),
    );
  }
}
