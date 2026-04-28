import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
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
  static const List<String> _tamanhosPadrao = ['PP', 'P', 'M', 'G', 'GG', 'XGG', 'EXGG'];
  final _formKey = GlobalKey<FormState>();
  final _api = ApiService();
  final _nomeController = TextEditingController();
  final _celularController = TextEditingController();
  final _usuarioController = TextEditingController();
  final _senhaController = TextEditingController();
  final _picker = ImagePicker();

  bool _salvando = false;
  bool _senhaVisivel = false;
  String? _fotoPath;
  String? _tamanhoCamisa;

  bool get _editando => widget.membro != null;

  @override
  void initState() {
    super.initState();
    final membro = widget.membro;
    if (membro != null) {
      _nomeController.text = membro.nome;
      _celularController.text = _formatarCelular(membro.celular ?? '');
      _usuarioController.text = membro.usuario ?? '';
      _tamanhoCamisa = (membro.tamanhoCamisa?.trim().isEmpty ?? true)
          ? null
          : membro.tamanhoCamisa?.trim();
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
    final permiteAcessoPescador = config.permitirRegistroPublicoMembro;

    final usuario = _usuarioController.text.trim();
    final senha = _senhaController.text;
    if (permiteAcessoPescador &&
        ((usuario.isEmpty && senha.isNotEmpty) || (usuario.isNotEmpty && !_editando && senha.isEmpty))) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Informe usuario e senha juntos para habilitar o acesso do pescador.'), backgroundColor: Colors.red),
      );
      return;
    }

    setState(() => _salvando = true);

    try {
      if (_editando) {
        await _api.putMultipart(
          '${ApiConstants.membros(auth!.slug!)}/${widget.membro!.id}',
          fields: {
            'nome': _nomeController.text.trim(),
            'celular': _celularController.text.trim(),
            'tamanhoCamisa': config.exibirModuloFinanceiro
                ? (_tamanhoCamisa ?? '')
                : (widget.membro?.tamanhoCamisa ?? ''),
            'usuario': permiteAcessoPescador ? usuario : '',
            'senha': permiteAcessoPescador ? senha : '',
          },
          files: _fotoPath != null ? {'foto': _fotoPath!} : null,
          token: auth.token,
        );
      } else {
        await _api.postMultipart(
          ApiConstants.membros(auth!.slug!),
          fields: {
            'nome': _nomeController.text.trim(),
            'celular': _celularController.text.trim(),
            'tamanhoCamisa': config.exibirModuloFinanceiro ? (_tamanhoCamisa ?? '') : '',
            'usuario': permiteAcessoPescador ? usuario : '',
            'senha': permiteAcessoPescador ? senha : '',
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
    final exibirFinanceiro = config?.exibirModuloFinanceiro ?? true;
    final exibirAcessoPescador = config?.permitirRegistroPublicoMembro ?? false;
    final fotoAtualUrl = AppConfig.resolverUrl(widget.membro?.fotoUrl);
    final tamanhosCamisa = {
      ..._tamanhosPadrao,
      if ((_tamanhoCamisa ?? '').isNotEmpty) _tamanhoCamisa!,
    }.toList();

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
            TextFormField(
              controller: _celularController,
              keyboardType: TextInputType.phone,
              inputFormatters: const [_CelularInputFormatter()],
              decoration: const InputDecoration(
                labelText: 'Celular',
                hintText: 'Opcional',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 16),
            if (exibirAcessoPescador) ...[
              TextFormField(
                controller: _usuarioController,
                decoration: const InputDecoration(
                  labelText: 'Usuario',
                  hintText: 'Opcional',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _senhaController,
                obscureText: !_senhaVisivel,
                decoration: InputDecoration(
                  labelText: _editando ? 'Nova senha' : 'Senha',
                  hintText: _editando ? 'Deixe em branco para manter a atual' : 'Opcional',
                  border: const OutlineInputBorder(),
                  suffixIcon: IconButton(
                    onPressed: () => setState(() => _senhaVisivel = !_senhaVisivel),
                    icon: Icon(_senhaVisivel ? Icons.visibility_off : Icons.visibility),
                  ),
                ),
                validator: (value) {
                  if ((value ?? '').isEmpty) return null;
                  if (value!.length < 6) {
                    return 'A senha deve ter no minimo 6 caracteres.';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
            ],
            if (exibirFinanceiro) ...[
              DropdownButtonFormField<String>(
                initialValue: _tamanhoCamisa,
                decoration: const InputDecoration(
                  labelText: 'Tamanho da camisa',
                  border: OutlineInputBorder(),
                ),
                items: [
                  const DropdownMenuItem<String>(
                    value: null,
                    child: Text('Nao informado'),
                  ),
                  ...tamanhosCamisa.map(
                    (tamanho) => DropdownMenuItem<String>(
                      value: tamanho,
                      child: Text(tamanho),
                    ),
                  ),
                ],
                onChanged: (value) => setState(() => _tamanhoCamisa = value),
              ),
              const SizedBox(height: 8),
            ],
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
    _celularController.dispose();
    _usuarioController.dispose();
    _senhaController.dispose();
    super.dispose();
  }

  static String _formatarCelular(String valor) {
    final digitos = valor.replaceAll(RegExp(r'[^0-9]'), '');
    if (digitos.isEmpty) return '';

    final buffer = StringBuffer('(');
    final tamanhoDdd = digitos.length < 2 ? digitos.length : 2;
    final ddd = digitos.substring(0, tamanhoDdd);
    buffer.write(ddd);

    if (digitos.length <= 2) {
      return buffer.toString();
    }

    buffer.write(') ');
    final fimParteInicial = digitos.length < 7 ? digitos.length : 7;
    final parteInicial = digitos.substring(2, fimParteInicial);
    buffer.write(parteInicial);

    if (digitos.length <= 7) {
      return buffer.toString();
    }

    buffer.write('-');
    final fimParteFinal = digitos.length < 11 ? digitos.length : 11;
    buffer.write(digitos.substring(7, fimParteFinal));
    return buffer.toString();
  }
}

class _CelularInputFormatter extends TextInputFormatter {
  const _CelularInputFormatter();

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    final digitos = newValue.text.replaceAll(RegExp(r'[^0-9]'), '');
    final limitado = digitos.length > 11 ? digitos.substring(0, 11) : digitos;
    final formatado = _MembroFormScreenState._formatarCelular(limitado);

    return TextEditingValue(
      text: formatado,
      selection: TextSelection.collapsed(offset: formatado.length),
    );
  }
}
