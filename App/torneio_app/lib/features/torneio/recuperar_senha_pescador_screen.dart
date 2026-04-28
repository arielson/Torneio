import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class RecuperarSenhaPescadorScreen extends StatefulWidget {
  const RecuperarSenhaPescadorScreen({super.key});

  @override
  State<RecuperarSenhaPescadorScreen> createState() => _RecuperarSenhaPescadorScreenState();
}

class _RecuperarSenhaPescadorScreenState extends State<RecuperarSenhaPescadorScreen> {
  final _api = ApiService();
  final _usuarioController = TextEditingController();
  final _celularController = TextEditingController();
  final _codigoController = TextEditingController();
  final _novaSenhaController = TextEditingController();

  bool _codigoEnviado = false;
  bool _enviando = false;
  bool _confirmando = false;
  bool _senhaVisivel = false;
  String? _celularMascarado;

  Future<void> _solicitarCodigo() async {
    final config = context.read<ConfigProvider>().config;
    if (config == null) return;

    setState(() => _enviando = true);
    try {
      final data = await _api.post(ApiConstants.recuperarSenhaPescadorSolicitarCodigo(config.slug), {
        'usuario': _usuarioController.text.trim(),
        'celular': _celularController.text.trim(),
      });

      if (!mounted) return;
      setState(() {
        _codigoEnviado = true;
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
      if (mounted) setState(() => _enviando = false);
    }
  }

  Future<void> _confirmar() async {
    final config = context.read<ConfigProvider>().config;
    if (config == null) return;
    if (_novaSenhaController.text.length < 6) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('A nova senha deve ter no minimo 6 caracteres.'), backgroundColor: Colors.red),
      );
      return;
    }

    setState(() => _confirmando = true);
    try {
      final data = await _api.post(ApiConstants.recuperarSenhaPescadorConfirmar(config.slug), {
        'usuario': _usuarioController.text.trim(),
        'celular': _celularController.text.trim(),
        'codigo': _codigoController.text.trim(),
        'novaSenha': _novaSenhaController.text,
      });

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(data['mensagem'] as String? ?? 'Senha redefinida com sucesso.')),
      );
      Navigator.pop(context);
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) setState(() => _confirmando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelMembro = config?.labelMembro ?? 'Pescador';

    if (config != null && !config.permitirRegistroPublicoMembro) {
      return Scaffold(
        appBar: AppBar(title: Text('Recuperar senha do $labelMembro')),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Text(
              'O acesso do ${labelMembro.toLowerCase()} nao esta habilitado neste torneio.',
              textAlign: TextAlign.center,
            ),
          ),
        ),
      );
    }

    return Scaffold(
      appBar: AppBar(title: Text('Recuperar senha do $labelMembro')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          TextField(
            controller: _usuarioController,
            decoration: const InputDecoration(
              labelText: 'Usuario',
              border: OutlineInputBorder(),
            ),
          ),
          const SizedBox(height: 16),
          TextField(
            controller: _celularController,
            keyboardType: TextInputType.phone,
            inputFormatters: const [_CelularInputFormatter()],
            decoration: const InputDecoration(
              labelText: 'Celular',
              border: OutlineInputBorder(),
            ),
          ),
          const SizedBox(height: 16),
          FilledButton.icon(
            onPressed: _enviando ? null : _solicitarCodigo,
            icon: _enviando
                ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                : const Icon(Icons.sms_outlined),
            label: Text(_enviando ? 'Enviando...' : 'Enviar codigo por SMS'),
          ),
          if (_codigoEnviado) ...[
            const SizedBox(height: 24),
            Text('Codigo enviado para ${_celularMascarado ?? 'o celular informado'}.'),
            const SizedBox(height: 16),
            TextField(
              controller: _codigoController,
              keyboardType: TextInputType.number,
              decoration: const InputDecoration(
                labelText: 'Codigo',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: _novaSenhaController,
              obscureText: !_senhaVisivel,
              decoration: InputDecoration(
                labelText: 'Nova senha',
                border: const OutlineInputBorder(),
                suffixIcon: IconButton(
                  onPressed: () => setState(() => _senhaVisivel = !_senhaVisivel),
                  icon: Icon(_senhaVisivel ? Icons.visibility_off : Icons.visibility),
                ),
              ),
            ),
            const SizedBox(height: 16),
            FilledButton.icon(
              onPressed: _confirmando ? null : _confirmar,
              icon: _confirmando
                  ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                  : const Icon(Icons.lock_reset),
              label: Text(_confirmando ? 'Redefinindo...' : 'Redefinir senha'),
            ),
          ],
        ],
      ),
    );
  }

  @override
  void dispose() {
    _usuarioController.dispose();
    _celularController.dispose();
    _codigoController.dispose();
    _novaSenhaController.dispose();
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
    return '(${valor.substring(0, 2)}) ${valor.substring(2, 7)}-${valor.substring(7)}';
  }
}
