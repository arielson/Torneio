import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class RegistroPescadorScreen extends StatefulWidget {
  const RegistroPescadorScreen({super.key});

  @override
  State<RegistroPescadorScreen> createState() => _RegistroPescadorScreenState();
}

class _RegistroPescadorScreenState extends State<RegistroPescadorScreen> {
  final _api = ApiService();
  final _formKey = GlobalKey<FormState>();
  final _nomeController = TextEditingController();
  final _celularController = TextEditingController();
  final _codigoController = TextEditingController();
  final _usuarioController = TextEditingController();
  final _senhaController = TextEditingController();

  bool _enviando = false;
  bool _confirmando = false;
  bool _codigoEnviado = false;
  bool _senhaVisivel = false;
  String? _registroId;
  String? _celularMascarado;
  String? _tamanhoCamisa;

  Future<void> _solicitarCodigo() async {
    if (!_formKey.currentState!.validate()) return;

    final config = context.read<ConfigProvider>().config;
    if (config == null) return;

    setState(() => _enviando = true);

    try {
      final data = await _api.post(ApiConstants.registroPescadorSolicitarCodigo(config.slug), {
        'nome': _nomeController.text.trim(),
        'celular': _celularController.text.trim(),
        'tamanhoCamisa': config.exibirModuloFinanceiro ? _tamanhoCamisa : null,
      });

      if (!mounted) return;
      setState(() {
        _codigoEnviado = true;
        _registroId = data['registroId'] as String?;
        _celularMascarado = data['celularMascarado'] as String?;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(data['mensagem'] as String? ?? 'Codigo enviado por SMS.')),
      );
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) {
        setState(() => _enviando = false);
      }
    }
  }

  Future<void> _confirmar() async {
    if ((_registroId ?? '').isEmpty) return;
    if (_codigoController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Informe o codigo recebido por SMS.'), backgroundColor: Colors.red),
      );
      return;
    }

    final config = context.read<ConfigProvider>().config;
    if (config == null) return;

    setState(() => _confirmando = true);

    try {
      await _api.post(ApiConstants.registroPescadorConfirmar(config.slug), {
        'registroId': _registroId,
        'codigo': _codigoController.text.trim(),
        'usuario': _usuarioController.text.trim(),
        'senha': _senhaController.text,
      });

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('${config.labelMembro} cadastrado com sucesso.')),
      );
      setState(() {
        _codigoEnviado = false;
        _registroId = null;
        _celularMascarado = null;
        _tamanhoCamisa = null;
      });
      _formKey.currentState?.reset();
      _nomeController.clear();
      _celularController.clear();
      _codigoController.clear();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) {
        setState(() => _confirmando = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    if (config == null) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (!config.permitirRegistroPublicoMembro) {
      return Scaffold(
        appBar: AppBar(title: Text('Registro de ${config.labelMembro}')),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Text(
              'O cadastro publico de ${config.labelMembroPlural.toLowerCase()} nao esta habilitado neste torneio.',
              textAlign: TextAlign.center,
            ),
          ),
        ),
      );
    }

    return Scaffold(
      appBar: AppBar(title: Text('Registro de ${config.labelMembro}')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      config.nomeTorneio,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _nomeController,
                      textCapitalization: TextCapitalization.words,
                      decoration: InputDecoration(
                        labelText: 'Nome do ${config.labelMembro.toLowerCase()}',
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
                        helperText: 'Voce recebera um codigo por SMS para confirmar o cadastro.',
                        border: OutlineInputBorder(),
                      ),
                      validator: (value) {
                        final digits = (value ?? '').replaceAll(RegExp(r'[^0-9]'), '');
                        if (digits.length < 10) return 'Informe um celular valido.';
                        return null;
                      },
                    ),
                    if (config.exibirModuloFinanceiro) ...[
                      const SizedBox(height: 16),
                      DropdownButtonFormField<String>(
                        initialValue: _tamanhoCamisa,
                        decoration: const InputDecoration(
                          labelText: 'Tamanho da camisa',
                          border: OutlineInputBorder(),
                        ),
                        items: [
                          const DropdownMenuItem<String>(value: null, child: Text('Nao informado')),
                          ...['PP', 'P', 'M', 'G', 'GG', 'XGG', 'EXGG'].map(
                            (tamanho) => DropdownMenuItem<String>(
                              value: tamanho,
                              child: Text(tamanho),
                            ),
                          ),
                        ],
                        onChanged: (value) => setState(() => _tamanhoCamisa = value),
                      ),
                    ],
                    const SizedBox(height: 24),
                    FilledButton.icon(
                      onPressed: _enviando ? null : _solicitarCodigo,
                      icon: _enviando
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Icon(Icons.sms_outlined),
                      label: Text(_enviando ? 'Enviando...' : 'Enviar codigo por SMS'),
                    ),
                  ],
                ),
              ),
            ),
          ),
          if (_codigoEnviado) ...[
            const SizedBox(height: 16),
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      'Validacao do codigo',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 8),
                    Text('Digite o codigo enviado para ${_celularMascarado ?? 'o celular informado'}.'),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _usuarioController,
                      decoration: const InputDecoration(
                        labelText: 'Usuario',
                        hintText: 'Opcional',
                        helperText: 'Defina usuario e senha para acompanhar suas cobrancas.',
                        border: OutlineInputBorder(),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _senhaController,
                      obscureText: !_senhaVisivel,
                      decoration: InputDecoration(
                        labelText: 'Senha',
                        hintText: 'Opcional',
                        border: const OutlineInputBorder(),
                        suffixIcon: IconButton(
                          onPressed: () => setState(() => _senhaVisivel = !_senhaVisivel),
                          icon: Icon(_senhaVisivel ? Icons.visibility_off : Icons.visibility),
                        ),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _codigoController,
                      keyboardType: TextInputType.number,
                      decoration: const InputDecoration(
                        labelText: 'Codigo',
                        border: OutlineInputBorder(),
                      ),
                    ),
                    const SizedBox(height: 16),
                    FilledButton.icon(
                      onPressed: _confirmando ? null : _confirmar,
                      icon: _confirmando
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Icon(Icons.verified_user_outlined),
                      label: Text(_confirmando ? 'Confirmando...' : 'Confirmar cadastro'),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }

  @override
  void dispose() {
    _nomeController.dispose();
    _celularController.dispose();
    _codigoController.dispose();
    _usuarioController.dispose();
    _senhaController.dispose();
    super.dispose();
  }
}

class _CelularInputFormatter extends TextInputFormatter {
  const _CelularInputFormatter();

  @override
  TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
    final digitos = newValue.text.replaceAll(RegExp(r'[^0-9]'), '');
    final limitado = digitos.length > 11 ? digitos.substring(0, 11) : digitos;
    final formatado = _formatarCelular(limitado);

    return TextEditingValue(
      text: formatado,
      selection: TextSelection.collapsed(offset: formatado.length),
    );
  }

  static String _formatarCelular(String valor) {
    if (valor.isEmpty) return '';
    if (valor.length <= 2) return '($valor';
    if (valor.length <= 7) return '(${valor.substring(0, 2)}) ${valor.substring(2)}';
    final finalIndex = valor.length > 11 ? 11 : valor.length;
    return '(${valor.substring(0, 2)}) ${valor.substring(2, 7)}-${valor.substring(7, finalIndex)}';
  }
}
