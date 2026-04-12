import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'core/services/api_service.dart';
import 'core/providers/auth_provider.dart';
import 'core/providers/captura_provider.dart';
import 'core/providers/config_provider.dart';
import 'features/entrada/entrada_screen.dart';
import 'features/login/login_screen.dart';
import 'features/telespectador/home_screen.dart';
import 'features/fiscal/home_screen.dart';
import 'features/fiscal/registrar_captura_screen.dart';
import 'features/fiscal/capturas_screen.dart';
import 'features/fiscal/sync_screen.dart';

class TorneioApp extends StatelessWidget {
  const TorneioApp({super.key});

  @override
  Widget build(BuildContext context) {
    final api = ApiService();

    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider(api)),
        ChangeNotifierProvider(create: (_) => ConfigProvider(api)),
        ChangeNotifierProvider(create: (_) => CapturaProvider(api)),
      ],
      child: MaterialApp(
        title: 'Torneio',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: Colors.blue),
          useMaterial3: true,
        ),
        initialRoute: '/',
        routes: {
          '/': (_) => const _SplashRedirect(),
          '/entrada': (_) => const EntradaScreen(),
          '/login': (_) => const LoginScreen(),
          '/publico/home': (_) => const HomeTelespectadorScreen(),
          '/fiscal/home': (_) => const HomeFiscalScreen(),
          '/fiscal/registrar': (_) => const RegistrarCapturaScreen(),
          '/fiscal/capturas': (_) => const CapturasScreen(),
          '/fiscal/sync': (_) => const SyncScreen(),
        },
      ),
    );
  }
}

/// Tela de splash que decide para onde redirecionar
class _SplashRedirect extends StatefulWidget {
  const _SplashRedirect();

  @override
  State<_SplashRedirect> createState() => _SplashRedirectState();
}

class _SplashRedirectState extends State<_SplashRedirect> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _redirecionar());
  }

  Future<void> _redirecionar() async {
    final auth = context.read<AuthProvider>();
    await auth.restaurarSessao();

    if (!mounted) return;

    if (auth.autenticado && auth.usuario?.slug != null) {
      // Restaura config do torneio salvo
      final config = context.read<ConfigProvider>();
      await config.carregarConfig(auth.usuario!.slug!);
      if (!mounted) return;
      Navigator.pushReplacementNamed(context, '/fiscal/home');
    } else {
      Navigator.pushReplacementNamed(context, '/entrada');
    }
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.emoji_events, size: 80, color: Colors.blue),
            SizedBox(height: 16),
            CircularProgressIndicator(),
          ],
        ),
      ),
    );
  }
}
