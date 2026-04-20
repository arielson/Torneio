import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/flavor_config.dart';
import '../../core/models/equipe.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/captura_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../widgets/expandable_network_image.dart';
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
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final capProv = context.read<CapturaProvider>();
      if (capProv.equipes.isEmpty &&
          capProv.membros.isEmpty &&
          capProv.itens.isEmpty) {
        _carregar();
      }
    });
  }

  Future<void> _carregar() async {
    final auth = context.read<AuthProvider>();
    final config = context.read<ConfigProvider>().config;
    final capProv = context.read<CapturaProvider>();

    if (auth.usuario == null || config == null) return;

    await capProv.carregarDadosEquipe(config.slug, auth.usuario!.token, '');
  }

  Future<void> _logout() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder:
          (_) => AlertDialog(
            title: const Text('Sair'),
            content: const Text('Deseja encerrar a sessão?'),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context, false),
                child: const Text('Cancelar'),
              ),
              FilledButton(
                onPressed: () => Navigator.pop(context, true),
                child: const Text('Sair'),
              ),
            ],
          ),
    );
    if (confirm == true && mounted) {
      context.read<AuthProvider>().logout();
      context.read<CapturaProvider>().limpar();
      context.read<ConfigProvider>().limpar();
      Navigator.pushNamedAndRemoveUntil(context, '/home', (_) => false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final config = context.watch<ConfigProvider>().config;
    final capProv = context.watch<CapturaProvider>();
    final pendentes = capProv.pendentesSync;

    final fiscalId = auth.usuario?.id ?? '';
    final minhasEquipes =
        capProv.equipes.where((e) => e.fiscalId == fiscalId).toList();
    final exibirContagem = config?.modoSorteio != 'Nenhum';

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
                    onPressed:
                        () => Navigator.pushNamed(context, '/fiscal/sync'),
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
      body:
          capProv.carregando
              ? const Center(child: CircularProgressIndicator())
              : RefreshIndicator(
                onRefresh: _carregar,
                child: SingleChildScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Alerta de capturas pendentes
                      if (pendentes > 0)
                        GestureDetector(
                          onTap:
                              () =>
                                  Navigator.pushNamed(context, '/fiscal/sync'),
                          child: Container(
                            margin: const EdgeInsets.only(bottom: 16),
                            padding: const EdgeInsets.symmetric(
                              horizontal: 16,
                              vertical: 12,
                            ),
                            decoration: BoxDecoration(
                              color: Colors.orange.shade50,
                              borderRadius: BorderRadius.circular(10),
                              border: Border.all(color: Colors.orange.shade300),
                            ),
                            child: Row(
                              children: [
                                const Icon(
                                  Icons.sync_problem,
                                  color: Colors.orange,
                                ),
                                const SizedBox(width: 10),
                                Expanded(
                                  child: Text(
                                    '$pendentes ${pendentes == 1 ? "captura não sincronizada" : "capturas não sincronizadas"}',
                                    style: const TextStyle(
                                      color: Colors.orange,
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                ),
                                const Icon(
                                  Icons.chevron_right,
                                  color: Colors.orange,
                                ),
                              ],
                            ),
                          ),
                        ),

                      // Saudação
                      Text(
                        'Olá, ${auth.usuario?.nome ?? ''}!',
                        style: Theme.of(context).textTheme.titleLarge,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Perfil: ${config?.labelSupervisor ?? 'Fiscal'}',
                        style: Theme.of(
                          context,
                        ).textTheme.bodyMedium?.copyWith(color: Colors.grey),
                      ),
                      const SizedBox(height: 24),

                      // Embarcações
                      if (minhasEquipes.isNotEmpty) ...[
                        Text(
                          minhasEquipes.length == 1
                              ? (config?.labelEquipe ?? 'Equipe')
                              : '${config?.labelEquipePlural ?? "${config?.labelEquipe ?? "Equipes"}s"} (${minhasEquipes.length})',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                        const SizedBox(height: 8),
                        ...minhasEquipes.map(
                          (e) => _EquipeCard(
                            equipe: e,
                            labelMembro: config?.labelMembro ?? 'membro',
                            exibirContagem: exibirContagem,
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
                            label:
                                'Registrar\n${config?.labelCaptura ?? "Captura"}',
                            color: Colors.green,
                            onTap:
                                () => Navigator.pushNamed(
                                  context,
                                  '/fiscal/registrar',
                                ),
                          ),
                          _ActionItem(
                            icon: Icons.list_alt,
                            label:
                                '${config?.labelCaptura ?? "Capturas"}\nRegistradas',
                            color: Colors.blue,
                            onTap:
                                () => Navigator.pushNamed(
                                  context,
                                  '/fiscal/capturas',
                                ),
                          ),
                          _ActionItem(
                            icon: Icons.sync,
                            label:
                                'Sincronizar\n${pendentes > 0 ? "($pendentes)" : ""}',
                            color: pendentes > 0 ? Colors.orange : Colors.grey,
                            onTap:
                                () => Navigator.pushNamed(
                                  context,
                                  '/fiscal/sync',
                                ),
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

class _EquipeCard extends StatelessWidget {
  final Equipe equipe;
  final String labelMembro;
  final bool exibirContagem;

  const _EquipeCard({
    required this.equipe,
    required this.labelMembro,
    required this.exibirContagem,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                ExpandableAvatar(
                  imageUrl: AppConfig.resolverUrl(equipe.fotoUrl),
                  fallbackIcon: Icons.directions_boat,
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    equipe.nome,
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 4),
            Text('Capitão: ${equipe.capitao}'),
            if (exibirContagem)
              Text(
                '${equipe.qtdMembros}/${equipe.qtdVagas} $labelMembro${equipe.qtdVagas != 1 ? "s" : ""}',
                style: const TextStyle(color: Colors.grey),
              ),
          ],
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
              style: TextStyle(
                fontSize: 11,
                color: color,
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
