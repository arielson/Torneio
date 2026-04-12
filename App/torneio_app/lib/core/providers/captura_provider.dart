import 'package:flutter/foundation.dart';
import 'package:connectivity_plus/connectivity_plus.dart';
import '../constants.dart';
import '../models/captura.dart';
import '../models/equipe.dart';
import '../models/membro.dart';
import '../models/item.dart';
import '../services/api_service.dart';
import '../services/local_db.dart';
import '../services/sync_service.dart';

class CapturaProvider extends ChangeNotifier {
  final ApiService _api;
  final SyncService _sync;

  List<Captura> _capturas = [];
  List<Equipe> _equipes = [];
  List<Membro> _membros = [];
  List<Item> _itens = [];
  int _pendentesSync = 0;
  bool _carregando = false;
  bool _sincronizando = false;
  String? _erro;
  String? _mensagemSync;

  CapturaProvider(this._api) : _sync = SyncService(_api);

  List<Captura> get capturas => _capturas;
  List<Equipe> get equipes => _equipes;
  List<Membro> get membros => _membros;
  List<Item> get itens => _itens;
  int get pendentesSync => _pendentesSync;
  bool get carregando => _carregando;
  bool get sincronizando => _sincronizando;
  String? get erro => _erro;
  String? get mensagemSync => _mensagemSync;

  Future<void> carregarDadosEquipe(
    String slug,
    String token,
    String anoTorneioId,
    String equipeId,
  ) async {
    _carregando = true;
    _erro = null;
    notifyListeners();

    try {
      await Future.wait([
        _carregarEquipes(slug, token, anoTorneioId),
        _carregarMembros(slug, token, anoTorneioId),
        _carregarItens(slug, token),
        _carregarCapturas(slug, token, anoTorneioId, equipeId),
        _atualizarContadorPendentes(),
      ]);
    } catch (e) {
      _erro = 'Erro ao carregar dados.';
    } finally {
      _carregando = false;
      notifyListeners();
    }
  }

  Future<void> _carregarEquipes(String slug, String token, String anoTorneioId) async {
    final data = await _api.get(
      '${ApiConstants.equipes(slug)}?anoTorneioId=$anoTorneioId',
      token: token,
    );
    if (data is List) {
      _equipes = data.map((e) => Equipe.fromJson(e as Map<String, dynamic>)).toList();
    }
  }

  Future<void> _carregarMembros(String slug, String token, String anoTorneioId) async {
    final data = await _api.get(
      '${ApiConstants.membros(slug)}?anoTorneioId=$anoTorneioId',
      token: token,
    );
    if (data is List) {
      _membros = data.map((e) => Membro.fromJson(e as Map<String, dynamic>)).toList();
    }
  }

  Future<void> _carregarItens(String slug, String token) async {
    final data = await _api.get(ApiConstants.itens(slug), token: token);
    if (data is List) {
      _itens = data.map((e) => Item.fromJson(e as Map<String, dynamic>)).toList();
    }
  }

  Future<void> _carregarCapturas(
    String slug,
    String token,
    String anoTorneioId,
    String equipeId,
  ) async {
    final data = await _api.get(
      '${ApiConstants.capturas(slug)}?anoTorneioId=$anoTorneioId&equipeId=$equipeId',
      token: token,
    );
    if (data is List) {
      _capturas = data.map((e) => Captura.fromJson(e as Map<String, dynamic>)).toList()
        ..sort((a, b) => b.dataHora.compareTo(a.dataHora));
    }
  }

  Future<void> _atualizarContadorPendentes() async {
    _pendentesSync = await LocalDb.contarPendentes();
  }

  /// Registra captura — online direto na API, offline salva no SQLite
  Future<bool> registrarCaptura({
    required String slug,
    required String token,
    required RegistrarCapturaRequest req,
  }) async {
    final connectivity = await Connectivity().checkConnectivity();
    final online = !connectivity.contains(ConnectivityResult.none);

    if (online) {
      try {
        final data = await _api.post(
          ApiConstants.capturas(slug),
          req.toJson(),
          token: token,
        );
        if (data != null) {
          _capturas.insert(0, Captura.fromJson(data as Map<String, dynamic>));
          notifyListeners();
          return true;
        }
      } on ApiException {
        // Cai para offline se a API falhar
      }
    }

    // Modo offline
    await LocalDb.salvarCapturaPendente(req.copyWith(pendenteSync: true));
    _pendentesSync++;
    notifyListeners();
    return true;
  }

  /// Sincroniza capturas pendentes com o servidor
  Future<void> sincronizar(String slug, String token) async {
    if (_sincronizando) return;
    _sincronizando = true;
    _mensagemSync = null;
    _erro = null;
    notifyListeners();

    try {
      final total = await _sync.sincronizar(slug, token);
      _pendentesSync = 0;
      _mensagemSync = total > 0
          ? '$total captura(s) sincronizada(s) com sucesso!'
          : 'Nenhuma captura pendente.';
    } on ApiException catch (e) {
      _erro = 'Erro ao sincronizar: ${e.message}';
    } catch (e) {
      _erro = 'Sem conexão. Tente novamente mais tarde.';
    } finally {
      _sincronizando = false;
      notifyListeners();
    }
  }

  void limpar() {
    _capturas = [];
    _equipes = [];
    _membros = [];
    _itens = [];
    _pendentesSync = 0;
    _erro = null;
    _mensagemSync = null;
    notifyListeners();
  }
}

extension on RegistrarCapturaRequest {
  RegistrarCapturaRequest copyWith({bool? pendenteSync}) => RegistrarCapturaRequest(
        torneioId: torneioId,
        anoTorneioId: anoTorneioId,
        itemId: itemId,
        membroId: membroId,
        equipeId: equipeId,
        tamanhoMedida: tamanhoMedida,
        fotoUrl: fotoUrl,
        dataHora: dataHora,
        pendenteSync: pendenteSync ?? this.pendenteSync,
      );
}
