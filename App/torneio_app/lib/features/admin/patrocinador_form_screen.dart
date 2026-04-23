import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/patrocinador.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';
import 'widgets/admin_photo_picker.dart';

class PatrocinadorFormScreen extends StatefulWidget {
  final Patrocinador? patrocinador;

  const PatrocinadorFormScreen({
    super.key,
    this.patrocinador,
  });

  @override
  State<PatrocinadorFormScreen> createState() => _PatrocinadorFormScreenState();
}

class _PatrocinadorFormScreenState extends State<PatrocinadorFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _api = ApiService();
  final _picker = ImagePicker();
  final _nomeController = TextEditingController();
  final _instagramController = TextEditingController();
  final _facebookController = TextEditingController();
  final _siteController = TextEditingController();
  final _zapController = TextEditingController();
  bool _exibirNaTelaInicial = true;
  bool _exibirNosRelatorios = true;

  bool _salvando = false;
  String? _fotoPath;

  bool get _editando => widget.patrocinador != null;

  @override
  void initState() {
    super.initState();
    final patrocinador = widget.patrocinador;
    if (patrocinador != null) {
      _nomeController.text = patrocinador.nome;
      _instagramController.text = patrocinador.instagram ?? '';
      _facebookController.text = patrocinador.facebook ?? '';
      _siteController.text = patrocinador.site ?? '';
      _zapController.text = patrocinador.zap ?? '';
      _exibirNaTelaInicial = patrocinador.exibirNaTelaInicial;
      _exibirNosRelatorios = patrocinador.exibirNosRelatorios;
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
    if (auth?.slug == null || auth?.token == null) return;

    if (!_editando && _fotoPath == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Selecione a imagem do patrocinador.'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    setState(() => _salvando = true);

    final fields = {
      'nome': _nomeController.text.trim(),
      'instagram': _instagramController.text.trim(),
      'facebook': _facebookController.text.trim(),
      'site': _siteController.text.trim(),
      'zap': _zapController.text.trim(),
      'exibirNaTelaInicial': _exibirNaTelaInicial,
      'exibirNosRelatorios': _exibirNosRelatorios,
    };

    try {
      if (_editando) {
        await _api.putMultipart(
          '${ApiConstants.patrocinadores(auth!.slug!)}/${widget.patrocinador!.id}',
          fields: fields,
          files: _fotoPath != null ? {'foto': _fotoPath!} : null,
          token: auth.token,
        );
      } else {
        await _api.postMultipart(
          ApiConstants.patrocinadores(auth!.slug!),
          fields: fields,
          files: {'foto': _fotoPath!},
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
    final fotoAtualUrl = widget.patrocinador?.fotoUrl;

    return Scaffold(
      appBar: AppBar(title: Text(_editando ? 'Editar patrocinador' : 'Novo patrocinador')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _nomeController,
              decoration: const InputDecoration(
                labelText: 'Nome',
                border: OutlineInputBorder(),
              ),
              validator: (value) =>
                  (value == null || value.trim().isEmpty) ? 'Informe o nome.' : null,
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _instagramController,
              decoration: const InputDecoration(
                labelText: 'Instagram (opcional)',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _facebookController,
              decoration: const InputDecoration(
                labelText: 'Facebook (opcional)',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _siteController,
              decoration: const InputDecoration(
                labelText: 'Site (opcional)',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _zapController,
              decoration: const InputDecoration(
                labelText: 'Zap (opcional)',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 12),
            SwitchListTile(
              value: _exibirNaTelaInicial,
              contentPadding: EdgeInsets.zero,
              title: const Text('Exibir na tela inicial do torneio'),
              onChanged: (value) => setState(() => _exibirNaTelaInicial = value),
            ),
            SwitchListTile(
              value: _exibirNosRelatorios,
              contentPadding: EdgeInsets.zero,
              title: const Text('Exibir ao final dos relatórios'),
              onChanged: (value) => setState(() => _exibirNosRelatorios = value),
            ),
            const SizedBox(height: 16),
            AdminPhotoPicker(
              titulo: 'Imagem do patrocinador',
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
    _instagramController.dispose();
    _facebookController.dispose();
    _siteController.dispose();
    _zapController.dispose();
    super.dispose();
  }
}
