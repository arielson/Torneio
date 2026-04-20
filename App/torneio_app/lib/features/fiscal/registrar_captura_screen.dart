import 'dart:io';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../../core/models/captura.dart';
import '../../core/models/equipe.dart';
import '../../core/models/item.dart';
import '../../core/models/membro.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/captura_provider.dart';
import '../../core/providers/config_provider.dart';

class RegistrarCapturaScreen extends StatefulWidget {
  const RegistrarCapturaScreen({super.key});

  @override
  State<RegistrarCapturaScreen> createState() => _RegistrarCapturaScreenState();
}

class _RegistrarCapturaScreenState extends State<RegistrarCapturaScreen> {
  final _formKey = GlobalKey<FormState>();
  final _tamanhoController = TextEditingController();

  String? _membroId;
  String? _itemId;
  String? _equipeId;
  String? _fotoPath;
  int? _fonteFoto; // 0 = Camera, 1 = Galeria
  bool _salvando = false;

  Future<void> _escolherFoto(ImageSource source) async {
    final picker = ImagePicker();
    final foto = await picker.pickImage(
      source: source,
      maxWidth: 1280,
      maxHeight: 1280,
      imageQuality: 85,
    );
    if (foto != null) {
      setState(() {
        _fotoPath = foto.path;
        _fonteFoto = source == ImageSource.camera ? 0 : 1;
      });
    }
  }

