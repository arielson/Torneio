import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/membro.dart';
import '../../core/models/produto_extra_membro.dart';
import '../../core/models/produto_extra_torneio.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';

class ExtrasAdminScreen extends StatefulWidget {
  const ExtrasAdminScreen({super.key});

  @override
  State<ExtrasAdminScreen> createState() => _ExtrasAdminScreenState();
}

class _ExtrasAdminScreenState extends State<ExtrasAdminScreen> {
  final _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<ProdutoExtraTorneio> _produtos = const [];

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
      final data = await _api.get(ApiConstants.financeiroExtras(auth!.slug!), token: auth.token);
      if (!mounted) return;
      setState(() {
        _produtos = data is List
            ? data.map((e) => ProdutoExtraTorneio.fromJson(e as Map<String, dynamic>)).toList()
            : <ProdutoExtraTorneio>[];
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar extras.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _abrirFormulario({ProdutoExtraTorneio? produto}) async {
    await showDialog<void>(
      context: context,
      builder: (_) => _ExtraFormDialog(produto: produto),
    );
    if (mounted) await _carregar();
  }

  Future<void> _gerenciarMembros(ProdutoExtraTorneio produto) async {
    await Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => _ExtraMembrosScreen(produto: produto)),
    );
    if (mounted) await _carregar();
  }

  Future<void> _abrirVendaDireta() async {
    if (_produtos.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Cadastre ao menos um produto extra antes de registrar uma venda.')),
      );
      return;
    }

    final produto = await showDialog<ProdutoExtraTorneio>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Registrar venda'),
        content: SizedBox(
          width: 420,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: _produtos
                .where((p) => p.ativo)
                .map(
                  (p) => ListTile(
                    leading: const Icon(Icons.shopping_bag_outlined),
                    title: Text(p.nome),
                    subtitle: Text('Valor unitario: R\$ ${p.valor.toStringAsFixed(2)}'),
                    onTap: () => Navigator.pop(context, p),
                  ),
                )
                .toList(),
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('Fechar')),
        ],
      ),
    );

    if (produto == null || !mounted) return;
    await _gerenciarMembros(produto);
  }

  Future<void> _remover(ProdutoExtraTorneio produto) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Remover extra'),
        content: Text('Deseja remover "${produto.nome}"?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Remover')),
        ],
      ),
    );
    if (confirmar != true) return;

    try {
      await _api.delete('${ApiConstants.financeiroExtras(auth!.slug!)}/${produto.id}', token: auth.token);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Produto extra removido.')));
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.message), backgroundColor: Colors.red));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Produtos extras')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: const Text('Novo extra'),
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
                      child: ListTile(
                        leading: const Icon(Icons.point_of_sale_outlined),
                        title: const Text('Registrar venda'),
                        subtitle: const Text('Escolha o produto extra, informe a quantidade e gere a cobranca do pescador.'),
                        trailing: const Icon(Icons.chevron_right),
                        onTap: _abrirVendaDireta,
                      ),
                    ),
                  if (_erro == null && _produtos.isEmpty)
                    const Padding(
                      padding: EdgeInsets.only(top: 24),
                      child: Text('Nenhum produto extra cadastrado.', textAlign: TextAlign.center),
                    ),
                  ..._produtos.map((produto) => Card(
                        margin: const EdgeInsets.only(top: 12),
                        child: Padding(
                          padding: const EdgeInsets.all(16),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  Expanded(
                                    child: Text(produto.nome, style: Theme.of(context).textTheme.titleMedium),
                                  ),
                                  Chip(label: Text(produto.ativo ? 'Ativo' : 'Inativo')),
                                ],
                              ),
                              Text('Valor: R\$ ${produto.valor.toStringAsFixed(2)}'),
                              Text('Vendas: ${produto.quantidadeAderidos}'),
                              if ((produto.descricao ?? '').trim().isNotEmpty)
                                Padding(
                                  padding: const EdgeInsets.only(top: 6),
                                  child: Text(produto.descricao!),
                                ),
                              const SizedBox(height: 12),
                              Wrap(
                                spacing: 8,
                                runSpacing: 8,
                                children: [
                                  OutlinedButton.icon(
                                    onPressed: () => _gerenciarMembros(produto),
                                    icon: const Icon(Icons.point_of_sale_outlined),
                                    label: const Text('Vendas'),
                                  ),
                                  OutlinedButton.icon(
                                    onPressed: () => _abrirFormulario(produto: produto),
                                    icon: const Icon(Icons.edit),
                                    label: const Text('Editar'),
                                  ),
                                  OutlinedButton.icon(
                                    onPressed: () => _remover(produto),
                                    icon: const Icon(Icons.delete_outline),
                                    label: const Text('Remover'),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      )),
                ],
              ),
            ),
    );
  }
}

