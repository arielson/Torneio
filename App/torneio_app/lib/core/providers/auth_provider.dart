import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../constants.dart';
import '../models/usuario_autenticado.dart';
import '../services/api_service.dart';

class AuthProvider extends ChangeNotifier {
  final ApiService _api;

  UsuarioAutenticado? _usuario;
  bool _carregando = false;
  String? _erro;

  AuthProvider(this._api);

  UsuarioAutenticado? get usuario => _usuario;
  ApiService get api => _api;
  bool get carregando => _carregando;
  String? get erro => _erro;
  bool get autenticado => _usuario != null && !(_usuario!.tokenExpirado);

  Future<void> restaurarSessao() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString(StorageKeys.token);
    if (token == null) return;

    final expiraEmStr = prefs.getString('auth_expira_em');
    if (expiraEmStr == null) return;

    final expiraEm = DateTime.tryParse(expiraEmStr);
    if (expiraEm == null || DateTime.now().isAfter(expiraEm)) {
      await _limparPrefs(prefs);
      return;
    }

    _usuario = UsuarioAutenticado(
      id: prefs.getString(StorageKeys.userId) ?? _extrairUserIdDoToken(token),
      nome: prefs.getString(StorageKeys.userName) ?? '',
      perfil: prefs.getString(StorageKeys.perfil) ?? '',
      torneioId: prefs.getString(StorageKeys.torneioId),
      slug: prefs.getString(StorageKeys.slug),
      token: token,
      expiraEm: expiraEm,
    );
    notifyListeners();
  }

  Future<void> loginTorneio(
    String slug,
    String usuario,
    String senha, {
    String? perfil,
  }) async {
    await _login(
      url: ApiConstants.login(slug),
      usuario: usuario,
      senha: senha,
      slug: slug,
      perfil: perfil,
    );
  }

  Future<void> loginFiscal(String slug, String usuario, String senha) =>
      loginTorneio(slug, usuario, senha, perfil: 'Fiscal');

  Future<void> _login({
    required String url,
    required String usuario,
    required String senha,
    String? slug,
    String? perfil,
  }) async {
    _carregando = true;
    _erro = null;
    _usuario = null;
    notifyListeners();

    try {
      final data = await _api.post(url, {
        'usuario': usuario,
        'senha': senha,
        if (slug != null) 'slug': slug,
        if (perfil != null) 'perfil': perfil,
      });

      final token = data['token'] as String;
      final expiraEm = DateTime.parse(data['expiraEm'] as String);

      _usuario = UsuarioAutenticado(
        id: _extrairUserIdDoToken(token),
        nome: data['nome'] as String,
        perfil: data['perfil'] as String,
        torneioId: data['torneioId'] as String?,
        slug: data['slug'] as String?,
        token: token,
        expiraEm: expiraEm,
      );

      await _salvarPrefs(_usuario!);
    } on ApiException catch (e) {
      _usuario = null;
      _erro = e.message;
    } catch (e) {
      _usuario = null;
      _erro = 'Erro de conexao. Verifique sua internet.';
    } finally {
      _carregando = false;
      notifyListeners();
    }
  }

  Future<void> logout() async {
    _usuario = null;
    final prefs = await SharedPreferences.getInstance();
    await _limparPrefs(prefs);
    notifyListeners();
  }

  Future<void> _salvarPrefs(UsuarioAutenticado u) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(StorageKeys.token, u.token);
    await prefs.setString(StorageKeys.userId, u.id);
    await prefs.setString(StorageKeys.perfil, u.perfil);
    await prefs.setString('auth_expira_em', u.expiraEm.toIso8601String());
    await prefs.setString(StorageKeys.userName, u.nome);
    if (u.torneioId != null) await prefs.setString(StorageKeys.torneioId, u.torneioId!);
    if (u.slug != null) await prefs.setString(StorageKeys.slug, u.slug!);
  }

  Future<void> _limparPrefs(SharedPreferences prefs) async {
    await prefs.remove(StorageKeys.token);
    await prefs.remove(StorageKeys.perfil);
    await prefs.remove('auth_expira_em');
    await prefs.remove(StorageKeys.userId);
    await prefs.remove(StorageKeys.userName);
    await prefs.remove(StorageKeys.torneioId);
    await prefs.remove(StorageKeys.slug);
  }

  String _extrairUserIdDoToken(String token) {
    try {
      final parts = token.split('.');
      if (parts.length < 2) return '';
      final normalized = base64Url.normalize(parts[1]);
      final payload = utf8.decode(base64Url.decode(normalized));
      final data = json.decode(payload) as Map<String, dynamic>;
      return data['sub'] as String? ?? '';
    } catch (_) {
      return '';
    }
  }
}
