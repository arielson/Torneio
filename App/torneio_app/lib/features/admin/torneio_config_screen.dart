import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/flavor_config.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import 'widgets/admin_photo_picker.dart';

class TorneioConfigScreen extends StatefulWidget {
  const TorneioConfigScreen({super.key});

  @override
  State<TorneioConfigScreen> createState() => _TorneioConfigScreenState();
}

class _TorneioConfigScreenState extends State<TorneioConfigScreen> {
  final _api = ApiService();
  final _formKey = GlobalKey<FormState>();
  final _picker = ImagePicker();
  final _nomeController = TextEditingController();
  final _descricaoController = TextEditingController();
  final _observacoesController = TextEditingController();
  final _qtdGanhadoresController = TextEditingController();
  final _corPrimariaController = TextEditingController();

  bool _carregando = true;
  bool _salvando = false;
  String? _erro;
  String? _logoUrl;
  String? _logoPath;
  bool _usarFatorMultiplicador = false;
  bool _permitirCapturaOffline = false;
  bool _exibirModuloFinanceiro = true;
  bool _exibirParticipantesPublicos = false;
  bool _exibirNaListaInicialPublica = true;
  bool _exibirNaPesquisaPublica = true;
  bool _premiacaoPorEquipe = true;
  bool _premiacaoPorMembro = false;
  bool _apenasMaiorCapturaPorPescador = false;

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
      final data = await _api.get(
        ApiConstants.torneioConfiguracaoAdmin(auth!.slug!),
        token: auth.token,
      ) as Map<String, dynamic>;

