import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../core/constants.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';

class EntradaScreen extends StatefulWidget {
  const EntradaScreen({super.key});

  @override
  State<EntradaScreen> createState() => _EntradaScreenState();
}

class _EntradaScreenState extends State<EntradaScreen> {
  final _slugController = TextEditingController();
  bool _carregando = false;

  @override
  void initState() {
    super.initState();
    _restaurarUltimoSlug();
  }

  Future<void> _restaurarUltimoSlug() async {
    final prefs = await SharedPreferences.getInstance();
    final ultimo = prefs.getString(StorageKeys.ultimoSlug);
    if (ultimo != null) {
      _slugController.text = ultimo;
    }
  }

  Future<void> _entrar() async {
    final slug = _slugController.text.trim().toLowerCase();
    if (slug.isEmpty) return;

    setState(() => _carregando = true);

    final configProvider = context.read<ConfigProvider>();
    await configProvider.carregarConfig(slug);

    if (!mounted) return;
    setState(() => _carregando = false);

    if (configProvider.erro != null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(configProvider.erro!), backgroundColor: Colors.red),
      );
      return;
    }

    // Verifica se há sessão ativa para redirecionar
    final auth = context.read<AuthProvider>();
    if (auth.autenticado && auth.usuario?.slug == slug) {
      Navigator.pushReplacementNamed(context, '/fiscal/home');
    } else {
      Navigator.pushReplacementNamed(context, '/home');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Icon(Icons.emoji_events, size: 80, color: Colors.blue),
              const SizedBox(height: 16),
              Text(
                'Torneio',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
              const SizedBox(height: 8),
              Text(
                'Digite o slug do seu torneio para continuar',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: Colors.grey,
                    ),
              ),
              const SizedBox(height: 40),
              TextField(
                controller: _slugController,
                decoration: const InputDecoration(
                  labelText: 'Slug do torneio',
                  hintText: 'ex: amigosdapesca',
                  border: OutlineInputBorder(),
                  prefixIcon: Icon(Icons.link),
                ),
                textInputAction: TextInputAction.go,
                autocorrect: false,
                onSubmitted: (_) => _entrar(),
              ),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: _carregando ? null : _entrar,
                child: _carregando
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
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
    _slugController.dispose();
    super.dispose();
  }
}
