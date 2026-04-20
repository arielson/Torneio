import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class HomeAdminScreen extends StatefulWidget {
  const HomeAdminScreen({super.key});

  @override
  State<HomeAdminScreen> createState() => _HomeAdminScreenState();
}

class _HomeAdminScreenState extends State<HomeAdminScreen> {
  late final ApiService _api;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _api = context.read<AuthProvider>().api;
  }

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
      context.read<ConfigProvider>().limpar();
      Navigator.pushNamedAndRemoveUntil(context, '/home', (_) => false);
    }
  }

  void _abrirSecao(String rota) {
    Navigator.pushNamed(context, rota);
  }

  Future<void> _alterarStatus({
    required String titulo,
    required String mensagem,
    required String endpoint,
    required String sucesso,
  }) async {
    final auth = context.read<AuthProvider>().usuario;
    final configProvider = context.read<ConfigProvider>();
    final config = configProvider.config;
    if (auth == null || config == null) return;

    final confirm = await showDialog<bool>(
      context: context,
      builder:
          (_) => AlertDialog(
            title: Text(titulo),
            content: Text(mensagem),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context, false),
                child: const Text('Cancelar'),
              ),
              FilledButton(
                onPressed: () => Navigator.pop(context, true),
                child: const Text('Confirmar'),
              ),
            ],
          ),
    );

    if (confirm != true || !mounted) return;

    try {
      await _api.post(endpoint, null, token: auth.token);
      await configProvider.carregarConfig(auth.slug!);
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(sucesso)));
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(e.message)));
    } catch (_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Nao foi possivel alterar o status do torneio.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final config = context.watch<ConfigProvider>().config;

    final labelEquipePlural = config?.labelEquipePlural ?? 'Equipes';
    final labelMembroPlural = config?.labelMembroPlural ?? 'Membros';
    final labelItemPlural = config?.labelItemPlural ?? 'Itens';
    final labelFiscalPlural = config?.labelSupervisorPlural ?? 'Fiscais';
    final labelCapturaPlural = config?.labelCapturaPlural ?? 'Capturas';
    final exibirSorteio = config?.modoSorteio != 'Nenhum';
    final exibirGrupos = config?.modoSorteio == 'GrupoEquipe';
    final exibirReorganizacao = config?.modoSorteio != 'Nenhum';

    return Scaffold(
      appBar: AppBar(
        title: Text(config?.nomeTorneio ?? 'Admin'),
        actions: [
          if (exibirReorganizacao)
            IconButton(
              icon: const Icon(Icons.warning_amber_rounded),
              tooltip: 'Reorganizacao emergencial',
              onPressed: () => _abrirSecao('/admin/reorganizacao-emergencial'),
            ),
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
                style: Theme.of(
                  context,
                ).textTheme.bodyMedium?.copyWith(color: Colors.grey),
              ),
              if (config != null) ...[
                const SizedBox(height: 12),
                _StatusBadge(status: config.status),
                const SizedBox(height: 12),
                _StatusActions(
                  status: config.status,
                  onLiberar:
                      () => _alterarStatus(
                        titulo: 'Liberar torneio',
                        mensagem: 'Deseja alterar o status do torneio para Liberado?',
                        endpoint: ApiConstants.torneioLiberar(config.slug),
                        sucesso: 'Torneio liberado com sucesso.',
                      ),
                  onReabrir:
                      () => _alterarStatus(
                        titulo: 'Voltar para aberto',
                        mensagem: 'Deseja voltar o torneio para o status Aberto?',
                        endpoint: ApiConstants.torneioReabrir(config.slug),
                        sucesso: 'Torneio reaberto com sucesso.',
                      ),
                  onFinalizar:
                      () => _alterarStatus(
                        titulo: 'Finalizar torneio',
                        mensagem: 'Deseja alterar o status do torneio para Finalizado?',
                        endpoint: ApiConstants.torneioFinalizar(config.slug),
                        sucesso: 'Torneio finalizado com sucesso.',
                      ),
                ),
              ],
              const SizedBox(height: 20),
              Text('Cadastros', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              _NavGrid(
                items: [
                  _NavItem(
                    icon: Icons.groups,
                    label: labelEquipePlural,
                    color: Colors.indigo,
                    onTap: () => _abrirSecao('/admin/equipes'),
                  ),
                  _NavItem(
                    icon: Icons.person,
                    label: labelMembroPlural,
                    color: Colors.teal,
                    onTap: () => _abrirSecao('/admin/membros'),
                  ),
                  _NavItem(
                    icon: Icons.inventory_2,
                    label: labelItemPlural,
                    color: Colors.brown,
                    onTap: () => _abrirSecao('/admin/itens'),
                  ),
                  _NavItem(
                    icon: Icons.badge,
                    label: labelFiscalPlural,
                    color: Colors.purple,
                    onTap: () => _abrirSecao('/admin/fiscais'),
                  ),
                  if (exibirGrupos)
                    _NavItem(
                      icon: Icons.people_outline,
                      label: 'Grupos',
                      color: Colors.deepOrange,
                      onTap: () => _abrirSecao('/admin/grupos'),
                    ),
                ],
              ),
              const SizedBox(height: 20),
              Text('Operações', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              _NavGrid(
                items: [
                  _NavItem(
                    icon: Icons.list_alt,
                    label: labelCapturaPlural,
                    color: Colors.green,
                    onTap: () => _abrirSecao('/admin/capturas'),
                  ),
                  if (exibirSorteio)
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
                ],
              ),
              if (exibirReorganizacao) ...[
                const SizedBox(height: 12),
                FilledButton.icon(
                  onPressed:
                      () => _abrirSecao('/admin/reorganizacao-emergencial'),
                  style: FilledButton.styleFrom(
                    backgroundColor: Colors.red,
                    foregroundColor: Colors.white,
                    minimumSize: const Size.fromHeight(48),
                  ),
                  icon: const Icon(Icons.warning_amber_rounded),
                  label: const Text('Abrir Reorganizacao Emergencial'),
                ),
                const SizedBox(height: 12),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.orange.shade50,
                    borderRadius: BorderRadius.circular(10),
                    border: Border.all(color: Colors.orange.shade200),
                  ),
                  child: Text(
                    'A reorganizacao emergencial de ${labelMembroPlural.toLowerCase()} entre ${labelEquipePlural.toLowerCase()} deve ser usada apenas em caso excepcional e exige confirmacao do admin do torneio.',
                    style: TextStyle(color: Colors.orange.shade900),
                  ),
                ),
              ],
              const SizedBox(height: 24),
            ],
          ),
        ),
      ),
    );
  }
}

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

