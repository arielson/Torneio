import '../flavor_config.dart';

class TorneioConfig {
  final String id;
  final String slug;
  final String nomeTorneio;
  final String? logoUrl;
  final bool ativo;
  final String status;
  final String labelEquipe;
  final String labelMembro;
  final String labelSupervisor;
  final String labelItem;
  final String labelCaptura;
  final bool usarFatorMultiplicador;
  final String medidaCaptura;
  final bool permitirCapturaOffline;
  final String modoSorteio;
  final bool permitirEscolhaManual;
  final int qtdGanhadores;

  const TorneioConfig({
    required this.id,
    required this.slug,
    required this.nomeTorneio,
    this.logoUrl,
    required this.ativo,
    this.status = 'Aberto',
    required this.labelEquipe,
    required this.labelMembro,
    required this.labelSupervisor,
    required this.labelItem,
    required this.labelCaptura,
    required this.usarFatorMultiplicador,
    required this.medidaCaptura,
    required this.permitirCapturaOffline,
    required this.modoSorteio,
    required this.permitirEscolhaManual,
    required this.qtdGanhadores,
  });

  factory TorneioConfig.fromJson(Map<String, dynamic> json) => TorneioConfig(
        id: json['id'] as String,
        slug: json['slug'] as String,
        nomeTorneio: json['nomeTorneio'] as String,
        logoUrl: AppConfig.resolverUrl(json['logoUrl'] as String?),
        ativo: json['ativo'] as bool? ?? true,
        status: json['status'] as String? ?? 'Aberto',
        labelEquipe: json['labelEquipe'] as String? ?? 'Equipe',
        labelMembro: json['labelMembro'] as String? ?? 'Membro',
        labelSupervisor: json['labelSupervisor'] as String? ?? 'Fiscal',
        labelItem: json['labelItem'] as String? ?? 'Item',
        labelCaptura: json['labelCaptura'] as String? ?? 'Captura',
        usarFatorMultiplicador: json['usarFatorMultiplicador'] as bool? ?? false,
        medidaCaptura: json['medidaCaptura'] as String? ?? 'cm',
        permitirCapturaOffline: json['permitirCapturaOffline'] as bool? ?? true,
        modoSorteio: json['modoSorteio'] as String? ?? 'Sorteio',
        permitirEscolhaManual: json['permitirEscolhaManual'] as bool? ?? false,
        qtdGanhadores: json['qtdGanhadores'] as int? ?? 3,
      );
}