class _ExtraFormDialog extends StatefulWidget {
  final ProdutoExtraTorneio? produto;

  const _ExtraFormDialog({this.produto});

  @override
  State<_ExtraFormDialog> createState() => _ExtraFormDialogState();
}

class _ExtraFormDialogState extends State<_ExtraFormDialog> {
  final _api = ApiService();
  final _nomeController = TextEditingController();
  final _valorController = TextEditingController(text: '0');
  final _descricaoController = TextEditingController();
  bool _ativo = true;
  bool _salvando = false;

  bool get _editando => widget.produto != null;

  @override
  void initState() {
    super.initState();
    final produto = widget.produto;
    if (produto != null) {
      _nomeController.text = produto.nome;
      _valorController.text = produto.valor.toStringAsFixed(2);
      _descricaoController.text = produto.descricao ?? '';
      _ativo = produto.ativo;
    }
  }

  Future<void> _salvar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    final valor = double.tryParse(_valorController.text.replaceAll(',', '.'));
    if (_nomeController.text.trim().isEmpty || valor == null || valor < 0) return;

    setState(() => _salvando = true);
    try {
      final body = {
        'nome': _nomeController.text.trim(),
        'valor': valor,
        'descricao': _descricaoController.text.trim(),
        'ativo': _ativo,
      };
      if (_editando) {
        await _api.put('${ApiConstants.financeiroExtras(auth!.slug!)}/${widget.produto!.id}', body, token: auth.token);
      } else {
        await _api.post(ApiConstants.financeiroExtras(auth!.slug!), body, token: auth.token);
      }
      if (!mounted) return;
      Navigator.pop(context);
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.message), backgroundColor: Colors.red));
    } finally {
      if (mounted) setState(() => _salvando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(_editando ? 'Editar extra' : 'Novo extra'),
      content: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(controller: _nomeController, decoration: const InputDecoration(labelText: 'Nome')),
            const SizedBox(height: 12),
            TextField(
              controller: _valorController,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: const InputDecoration(labelText: 'Valor'),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _descricaoController,
              maxLines: 3,
              decoration: const InputDecoration(labelText: 'Descricao'),
            ),
            if (_editando)
              SwitchListTile(
                value: _ativo,
                contentPadding: EdgeInsets.zero,
                title: const Text('Ativo'),
                onChanged: (value) => setState(() => _ativo = value),
              ),
          ],
        ),
      ),
      actions: [
        TextButton(onPressed: _salvando ? null : () => Navigator.pop(context), child: const Text('Cancelar')),
        FilledButton(onPressed: _salvando ? null : _salvar, child: Text(_salvando ? 'Salvando...' : 'Salvar')),
      ],
    );
  }

  @override
  void dispose() {
    _nomeController.dispose();
    _valorController.dispose();
    _descricaoController.dispose();
    super.dispose();
  }
}

class _ExtraMembrosScreen extends StatefulWidget {
  final ProdutoExtraTorneio produto;

  const _ExtraMembrosScreen({required this.produto});

  @override
  State<_ExtraMembrosScreen> createState() => _ExtraMembrosScreenState();
}

