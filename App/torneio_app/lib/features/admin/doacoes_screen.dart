import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/doacao_patrocinador.dart';
import '../../core/models/patrocinador.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';

class DoacoesAdminScreen extends StatefulWidget {
  const DoacoesAdminScreen({super.key});

  @override
  State<DoacoesAdminScreen> createState() => _DoacoesAdminScreenState();
}

class _DoacoesAdminScreenState extends State<DoacoesAdminScreen> {
  final _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<DoacaoPatrocinador> _doacoes = const [];
  List<Patrocinador> _patrocinadores = const [];

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
      final doacoesData = await _api.get(
        ApiConstants.financeiroDoacoes(auth!.slug!),
        token: auth.token,
      );
      final patrocinadoresData = await _api.get(
        ApiConstants.patrocinadores(auth.slug!),
        token: auth.token,
      );
      if (!mounted) return;
      setState(() {
        _doacoes = doacoesData is List
            ? doacoesData
                .map((e) => DoacaoPatrocinador.fromJson(e as Map<String, dynamic>))
                .toList()
            : <DoacaoPatrocinador>[];
        _patrocinadores = patrocinadoresData is List
            ? patrocinadoresData
                .map((e) => Patrocinador.fromJson(e as Map<String, dynamic>))
                .toList()
            : <Patrocinador>[];
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar doa\u00e7\u00f5es.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _abrirFormulario({DoacaoPatrocinador? doacao}) async {
    await showDialog<void>(
      context: context,
      builder: (_) => _DoacaoFormDialog(
        doacao: doacao,
        patrocinadores: _patrocinadores,
      ),
    );
    if (mounted) await _carregar();
  }

  Future<void> _remover(DoacaoPatrocinador doacao) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Remover doa\u00e7\u00e3o'),
        content: Text('Deseja remover a doa\u00e7\u00e3o de ${doacao.nomePatrocinador}?'),
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
      await _api.delete(
        '${ApiConstants.financeiroDoacoes(auth!.slug!)}/${doacao.id}',
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Doa\u00e7\u00e3o removida.')),
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
    final moeda = NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$');
    final dataFormatada = DateFormat('dd/MM/yyyy');

    return Scaffold(
      appBar: AppBar(title: const Text('Doa\u00e7\u00f5es')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: const Text('Nova doa\u00e7\u00e3o'),
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  if (_erro != null) Text(_erro!, textAlign: TextAlign.center),
                  if (_erro == null && _doacoes.isEmpty)
                    const Padding(
                      padding: EdgeInsets.only(top: 24),
                      child: Text(
                        'Nenhuma doa\u00e7\u00e3o registrada.',
                        textAlign: TextAlign.center,
                      ),
                    ),
                  ..._doacoes.map(
                    (doacao) => Card(
                      margin: const EdgeInsets.only(top: 12),
                      child: ListTile(
                        title: Text(doacao.nomePatrocinador),
                        subtitle: Text(
                          '${doacao.tipo} - ${doacao.descricao}\n'
                          'Data: ${dataFormatada.format(doacao.dataDoacao)}'
                          '${doacao.quantidade != null ? ' - Quantidade: ${doacao.quantidade!.toStringAsFixed(2)}' : ''}'
                          '${doacao.valor != null ? ' - Valor: ${moeda.format(doacao.valor)}' : ''}'
                          '${(doacao.observacao ?? '').trim().isNotEmpty ? '\n${doacao.observacao}' : ''}',
                        ),
                        isThreeLine: true,
                        trailing: Wrap(
                          spacing: 4,
                          children: [
                            IconButton(
                              onPressed: () => _abrirFormulario(doacao: doacao),
                              icon: const Icon(Icons.edit),
                            ),
                            IconButton(
                              onPressed: () => _remover(doacao),
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

class _DoacaoFormDialog extends StatefulWidget {
  final DoacaoPatrocinador? doacao;
  final List<Patrocinador> patrocinadores;

  const _DoacaoFormDialog({
    this.doacao,
    required this.patrocinadores,
  });

  @override
  State<_DoacaoFormDialog> createState() => _DoacaoFormDialogState();
}

class _DoacaoFormDialogState extends State<_DoacaoFormDialog> {
  final _api = ApiService();
  final _descricaoController = TextEditingController();
  final _quantidadeController = TextEditingController();
  final _valorController = TextEditingController();
  final _observacaoController = TextEditingController();
  String? _patrocinadorId;
  String _tipo = 'Dinheiro';
  DateTime _dataDoacao = DateTime.now();
  bool _salvando = false;

  bool get _editando => widget.doacao != null;
  bool get _ehDinheiro => _tipo == 'Dinheiro';

  @override
  void initState() {
    super.initState();
    final doacao = widget.doacao;
    if (doacao != null) {
      _patrocinadorId = doacao.patrocinadorId;
      _tipo = doacao.tipo;
      _dataDoacao = doacao.dataDoacao;
      _descricaoController.text = doacao.descricao;
      _quantidadeController.text = doacao.quantidade?.toStringAsFixed(2) ?? '';
      _valorController.text = doacao.valor?.toStringAsFixed(2) ?? '';
      _observacaoController.text = doacao.observacao ?? '';
    }
  }

  Future<void> _selecionarData() async {
    final selecionada = await showDatePicker(
      context: context,
      initialDate: _dataDoacao,
      firstDate: DateTime(2020),
      lastDate: DateTime(2100),
    );
    if (selecionada != null && mounted) {
      setState(() => _dataDoacao = selecionada);
    }
  }

  Future<void> _salvar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final quantidade = _quantidadeController.text.trim().isEmpty
        ? null
        : double.tryParse(_quantidadeController.text.replaceAll(',', '.'));
    final valor = _valorController.text.trim().isEmpty
        ? null
        : double.tryParse(_valorController.text.replaceAll(',', '.'));

    if (_patrocinadorId == null || _descricaoController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Selecione o patrocinador e informe a descri\u00e7\u00e3o da doa\u00e7\u00e3o.',
          ),
        ),
      );
      return;
    }

    if (_ehDinheiro && (valor == null || valor <= 0)) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Informe um valor maior que zero para doa\u00e7\u00f5es em dinheiro.'),
        ),
      );
      return;
    }

    if (quantidade == null && _quantidadeController.text.trim().isNotEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Quantidade inv\u00e1lida.')),
      );
      return;
    }

    setState(() => _salvando = true);
    try {
      final body = {
        'patrocinadorId': _patrocinadorId,
        'nomePatrocinador': '',
        'tipo': _tipo,
        'descricao': _descricaoController.text.trim(),
        'quantidade': quantidade,
        'valor': _ehDinheiro ? valor : null,
        'observacao': _observacaoController.text.trim(),
        'dataDoacao': _dataDoacao.toIso8601String(),
      };
      if (_editando) {
        await _api.put(
          '${ApiConstants.financeiroDoacoes(auth!.slug!)}/${widget.doacao!.id}',
          body,
          token: auth.token,
        );
      } else {
        await _api.post(
          ApiConstants.financeiroDoacoes(auth!.slug!),
          body,
          token: auth.token,
        );
      }
      if (!mounted) return;
      Navigator.pop(context);
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) setState(() => _salvando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final dataFormatada = DateFormat('dd/MM/yyyy').format(_dataDoacao);
    return AlertDialog(
      title: Text(_editando ? 'Editar doa\u00e7\u00e3o' : 'Nova doa\u00e7\u00e3o'),
      content: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            DropdownButtonFormField<String?>(
              initialValue: _patrocinadorId,
              decoration: const InputDecoration(labelText: 'Patrocinador cadastrado'),
              items: [
                const DropdownMenuItem<String?>(
                  value: null,
                  child: Text('Selecione'),
                ),
                ...widget.patrocinadores.map(
                  (p) => DropdownMenuItem<String?>(value: p.id, child: Text(p.nome)),
                ),
              ],
              onChanged: (value) => setState(() => _patrocinadorId = value),
            ),
            const SizedBox(height: 12),
            DropdownButtonFormField<String>(
              initialValue: _tipo,
              decoration: const InputDecoration(labelText: 'Tipo'),
              items: const [
                DropdownMenuItem(value: 'Dinheiro', child: Text('Dinheiro')),
                DropdownMenuItem(value: 'Produto', child: Text('Produto')),
              ],
              onChanged: (value) {
                setState(() => _tipo = value ?? 'Dinheiro');
                if (!_ehDinheiro) {
                  _valorController.clear();
                }
              },
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _descricaoController,
              decoration: const InputDecoration(labelText: 'Descri\u00e7\u00e3o'),
            ),
            const SizedBox(height: 12),
            OutlinedButton.icon(
              onPressed: _selecionarData,
              icon: const Icon(Icons.calendar_month_outlined),
              label: Text('Data: $dataFormatada'),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _quantidadeController,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: const InputDecoration(labelText: 'Quantidade opcional'),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _valorController,
              enabled: _ehDinheiro,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: InputDecoration(
                labelText: 'Valor',
                helperText: _ehDinheiro
                    ? 'Obrigat\u00f3rio para doa\u00e7\u00f5es em dinheiro.'
                    : 'Doa\u00e7\u00f5es em produto n\u00e3o entram como receita.',
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _observacaoController,
              maxLines: 3,
              decoration: const InputDecoration(labelText: 'Observa\u00e7\u00e3o'),
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: _salvando ? null : () => Navigator.pop(context),
          child: const Text('Cancelar'),
        ),
        FilledButton(
          onPressed: _salvando ? null : _salvar,
          child: Text(_salvando ? 'Salvando...' : 'Salvar'),
        ),
      ],
    );
  }

  @override
  void dispose() {
    _descricaoController.dispose();
    _quantidadeController.dispose();
    _valorController.dispose();
    _observacaoController.dispose();
    super.dispose();
  }
}
