import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/especie_peixe.dart';
import '../../core/models/item.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import '../../widgets/expandable_network_image.dart';

class ItemFormScreen extends StatefulWidget {
  final Item? item;

  const ItemFormScreen({
    super.key,
    this.item,
  });

  @override
  State<ItemFormScreen> createState() => _ItemFormScreenState();
}

class _ItemFormScreenState extends State<ItemFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _api = ApiService();
  final _comprimentoController = TextEditingController();
  final _fatorController = TextEditingController(text: '1.00');

  bool _salvando = false;
  bool _carregandoEspecies = true;
  String? _erroEspecies;
  List<EspeciePeixe> _especies = const [];
  EspeciePeixe? _especieSelecionada;

  bool get _editando => widget.item != null;

  @override
  void initState() {
    super.initState();
    final item = widget.item;
    if (item != null) {
      _comprimentoController.text =
          item.comprimento != null ? item.comprimento!.toStringAsFixed(2) : '';
      _fatorController.text = item.fatorMultiplicador.toStringAsFixed(2);
    }
    _carregarEspecies();
  }

  Future<void> _carregarEspecies() async {
    setState(() {
      _carregandoEspecies = true;
      _erroEspecies = null;
    });

    try {
      final auth = context.read<AuthProvider>().usuario;
      if (auth?.slug == null || auth?.token == null) {
        throw const ApiException(401, 'Sessao invalida.');
      }

      final itensData = await _api.get(ApiConstants.itens(auth!.slug!), token: auth.token);
      final itensExistentes = itensData is List
          ? itensData.map((e) => Item.fromJson(e as Map<String, dynamic>)).toList()
          : <Item>[];
      final data = await _api.get(ApiConstants.especiesPeixe());
      var especies = data is List
          ? data.map((e) => EspeciePeixe.fromJson(e as Map<String, dynamic>)).toList()
          : <EspeciePeixe>[];
      especies.sort((a, b) => a.nome.compareTo(b.nome));

      if (!_editando) {
        final idsJaCadastrados = itensExistentes.map((item) => item.especiePeixeId).toSet();
        especies = especies.where((especie) => !idsJaCadastrados.contains(especie.id)).toList();
      }

      EspeciePeixe? selecionada;
      if (_editando) {
        final especieId = widget.item!.especiePeixeId;
        selecionada = especies.cast<EspeciePeixe?>().firstWhere(
              (e) => e?.id == especieId,
              orElse: () => null,
            );
      }

      if (mounted) {
        setState(() {
          _especies = especies;
          _especieSelecionada = selecionada;
        });
      }
    } on ApiException catch (e) {
      _erroEspecies = e.message;
    } catch (_) {
      _erroEspecies = 'Erro ao carregar especies.';
    } finally {
      if (mounted) {
        setState(() => _carregandoEspecies = false);
      }
    }
  }

  Future<void> _selecionarEspecie() async {
    final selecionada = await showModalBottomSheet<EspeciePeixe>(
      context: context,
      isScrollControlled: true,
      builder: (_) => _EspecieSelectorSheet(
        especies: _especies,
        selecionadaId: _especieSelecionada?.id,
      ),
    );

    if (selecionada != null && mounted) {
      setState(() => _especieSelecionada = selecionada);
    }
  }

  Future<void> _salvar() async {
    if (!_formKey.currentState!.validate()) return;

    final auth = context.read<AuthProvider>().usuario;
    final config = context.read<ConfigProvider>().config;
    if (auth?.slug == null || auth?.token == null || config == null) return;

    setState(() => _salvando = true);

    final comprimento = _comprimentoController.text.trim().replaceAll(',', '.');
    final usarFator = config.usarFatorMultiplicador;
    final body = {
      'especiePeixeId': _especieSelecionada!.id,
      'comprimento': comprimento.isEmpty ? null : double.tryParse(comprimento),
      'fatorMultiplicador':
          usarFator ? double.parse(_fatorController.text.trim().replaceAll(',', '.')) : 1.00,
    };

    try {
      if (_editando) {
        await _api.put(
          '${ApiConstants.itens(auth!.slug!)}/${widget.item!.id}',
          body,
          token: auth.token,
        );
      } else {
        await _api.post(
          ApiConstants.itens(auth!.slug!),
          body,
          token: auth.token,
        );
      }

      if (!mounted) return;
      Navigator.pop(context, true);
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
    final config = context.watch<ConfigProvider>().config;
    final label = config?.labelItem ?? 'Item';
    final medida = config?.medidaCaptura ?? 'cm';
    final usarFator = config?.usarFatorMultiplicador ?? false;

    return Scaffold(
      appBar: AppBar(title: Text(_editando ? 'Editar $label' : 'Novo $label')),
      body: _carregandoEspecies
          ? const Center(child: CircularProgressIndicator())
          : _erroEspecies != null
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Text(_erroEspecies!, textAlign: TextAlign.center),
                  ),
                )
              : Form(
                  key: _formKey,
                  child: ListView(
                    padding: const EdgeInsets.all(16),
                    children: [
                      TextFormField(
                        readOnly: true,
                        onTap: _editando ? null : _selecionarEspecie,
                        decoration: InputDecoration(
                          labelText: 'Especie',
                          border: const OutlineInputBorder(),
                          suffixIcon: Icon(_editando ? Icons.lock_outline : Icons.search),
                          hintText: _especieSelecionada?.nome ??
                              (_editando ? 'Especie do item' : 'Selecione uma especie'),
                        ),
                        controller: TextEditingController(text: _especieSelecionada?.nome ?? ''),
                        validator: (_) =>
                            _especieSelecionada == null ? 'Selecione uma especie.' : null,
                      ),
                      if (_especieSelecionada != null) ...[
                        const SizedBox(height: 12),
                        Card(
                          child: ListTile(
                            leading: ExpandableRectImage(
                              imageUrl: _especieSelecionada!.fotoUrl,
                              fallbackIcon: Icons.set_meal_outlined,
                              width: 56,
                              height: 56,
                            ),
                            title: Text(_especieSelecionada!.nome),
                          subtitle: Text(
                              (_especieSelecionada!.nomeCientifico ?? '').trim().isEmpty
                                  ? (_editando ? 'Especie fixa deste item' : 'Especie selecionada')
                                  : _especieSelecionada!.nomeCientifico!,
                            ),
                          ),
                        ),
                      ],
                      if (!_editando && _especies.isEmpty) ...[
                        const SizedBox(height: 12),
                        const Card(
                          child: Padding(
                            padding: EdgeInsets.all(16),
                            child: Text(
                              'Todas as especies ja foram adicionadas a este torneio.',
                              textAlign: TextAlign.center,
                            ),
                          ),
                        ),
                      ],
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _comprimentoController,
                        decoration: InputDecoration(
                          labelText: 'Comprimento minimo ($medida) - opcional',
                          border: const OutlineInputBorder(),
                        ),
                        keyboardType: const TextInputType.numberWithOptions(decimal: true),
                        validator: (value) {
                          if (value == null || value.trim().isEmpty) return null;
                          final comprimento = double.tryParse(value.replaceAll(',', '.'));
                          if (comprimento == null || comprimento < 0) {
                            return 'Informe um comprimento valido.';
                          }
                          return null;
                        },
                      ),
                      if (usarFator) ...[
                        const SizedBox(height: 16),
                        TextFormField(
                          controller: _fatorController,
                          decoration: const InputDecoration(
                            labelText: 'Fator multiplicador',
                            border: OutlineInputBorder(),
                          ),
                          keyboardType: const TextInputType.numberWithOptions(decimal: true),
                          validator: (value) {
                            final fator = double.tryParse((value ?? '').replaceAll(',', '.'));
                            if (fator == null || fator <= 0) {
                              return 'Informe um fator valido.';
                            }
                            return null;
                          },
                        ),
                      ],
                      const SizedBox(height: 24),
                      FilledButton.icon(
                        onPressed: _salvando ? null : _salvar,
                        icon: _salvando
                            ? const SizedBox(
                                width: 18,
                                height: 18,
                                child: CircularProgressIndicator(strokeWidth: 2),
                              )
                            : const Icon(Icons.save),
                        label: Text(_salvando ? 'Salvando...' : 'Salvar'),
                      ),
                    ],
                  ),
                ),
    );
  }

  @override
  void dispose() {
    _comprimentoController.dispose();
    _fatorController.dispose();
    super.dispose();
  }
}

