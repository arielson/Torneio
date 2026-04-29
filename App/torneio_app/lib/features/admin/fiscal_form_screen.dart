import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/flavor_config.dart';
import '../../core/models/equipe.dart';
import '../../core/models/fiscal.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import 'widgets/admin_photo_picker.dart';

class FiscalFormScreen extends StatefulWidget {
  final Fiscal? fiscal;

  const FiscalFormScreen({
    super.key,
    this.fiscal,
  });

  @override
  State<FiscalFormScreen> createState() => _FiscalFormScreenState();
}

class _FiscalFormScreenState extends State<FiscalFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _api = ApiService();
  final _picker = ImagePicker();
  final _nomeController = TextEditingController();
  final _usuarioController = TextEditingController();
  final _senhaController = TextEditingController();

  bool _salvando = false;
  bool _carregandoEquipes = true;
  String? _fotoPath;
  List<Equipe> _equipes = const [];
  final Set<String> _equipeIdsSelecionadas = <String>{};

  bool get _editando => widget.fiscal != null;

  @override
  void initState() {
    super.initState();
    final fiscal = widget.fiscal;
    if (fiscal != null) {
      _nomeController.text = fiscal.nome;
      _usuarioController.text = fiscal.usuario;
      _equipeIdsSelecionadas.addAll(fiscal.equipeIds);
    }
    _carregarEquipes();
  }

  Future<void> _carregarEquipes() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    try {
      final data = await _api.get(
        ApiConstants.equipes(auth!.slug!),
        token: auth.token,
      );

      final equipes = data is List
          ? data.map((e) => Equipe.fromJson(e as Map<String, dynamic>)).toList()
          : <Equipe>[];

      if (!mounted) return;
      setState(() => _equipes = equipes);
    } catch (_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Nao foi possivel carregar as embarcações.'),
          backgroundColor: Colors.red,
        ),
      );
    } finally {
      if (mounted) setState(() => _carregandoEquipes = false);
    }
  }

  Future<void> _selecionarFoto(ImageSource source) async {
    final foto = await _picker.pickImage(
      source: source,
      maxWidth: 1280,
      maxHeight: 1280,
      imageQuality: 85,
    );
    if (foto != null && mounted) setState(() => _fotoPath = foto.path);
  }

  Future<void> _abrirSeletorFoto() async {
    await showModalBottomSheet<void>(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.photo_library_outlined),
              title: const Text('Escolher da galeria'),
              onTap: () async {
                Navigator.pop(context);
                await _selecionarFoto(ImageSource.gallery);
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_camera_outlined),
              title: const Text('Tirar foto'),
              onTap: () async {
                Navigator.pop(context);
                await _selecionarFoto(ImageSource.camera);
              },
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _salvar() async {
    if (!_formKey.currentState!.validate()) return;
    if (_equipeIdsSelecionadas.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Selecione ao menos uma embarcacao.'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    setState(() => _salvando = true);

    final fields = {
      'nome': _nomeController.text.trim(),
      'usuario': _usuarioController.text.trim(),
      if (_senhaController.text.trim().isNotEmpty) 'senha': _senhaController.text.trim(),
      'equipeIds': _equipeIdsSelecionadas.toList(),
    };

    try {
      if (_editando) {
        await _api.putMultipart(
          '${ApiConstants.fiscais(auth!.slug!)}/${widget.fiscal!.id}',
          fields: fields,
          files: _fotoPath != null ? {'foto': _fotoPath!} : null,
          token: auth.token,
        );
      } else {
        await _api.postMultipart(
          ApiConstants.fiscais(auth!.slug!),
          fields: {
            ...fields,
            'senha': _senhaController.text.trim(),
          },
          files: _fotoPath != null ? {'foto': _fotoPath!} : null,
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
    final label = config?.labelSupervisor ?? 'Fiscal';
    final labelEquipePlural = config?.labelEquipePlural ?? 'Embarcações';
    final fotoAtualUrl = AppConfig.resolverUrl(widget.fiscal?.fotoUrl);

    return Scaffold(
      appBar: AppBar(title: Text(_editando ? 'Editar $label' : 'Novo $label')),
      body: _carregandoEquipes
          ? const Center(child: CircularProgressIndicator())
          : Form(
              key: _formKey,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  TextFormField(
                    controller: _nomeController,
                    decoration: InputDecoration(
                      labelText: 'Nome do ${label.toLowerCase()}',
                      border: const OutlineInputBorder(),
                    ),
                    validator: (value) =>
                        (value == null || value.trim().isEmpty) ? 'Informe o nome.' : null,
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _usuarioController,
                    decoration: const InputDecoration(
                      labelText: 'Usuario',
                      border: OutlineInputBorder(),
                    ),
                    validator: (value) =>
                        (value == null || value.trim().isEmpty) ? 'Informe o usuario.' : null,
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _senhaController,
                    decoration: InputDecoration(
                      labelText: _editando ? 'Nova senha - opcional' : 'Senha',
                      border: const OutlineInputBorder(),
                    ),
                    obscureText: true,
                    validator: (value) {
                      if (!_editando && (value == null || value.trim().isEmpty)) {
                        return 'Informe a senha.';
                      }
                      if (value != null && value.trim().isNotEmpty && value.trim().length < 6) {
                        return 'A senha deve ter no minimo 6 caracteres.';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),
                  Text(
                    labelEquipePlural,
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  if (_equipes.isEmpty)
                    const Text(
                      'Nenhuma embarcacao cadastrada no torneio.',
                      style: TextStyle(color: Colors.grey),
                    )
                  else
                    ..._equipes.map(
                      (equipe) => CheckboxListTile(
                        value: _equipeIdsSelecionadas.contains(equipe.id),
                        contentPadding: EdgeInsets.zero,
                        title: Text(equipe.nome),
                        subtitle: Text('Capitao: ${equipe.capitao}'),
                        onChanged: (value) {
                          setState(() {
                            if (value == true) {
                              _equipeIdsSelecionadas.add(equipe.id);
                            } else {
                              _equipeIdsSelecionadas.remove(equipe.id);
                            }
                          });
                        },
                      ),
                    ),
                  const SizedBox(height: 16),
                  AdminPhotoPicker(
                    titulo: 'Foto do ${label.toLowerCase()}',
                    fotoAtualUrl: fotoAtualUrl,
                    fotoLocalPath: _fotoPath,
                    onTap: _abrirSeletorFoto,
                  ),
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
    _nomeController.dispose();
    _usuarioController.dispose();
    _senhaController.dispose();
    super.dispose();
  }
}
