import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';

enum _Perfil { fiscal, admin }

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _usuarioController = TextEditingController();
  final _senhaController = TextEditingController();
  bool _senhaVisivel = false;
  _Perfil _perfil = _Perfil.fiscal;

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;

    final config = context.read<ConfigProvider>().config!;
    final auth = context.read<AuthProvider>();

    await auth.loginTorneio(
      config.slug,
      _usuarioController.text.trim(),
      _senhaController.text,
      perfil: _perfil == _Perfil.fiscal ? 'Fiscal' : 'Admin',
    );

    if (!mounted) return;

    if (auth.erro != null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(auth.erro!), backgroundColor: Colors.red),
      );
      return;
    }

    // Rota baseada no perfil retornado pelo backend
    switch (auth.usuario!.perfil) {
      case 'Fiscal':
        Navigator.pushReplacementNamed(context, '/fiscal/home');
      case 'AdminTorneio':
      case 'AdminGeral':
        Navigator.pushReplacementNamed(context, '/admin/home');
      default:
        Navigator.pushReplacementNamed(context, '/home');
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final auth = context.watch<AuthProvider>();
    final labelFiscal = config?.labelSupervisor ?? 'Fiscal';

    return Scaffold(
      appBar: AppBar(
        title: Text(config?.nomeTorneio ?? 'Login'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => Navigator.pushReplacementNamed(context, '/home'),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 16),

              // Seletor de perfil
              Center(
                child: SegmentedButton<_Perfil>(
                  segments: [
                    ButtonSegment(
                      value: _Perfil.fiscal,
                      label: Text(labelFiscal),
                      icon: const Icon(Icons.person),
                    ),
                    const ButtonSegment(
                      value: _Perfil.admin,
                      label: Text('Admin'),
                      icon: Icon(Icons.admin_panel_settings),
                    ),
                  ],
                  selected: {_perfil},
                  onSelectionChanged: (s) => setState(() => _perfil = s.first),
                ),
              ),

              const SizedBox(height: 32),

              Text(
                _perfil == _Perfil.fiscal
                    ? 'Acesso $labelFiscal'
                    : 'Acesso Administrador',
                style: Theme.of(context).textTheme.headlineSmall,
                textAlign: TextAlign.center,
              ),

              const SizedBox(height: 32),

              TextFormField(
                controller: _usuarioController,
                decoration: const InputDecoration(
                  labelText: 'Usuário',
                  prefixIcon: Icon(Icons.person),
                ),
                autocorrect: false,
                textInputAction: TextInputAction.next,
                validator: (v) =>
                    (v == null || v.trim().isEmpty) ? 'Informe o usuário' : null,
              ),

              const SizedBox(height: 16),

              TextFormField(
                controller: _senhaController,
                decoration: InputDecoration(
                  labelText: 'Senha',
                  prefixIcon: const Icon(Icons.lock),
                  suffixIcon: IconButton(
                    icon: Icon(
                        _senhaVisivel ? Icons.visibility_off : Icons.visibility),
                    onPressed: () =>
                        setState(() => _senhaVisivel = !_senhaVisivel),
                  ),
                ),
                obscureText: !_senhaVisivel,
                textInputAction: TextInputAction.done,
                onFieldSubmitted: (_) => _login(),
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Informe a senha' : null,
              ),

              const SizedBox(height: 24),

              FilledButton(
                onPressed: auth.carregando ? null : _login,
                child: auth.carregando
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(
                            strokeWidth: 2, color: Colors.white),
                      )
                    : const Text('Entrar'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _usuarioController.dispose();
    _senhaController.dispose();
    super.dispose();
  }
}