class _StatusActions extends StatelessWidget {
  final String status;
  final VoidCallback onLiberar;
  final VoidCallback onReabrir;
  final VoidCallback onFinalizar;

  const _StatusActions({
    required this.status,
    required this.onLiberar,
    required this.onReabrir,
    required this.onFinalizar,
  });

  @override
  Widget build(BuildContext context) {
    if (status == 'Aberto') {
      return Wrap(
        spacing: 8,
        runSpacing: 8,
        children: [
          FilledButton.icon(
            onPressed: onLiberar,
            icon: const Icon(Icons.play_circle_outline),
            label: const Text('Liberar'),
          ),
        ],
      );
    }

    if (status == 'Liberado') {
      return Wrap(
        spacing: 8,
        runSpacing: 8,
        children: [
          OutlinedButton.icon(
            onPressed: onReabrir,
            icon: const Icon(Icons.arrow_back),
            label: const Text('Voltar para Aberto'),
          ),
          FilledButton.icon(
            onPressed: onFinalizar,
            icon: const Icon(Icons.check_circle_outline),
            label: const Text('Finalizar'),
          ),
        ],
      );
    }

    if (status == 'Finalizado') {
      return Wrap(
        spacing: 8,
        runSpacing: 8,
        children: [
          OutlinedButton.icon(
            onPressed: onReabrir,
            icon: const Icon(Icons.refresh),
            label: const Text('Reabrir'),
          ),
        ],
      );
    }

    return const SizedBox.shrink();
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
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
