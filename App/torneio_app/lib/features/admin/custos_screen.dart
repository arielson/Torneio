import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/custo_torneio.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';

class CustosAdminScreen extends StatefulWidget {
  const CustosAdminScreen({super.key});

  @override
  State<CustosAdminScreen> createState() => _CustosAdminScreenState();
}

class _CustosAdminScreenState extends State<CustosAdminScreen> {
  static const _categorias = [
    'Camisas',
    'Alimentacao',
    'Combustivel',
    'Trofeus',
    'Outros',
  ];

  final _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<CustoTorneio> _custos = const [];

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
      final data = await _api.get(ApiConstants.financeiroCustos(auth!.slug!), token: auth.token);
      if (!mounted) return;
      setState(() {
        _custos = data is List
            ? data.map((e) => CustoTorneio.fromJson(e as Map<String, dynamic>)).toList()
            : <CustoTorneio>[];
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar custos.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _abrirFormulario({CustoTorneio? custo}) async {
    final descricaoController = TextEditingController(text: custo?.descricao ?? '');
    final quantidadeController = TextEditingController(
      text: custo != null ? custo.quantidade.toStringAsFixed(2) : '1',
    );
    final valorUnitarioController = TextEditingController(
      text: custo != null ? custo.valorUnitario.toStringAsFixed(2) : '0',
    );
    final responsavelController = TextEditingController(text: custo?.responsavel ?? '');
    final observacaoController = TextEditingController(text: custo?.observacao ?? '');
    String categoria = _categorias.contains(custo?.categoria) ? custo!.categoria : 'Outros';

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => StatefulBuilder(
        builder: (context, setStateDialog) => AlertDialog(
          title: Text(custo == null ? 'Novo custo' : 'Editar custo'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                DropdownButtonFormField<String>(
                  initialValue: categoria,
                  decoration: const InputDecoration(
                    labelText: 'Categoria',
                    border: OutlineInputBorder(),
                  ),
                  items: _categorias
                      .map((c) => DropdownMenuItem(value: c, child: Text(c)))
                      .toList(),
                  onChanged: (value) => setStateDialog(() => categoria = value ?? categoria),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: descricaoController,
                  decoration: const InputDecoration(
                    labelText: 'Descricao',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: quantidadeController,
                  keyboardType: const TextInputType.numberWithOptions(decimal: true),
                  decoration: const InputDecoration(
                    labelText: 'Quantidade',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: valorUnitarioController,
                  keyboardType: const TextInputType.numberWithOptions(decimal: true),
                  decoration: const InputDecoration(
                    labelText: 'Valor unitario',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: responsavelController,
                  decoration: const InputDecoration(
                    labelText: 'Responsavel',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: observacaoController,
                  maxLines: 3,
                  decoration: const InputDecoration(
                    labelText: 'Observacao',
                    border: OutlineInputBorder(),
                  ),
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

    if (confirmar != true) return;
    if (!mounted) return;

    final quantidade = double.tryParse(quantidadeController.text.replaceAll(',', '.'));
    final valorUnitario = double.tryParse(valorUnitarioController.text.replaceAll(',', '.'));
    if (descricaoController.text.trim().isEmpty || quantidade == null || quantidade <= 0 || valorUnitario == null || valorUnitario < 0) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Preencha descricao, quantidade e valor unitario corretamente.')),
      );
      return;
    }

    if (!mounted) return;
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final body = {
      'categoria': categoria,
      'descricao': descricaoController.text.trim(),
      'quantidade': quantidade,
      'valorUnitario': valorUnitario,
      'responsavel': responsavelController.text.trim(),
      'observacao': observacaoController.text.trim(),
    };

    try {
      if (custo == null) {
        await _api.post(ApiConstants.financeiroCustos(auth!.slug!), body, token: auth.token);
      } else {
        await _api.put('${ApiConstants.financeiroCustos(auth!.slug!)}/${custo.id}', body, token: auth.token);
      }
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(custo == null ? 'Custo criado.' : 'Custo atualizado.')),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  Future<void> _remover(CustoTorneio custo) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Remover custo'),
        content: Text('Deseja remover ${custo.descricao}?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Remover')),
        ],
      ),
    );

    if (confirmar != true) return;

    try {
      await _api.delete('${ApiConstants.financeiroCustos(auth!.slug!)}/${custo.id}', token: auth.token);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Custo removido.')),
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
      appBar: AppBar(title: const Text('Custos')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: const Text('Novo custo'),
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  if (_erro != null) Text(_erro!, textAlign: TextAlign.center),
                  if (_erro == null)
                    Card(
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Text(
                          'Os custos das embarcacoes sao derivados do cadastro de embarcacoes. Os demais custos podem ser lancados aqui.',
                          style: Theme.of(context).textTheme.bodyMedium,
                        ),
                      ),
                    ),
                  if (_erro == null && _custos.isEmpty)
                    const Padding(
                      padding: EdgeInsets.only(top: 24),
                      child: Text('Nenhum custo lancado.', textAlign: TextAlign.center),
                    ),
                  ..._custos.map(
                    (custo) => Card(
                      margin: const EdgeInsets.only(top: 12),
                      child: ListTile(
                        title: Text(custo.descricao),
                        subtitle: Text(
                          '${custo.categoria} - ${custo.valorTotalFormatado}${(custo.observacao ?? '').trim().isNotEmpty ? '\n${custo.observacao}' : ''}',
                        ),
                        isThreeLine: (custo.observacao ?? '').trim().isNotEmpty,
                        trailing: custo.derivadoDaEmbarcacao
                            ? const Chip(label: Text('Embarcacao'))
                            : Wrap(
                                spacing: 4,
                                children: [
                                  IconButton(
                                    onPressed: () => _abrirFormulario(custo: custo),
                                    icon: const Icon(Icons.edit),
                                  ),
                                  IconButton(
                                    onPressed: () => _remover(custo),
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
