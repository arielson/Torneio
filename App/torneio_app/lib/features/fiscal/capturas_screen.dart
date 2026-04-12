import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/captura_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../widgets/captura_card.dart';

class CapturasScreen extends StatelessWidget {
  const CapturasScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final capProv = context.watch<CapturaProvider>();
    final config = context.watch<ConfigProvider>().config;
    final capturas = capProv.capturas;

    final totalPontos = capturas.fold<double>(0, (sum, c) => sum + c.pontuacao);

    return Scaffold(
      appBar: AppBar(
        title: Text(config?.labelCaptura ?? 'Capturas'),
        actions: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Center(
              child: Text(
                '${totalPontos.toStringAsFixed(2)} pts',
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
            ),
          ),
        ],
      ),
      body: capProv.carregando
          ? const Center(child: CircularProgressIndicator())
          : capturas.isEmpty
              ? Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(Icons.inbox, size: 64, color: Colors.grey),
                      const SizedBox(height: 16),
                      Text(
                        'Nenhuma ${(config?.labelCaptura ?? "captura").toLowerCase()} registrada.',
                        style: const TextStyle(color: Colors.grey),
                      ),
                    ],
                  ),
                )
              : ListView.builder(
                  padding: const EdgeInsets.all(12),
                  itemCount: capturas.length + 1,
                  itemBuilder: (context, index) {
                    if (index == 0) {
                      return Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              '${capturas.length} ${(config?.labelCaptura ?? "captura").toLowerCase()}(s)',
                              style: Theme.of(context).textTheme.bodySmall,
                            ),
                            Text(
                              'Total: ${totalPontos.toStringAsFixed(2)} pts',
                              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                                    fontWeight: FontWeight.bold,
                                  ),
                            ),
                          ],
                        ),
                      );
                    }
                    final c = capturas[index - 1];
                    return CapturaCard(
                      captura: c,
                      medidaLabel: config?.medidaCaptura ?? 'cm',
                      mostrarFator: config?.usarFatorMultiplicador ?? false,
                    );
                  },
                ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => Navigator.pushNamed(context, '/fiscal/registrar'),
        child: const Icon(Icons.add),
      ),
    );
  }
}
