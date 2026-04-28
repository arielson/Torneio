import 'dart:io';
import '../constants.dart';
import 'api_service.dart';
import 'local_db.dart';

class SyncService {
  final ApiService _api;

  SyncService(this._api);

  Future<int> sincronizar(
    String slug,
    String token, {
    bool? sincronizacaoManual,
  }) async {
    final pendentes = await LocalDb.listarPendentes(
      sincronizacaoManual: sincronizacaoManual,
    );
    if (pendentes.isEmpty) return 0;

    var sincronizadas = 0;
    for (final pendente in pendentes) {
      try {
        final fotoExiste = await File(pendente.request.fotoUrl).exists();
        if (fotoExiste) {
          await _api.postMultipart(
            ApiConstants.capturas(slug),
            fields: {
              'torneioId': pendente.request.torneioId,
              'itemId': pendente.request.itemId,
              'membroId': pendente.request.membroId,
              'equipeId': pendente.request.equipeId,
              'tamanhoMedida': pendente.request.tamanhoMedida,
              'dataHora': pendente.request.dataHora.toIso8601String(),
              'pendenteSync': false,
              if (pendente.request.fonteFoto != null)
                'fonteFoto': pendente.request.fonteFoto,
            },
            files: {'foto': pendente.request.fotoUrl},
            token: token,
          );
        } else {
          await _api.post(
            ApiConstants.capturas(slug),
            pendente.request.toJson(),
            token: token,
          );
        }

        await LocalDb.remover(pendente.id);
        sincronizadas++;
      } catch (_) {
        // Mantem a captura pendente para nova tentativa posterior.
      }
    }

    return sincronizadas;
  }
}
