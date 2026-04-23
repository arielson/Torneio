import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/patrocinador.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import '../../widgets/patrocinadores_section.dart';

class TorneioScreen extends StatefulWidget {
  const TorneioScreen({super.key});

  @override
  State<TorneioScreen> createState() => _TorneioScreenState();
}

class _TorneioScreenState extends State<TorneioScreen> {
  String? _slug;
  bool _inicializado = false;
  late ConfigProvider _configProvider;
  final ApiService _api = ApiService();
  List<Patrocinador> _patrocinadores = const [];

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (!_inicializado) {
      _configProvider = context.read<ConfigProvider>();
      _slug = ModalRoute.of(context)?.settings.arguments as String?;
      if (_slug != null) {
        WidgetsBinding.instance.addPostFrameCallback((_) {
          _configProvider.carregarConfig(_slug!);
          _carregarPatrocinadores(_slug!);
        });
      }
      _inicializado = true;
    }
  }

  Future<void> _carregarPatrocinadores(String slug) async {
    try {
      final data = await _api.get(ApiConstants.patrocinadores(slug));
      final lista = data is List
          ? data
              .map((e) => Patrocinador.fromJson(e as Map<String, dynamic>))
              .where((p) => p.exibirNaTelaInicial)
              .toList()
          : <Patrocinador>[];
      lista.sort((a, b) => a.nome.compareTo(b.nome));
      if (!mounted) return;
      setState(() => _patrocinadores = lista);
    } catch (_) {
      if (!mounted) return;
      setState(() => _patrocinadores = const []);
    }
  }

  @override
  void dispose() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _configProvider.limpar();
    });
    super.dispose();
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
            if (config.permitirRegistroPublicoMembro) ...[
              const SizedBox(height: 12),
              OutlinedButton.icon(
                icon: const Icon(Icons.badge_outlined),
                label: Text('Entrar como ${config.labelMembro.toLowerCase()}'),
                onPressed: () => Navigator.pushNamed(context, '/login', arguments: 'Membro'),
              ),
              const SizedBox(height: 12),
              OutlinedButton.icon(
                icon: const Icon(Icons.person_add_alt_1_outlined),
                label: Text('Registrar ${config.labelMembro.toLowerCase()}'),
                onPressed: () => Navigator.pushNamed(context, '/registro-pescador'),
              ),
              const SizedBox(height: 12),
              OutlinedButton.icon(
                icon: const Icon(Icons.lock_reset_outlined),
                label: Text('Recuperar senha do ${config.labelMembro.toLowerCase()}'),
                onPressed: () => Navigator.pushNamed(context, '/recuperar-senha-pescador'),
              ),
            ],
            if (_patrocinadores.isNotEmpty) ...[
              const SizedBox(height: 24),
              PatrocinadoresSection(patrocinadores: _patrocinadores),
            ],
          ],
        ),
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
