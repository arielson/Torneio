import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/captura_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../widgets/sync_badge.dart';

class HomeFiscalScreen extends StatefulWidget {
  const HomeFiscalScreen({super.key});

  @override
  State<HomeFiscalScreen> createState() => _HomeFiscalScreenState();
}

class _HomeFiscalScreenState extends State<HomeFiscalScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _carregar());
  }

  Future<void> _carregar() async {
    final auth = context.read<AuthProvider>();
    final config = context.read<ConfigProvider>().config;
    final capProv = context.read<CapturaProvider>();

    if (auth.usuario == null || config == null) return;

    // Encontra a equipe do fiscal: anoTorneio mais recente liberado
    final configProv = context.read<ConfigProvider>();
    await configProv.carregarAnos(config.slug, auth.usuario!.token);

    final anos = configProv.anos.where((a) => a.isLiberado).toList();
    if (anos.isEmpty) return;

    final anoAtivo = anos.first;
    await capProv.carregarDadosEquipe(
      config.slug,
      auth.usuario!.token,
      anoAtivo.id,
      // Equipe do fiscal — carregará todas e filtrará pela fiscalId
      '',
    );
  }

  Future<void> _logout() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Sair'),
        content: const Text('Deseja encerrar a sessão?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Sair')),
        ],
      ),
    );
    if (confirm == true && mounted) {
      context.read<AuthProvider>().logout();
      context.read<CapturaProvider>().limpar();
      Navigator.pushReplacementNamed(context, '/publico/home');
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final config = context.watch<ConfigProvider>().config;
    final capProv = context.watch<CapturaProvider>();
    final pendentes = capProv.pendentesSync;
    final equipes = capProv.equipes;

    // Equipe do fiscal (filtra por fiscalId)
    final fiscalId = auth.usuario?.id ?? '';
    final minhaEquipe = equipes.isNotEmpty
        ? equipes.where((e) => e.fiscalId == fiscalId).firstOrNull ?? equipes.first
        : null;

    return Scaffold(
      appBar: AppBar(
        title: Text(config?.nomeTorneio ?? 'Torneio'),
        actions: [
          if (pendentes > 0)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8),
              child: Stack(
                alignment: Alignment.topRight,
                children: [
                  IconButton(
                    icon: const Icon(Icons.sync),
                    tooltip: 'Sincronizar',
                    onPressed: () => Navigator.pushNamed(context, '/fiscal/sync'),
                  ),
                  SyncBadge(count: pendentes),
                ],
              ),
            ),
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: _logout,
            tooltip: 'Sair',
          ),
        ],
      ),
      body: capProv.carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Saudação
                    Text(
                      'Olá, ${auth.usuario?.nome ?? ''}!',
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Perfil: ${config?.labelSupervisor ?? 'Fiscal'}',
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.grey),
                    ),
                    const SizedBox(height: 24),

                    // Equipe
                    if (minhaEquipe != null) ...[
                      Text(
                        config?.labelEquipe ?? 'Equipe',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 8),
                      Card(
                        child: Padding(
                          padding: const EdgeInsets.all(16),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                minhaEquipe.nome,
                                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                      fontWeight: FontWeight.bold,
                                    ),
                              ),
                              const SizedBox(height: 4),
                              Text('Capitão: ${minhaEquipe.capitao}'),
                              Text(
                                '${minhaEquipe.qtdMembros}/${minhaEquipe.qtdVagas} ${config?.labelMembro ?? "membros"}',
                              ),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 24),
                    ],

                    // Ações rápidas
                    Text(
                      'Ações',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 8),
                    _ActionGrid(
                      actions: [
                        _ActionItem(
                          icon: Icons.add_circle,
                          label: 'Registrar\n${config?.labelCaptura ?? "Captura"}',
                          color: Colors.green,
                          onTap: () => Navigator.pushNamed(context, '/fiscal/registrar'),
                        ),
                        _ActionItem(
                          icon: Icons.list_alt,
                          label: '${config?.labelCaptura ?? "Capturas"}\nRegistradas',
                          color: Colors.blue,
                          onTap: () => Navigator.pushNamed(context, '/fiscal/capturas'),
                        ),
                        _ActionItem(
                          icon: Icons.sync,
                          label: 'Sincronizar\n${pendentes > 0 ? "($pendentes)" : ""}',
                          color: pendentes > 0 ? Colors.orange : Colors.grey,
                          onTap: () => Navigator.pushNamed(context, '/fiscal/sync'),
                        ),
                      ],
                    ),

                    if (capProv.erro != null) ...[
                      const SizedBox(height: 16),
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.red.shade50,
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Row(
                          children: [
                            const Icon(Icons.error, color: Colors.red),
                            const SizedBox(width: 8),
                            Expanded(child: Text(capProv.erro!)),
                          ],
                        ),
                      ),
                    ],
                  ],
                ),
              ),
            ),
    );
  }
}

class _ActionGrid extends StatelessWidget {
  final List<_ActionItem> actions;
  const _ActionGrid({required this.actions});

  @override
  Widget build(BuildContext context) {
    return GridView.count(
      crossAxisCount: 3,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      crossAxisSpacing: 8,
      mainAxisSpacing: 8,
      children: actions,
    );
  }
}

class _ActionItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;

  const _ActionItem({
    required this.icon,
    required this.label,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: Container(
        decoration: BoxDecoration(
          color: color.withAlpha(25),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: color.withAlpha(80)),
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, color: color, size: 32),
            const SizedBox(height: 6),
            Text(
              label,
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 11, color: color, fontWeight: FontWeight.w500),
            ),
          ],
        ),
      ),
    );
  }
}
