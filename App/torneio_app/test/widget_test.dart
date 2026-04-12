import 'package:flutter_test/flutter_test.dart';
import 'package:torneio_app/app.dart';
import 'package:provider/provider.dart';
import 'package:torneio_app/core/services/api_service.dart';
import 'package:torneio_app/core/providers/auth_provider.dart';
import 'package:torneio_app/core/providers/captura_provider.dart';
import 'package:torneio_app/core/providers/config_provider.dart';

void main() {
  testWidgets('App inicia sem erros', (WidgetTester tester) async {
    final api = ApiService();

    await tester.pumpWidget(
      MultiProvider(
        providers: [
          ChangeNotifierProvider(create: (_) => AuthProvider(api)),
          ChangeNotifierProvider(create: (_) => ConfigProvider(api)),
          ChangeNotifierProvider(create: (_) => CapturaProvider(api)),
        ],
        child: const TorneioApp(),
      ),
    );

    // A splash screen é exibida enquanto verifica a sessão
    expect(find.byType(TorneioApp), findsOneWidget);
  });
}
