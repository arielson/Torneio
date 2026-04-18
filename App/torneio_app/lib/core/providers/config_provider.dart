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

    try {
      final data = await _api.get(ApiConstants.config(slug));
      _config = TorneioConfig.fromJson(data as Map<String, dynamic>);

      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(StorageKeys.ultimoSlug, slug);
    } on ApiException catch (e) {
      _erro = e.message;
    } catch (e) {
      _erro = 'Não foi possível carregar a configuração do torneio.';
    } finally {
      _carregando = false;
      notifyListeners();
    }
  }

  void limpar() {
    _config = null;
    _erro = null;
    notifyListeners();
  }
}
