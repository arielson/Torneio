import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';

class HomeAdminScreen extends StatefulWidget {
  const HomeAdminScreen({super.key});

  @override
  State<HomeAdminScreen> createState() => _HomeAdminScreenState();
}

class _HomeAdminScreenState extends State<HomeAdminScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _carregar());
  }

  Future<void> _carregar() async {
    final auth = context.read<AuthProvider>();
    final config = context.read<ConfigProvider>().config;
    if (auth.usuario == null || config == null) return;
  }

  Future<void> _logout() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Sair'),
        content: const Text('Deseja encerrar a sessão?'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Cancelar')),
          FilledButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Sair')),
        ],
      ),
    );
    if (confirm == true && mounted) {
      context.read<AuthProvider>().logout();
      Navigator.pushReplacementNamed(context, '/home');
    }
  }

  void _abrirSecao(String rota) {
    Navigator.pushNamed(context, rota);
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final configProv = context.watch<ConfigProvider>();
    final config = configProv.config;

    final labelEquipe = config?.labelEquipe ?? 'Equipe';
    final labelMembro = config?.labelMembro ?? 'Membro';
    final labelItem = config?.labelItem ?? 'Item';
    final labelFiscal = config?.labelSupervisor ?? 'Fiscal';
    final labelCaptura = config?.labelCaptura ?? 'Captura';

    return Scaffold(
      appBar: AppBar(
        title: Text(config?.nomeTorneio ?? 'Admin'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Sair',
            onPressed: _logout,
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _carregar,
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Olá, ${auth.usuario?.nome ?? ''}',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              Text(
                'Administrador do torneio',
                style: Theme.of(context)
                    .textTheme
                    .bodyMedium
                    ?.copyWith(color: Colors.grey),
              ),

              if (config != null) ...[
                const SizedBox(height: 12),
                _StatusBadge(status: config.status),
              ],

              const SizedBox(height: 20),

              Text('Cadastros', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              _NavGrid(items: [
                _NavItem(
                  icon: Icons.groups,
                  label: '${labelEquipe}s',
                  color: Colors.indigo,
                  onTap: () => _abrirSecao('/admin/equipes'),
                ),
                _NavItem(
                  icon: Icons.person,
                  label: '${labelMembro}s',
                  color: Colors.teal,
                  onTap: () => _abrirSecao('/admin/membros'),
                ),
                _NavItem(
                  icon: Icons.inventory_2,
                  label: '${labelItem}s',
                  color: Colors.brown,
                  onTap: () => _abrirSecao('/admin/itens'),
                ),
                _NavItem(
                  icon: Icons.badge,
                  label: '${labelFiscal}s',
                  color: Colors.purple,
                  onTap: () => _abrirSecao('/admin/fiscais'),
                ),
              ]),

              const SizedBox(height: 20),

              Text('Operações', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              _NavGrid(items: [
                _NavItem(
                  icon: Icons.list_alt,
                  label: '${labelCaptura}s',
                  color: Colors.green,
                  onTap: () => _abrirSecao('/admin/capturas'),
                ),
                _NavItem(
                  icon: Icons.shuffle,
                  label: 'Sorteio',
                  color: Colors.deepOrange,
                  onTap: () => _abrirSecao('/admin/sorteio'),
                ),
                _NavItem(
                  icon: Icons.picture_as_pdf,
                  label: 'Relatórios',
                  color: Colors.red,
                  onTap: () => _abrirSecao('/admin/relatorios'),
                ),
              ]),

              const SizedBox(height: 24),
            ],
          ),
        ),
      ),
    );
  }
}

// ── Componentes ─────────────────────────────────────────────────────────────

class _StatusBadge extends StatelessWidget {
  final String status;
  const _StatusBadge({required this.status});

  Color get _cor => switch (status) {
        'Liberado' => Colors.green,
        'Finalizado' => Colors.grey,
        _ => Colors.orange,
      };

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(Icons.circle, size: 10, color: _cor),
        const SizedBox(width: 6),
        Text(
          'Status: $status',
          style: TextStyle(color: _cor, fontWeight: FontWeight.w500),
        ),
      ],
    );
  }
}

class _NavGrid extends StatelessWidget {
  final List<_NavItem> items;
  const _NavGrid({required this.items});

  @override
  Widget build(BuildContext context) {
    return GridView.count(
      crossAxisCount: 4,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      crossAxisSpacing: 8,
      mainAxisSpacing: 8,
      children: items,
    );
  }
}

class _NavItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;

  const _NavItem({
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
          color: color.withAlpha(20),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: color.withAlpha(70)),
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, color: color, size: 28),
            const SizedBox(height: 4),
            Text(
              label,
              textAlign: TextAlign.center,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                  fontSize: 10,
                  color: color,
                  fontWeight: FontWeight.w500),
            ),
          ],
        ),
      ),
    );
  }
}
