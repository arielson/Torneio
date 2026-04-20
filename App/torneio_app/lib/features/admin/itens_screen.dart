import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/flavor_config.dart';
import '../../core/models/item.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import '../../widgets/expandable_network_image.dart';
import 'item_form_screen.dart';

class ItensAdminScreen extends StatefulWidget {
  const ItensAdminScreen({super.key});

  @override
  State<ItensAdminScreen> createState() => _ItensAdminScreenState();
}

class _ItensAdminScreenState extends State<ItensAdminScreen> {
  final ApiService _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<Item> _itens = const [];

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
      final data = await _api.get(ApiConstants.itens(auth!.slug!), token: auth.token);
      final lista = data is List
          ? data.map((e) => Item.fromJson(e as Map<String, dynamic>)).toList()
          : <Item>[];
      setState(() {
        _itens = lista..sort((a, b) => a.nome.compareTo(b.nome));
      });
    } on ApiException catch (e) {
      setState(() => _erro = e.message);
    } catch (_) {
      setState(() => _erro = 'Erro ao carregar dados.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _abrirFormulario({Item? item}) async {
    await Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => ItemFormScreen(item: item)),
    );
    if (mounted) await _carregar();
  }

  Future<void> _remover(Item item) async {
    final config = context.read<ConfigProvider>().config;
    final auth = context.read<AuthProvider>().usuario;
    if (config == null || auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: Text('Remover ${config.labelItem.toLowerCase()}'),
        content: Text('Deseja remover ${item.nome}?'),
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
      await _api.delete('${ApiConstants.itens(auth!.slug!)}/${item.id}', token: auth.token);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('${config.labelItem} removido com sucesso.')),
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
    final label = config?.labelItem ?? 'Item';
    final labelPlural = config?.labelItemPlural ?? 'Itens';
    final medida = config?.medidaCaptura ?? 'cm';
    final usarFator = config?.usarFatorMultiplicador ?? false;

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
                  : _itens.isEmpty
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
                          itemCount: _itens.length,
                          separatorBuilder: (context, index) =>
                              const SizedBox(height: 12),
                          itemBuilder: (context, index) {
                            final item = _itens[index];
                            final fotoUrl = AppConfig.resolverUrl(item.fotoUrl);
                            final detalhes = <String>[
                              if (item.comprimento != null)
                                'Comprimento min.: ${item.comprimento!.toStringAsFixed(1)} $medida',
                              if (usarFator)
                                'Fator: ${item.fatorMultiplicador.toStringAsFixed(2)}',
                            ];

                            return Card(
                              child: ListTile(
                                leading: ExpandableAvatar(
                                  imageUrl: fotoUrl,
                                  fallbackIcon: Icons.set_meal_outlined,
                                ),
                                title: Text(item.nome),
                                subtitle: Text(
                                  detalhes.isEmpty
                                      ? 'Sem regras adicionais'
                                      : detalhes.join(' - '),
                                ),
                                trailing: Wrap(
                                  spacing: 8,
                                  children: [
                                    IconButton(
                                      onPressed: () => _abrirFormulario(item: item),
                                      icon: const Icon(Icons.edit),
                                      tooltip: 'Editar',
                                    ),
                                    IconButton(
                                      onPressed: () => _remover(item),
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
