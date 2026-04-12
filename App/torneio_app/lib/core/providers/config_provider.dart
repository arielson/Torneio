import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../constants.dart';
import '../models/torneio_config.dart';
import '../models/ano_torneio.dart';
import '../services/api_service.dart';

class ConfigProvider extends ChangeNotifier {
  final ApiService _api;

  TorneioConfig? _config;
  List<AnoTorneio> _anos = [];
  bool _carregando = false;
  String? _erro;

  ConfigProvider(this._api);

  TorneioConfig? get config => _config;
  List<AnoTorneio> get anos => _anos;
  bool get carregando => _carregando;
  String? get erro => _erro;

  Future<void> carregarConfig(String slug) async {
    _carregando = true;
    _erro = null;
    notifyListeners();

    try {
      final data = await _api.get(ApiConstants.config(slug));
      _config = TorneioConfig.fromJson(data as Map<String, dynamic>);

      // Salva slug para próxima abertura
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

  Future<void> carregarAnos(String slug, String token) async {
    try {
      final data = await _api.get(ApiConstants.anos(slug), token: token);
      if (data is List) {
        _anos = data
            .map((e) => AnoTorneio.fromJson(e as Map<String, dynamic>))
            .toList()
          ..sort((a, b) => b.ano.compareTo(a.ano));
        notifyListeners();
      }
    } catch (_) {}
  }

  void limpar() {
    _config = null;
    _anos = [];
    _erro = null;
    notifyListeners();
  }
}