  void _mostrarOpcoesFoto() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder:
          (_) => SafeArea(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const SizedBox(height: 8),
                Container(
                  width: 40,
                  height: 4,
                  decoration: BoxDecoration(
                    color: Colors.grey.shade300,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
                const SizedBox(height: 12),
                ListTile(
                  leading: const Icon(Icons.camera_alt, color: Colors.blue),
                  title: const Text('Tirar foto'),
                  onTap: () {
                    Navigator.pop(context);
                    _escolherFoto(ImageSource.camera);
                  },
                ),
                ListTile(
                  leading: const Icon(Icons.photo_library, color: Colors.green),
                  title: const Text('Escolher da galeria'),
                  onTap: () {
                    Navigator.pop(context);
                    _escolherFoto(ImageSource.gallery);
                  },
                ),
                const SizedBox(height: 8),
              ],
            ),
          ),
    );
  }

  Future<void> _salvar({bool forcarOffline = false}) async {
    if (!_formKey.currentState!.validate()) return;
    if (_fotoPath == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Adicione uma foto da captura.')),
      );
      return;
    }

    final auth = context.read<AuthProvider>();
    final config = context.read<ConfigProvider>().config!;
    final capProv = context.read<CapturaProvider>();
    final fiscalId = auth.usuario?.id ?? '';

    final minhasEquipes =
        capProv.equipes.where((e) => e.fiscalId == fiscalId).toList();

    final equipeIdFinal =
        _equipeId ??
        (minhasEquipes.length == 1 ? minhasEquipes.first.id : null);

    if (equipeIdFinal == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Selecione uma embarcacao.')),
      );
      return;
    }

    setState(() => _salvando = true);

    final req = RegistrarCapturaRequest(
      torneioId: config.id,
      itemId: _itemId!,
      membroId: _membroId!,
      equipeId: equipeIdFinal,
      tamanhoMedida: double.parse(_tamanhoController.text.replaceAll(',', '.')),
      fotoUrl: _fotoPath!,
      dataHora: DateTime.now(),
      fonteFoto: _fonteFoto,
    );

    final ok = await capProv.registrarCaptura(
      slug: config.slug,
      token: auth.usuario!.token,
      req: req,
      forcarOffline: forcarOffline,
    );

    if (!mounted) return;
    setState(() => _salvando = false);

    if (ok) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            forcarOffline || capProv.pendentesSync > 0
                ? 'Captura salva para sincronizar depois.'
                : 'Captura registrada com sucesso!',
          ),
          backgroundColor:
              forcarOffline || capProv.pendentesSync > 0
                  ? Colors.orange
                  : Colors.green,
        ),
      );
      Navigator.pop(context);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Erro ao registrar captura.'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final capProv = context.watch<CapturaProvider>();
    final auth = context.watch<AuthProvider>();
    final membros = capProv.membros;
    final itens = capProv.itens;
    final fiscalId = auth.usuario?.id ?? '';
    final minhasEquipes =
        capProv.equipes.where((e) => e.fiscalId == fiscalId).toList();
    final equipeSelecionadaId =
        _equipeId ??
        (minhasEquipes.length == 1 ? minhasEquipes.first.id : null);
    final membroIdsPermitidos =
        minhasEquipes
            .where(
              (e) => equipeSelecionadaId == null || e.id == equipeSelecionadaId,
            )
            .expand((e) => e.membroIds)
            .toSet();
    final membrosDisponiveis =
        membroIdsPermitidos.isEmpty
            ? membros
            : membros.where((m) => membroIdsPermitidos.contains(m.id)).toList();
    final multiEquipe = minhasEquipes.length > 1;

    return Scaffold(
      appBar: AppBar(
        title: Text('Registrar ${config?.labelCaptura ?? "Captura"}'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              if (multiEquipe) ...[
                _SearchSelectField<Equipe>(
                  label: config?.labelEquipe ?? 'Embarcacao',
                  value:
                      minhasEquipes.where((e) => e.id == _equipeId).firstOrNull,
                  items: minhasEquipes,
                  itemLabel: (e) => e.nome,
                  itemImageUrl: (e) => e.fotoUrl,
                  searchHint:
                      'Pesquisar ${(config?.labelEquipe ?? "embarcacao").toLowerCase()}',
                  emptyText: 'Nenhuma embarcacao encontrada.',
                  onChanged:
                      (e) => setState(() {
                        _equipeId = e?.id;
                        _membroId = null;
                      }),
                  validator:
                      () =>
                          _equipeId == null
                              ? 'Selecione uma ${config?.labelEquipe ?? "embarcacao"}'
                              : null,
                ),
                const SizedBox(height: 16),
              ],

              _SearchSelectField<Membro>(
                label: config?.labelMembro ?? 'Membro',
                value:
                    membrosDisponiveis
                        .where((m) => m.id == _membroId)
                        .firstOrNull,
                items: membrosDisponiveis,
                itemLabel: (m) => m.nome,
                itemImageUrl: (m) => m.fotoUrl,
                searchHint:
                    'Pesquisar ${(config?.labelMembro ?? "membro").toLowerCase()}',
                emptyText: 'Nenhum membro encontrado.',
                onChanged: (m) => setState(() => _membroId = m?.id),
                validator:
                    () =>
                        _membroId == null
                            ? 'Selecione um ${config?.labelMembro ?? "membro"}'
                            : null,
              ),
              const SizedBox(height: 16),

              _SearchSelectField<Item>(
                label: config?.labelItem ?? 'Item',
                value: itens.where((i) => i.id == _itemId).firstOrNull,
                items: itens,
                itemLabel:
                    (i) =>
                        i.comprimento != null
                            ? '${i.nome} (min. ${i.comprimento!.toStringAsFixed(1)} ${config?.medidaCaptura ?? "cm"})'
                            : i.nome,
                itemImageUrl: (i) => i.fotoUrl,
                searchHint:
                    'Pesquisar ${(config?.labelItem ?? "item").toLowerCase()}',
                emptyText: 'Nenhum item encontrado.',
                onChanged: (i) => setState(() => _itemId = i?.id),
                validator:
                    () =>
                        _itemId == null
                            ? 'Selecione um ${config?.labelItem ?? "item"}'
                            : null,
              ),
              const SizedBox(height: 16),

              TextFormField(
                controller: _tamanhoController,
                decoration: InputDecoration(
                  labelText: 'Medida (${config?.medidaCaptura ?? "cm"})',
                  border: const OutlineInputBorder(),
                  suffixText: config?.medidaCaptura ?? 'cm',
                ),
                keyboardType: const TextInputType.numberWithOptions(
                  decimal: true,
                ),
                validator: (v) {
                  if (v == null || v.trim().isEmpty) return 'Informe a medida';
                  final d = double.tryParse(v.replaceAll(',', '.'));
                  if (d == null || d <= 0) return 'Medida invalida';
                  return null;
                },
              ),
              const SizedBox(height: 24),

              Text(
                'Foto da ${config?.labelCaptura ?? "captura"}',
                style: Theme.of(context).textTheme.titleSmall,
              ),
              const SizedBox(height: 8),
              GestureDetector(
                onTap: _mostrarOpcoesFoto,
                child: Container(
                  height: 180,
                  decoration: BoxDecoration(
                    border: Border.all(color: Colors.grey),
                    borderRadius: BorderRadius.circular(8),
                    color: Colors.grey.shade100,
                  ),
                  child:
                      _fotoPath != null
                          ? Stack(
                            fit: StackFit.expand,
                            children: [
                              ClipRRect(
                                borderRadius: BorderRadius.circular(8),
                                child: Image.file(
                                  File(_fotoPath!),
                                  fit: BoxFit.cover,
                                ),
                              ),
                              Positioned(
                                top: 6,
                                right: 6,
                                child: _FotoOrigemBadge(fonteFoto: _fonteFoto),
                              ),
                            ],
                          )
                          : const Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(
                                Icons.add_a_photo,
                                size: 48,
                                color: Colors.grey,
                              ),
                              SizedBox(height: 8),
                              Text(
                                'Camera ou galeria',
                                style: TextStyle(color: Colors.grey),
                              ),
                            ],
                          ),
                ),
              ),
              if (_fotoPath != null)
                Padding(
                  padding: const EdgeInsets.only(top: 4),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      TextButton.icon(
                        icon: const Icon(Icons.camera_alt, size: 16),
                        label: const Text('Camera'),
                        onPressed: () => _escolherFoto(ImageSource.camera),
                      ),
                      TextButton.icon(
                        icon: const Icon(Icons.photo_library, size: 16),
                        label: const Text('Galeria'),
                        onPressed: () => _escolherFoto(ImageSource.gallery),
                      ),
                    ],
                  ),
                ),
              const SizedBox(height: 24),

              FilledButton.icon(
                icon: const Icon(Icons.send),
                label: Text(_salvando ? 'Salvando...' : 'Registrar agora'),
                onPressed:
                    _salvando ? null : () => _salvar(forcarOffline: false),
              ),
              const SizedBox(height: 8),
              OutlinedButton.icon(
                icon: const Icon(Icons.schedule),
                label: Text(_salvando ? 'Salvando...' : 'Sincronizar depois'),
                onPressed:
                    _salvando ? null : () => _salvar(forcarOffline: true),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _tamanhoController.dispose();
    super.dispose();
  }
}

