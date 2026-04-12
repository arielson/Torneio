import 'api_service.dart';
import 'local_db.dart';
import '../constants.dart';

class SyncService {
  final ApiService _api;

  SyncService(this._api);

  /// Sincroniza todas as capturas pendentes com o servidor.
  /// Retorna o número de capturas sincronizadas.
  Future<int> sincronizar(String slug, String token) async {
    final pendentes = await LocalDb.listarPendentes();
    if (pendentes.isEmpty) return 0;

    final payload = pendentes.map((c) => c.toJson()).toList();
    final result = await _api.post(
      ApiConstants.sync(slug),
      payload,
      token: token,
    );
    final sincronizadas = (result?['sincronizadas'] as int?) ?? 0;
    await LocalDb.removerSincronizadas();
    return sincronizadas;
  }
}
