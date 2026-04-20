import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_cache_manager/flutter_cache_manager.dart';
import '../constants.dart';
import '../models/captura.dart';
import '../models/equipe.dart';
import '../models/item.dart';
import '../models/membro.dart';
import '../services/api_service.dart';
import '../services/local_db.dart';
import '../services/sync_service.dart';

class CapturaProvider extends ChangeNotifier {
  static const _cacheEquipes = 'equipes';
  static const _cacheMembros = 'membros';
  static const _cacheItens = 'itens';

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
    String equipeId,
  ) async {
    _carregando = true;
    _erro = null;
    notifyListeners();

    try {
      final resultados = await Future.wait<dynamic>([
        _carregarEquipes(slug, token),
        _carregarMembros(slug, token),
        _carregarItens(slug, token),
        _carregarCapturas(slug, token, equipeId),
        _atualizarContadorPendentes(),
      ]);

      final colecoesCarregadas = resultados
          .take(4)
          .whereType<bool>()
          .any((carregado) => carregado);
      if (!colecoesCarregadas) {
        _erro = 'Erro ao carregar dados.';
      }

      await _sincronizarAutomaticasSePossivel(slug, token);
    } catch (e) {
      _erro = 'Erro ao carregar dados.';
    } finally {
      _carregando = false;
      notifyListeners();
    }
  }

  Future<bool> _carregarEquipes(String slug, String token) async {
    try {
      final data = await _api.get(ApiConstants.equipes(slug), token: token);
      if (data is List) {
        final rows =
            data.map((e) => Map<String, dynamic>.from(e as Map)).toList();
        _equipes = rows.map(Equipe.fromJson).toList();
        await LocalDb.salvarCacheLista(slug, _cacheEquipes, rows);
        await _precarregarImagens([
          for (final equipe in _equipes) equipe.fotoUrl,
          for (final equipe in _equipes) equipe.fotoCapitaoUrl,
        ]);
        return true;
      }
    } catch (_) {
      // Usa cache local.
    }

    final cache = await LocalDb.carregarCacheLista(slug, _cacheEquipes);
    if (cache == null) return false;
    _equipes = cache.map(Equipe.fromJson).toList();
    return true;
  }

  Future<bool> _carregarMembros(String slug, String token) async {
    try {
      final data = await _api.get(ApiConstants.membros(slug), token: token);
      if (data is List) {
        final rows =
            data.map((e) => Map<String, dynamic>.from(e as Map)).toList();
        _membros = rows.map(Membro.fromJson).toList();
        await LocalDb.salvarCacheLista(slug, _cacheMembros, rows);
        await _precarregarImagens(_membros.map((m) => m.fotoUrl));
        return true;
      }
    } catch (_) {
      // Usa cache local.
    }

    final cache = await LocalDb.carregarCacheLista(slug, _cacheMembros);
    if (cache == null) return false;
    _membros = cache.map(Membro.fromJson).toList();
    return true;
  }

  Future<bool> _carregarItens(String slug, String token) async {
    try {
      final data = await _api.get(ApiConstants.itens(slug), token: token);
      if (data is List) {
        final rows =
            data.map((e) => Map<String, dynamic>.from(e as Map)).toList();
        _itens = rows.map(Item.fromJson).toList();
        await LocalDb.salvarCacheLista(slug, _cacheItens, rows);
        await _precarregarImagens(_itens.map((i) => i.fotoUrl));
        return true;
      }
    } catch (_) {
      // Usa cache local.
    }

    final cache = await LocalDb.carregarCacheLista(slug, _cacheItens);
    if (cache == null) return false;
    _itens = cache.map(Item.fromJson).toList();
    return true;
  }

  Future<bool> _carregarCapturas(
    String slug,
    String token,
    String equipeId,
  ) async {
    try {
      final url =
          equipeId.trim().isEmpty
              ? ApiConstants.capturas(slug)
              : '${ApiConstants.capturas(slug)}?equipeId=$equipeId';
      final data = await _api.get(url, token: token);
      if (data is List) {
        _capturas =
            data
                .map((e) => Captura.fromJson(e as Map<String, dynamic>))
                .toList()
              ..sort((a, b) => b.dataHora.compareTo(a.dataHora));
        return true;
      }
    } catch (_) {
      // Capturas podem falhar sem bloquear o cadastro offline.
    }

    return _capturas.isNotEmpty;
  }

  Future<void> _atualizarContadorPendentes() async {
    _pendentesSync = await LocalDb.contarPendentes();
  }

  Future<void> _sincronizarAutomaticasSePossivel(
    String slug,
    String token,
  ) async {
    if (_sincronizando) return;

    final connectivity = await Connectivity().checkConnectivity();
    final online = !connectivity.contains(ConnectivityResult.none);
    if (!online) return;

    final pendentesAutomaticas = await LocalDb.contarPendentes(
      sincronizacaoManual: false,
    );
    if (pendentesAutomaticas == 0) return;

    try {
      await _sync.sincronizar(slug, token, sincronizacaoManual: false);
      await _atualizarContadorPendentes();
    } catch (_) {
      // Sincronizacao automatica e silenciosa.
    }
  }

  Future<bool> registrarCaptura({
    required String slug,
    required String token,
    required RegistrarCapturaRequest req,
    bool forcarOffline = false,
  }) async {
    if (!forcarOffline) {
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
          // Cai para sincronizacao automatica posterior se a API falhar agora.
        }
      }
    }

    await LocalDb.salvarCapturaPendente(
      req.copyWith(pendenteSync: true),
      sincronizacaoManual: forcarOffline,
    );
    _pendentesSync++;
    notifyListeners();
    return true;
  }

  Future<void> sincronizar(String slug, String token) async {
    if (_sincronizando) return;
    _sincronizando = true;
    _mensagemSync = null;
    _erro = null;
    notifyListeners();

    try {
      final total = await _sync.sincronizar(slug, token);
      await _atualizarContadorPendentes();
      _mensagemSync =
          total > 0
              ? '$total captura(s) sincronizada(s) com sucesso!'
              : 'Nenhuma captura pendente.';
    } on ApiException catch (e) {
      _erro = 'Erro ao sincronizar: ${e.message}';
    } catch (e) {
      _erro = 'Sem conexao. Tente novamente mais tarde.';
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

extension on CapturaProvider {
  Future<void> _precarregarImagens(Iterable<String?> urls) async {
    final imagens =
        urls
            .whereType<String>()
            .where((url) => url.trim().isNotEmpty && url.startsWith('http'))
            .toSet();

    for (final url in imagens) {
      try {
        await DefaultCacheManager().downloadFile(url);
      } catch (_) {
        // Cache de imagem e oportunistico.
      }
    }
  }
}

extension on RegistrarCapturaRequest {
  RegistrarCapturaRequest copyWith({bool? pendenteSync}) =>
      RegistrarCapturaRequest(
        torneioId: torneioId,
        itemId: itemId,
        membroId: membroId,
        equipeId: equipeId,
        tamanhoMedida: tamanhoMedida,
        fotoUrl: fotoUrl,
        dataHora: dataHora,
        pendenteSync: pendenteSync ?? this.pendenteSync,
        fonteFoto: fonteFoto,
      );
}
