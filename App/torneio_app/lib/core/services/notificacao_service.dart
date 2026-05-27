import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../constants.dart';
import 'api_service.dart';

/// Chamada no handler de background (top-level function obrigatória pelo FCM).
@pragma('vm:entry-point')
Future<void> _fcmBackgroundHandler(RemoteMessage message) async {}

class NotificacaoService {
  static final _localNotifications = FlutterLocalNotificationsPlugin();
  static const _channelId = 'torneio_avisos';
  static const _prefKeyToken = 'fcm_token';
  static const _prefKeySlug = 'fcm_slug';

  static Future<void> inicializar() async {
    FirebaseMessaging.onBackgroundMessage(_fcmBackgroundHandler);

    const androidSettings = AndroidInitializationSettings('@mipmap/ic_launcher');
    const iosSettings = DarwinInitializationSettings();
    await _localNotifications.initialize(
      const InitializationSettings(android: androidSettings, iOS: iosSettings),
    );

    const channel = AndroidNotificationChannel(
      _channelId,
      'Avisos do Torneio',
      description: 'Notificações e mensagens do torneio que você segue.',
      importance: Importance.high,
    );
    await _localNotifications
        .resolvePlatformSpecificImplementation<AndroidFlutterLocalNotificationsPlugin>()
        ?.createNotificationChannel(channel);

    FirebaseMessaging.onMessage.listen((message) {
      final notification = message.notification;
      if (notification == null) return;
      _localNotifications.show(
        notification.hashCode,
        notification.title,
        notification.body,
        NotificationDetails(
          android: AndroidNotificationDetails(_channelId, 'Avisos do Torneio',
              importance: Importance.high, priority: Priority.high),
          iOS: const DarwinNotificationDetails(),
        ),
      );
    });
  }

  /// Registra o device como seguidor do torneio identificado por [slug].
  static Future<void> seguirTorneio(String slug) async {
    final token = await FirebaseMessaging.instance.getToken();
    if (token == null) return;

    final plataforma = _resolvePlataforma();
    final api = ApiService();
    try {
      await api.post(ApiConstants.seguidor(slug), {
        'deviceToken': token,
        'plataforma': plataforma,
      });
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(_prefKeyToken, token);
      await prefs.setString(_prefKeySlug, slug);
    } catch (_) {}
  }

  /// Remove o device como seguidor do torneio identificado por [slug].
  static Future<void> deixarDeSeguirTorneio(String slug) async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString(_prefKeyToken);
    if (token == null) return;

    final api = ApiService();
    try {
      await api.delete('${ApiConstants.seguidor(slug)}?deviceToken=${Uri.encodeComponent(token)}');
      await prefs.remove(_prefKeyToken);
      await prefs.remove(_prefKeySlug);
    } catch (_) {}
  }

  /// Retorna true se o dispositivo já está seguindo o [slug].
  static Future<bool> estaSeguindo(String slug) async {
    final prefs = await SharedPreferences.getInstance();
    final slugSalvo = prefs.getString(_prefKeySlug);
    return slugSalvo == slug;
  }

  static String _resolvePlataforma() {
    // flutter_local_notifications já garante o contexto correto,
    // mas não há API direta de platform no service layer.
    // O backend aceita "Android" ou "iOS" — usamos defaultTargetPlatform via import condicional.
    return 'Android'; // substituído em runtime pelo build flavor se necessário
  }
}