      _nomeController.text = data['nomeTorneio'] as String? ?? '';
      _descricaoController.text = data['descricao'] as String? ?? '';
      _observacoesController.text = data['observacoesInternas'] as String? ?? '';
      _qtdGanhadoresController.text = '${data['qtdGanhadores'] as int? ?? 3}';
      _corPrimariaController.text = data['corPrimaria'] as String? ?? '';
      _logoUrl = AppConfig.resolverUrl(data['logoUrl'] as String?);
      _usarFatorMultiplicador = data['usarFatorMultiplicador'] as bool? ?? false;
      _permitirCapturaOffline = data['permitirCapturaOffline'] as bool? ?? false;
      _exibirModuloFinanceiro = data['exibirModuloFinanceiro'] as bool? ?? true;
      _exibirParticipantesPublicos = data['exibirParticipantesPublicos'] as bool? ?? false;
      _exibirNaListaInicialPublica = data['exibirNaListaInicialPublica'] as bool? ?? true;
      _exibirNaPesquisaPublica = data['exibirNaPesquisaPublica'] as bool? ?? true;
      _premiacaoPorEquipe = data['premiacaoPorEquipe'] as bool? ?? true;
      _premiacaoPorMembro = data['premiacaoPorMembro'] as bool? ?? false;
      _apenasMaiorCapturaPorPescador = data['apenasMaiorCapturaPorPescador'] as bool? ?? false;
    } on ApiException catch (e) {
      _erro = e.message;
    } catch (_) {
      _erro = 'Erro ao carregar os dados do torneio.';
    } finally {
      if (mounted) {
        setState(() => _carregando = false);
      }
    }
  }

  Future<void> _selecionarLogo(ImageSource source) async {
    final arquivo = await _picker.pickImage(
      source: source,
      maxWidth: 1280,
      maxHeight: 1280,
      imageQuality: 85,
    );
    if (arquivo != null && mounted) {
      setState(() => _logoPath = arquivo.path);
    }
  }

  Future<void> _abrirSeletorLogo() async {
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
                await _selecionarLogo(ImageSource.gallery);
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_camera_outlined),
              title: const Text('Tirar foto'),
              onTap: () async {
                Navigator.pop(context);
                await _selecionarLogo(ImageSource.camera);
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

    setState(() => _salvando = true);
    try {
      await _api.putMultipart(
        ApiConstants.torneioConfiguracaoAdmin(auth!.slug!),
        fields: {
          'nomeTorneio': _nomeController.text.trim(),
          'descricao': _descricaoController.text.trim(),
          'observacoesInternas': _observacoesController.text.trim(),
          'qtdGanhadores': _qtdGanhadoresController.text.trim(),
          'usarFatorMultiplicador': _usarFatorMultiplicador,
          'permitirCapturaOffline': _permitirCapturaOffline,
          'exibirModuloFinanceiro': _exibirModuloFinanceiro,
          'exibirParticipantesPublicos': _exibirParticipantesPublicos,
          'exibirNaListaInicialPublica': _exibirNaListaInicialPublica,
          'exibirNaPesquisaPublica': _exibirNaPesquisaPublica,
          'premiacaoPorEquipe': _premiacaoPorEquipe,
          'premiacaoPorMembro': _premiacaoPorMembro,
          'apenasMaiorCapturaPorPescador': _apenasMaiorCapturaPorPescador,
          'corPrimaria': _corPrimariaController.text.trim(),
        },
        files: _logoPath != null ? {'logo': _logoPath!} : null,
        token: auth.token,
      );

      if (!mounted) return;
      await context.read<ConfigProvider>().carregarConfig(auth.slug!);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Dados do torneio atualizados com sucesso.')),
      );
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
    return Scaffold(
      appBar: AppBar(title: const Text('Dados do torneio')),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : _erro != null
              ? Center(child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Text(_erro!, textAlign: TextAlign.center),
                ))
              : Form(
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
                        controller: _descricaoController,
                        minLines: 3,
                        maxLines: 5,
                        decoration: const InputDecoration(
                          labelText: 'Descricao',
                          border: OutlineInputBorder(),
                        ),
                        validator: (value) {
                          if ((value ?? '').length > 2000) {
                            return 'A descricao deve ter no maximo 2000 caracteres.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _observacoesController,
                        minLines: 3,
                        maxLines: 5,
                        decoration: const InputDecoration(
                          labelText: 'Observacoes internas',
                          helperText: 'Campo interno. Nao e exibido publicamente.',
                          border: OutlineInputBorder(),
                        ),
                        validator: (value) {
                          if ((value ?? '').length > 4000) {
                            return 'As observacoes internas devem ter no maximo 4000 caracteres.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _qtdGanhadoresController,
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          labelText: 'Quantidade de ganhadores',
                          border: OutlineInputBorder(),
                        ),
                        validator: (value) {
                          final numero = int.tryParse((value ?? '').trim());
                          if (numero == null || numero < 1 || numero > 100) {
                            return 'Informe entre 1 e 100 ganhadores.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _corPrimariaController,
                        decoration: const InputDecoration(
                          labelText: 'Cor primaria',
                          hintText: '#106962',
                          border: OutlineInputBorder(),
                        ),
                        validator: (value) {
                          final cor = (value ?? '').trim();
                          if (cor.isEmpty) return null;
                          final regex = RegExp(r'^#[0-9A-Fa-f]{6}$');
                          if (!regex.hasMatch(cor)) {
                            return 'Use o formato #RRGGBB.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 8),
                      AdminPhotoPicker(
                        titulo: 'Logo do torneio',
                        fotoAtualUrl: _logoUrl,
                        fotoLocalPath: _logoPath,
                        onTap: _abrirSeletorLogo,
                      ),
                      const SizedBox(height: 8),
                      SwitchListTile(
                        value: _usarFatorMultiplicador,
                        onChanged: (value) => setState(() => _usarFatorMultiplicador = value),
                        title: const Text('Usar Fator Multiplicador'),
                      ),
                      SwitchListTile(
                        value: _permitirCapturaOffline,
                        onChanged: (value) => setState(() => _permitirCapturaOffline = value),
                        title: const Text('Permitir Captura Offline'),
                      ),
                      SwitchListTile(
                        value: _exibirModuloFinanceiro,
                        onChanged: (value) => setState(() => _exibirModuloFinanceiro = value),
                        title: const Text('Exibir Modulo Financeiro'),
                      ),
                      SwitchListTile(
                        value: _exibirParticipantesPublicos,
                        onChanged: (value) => setState(() => _exibirParticipantesPublicos = value),
                        title: const Text('Exibir participantes na tela inicial publica do torneio'),
                      ),
                      SwitchListTile(
                        value: _exibirNaListaInicialPublica,
                        onChanged: (value) => setState(() => _exibirNaListaInicialPublica = value),
                        title: const Text('Exibir torneio na lista inicial publica'),
                      ),
                      SwitchListTile(
                        value: _exibirNaPesquisaPublica,
                        onChanged: (value) => setState(() => _exibirNaPesquisaPublica = value),
                        title: const Text('Exibir torneio na pesquisa publica'),
                      ),
                      SwitchListTile(
                        value: _premiacaoPorEquipe,
                        onChanged: (value) => setState(() => _premiacaoPorEquipe = value),
                        title: const Text('Premiar por Embarcacoes (ranking de equipes)'),
                      ),
                      SwitchListTile(
                        value: _premiacaoPorMembro,
                        onChanged: (value) => setState(() => _premiacaoPorMembro = value),
                        title: const Text('Premiar por Pescadores (ranking individual)'),
                      ),
                      SwitchListTile(
                        value: _apenasMaiorCapturaPorPescador,
                        onChanged: (value) => setState(() => _apenasMaiorCapturaPorPescador = value),
                        title: const Text('Apenas a maior captura por pescador no ranking'),
                        subtitle: const Text(
                          'Quando ativado, cada pescador contribui somente com sua melhor captura. Uma nova captura maior substitui a anterior.',
                        ),
                      ),
                      const SizedBox(height: 16),
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
    _descricaoController.dispose();
    _observacoesController.dispose();
    _qtdGanhadoresController.dispose();
    _corPrimariaController.dispose();
    super.dispose();
  }
}