class _EspecieSelectorSheet extends StatefulWidget {
  final List<EspeciePeixe> especies;
  final String? selecionadaId;

  const _EspecieSelectorSheet({
    required this.especies,
    required this.selecionadaId,
  });

  @override
  State<_EspecieSelectorSheet> createState() => _EspecieSelectorSheetState();
}

class _EspecieSelectorSheetState extends State<_EspecieSelectorSheet> {
  final _buscaController = TextEditingController();
  late List<EspeciePeixe> _filtradas;

  @override
  void initState() {
    super.initState();
    _filtradas = widget.especies;
    _buscaController.addListener(_filtrar);
  }

  void _filtrar() {
    final termo = _buscaController.text.trim().toLowerCase();
    setState(() {
      if (termo.isEmpty) {
        _filtradas = widget.especies;
      } else {
        _filtradas = widget.especies.where((especie) {
          return especie.nome.toLowerCase().contains(termo) ||
              (especie.nomeCientifico ?? '').toLowerCase().contains(termo);
        }).toList();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final altura = MediaQuery.of(context).size.height * 0.85;
    return SafeArea(
      child: SizedBox(
        height: altura,
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 12, 8, 8),
              child: Row(
                children: [
                  const Expanded(
                    child: Text(
                      'Selecionar especie',
                      style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600),
                    ),
                  ),
                  IconButton(
                    onPressed: () => Navigator.pop(context),
                    icon: const Icon(Icons.close),
                  ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: TextField(
                controller: _buscaController,
                decoration: const InputDecoration(
                  labelText: 'Pesquisar especie',
                  border: OutlineInputBorder(),
                  prefixIcon: Icon(Icons.search),
                ),
              ),
            ),
            const SizedBox(height: 12),
            Expanded(
              child: _filtradas.isEmpty
                  ? const Center(child: Text('Nenhuma especie encontrada.'))
                  : ListView.separated(
                      padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                      itemCount: _filtradas.length,
                      separatorBuilder: (context, index) => const SizedBox(height: 8),
                      itemBuilder: (context, index) {
                        final especie = _filtradas[index];
                        final selecionada = especie.id == widget.selecionadaId;
                        return Card(
                          child: ListTile(
                            onTap: () => Navigator.pop(context, especie),
                            leading: ExpandableRectImage(
                              imageUrl: especie.fotoUrl,
                              fallbackIcon: Icons.set_meal_outlined,
                              width: 56,
                              height: 56,
                            ),
                            title: Text(especie.nome),
                            subtitle: Text(
                              (especie.nomeCientifico ?? '').trim().isEmpty
                                  ? 'Toque para selecionar'
                                  : especie.nomeCientifico!,
                            ),
                            trailing: selecionada
                                ? const Icon(Icons.check_circle, color: Colors.green)
                                : null,
                          ),
                        );
                      },
                    ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  void dispose() {
    _buscaController.dispose();
    super.dispose();
  }
}
