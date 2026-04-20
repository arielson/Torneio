import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/flavor_config.dart';
import '../../core/models/fiscal.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import '../../widgets/expandable_network_image.dart';
import 'fiscal_form_screen.dart';

class FiscaisAdminScreen extends StatefulWidget {
  const FiscaisAdminScreen({super.key});

  @override
  State<FiscaisAdminScreen> createState() => _FiscaisAdminScreenState();
}

class _FiscaisAdminScreenState extends State<FiscaisAdminScreen> {
  final ApiService _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<Fiscal> _fiscais = const [];

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final data = await _api.get(ApiConstants.fiscais(auth!.slug!), token: auth.token);
      final lista = data is List
          ? data.map((e) => Fiscal.fromJson(e as Map<String, dynamic>)).toList()
          : <Fiscal>[];
      setState(() => _fiscais = lista..sort((a, b) => a.nome.compareTo(b.nome)));
    } on ApiException catch (e) {
      setState(() => _erro = e.message);
    } catch (_) {
      setState(() => _erro = 'Erro ao carregar dados.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _abrirFormulario({Fiscal? fiscal}) async {
    await Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => FiscalFormScreen(fiscal: fiscal)),
    );
    if (mounted) await _carregar();
  }

  Future<void> _remover(Fiscal fiscal) async {
    final config = context.read<ConfigProvider>().config;
    final auth = context.read<AuthProvider>().usuario;
    if (config == null || auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: Text('Remover ${config.labelSupervisor.toLowerCase()}'),
        content: Text('Deseja remover ${fiscal.nome}?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Remover'),
          ),
        ],
      ),
    );

    if (confirmar != true) return;

    try {
      await _api.delete('${ApiConstants.fiscais(auth!.slug!)}/${fiscal.id}', token: auth.token);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('${config.labelSupervisor} removido com sucesso.')),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final label = config?.labelSupervisor ?? 'Fiscal';
    final labelPlural = config?.labelSupervisorPlural ?? 'Fiscais';

    return Scaffold(
      appBar: AppBar(title: Text(labelPlural)),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: Text('Novo $label'),
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: _erro != null
                  ? ListView(
                      children: [
                        Padding(
                          padding: const EdgeInsets.all(24),
                          child: Text(_erro!, textAlign: TextAlign.center),
                        ),
                      ],
                    )
                  : _fiscais.isEmpty
                      ? ListView(
                          children: [
                            Padding(
                              padding: const EdgeInsets.all(24),
                              child: Text(
                                'Nenhum ${label.toLowerCase()} cadastrado.',
                                textAlign: TextAlign.center,
                              ),
                            ),
                          ],
                        )
                      : ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: _fiscais.length,
                          separatorBuilder: (context, index) =>
                              const SizedBox(height: 12),
                          itemBuilder: (context, index) {
                            final fiscal = _fiscais[index];
                            final fotoUrl = AppConfig.resolverUrl(fiscal.fotoUrl);
                            return Card(
                              child: ListTile(
                                leading: ExpandableAvatar(
                                  imageUrl: fotoUrl,
                                  fallbackIcon: Icons.badge,
                                ),
                                title: Text(fiscal.nome),
                                subtitle: Text(fiscal.usuario),
                                trailing: Wrap(
                                  spacing: 8,
                                  children: [
                                    IconButton(
                                      onPressed: () => _abrirFormulario(fiscal: fiscal),
                                      icon: const Icon(Icons.edit),
                                      tooltip: 'Editar',
                                    ),
                                    IconButton(
                                      onPressed: () => _remover(fiscal),
                                      icon: const Icon(Icons.delete_outline),
                                      tooltip: 'Remover',
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
            ),
    );
  }
}
