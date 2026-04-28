import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/captura_provider.dart';
import '../../core/providers/config_provider.dart';

enum _Perfil { fiscal, admin, membro }

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
  bool _preparandoAcesso = false;
  _Perfil _perfil = _Perfil.fiscal;

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;

    final config = context.read<ConfigProvider>().config!;
    final auth = context.read<AuthProvider>();

    await auth.loginTorneio(
      config.slug,
      _usuarioController.text.trim(),
      _senhaController.text,
      perfil: switch (_perfil) {
        _Perfil.fiscal => 'Fiscal',
        _Perfil.admin => 'Admin',
        _Perfil.membro => 'Membro',
      },
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
        setState(() => _preparandoAcesso = true);
        final capturaProvider = context.read<CapturaProvider>();
        await capturaProvider.carregarDadosFiscal(
          config.slug,
          auth.usuario!.token,
          incluirCapturas: false,
        );
        if (!mounted) return;
        setState(() => _preparandoAcesso = false);
        if (capturaProvider.erro != null && !capturaProvider.possuiDadosBasicos) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(capturaProvider.erro!), backgroundColor: Colors.red),
          );
          return;
        }
        if (!mounted) return;
        Navigator.pushReplacementNamed(context, '/fiscal/home');
        return;
      case 'Membro':
        Navigator.pushReplacementNamed(context, '/membro/cobrancas');
        return;
      case 'AdminTorneio':
      case 'AdminGeral':
        Navigator.pushReplacementNamed(context, '/admin/home');
        return;
      default:
        Navigator.pushReplacementNamed(context, '/home');
        return;
    }
  }

  @override
  Widget build(BuildContext context) {
    final routeArgs = ModalRoute.of(context)?.settings.arguments;
    if (routeArgs is String) {
      final perfilInicial = switch (routeArgs.toLowerCase()) {
        'membro' || 'pescador' => _Perfil.membro,
        'admin' => _Perfil.admin,
        _ => _Perfil.fiscal,
      };
      if (_perfil != perfilInicial) {
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (mounted) setState(() => _perfil = perfilInicial);
        });
      }
    }

    final config = context.watch<ConfigProvider>().config;
    final auth = context.watch<AuthProvider>();
    final labelFiscal = config?.labelSupervisor ?? 'Fiscal';
    final labelMembro = config?.labelMembro ?? 'Pescador';
    final permiteAcessoMembro = config?.permitirRegistroPublicoMembro ?? false;

    if (!permiteAcessoMembro && _perfil == _Perfil.membro) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (mounted) setState(() => _perfil = _Perfil.fiscal);
      });
    }

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
                    if (permiteAcessoMembro)
                      ButtonSegment(
                        value: _Perfil.membro,
                        label: Text(labelMembro),
                        icon: const Icon(Icons.badge_outlined),
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
                    : _perfil == _Perfil.admin
                        ? 'Acesso Administrador'
                        : 'Acesso $labelMembro',
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
                validator:
                    (v) =>
                        (v == null || v.trim().isEmpty)
                            ? 'Informe o usuário'
                            : null,
              ),

              const SizedBox(height: 16),

              TextFormField(
                controller: _senhaController,
                decoration: InputDecoration(
                  labelText: 'Senha',
                  prefixIcon: const Icon(Icons.lock),
                  suffixIcon: IconButton(
                    icon: Icon(
                      _senhaVisivel ? Icons.visibility_off : Icons.visibility,
                    ),
                    onPressed:
                        () => setState(() => _senhaVisivel = !_senhaVisivel),
                  ),
                ),
                obscureText: !_senhaVisivel,
                textInputAction: TextInputAction.done,
                onFieldSubmitted: (_) => _login(),
                validator:
                    (v) => (v == null || v.isEmpty) ? 'Informe a senha' : null,
              ),

              const SizedBox(height: 24),

              FilledButton(
                onPressed: (auth.carregando || _preparandoAcesso) ? null : _login,
                child:
                    (auth.carregando || _preparandoAcesso)
                        ? const SizedBox(
                          height: 20,
                          width: 20,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                        : const Text('Entrar'),
              ),
              if (_preparandoAcesso) ...[
                const SizedBox(height: 12),
                const Text(
                  'Preparando dados para uso do fiscal, inclusive para funcionamento offline...',
                  textAlign: TextAlign.center,
                ),
              ],
              if (_perfil == _Perfil.membro) ...[
                const SizedBox(height: 16),
                TextButton(
                  onPressed: () => Navigator.pushNamed(context, '/recuperar-senha-pescador'),
                  child: const Text('Esqueci minha senha'),
                ),
                if (config?.permitirRegistroPublicoMembro ?? false)
                  TextButton(
                    onPressed: () => Navigator.pushNamed(context, '/registro-pescador'),
                    child: const Text('Quero me registrar'),
                  ),
              ],
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
