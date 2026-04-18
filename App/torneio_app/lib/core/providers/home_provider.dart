import 'package:flutter/material.dart';
import '../models/torneio_resumo.dart';
import '../models/banner_app.dart';
import '../services/api_service.dart';
import '../constants.dart';

class HomeProvider extends ChangeNotifier {
  final ApiService _api;

  List<TorneioResumo> torneiosRecentes = [];
  List<TorneioResumo> resultadosBusca = [];
  List<BannerApp> banners = [];
  bool carregando = false;
  bool buscando = false;
  String? erro;
  bool _buscaAtiva = false;

  bool get buscaAtiva => _buscaAtiva;
  List<TorneioResumo> get torneiosExibidos => _buscaAtiva ? resultadosBusca : torneiosRecentes;

  HomeProvider(this._api);

  Future<void> carregarHome() async {
    carregando = true;
    erro = null;
    notifyListeners();
    try {
      final resRecentes = await _api.get(ApiConstants.torneiosRecentes());
      final resBanners = await _api.get(ApiConstants.banners());
      torneiosRecentes = (resRecentes as List)
          .map((j) => TorneioResumo.fromJson(j as Map<String, dynamic>))
          .toList();
      banners = (resBanners as List)
          .map((j) => BannerApp.fromJson(j as Map<String, dynamic>))
          .toList();
    } catch (e) {
      erro = e.toString();
    } finally {
      carregando = false;
      notifyListeners();
    }
  }

  Future<void> buscar(String q) async {
    if (q.trim().isEmpty) {
      _buscaAtiva = false;
      resultadosBusca = [];
      notifyListeners();
      return;
    }
    _buscaAtiva = true;
    buscando = true;
    notifyListeners();
    try {
      final res = await _api.get(ApiConstants.torneiosBuscar(q));
      resultadosBusca = (res as List)
          .map((j) => TorneioResumo.fromJson(j as Map<String, dynamic>))
          .toList();
    } catch (_) {
      resultadosBusca = [];
    } finally {
      buscando = false;
      notifyListeners();
    }
  }

  void limparBusca() {
    _buscaAtiva = false;
    resultadosBusca = [];
    notifyListeners();
  }
}