class _SearchSelectField<T> extends FormField<T> {
  _SearchSelectField({
    required String label,
    required T? value,
    required List<T> items,
    required String Function(T item) itemLabel,
    required String? Function(T item) itemImageUrl,
    required String searchHint,
    required String emptyText,
    required ValueChanged<T?> onChanged,
    required String? Function() validator,
  }) : super(
         initialValue: value,
         validator: (_) => validator(),
         builder: (state) {
           final selected = state.value;
           return InkWell(
             onTap: () async {
               final selectedItem = await showModalBottomSheet<T>(
                 context: state.context,
                 isScrollControlled: true,
                 builder:
                     (_) => _SearchSelectionSheet<T>(
                       title: label,
                       items: items,
                       itemLabel: itemLabel,
                       itemImageUrl: itemImageUrl,
                       searchHint: searchHint,
                       emptyText: emptyText,
                     ),
               );

               if (selectedItem != null) {
                 state.didChange(selectedItem);
                 onChanged(selectedItem);
               }
             },
             child: InputDecorator(
               decoration: InputDecoration(
                 labelText: label,
                 floatingLabelBehavior: FloatingLabelBehavior.always,
                 border: const OutlineInputBorder(),
                 errorText: state.errorText,
                 suffixIcon: const Icon(Icons.search),
                 contentPadding: const EdgeInsets.fromLTRB(12, 18, 12, 12),
               ),
               isEmpty: selected == null,
               child: Row(
                 children: [
                   if (selected != null) ...[
                     _SelectionAvatar(url: itemImageUrl(selected)),
                     const SizedBox(width: 12),
                   ],
                   Expanded(
                     child: Text(
                       selected == null
                           ? 'Toque para pesquisar e selecionar'
                           : itemLabel(selected),
                       style:
                           selected == null
                               ? TextStyle(
                                 color: Theme.of(state.context).hintColor,
                               )
                               : null,
                     ),
                   ),
                 ],
               ),
             ),
           );
         },
       );
}

class _SearchSelectionSheet<T> extends StatefulWidget {
  final String title;
  final List<T> items;
  final String Function(T item) itemLabel;
  final String? Function(T item) itemImageUrl;
  final String searchHint;
  final String emptyText;

  const _SearchSelectionSheet({
    required this.title,
    required this.items,
    required this.itemLabel,
    required this.itemImageUrl,
    required this.searchHint,
    required this.emptyText,
  });

  @override
  State<_SearchSelectionSheet<T>> createState() =>
      _SearchSelectionSheetState<T>();
}

class _SearchSelectionSheetState<T> extends State<_SearchSelectionSheet<T>> {
  final _searchController = TextEditingController();
  String _query = '';

