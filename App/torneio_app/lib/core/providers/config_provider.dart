import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../constants.dart';
import '../models/torneio_config.dart';
import '../services/api_service.dart';

class ConfigProvider extends ChangeNotifier {
  final ApiService _api;

  TorneioConfig? _config;
  bool _carregando = false;
  String? _erro;

  ConfigProvider(this._api);

  TorneioConfig? get config => _config;
  bool get carregando => _carregando;
  String? get erro => _erro;

  Future<void> carregarConfig(String slug) async {
    _carregando = true;
    _erro = null;
    notifyListeners();

    final prefs = await SharedPreferences.getInstance();
    final cacheKey = '${StorageKeys.torneioConfigPrefix}$slug';

    try {
      final data = await _api.get(ApiConstants.config(slug));
      final map = Map<String, dynamic>.from(data as Map);
      _config = TorneioConfig.fromJson(map);
      await prefs.setString(cacheKey, jsonEncode(map));
      await prefs.setString(StorageKeys.ultimoSlug, slug);
    } on ApiException catch (e) {
      if (!await _restaurarCache(prefs, cacheKey)) {
        _erro = e.message;
      }
    } catch (_) {
      if (!await _restaurarCache(prefs, cacheKey)) {
        _erro = 'Nao foi possivel carregar a configuracao do torneio.';
      }
    } finally {
      _carregando = false;
      notifyListeners();
    }
  }

  Future<bool> _restaurarCache(SharedPreferences prefs, String cacheKey) async {
    final cache = prefs.getString(cacheKey);
    if (cache == null || cache.isEmpty) return false;

    final data = jsonDecode(cache);
    if (data is! Map) return false;

    _config = TorneioConfig.fromJson(Map<String, dynamic>.from(data));
    return true;
  }

  void limpar() {
    _config = null;
    _erro = null;
    notifyListeners();
  }
}
