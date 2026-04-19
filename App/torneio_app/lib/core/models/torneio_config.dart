import '../flavor_config.dart';

class TorneioConfig {
  final String id;
  final String slug;
  final String nomeTorneio;
  final String? logoUrl;
  final bool ativo;
  final String status;
  final String labelEquipe;
  final String labelEquipePlural;
  final String labelMembro;
  final String labelMembroPlural;
  final String labelSupervisor;
  final String labelSupervisorPlural;
  final String labelItem;
  final String labelItemPlural;
  final String labelCaptura;
  final String labelCapturaPlural;
  final bool usarFatorMultiplicador;
  final String medidaCaptura;
  final bool permitirCapturaOffline;
  final String modoSorteio;
  final bool permitirEscolhaManual;
  final int qtdGanhadores;
  final bool premiacaoPorEquipe;
  final bool premiacaoPorMembro;
  final String? corPrimaria;

  const TorneioConfig({
    required this.id,
    required this.slug,
    required this.nomeTorneio,
    this.logoUrl,
    required this.ativo,
    this.status = 'Aberto',
    required this.labelEquipe,
    required this.labelEquipePlural,
    required this.labelMembro,
    required this.labelMembroPlural,
    required this.labelSupervisor,
    required this.labelSupervisorPlural,
    required this.labelItem,
    required this.labelItemPlural,
    required this.labelCaptura,
    required this.labelCapturaPlural,
    required this.usarFatorMultiplicador,
    required this.medidaCaptura,
    required this.permitirCapturaOffline,
    required this.modoSorteio,
    required this.permitirEscolhaManual,
    required this.qtdGanhadores,
    required this.premiacaoPorEquipe,
    required this.premiacaoPorMembro,
    this.corPrimaria,
  });

  factory TorneioConfig.fromJson(Map<String, dynamic> json) => TorneioConfig(
        id: json['id'] as String,
        slug: json['slug'] as String,
        nomeTorneio: json['nomeTorneio'] as String,
        logoUrl: AppConfig.resolverUrl(json['logoUrl'] as String?),
        ativo: json['ativo'] as bool? ?? true,
        status: json['status'] as String? ?? 'Aberto',
        labelEquipe: json['labelEquipe'] as String? ?? 'Equipe',
        labelEquipePlural: json['labelEquipePlural'] as String? ?? 'Equipes',
        labelMembro: json['labelMembro'] as String? ?? 'Membro',
        labelMembroPlural: json['labelMembroPlural'] as String? ?? 'Membros',
        labelSupervisor: json['labelSupervisor'] as String? ?? 'Fiscal',
        labelSupervisorPlural: json['labelSupervisorPlural'] as String? ?? 'Fiscais',
        labelItem: json['labelItem'] as String? ?? 'Item',
        labelItemPlural: json['labelItemPlural'] as String? ?? 'Itens',
        labelCaptura: json['labelCaptura'] as String? ?? 'Captura',
        labelCapturaPlural: json['labelCapturaPlural'] as String? ?? 'Capturas',
        usarFatorMultiplicador: json['usarFatorMultiplicador'] as bool? ?? false,
        medidaCaptura: json['medidaCaptura'] as String? ?? 'cm',
        permitirCapturaOffline: json['permitirCapturaOffline'] as bool? ?? true,
        modoSorteio: json['modoSorteio'] as String? ?? 'Sorteio',
        permitirEscolhaManual: json['permitirEscolhaManual'] as bool? ?? false,
        qtdGanhadores: json['qtdGanhadores'] as int? ?? 3,
        premiacaoPorEquipe: json['premiacaoPorEquipe'] as bool? ?? true,
        premiacaoPorMembro: json['premiacaoPorMembro'] as bool? ?? false,
        corPrimaria: json['corPrimaria'] as String?,
      );
}
