import 'dart:async';
import 'package:app_links/app_links.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'core/services/api_service.dart';
import 'core/providers/auth_provider.dart';
import 'core/providers/captura_provider.dart';
import 'core/providers/config_provider.dart';
import 'core/providers/home_provider.dart';
import 'features/home/home_screen.dart';
import 'features/torneio/torneio_screen.dart';
import 'features/login/login_screen.dart';
import 'features/fiscal/home_screen.dart';
import 'features/fiscal/registrar_captura_screen.dart';
import 'features/fiscal/capturas_screen.dart';
import 'features/fiscal/sync_screen.dart';
import 'features/admin/home_screen.dart';
import 'features/admin/capturas_screen.dart';
import 'features/admin/equipes_screen.dart';
import 'features/admin/fiscais_screen.dart';
import 'features/admin/itens_screen.dart';
import 'features/admin/patrocinadores_screen.dart';
import 'features/admin/membros_screen.dart';
import 'features/admin/relatorios_equipes_screen.dart';
import 'features/admin/relatorios_membros_screen.dart';
import 'features/admin/relatorios_screen.dart';
import 'features/admin/relatorios_ganhadores_screen.dart';
import 'features/admin/reorganizacao_emergencial_screen.dart';
import 'features/admin/sorteio_screen.dart';
import 'features/admin/grupos_screen.dart';
import 'features/admin/financeiro_screen.dart';
import 'features/admin/financeiro_configuracao_screen.dart';
import 'features/admin/parcelas_screen.dart';
import 'features/admin/custos_screen.dart';
import 'features/admin/checklist_screen.dart';
import 'features/admin/doacoes_screen.dart';
import 'features/admin/extras_screen.dart';
import 'theme/app_theme.dart';

class TorneioApp extends StatefulWidget {
  const TorneioApp({super.key});

  @override
  State<TorneioApp> createState() => _TorneioAppState();
}

class _TorneioAppState extends State<TorneioApp> {
  final _navigatorKey = GlobalKey<NavigatorState>();
  late final AppLinks _appLinks;
  StreamSubscription<Uri>? _linkSub;

  @override
  void initState() {
    super.initState();
    _appLinks = AppLinks();
    _linkSub = _appLinks.uriLinkStream.listen(_handleDeepLink);
  }

  @override
  void dispose() {
    _linkSub?.cancel();
    super.dispose();
  }

  void _handleDeepLink(Uri uri) {
    // https://torneio.ari.net.br/slug
    final slug = uri.pathSegments.isNotEmpty ? uri.pathSegments.first : null;
    if (slug != null && slug.isNotEmpty) {
      _navigatorKey.currentState?.pushNamed('/torneio', arguments: slug);
    }
  }

  @override
  Widget build(BuildContext context) {
    final api = ApiService();

    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider(api)),
        ChangeNotifierProvider(create: (_) => ConfigProvider(api)),
        ChangeNotifierProvider(create: (_) => CapturaProvider(api)),
        ChangeNotifierProvider(create: (_) => HomeProvider(api)),
      ],
      child: Consumer<ConfigProvider>(
        builder:
            (context, configProvider, _) => MaterialApp(
              navigatorKey: _navigatorKey,
              title: 'Torneio',
              debugShowCheckedModeBanner: false,
              theme: AppTheme.fromConfig(configProvider.config),
              initialRoute: '/',
              routes: {
                '/': (_) => const _SplashRedirect(),
                '/home': (_) => const HomeScreen(),
                '/torneio': (_) => const TorneioScreen(),
                '/login': (_) => const LoginScreen(),
                '/fiscal/home': (_) => const HomeFiscalScreen(),
                '/fiscal/registrar': (_) => const RegistrarCapturaScreen(),
                '/fiscal/capturas': (_) => const CapturasScreen(),
                '/fiscal/sync': (_) => const SyncScreen(),
                '/admin/home': (_) => const HomeAdminScreen(),
                '/admin/equipes': (_) => const EquipesAdminScreen(),
                '/admin/membros': (_) => const MembrosAdminScreen(),
                '/admin/itens': (_) => const ItensAdminScreen(),
                '/admin/patrocinadores': (_) => const PatrocinadoresAdminScreen(),
                '/admin/fiscais': (_) => const FiscaisAdminScreen(),
                '/admin/capturas': (_) => const CapturasAdminScreen(),
                '/admin/reorganizacao-emergencial':
                    (_) => const ReorganizacaoEmergencialScreen(),
                '/admin/sorteio': (_) => const SorteioAdminScreen(),
                '/admin/grupos': (_) => const GruposAdminScreen(),
                '/admin/financeiro': (_) => const FinanceiroAdminScreen(),
                '/admin/financeiro/configuracao': (_) => const FinanceiroConfiguracaoScreen(),
                '/admin/financeiro/cobrancas': (_) => const ParcelasAdminScreen(),
                '/admin/financeiro/custos': (_) => const CustosAdminScreen(),
                '/admin/financeiro/checklist': (_) => const ChecklistAdminScreen(),
                '/admin/financeiro/doacoes': (_) => const DoacoesAdminScreen(),
                '/admin/financeiro/extras': (_) => const ExtrasAdminScreen(),
                '/admin/relatorios': (_) => const RelatoriosAdminScreen(),
                '/admin/relatorios/equipes':
                    (_) => const RelatoriosEquipesScreen(),
                '/admin/relatorios/membros':
                    (_) => const RelatoriosMembrosScreen(),
                '/admin/relatorios/ganhadores':
                    (_) => const RelatoriosGanhadoresScreen(),
              },
            ),
      ),
    );
  }
}

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
    final configProvider = context.read<ConfigProvider>();

    await auth.restaurarSessao();
    if (!mounted) return;

    if (auth.autenticado && auth.usuario?.slug != null) {
      await configProvider.carregarConfig(auth.usuario!.slug!);
      if (!mounted) return;
      _irParaHome(auth.usuario!.perfil);
    } else {
      Navigator.pushReplacementNamed(context, '/home');
    }
  }

  void _irParaHome(String perfil) {
    switch (perfil) {
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
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.primary,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.emoji_events,
              size: 80,
              color: Theme.of(context).colorScheme.onPrimary,
            ),
            const SizedBox(height: 16),
            CircularProgressIndicator(
              valueColor: AlwaysStoppedAnimation<Color>(
                Theme.of(context).colorScheme.onPrimary,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