  @override
  Widget build(BuildContext context) {
    final filtered =
        widget.items.where((item) {
          final label = widget.itemLabel(item).toLowerCase();
          return _query.isEmpty || label.contains(_query.toLowerCase());
        }).toList();

    return SafeArea(
      child: Padding(
        padding: EdgeInsets.only(
          bottom: MediaQuery.of(context).viewInsets.bottom,
        ),
        child: SizedBox(
          height: MediaQuery.of(context).size.height * 0.75,
          child: Column(
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
                child: Row(
                  children: [
                    Expanded(
                      child: Text(
                        widget.title,
                        style: Theme.of(context).textTheme.titleMedium,
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
                  controller: _searchController,
                  autofocus: true,
                  decoration: InputDecoration(
                    hintText: widget.searchHint,
                    prefixIcon: const Icon(Icons.search),
                    border: const OutlineInputBorder(),
                  ),
                  onChanged: (value) => setState(() => _query = value.trim()),
                ),
              ),
              const SizedBox(height: 12),
              Expanded(
                child:
                    filtered.isEmpty
                        ? Center(
                          child: Text(
                            widget.emptyText,
                            style: Theme.of(context).textTheme.bodyMedium
                                ?.copyWith(color: Colors.grey),
                          ),
                        )
                        : ListView.separated(
                          itemCount: filtered.length,
                          separatorBuilder:
                              (_, separatorIndex) => const Divider(height: 1),
                          itemBuilder: (context, index) {
                            final item = filtered[index];
                            return ListTile(
                              leading: _SelectionAvatar(
                                url: widget.itemImageUrl(item),
                              ),
                              title: Text(widget.itemLabel(item)),
                              onTap: () => Navigator.pop(context, item),
                            );
                          },
                        ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }
}

class _SelectionAvatar extends StatelessWidget {
  final String? url;

  const _SelectionAvatar({this.url});

  @override
  Widget build(BuildContext context) {
    if (url == null || url!.isEmpty) {
      return const CircleAvatar(
        radius: 20,
        child: Icon(Icons.image_outlined, size: 18),
      );
    }

    return GestureDetector(
      onTap: () => _abrirImagem(context, url!),
      child: CircleAvatar(
        radius: 20,
        backgroundColor: Colors.grey.shade200,
        child: ClipOval(
          child: CachedNetworkImage(
            imageUrl: url!,
            width: 40,
            height: 40,
            fit: BoxFit.cover,
            errorWidget:
                (context, imageUrl, error) =>
                    const Icon(Icons.image_not_supported, size: 18),
          ),
        ),
      ),
    );
  }

  Future<void> _abrirImagem(BuildContext context, String imageUrl) {
    return showDialog<void>(
      context: context,
      builder:
          (_) => Dialog.fullscreen(
            child: Scaffold(
              appBar: AppBar(title: const Text('Visualizar imagem')),
              backgroundColor: Colors.black,
              body: Stack(
                children: [
                  Positioned.fill(
                    child: InteractiveViewer(
                      minScale: 0.8,
                      maxScale: 4,
                      child: Center(
                        child: CachedNetworkImage(
                          imageUrl: imageUrl,
                          fit: BoxFit.contain,
                          errorWidget:
                              (context, failedUrl, error) => const Center(
                                child: Icon(
                                  Icons.broken_image_outlined,
                                  color: Colors.white70,
                                  size: 56,
                                ),
                              ),
                        ),
                      ),
                    ),
                  ),
                  Positioned(
                    right: 16,
                    bottom: 16,
                    child: DecoratedBox(
                      decoration: BoxDecoration(
                        color: Colors.black54,
                        borderRadius: BorderRadius.circular(999),
                      ),
                      child: const Padding(
                        padding: EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 8,
                        ),
                        child: Text(
                          'Pinça para ampliar',
                          style: TextStyle(color: Colors.white),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
    );
  }
}

class _FotoOrigemBadge extends StatelessWidget {
  final int? fonteFoto;
  const _FotoOrigemBadge({this.fonteFoto});

  @override
  Widget build(BuildContext context) {
    final isGaleria = fonteFoto == 1;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: Colors.black54,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            isGaleria ? Icons.photo_library : Icons.camera_alt,
            color: Colors.white,
            size: 12,
          ),
          const SizedBox(width: 4),
          Text(
            isGaleria ? 'Galeria' : 'Camera',
            style: const TextStyle(color: Colors.white, fontSize: 11),
          ),
        ],
      ),
    );
  }
}

extension _IterableFirstOrNullExtension<T> on Iterable<T> {
  T? get firstOrNull => isEmpty ? null : first;
}
