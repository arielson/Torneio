import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/checklist_torneio_item.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';

class ChecklistAdminScreen extends StatefulWidget {
  const ChecklistAdminScreen({super.key});

  @override
  State<ChecklistAdminScreen> createState() => _ChecklistAdminScreenState();
}

class _ChecklistAdminScreenState extends State<ChecklistAdminScreen> {
  final _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<ChecklistTorneioItem> _itens = const [];
  List<String> _admins = const [];

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
      final data = await _api.get(ApiConstants.financeiroChecklist(auth!.slug!), token: auth.token);
      final adminsData = await _api.get(ApiConstants.adminsTorneio(auth.slug!), token: auth.token);
      if (!mounted) return;
      setState(() {
        _itens = data is List
            ? data.map((e) => ChecklistTorneioItem.fromJson(e as Map<String, dynamic>)).toList()
            : <ChecklistTorneioItem>[];
        _admins = adminsData is List
            ? (adminsData
                .map((e) => (e as Map<String, dynamic>)['nome'] as String? ?? '')
                .where((nome) => nome.trim().isNotEmpty)
                .toSet()
                .toList()
              ..sort()
            )
            : <String>[];
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar checklist.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _abrirFormulario({ChecklistTorneioItem? item}) async {
    final itemController = TextEditingController(text: item?.item ?? '');
    String? responsavel = item?.responsavel;
    bool concluido = item?.concluido ?? false;
    DateTime? data = item?.data;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => StatefulBuilder(
        builder: (context, setStateDialog) => AlertDialog(
          title: Text(item == null ? 'Novo item' : 'Editar item'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: itemController,
                  decoration: const InputDecoration(
                    labelText: 'Item',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                DropdownButtonFormField<String?>(
                  initialValue: _admins.contains(responsavel) ? responsavel : null,
                  decoration: const InputDecoration(
                    labelText: 'Responsável',
                    border: OutlineInputBorder(),
                  ),
                  items: [
                    const DropdownMenuItem<String?>(
                      value: null,
                      child: Text('Selecione'),
                    ),
                    ..._admins.map(
                      (admin) => DropdownMenuItem<String?>(
                        value: admin,
                        child: Text(admin),
                      ),
                    ),
                  ],
                  onChanged: (value) => responsavel = value,
                ),
                const SizedBox(height: 12),
                OutlinedButton.icon(
                  onPressed: () async {
                    final agora = DateTime.now();
                    final selecionada = await showDatePicker(
                      context: dialogContext,
                      initialDate: data ?? agora,
                      firstDate: DateTime(agora.year - 5),
                      lastDate: DateTime(agora.year + 10),
                    );
                    if (selecionada != null) {
                      setStateDialog(() => data = selecionada);
                    }
                  },
                  icon: const Icon(Icons.calendar_month_outlined),
                  label: Text(
                    data == null ? 'Selecionar data' : 'Data: ${DateFormat('dd/MM/yyyy').format(data!)}',
                  ),
                ),
                SwitchListTile(
                  value: concluido,
                  contentPadding: EdgeInsets.zero,
                  title: const Text('Concluido'),
                  onChanged: (value) => setStateDialog(() => concluido = value),
                ),
              ],
            ),
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(dialogContext, false), child: const Text('Cancelar')),
            FilledButton(onPressed: () => Navigator.pop(dialogContext, true), child: const Text('Salvar')),
          ],
        ),
      ),
    );

    if (confirmar != true || itemController.text.trim().isEmpty) return;
    if (!mounted) return;

    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final body = {
      'item': itemController.text.trim(),
      'data': data?.toIso8601String(),
      'responsavel': responsavel,
      'concluido': concluido,
    };

    try {
      if (item == null) {
        await _api.post(ApiConstants.financeiroChecklist(auth!.slug!), body, token: auth.token);
      } else {
        await _api.put('${ApiConstants.financeiroChecklist(auth!.slug!)}/${item.id}', body, token: auth.token);
      }
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(item == null ? 'Item criado.' : 'Item atualizado.')),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  Future<void> _remover(ChecklistTorneioItem item) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Remover item'),
        content: Text('Deseja remover "${item.item}"?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Remover')),
        ],
      ),
    );

    if (confirmar != true) return;

    try {
      await _api.delete('${ApiConstants.financeiroChecklist(auth!.slug!)}/${item.id}', token: auth.token);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Item removido.')),
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
    return Scaffold(
      appBar: AppBar(title: const Text('Checklist')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: const Text('Novo item'),
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  if (_erro != null) Text(_erro!, textAlign: TextAlign.center),
                  if (_erro == null && _itens.isEmpty)
                    const Padding(
                      padding: EdgeInsets.only(top: 24),
                      child: Text('Nenhum item cadastrado.', textAlign: TextAlign.center),
                    ),
                  ..._itens.map(
                    (item) => Card(
                      margin: const EdgeInsets.only(top: 12),
                      child: ListTile(
                        title: Text(item.item),
                        subtitle: Text(
                          '${item.responsavel ?? '-'}${item.data != null ? ' - ${DateFormat('dd/MM/yyyy').format(item.data!)}' : ''}',
                        ),
                        trailing: Wrap(
                          spacing: 4,
                          children: [
                            Chip(
                              label: Text(item.concluido ? 'Concluido' : 'Pendente'),
                              backgroundColor: item.concluido ? const Color(0xFFDFF5E1) : const Color(0xFFFFF1D6),
                            ),
                            IconButton(
                              onPressed: () => _abrirFormulario(item: item),
                              icon: const Icon(Icons.edit),
                            ),
                            IconButton(
                              onPressed: () => _remover(item),
                              icon: const Icon(Icons.delete_outline),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}
