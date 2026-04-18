import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/flavor_config.dart';
import '../../core/models/membro.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import 'widgets/admin_photo_picker.dart';

class MembroFormScreen extends StatefulWidget {
  final Membro? membro;

  const MembroFormScreen({
    super.key,
    this.membro,
  });

  @override
  State<MembroFormScreen> createState() => _MembroFormScreenState();
}

class _MembroFormScreenState extends State<MembroFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _api = ApiService();
  final _nomeController = TextEditingController();
  final _picker = ImagePicker();

  bool _salvando = false;
  String? _fotoPath;

  bool get _editando => widget.membro != null;

  @override
  void initState() {
    super.initState();
    final membro = widget.membro;
    if (membro != null) {
      _nomeController.text = membro.nome;
    }
  }

  Future<void> _selecionarFoto(ImageSource source) async {
    final foto = await _picker.pickImage(
      source: source,
      maxWidth: 1280,
      maxHeight: 1280,
      imageQuality: 85,
    );
    if (foto != null && mounted) {
      setState(() => _fotoPath = foto.path);
    }
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

    final auth = context.read<AuthProvider>().usuario;
    final config = context.read<ConfigProvider>().config;
    if (auth?.slug == null || auth?.token == null || config == null) return;

    setState(() => _salvando = true);

    try {
      if (_editando) {
        await _api.putMultipart(
          '${ApiConstants.membros(auth!.slug!)}/${widget.membro!.id}',
          fields: {
            'nome': _nomeController.text.trim(),
          },
          files: _fotoPath != null ? {'foto': _fotoPath!} : null,
          token: auth.token,
        );
      } else {
        await _api.postMultipart(
          ApiConstants.membros(auth!.slug!),
          fields: {
            'nome': _nomeController.text.trim(),
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
      if (mounted) {
        setState(() => _salvando = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final label = config?.labelMembro ?? 'Membro';
    final fotoAtualUrl = AppConfig.resolverUrl(widget.membro?.fotoUrl);

    return Scaffold(
      appBar: AppBar(
        title: Text(_editando ? 'Editar $label' : 'Novo $label'),
      ),
      body: Form(
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
    super.dispose();
  }
}