class _ExtraMembrosScreenState extends State<_ExtraMembrosScreen> {
  final _api = ApiService();
  List<ProdutoExtraMembro> _aderidos = const [];
  List<Membro> _membros = const [];
  bool _carregando = true;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    setState(() => _carregando = true);
    try {
      final aderidosData = await _api.get(
        ApiConstants.financeiroExtraMembros(auth!.slug!, widget.produto.id),
        token: auth.token,
      );
      final membrosData = await _api.get(ApiConstants.membros(auth.slug!), token: auth.token);
      if (!mounted) return;
      setState(() {
        _aderidos = aderidosData is List
            ? aderidosData.map((e) => ProdutoExtraMembro.fromJson(e as Map<String, dynamic>)).toList()
            : <ProdutoExtraMembro>[];
        _membros = membrosData is List
            ? membrosData.map((e) => Membro.fromJson(e as Map<String, dynamic>)).toList()
            : <Membro>[];
      });
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _adicionar() async {
    final resultado = await showDialog<_NovaAdesaoExtraResultado>(
      context: context,
      builder: (_) => _SelecionarMembroDialog(
        membros: _membros.where((m) => !_aderidos.any((a) => a.membroId == m.id)).toList(),
        valorPadrao: widget.produto.valor,
      ),
    );
    if (!mounted) return;
    if (resultado == null) return;
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    try {
      await _api.post(
        ApiConstants.financeiroExtraMembros(auth!.slug!, widget.produto.id),
        {
          'membroId': resultado.membroId,
          'quantidade': resultado.quantidade,
          'valorCobrado': resultado.valorCobrado,
          'observacao': resultado.observacao,
        },
        token: auth.token,
      );
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.message), backgroundColor: Colors.red));
      return;
    }
    await _carregar();
  }

  Future<void> _remover(ProdutoExtraMembro item) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    try {
      await _api.delete(ApiConstants.financeiroRemoverExtraMembro(auth!.slug!, item.id), token: auth.token);
      if (!mounted) return;
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.message), backgroundColor: Colors.red));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Vendas de ${widget.produto.nome}'),
        actions: [IconButton(onPressed: _adicionar, icon: const Icon(Icons.add_shopping_cart_outlined))],
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : ListView(
              padding: const EdgeInsets.all(16),
              children: [
                if (_aderidos.isEmpty)
                  const Text('Nenhuma venda registrada para este produto extra.', textAlign: TextAlign.center),
                ..._aderidos.map((item) => Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      child: ListTile(
                        title: Text(item.nomeMembro),
                        subtitle: Text(
                          'Quantidade: ${item.quantidade.toStringAsFixed(2)}\nValor: R\$ ${item.valorCobrado.toStringAsFixed(2)}',
                        ),
                        isThreeLine: true,
                        trailing: IconButton(
                          onPressed: () => _remover(item),
                          icon: const Icon(Icons.delete_outline),
                        ),
                      ),
                    )),
              ],
            ),
    );
  }

}

class _SelecionarMembroDialog extends StatefulWidget {
  final List<Membro> membros;
  final double valorPadrao;

  const _SelecionarMembroDialog({required this.membros, required this.valorPadrao});

  @override
  State<_SelecionarMembroDialog> createState() => _SelecionarMembroDialogState();
}

class _SelecionarMembroDialogState extends State<_SelecionarMembroDialog> {
  String? _membroId;
  late final TextEditingController _quantidadeController;
  late final TextEditingController _valorController;
  final _observacaoController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _quantidadeController = TextEditingController(text: '1');
    _valorController = TextEditingController(text: widget.valorPadrao.toStringAsFixed(2));
  }

  void _atualizarValor() {
    final quantidade = double.tryParse(_quantidadeController.text.replaceAll(',', '.')) ?? 0;
    final valorTotal = widget.valorPadrao * quantidade;
    _valorController.text = valorTotal.toStringAsFixed(2);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Registrar venda'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          DropdownButtonFormField<String>(
            initialValue: _membroId,
            items: widget.membros
                .map((m) => DropdownMenuItem<String>(value: m.id, child: Text(m.nome)))
                .toList(),
            onChanged: (value) => setState(() => _membroId = value),
            decoration: const InputDecoration(labelText: 'Pescador'),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _quantidadeController,
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            onChanged: (_) => _atualizarValor(),
            decoration: const InputDecoration(labelText: 'Quantidade'),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _valorController,
            readOnly: true,
            decoration: const InputDecoration(
              labelText: 'Valor total',
              helperText: 'Calculado automaticamente a partir de valor unitario x quantidade.',
            ),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _observacaoController,
            decoration: const InputDecoration(labelText: 'Observacao'),
          ),
        ],
      ),
      actions: [
        TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancelar')),
        FilledButton(
          onPressed: () {
            final quantidade = double.tryParse(_quantidadeController.text.replaceAll(',', '.'));
            final valor = double.tryParse(_valorController.text.replaceAll(',', '.'));
            if (_membroId == null || quantidade == null || quantidade <= 0 || valor == null || valor < 0) return;
            Navigator.pop(
              context,
              _NovaAdesaoExtraResultado(
                membroId: _membroId!,
                quantidade: quantidade,
                valorCobrado: valor,
                observacao: _observacaoController.text.trim(),
              ),
            );
          },
          child: const Text('Vender'),
        ),
      ],
    );
  }

  @override
  void dispose() {
    _quantidadeController.dispose();
    _valorController.dispose();
    _observacaoController.dispose();
    super.dispose();
  }
}

class _NovaAdesaoExtraResultado {
  final String membroId;
  final double quantidade;
  final double valorCobrado;
  final String observacao;

  const _NovaAdesaoExtraResultado({
    required this.membroId,
    required this.quantidade,
    required this.valorCobrado,
    required this.observacao,
  });
}
