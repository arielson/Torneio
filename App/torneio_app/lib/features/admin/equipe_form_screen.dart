import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/flavor_config.dart';
import '../../core/models/equipe.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import 'widgets/admin_photo_picker.dart';

class EquipeFormScreen extends StatefulWidget {
  final Equipe? equipe;

  const EquipeFormScreen({
    super.key,
    this.equipe,
  });

  @override
  State<EquipeFormScreen> createState() => _EquipeFormScreenState();
}

class _EquipeFormScreenState extends State<EquipeFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _api = ApiService();
  final _nomeController = TextEditingController();
  final _capitaoController = TextEditingController();
  final _qtdVagasController = TextEditingController(text: '1');
  final _custoController = TextEditingController(text: '0');
  final _picker = ImagePicker();
  String _statusFinanceiro = 'Pendente';
  DateTime? _dataVencimentoCusto;

  bool _salvando = false;
  String? _fotoEquipePath;
  String? _fotoCapitaoPath;

  bool get _editando => widget.equipe != null;

  @override
  void initState() {
    super.initState();
    final equipe = widget.equipe;
    if (equipe != null) {
      _nomeController.text = equipe.nome;
      _capitaoController.text = equipe.capitao;
      _qtdVagasController.text = equipe.qtdVagas.toString();
      _custoController.text = equipe.custo.toStringAsFixed(2);
      _statusFinanceiro = equipe.statusFinanceiro;
      _dataVencimentoCusto = equipe.dataVencimentoCusto;
    }
  }

  Future<void> _selecionarDataVencimento() async {
    final agora = DateTime.now();
    final data = await showDatePicker(
      context: context,
      initialDate: _dataVencimentoCusto ?? agora,
      firstDate: DateTime(agora.year - 5),
      lastDate: DateTime(agora.year + 10),
    );
    if (data != null && mounted) {
      setState(() => _dataVencimentoCusto = data);
    }
  }

  Future<void> _selecionarFoto({
    required bool capitao,
    required ImageSource source,
  }) async {
    final foto = await _picker.pickImage(
      source: source,
      maxWidth: 1280,
      maxHeight: 1280,
      imageQuality: 85,
    );
    if (foto != null && mounted) {
      setState(() {
        if (capitao) {
          _fotoCapitaoPath = foto.path;
        } else {
          _fotoEquipePath = foto.path;
        }
      });
    }
  }

  Future<void> _abrirSeletorFoto({required bool capitao}) async {
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
                await _selecionarFoto(capitao: capitao, source: ImageSource.gallery);
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_camera_outlined),
              title: const Text('Tirar foto'),
              onTap: () async {
                Navigator.pop(context);
                await _selecionarFoto(capitao: capitao, source: ImageSource.camera);
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

    final exibirVagas = config.modoSorteio != 'Nenhum';
    final exibirFinanceiro = config.exibirModuloFinanceiro;
    final qtdVagas = exibirVagas ? int.parse(_qtdVagasController.text) : 1;
    final custo = exibirFinanceiro
        ? double.tryParse(_custoController.text.replaceAll(',', '.'))
        : (_editando ? widget.equipe!.custo : 0);
    if (exibirFinanceiro && (custo == null || custo < 0)) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Informe um custo valido.'), backgroundColor: Colors.red),
      );
      setState(() => _salvando = false);
      return;
    }

    try {
      final fields = {
        'nome': _nomeController.text.trim(),
        'capitao': _capitaoController.text.trim(),
        'qtdVagas': '$qtdVagas',
        'custo': '${custo ?? 0}',
        'dataVencimentoCusto': _dataVencimentoCusto?.toIso8601String() ?? '',
        'statusFinanceiro': exibirFinanceiro
            ? _statusFinanceiro
            : (_editando ? widget.equipe!.statusFinanceiro : 'Pendente'),
      };

      final files = {
        if (_fotoEquipePath != null) 'foto': _fotoEquipePath!,
        if (_fotoCapitaoPath != null) 'fotoCapitao': _fotoCapitaoPath!,
      };

      if (_editando) {
        await _api.putMultipart(
          '${ApiConstants.equipes(auth!.slug!)}/${widget.equipe!.id}',
          fields: fields,
          files: files.isEmpty ? null : files,
          token: auth.token,
        );
      } else {
        await _api.postMultipart(
          ApiConstants.equipes(auth!.slug!),
          fields: fields,
          files: files.isEmpty ? null : files,
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
    final label = config?.labelEquipe ?? 'Equipe';
    final exibirVagas = config?.modoSorteio != 'Nenhum';
    final exibirFinanceiro = config?.exibirModuloFinanceiro ?? true;
    final fotoEquipeAtualUrl = AppConfig.resolverUrl(widget.equipe?.fotoUrl);
    final fotoCapitaoAtualUrl = AppConfig.resolverUrl(widget.equipe?.fotoCapitaoUrl);

    return Scaffold(
      appBar: AppBar(
        title: Text(_editando ? 'Editar $label' : 'Nova $label'),
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _nomeController,
              decoration: InputDecoration(
                labelText: 'Nome da ${label.toLowerCase()}',
                border: const OutlineInputBorder(),
              ),
              validator: (value) =>
                  (value == null || value.trim().isEmpty) ? 'Informe o nome.' : null,
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _capitaoController,
              decoration: const InputDecoration(
                labelText: 'Capitao',
                border: OutlineInputBorder(),
              ),
              validator: (value) =>
                  (value == null || value.trim().isEmpty) ? 'Informe o capitao.' : null,
            ),
            const SizedBox(height: 16),
            if (exibirVagas)
              TextFormField(
                controller: _qtdVagasController,
                decoration: const InputDecoration(
                  labelText: 'Quantidade de vagas',
                  border: OutlineInputBorder(),
                ),
                keyboardType: TextInputType.number,
                validator: (value) {
                  final vagas = int.tryParse(value ?? '');
                  if (vagas == null || vagas <= 0) {
                    return 'Informe ao menos 1 vaga.';
                  }
                  return null;
                },
              ),
            const SizedBox(height: 16),
            if (exibirFinanceiro) ...[
              TextFormField(
                controller: _custoController,
                decoration: const InputDecoration(
                  labelText: 'Custo da embarcacao',
                  border: OutlineInputBorder(),
                ),
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
                validator: (value) {
                  final custo = double.tryParse((value ?? '').replaceAll(',', '.'));
                  if (custo == null || custo < 0) {
                    return 'Informe um custo valido.';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<String>(
                initialValue: _statusFinanceiro,
                decoration: const InputDecoration(
                  labelText: 'Status financeiro',
                  border: OutlineInputBorder(),
                ),
                items: const [
                  DropdownMenuItem(value: 'Pendente', child: Text('Pendente')),
                  DropdownMenuItem(value: 'Confirmada', child: Text('Confirmada')),
                  DropdownMenuItem(value: 'Cancelada', child: Text('Cancelada')),
                ],
                onChanged: (value) {
                  if (value != null) {
                    setState(() => _statusFinanceiro = value);
                  }
                },
              ),
              const SizedBox(height: 12),
              OutlinedButton.icon(
                onPressed: _selecionarDataVencimento,
                icon: const Icon(Icons.calendar_month_outlined),
                label: Text(
                  _dataVencimentoCusto == null
                      ? 'Selecionar vencimento do custo'
                      : 'Vencimento do custo: ${_dataVencimentoCusto!.day.toString().padLeft(2, '0')}/${_dataVencimentoCusto!.month.toString().padLeft(2, '0')}/${_dataVencimentoCusto!.year}',
                ),
              ),
              if (_dataVencimentoCusto != null)
                Align(
                  alignment: Alignment.centerLeft,
                  child: TextButton(
                    onPressed: () => setState(() => _dataVencimentoCusto = null),
                    child: const Text('Limpar vencimento'),
                  ),
                ),
              const SizedBox(height: 16),
            ],
            AdminPhotoPicker(
              titulo: 'Foto da ${label.toLowerCase()}',
              fotoLocalPath: _fotoEquipePath,
              fotoAtualUrl: fotoEquipeAtualUrl,
              onTap: () => _abrirSeletorFoto(capitao: false),
            ),
            const SizedBox(height: 16),
            AdminPhotoPicker(
              titulo: 'Foto do capitao',
              fotoLocalPath: _fotoCapitaoPath,
              fotoAtualUrl: fotoCapitaoAtualUrl,
              onTap: () => _abrirSeletorFoto(capitao: true),
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
    _capitaoController.dispose();
    _qtdVagasController.dispose();
    _custoController.dispose();
    super.dispose();
  }
}
