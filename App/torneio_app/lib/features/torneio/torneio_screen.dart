import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/flavor_config.dart';
import '../../core/models/membro.dart';
import '../../core/models/patrocinador.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import '../../widgets/patrocinadores_section.dart';

class TorneioScreen extends StatefulWidget {
  const TorneioScreen({super.key});

  @override
  State<TorneioScreen> createState() => _TorneioScreenState();
}

class _TorneioScreenState extends State<TorneioScreen> {
  String? _slug;
  bool _inicializado = false;
  late ConfigProvider _configProvider;
  final ApiService _api = ApiService();
  List<Patrocinador> _patrocinadores = const [];
  List<Membro> _participantes = const [];

  // Ranking state
  bool _rankingCarregando = false;
  Map<String, dynamic>? _rankingData;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (!_inicializado) {
      _configProvider = context.read<ConfigProvider>();
      _slug = ModalRoute.of(context)?.settings.arguments as String?;
      if (_slug != null) {
        WidgetsBinding.instance.addPostFrameCallback((_) async {
          await _configProvider.carregarConfig(_slug!);
          _carregarPatrocinadores(_slug!);
          if (_configProvider.config?.exibirParticipantesPublicos ?? false) {
            _carregarParticipantes(_slug!);
          }
          final cfg = _configProvider.config;
          if (cfg != null && (cfg.status == 'Liberado' || cfg.status == 'Finalizado')) {
            _carregarRanking(_slug!);
          }
        });
      }
      _inicializado = true;
    }
  }

  Future<void> _carregarPatrocinadores(String slug) async {
    try {
      final data = await _api.get(ApiConstants.patrocinadores(slug));
      final lista = data is List
          ? data
              .map((e) => Patrocinador.fromJson(e as Map<String, dynamic>))
              .where((p) => p.exibirNaTelaInicial)
              .toList()
          : <Patrocinador>[];
      lista.sort((a, b) => a.nome.compareTo(b.nome));
      if (!mounted) return;
      setState(() => _patrocinadores = lista);
    } catch (_) {
      if (!mounted) return;
      setState(() => _patrocinadores = const []);
    }
  }

  Future<void> _carregarParticipantes(String slug) async {
    try {
      final data = await _api.get(ApiConstants.participantes(slug));
      final lista = data is List
          ? data
              .map((e) => Membro.fromJson(e as Map<String, dynamic>))
              .toList()
          : <Membro>[];
      lista.sort((a, b) => a.nome.compareTo(b.nome));
      if (!mounted) return;
      setState(() => _participantes = lista);
    } catch (_) {
      if (!mounted) return;
      setState(() => _participantes = const []);
    }
  }

  Future<void> _carregarRanking(String slug) async {
    if (!mounted) return;
    setState(() => _rankingCarregando = true);
    try {
      final data = await _api.get(ApiConstants.rankingPublico(slug));
      if (!mounted) return;
      setState(() {
        _rankingData = data as Map<String, dynamic>?;
        _rankingCarregando = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() => _rankingCarregando = false);
    }
  }

  @override
  void dispose() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _configProvider.limpar();
    });
    super.dispose();
  }

  /// Retorna os botões extras da barra fixa inferior.
  /// Vazio quando [permitirRegistroPublicoMembro] é false — sem barra.
  List<Widget> _buildBotoesExtras(BuildContext context, dynamic config) {
    if (!config.permitirRegistroPublicoMembro) return [];
    return [
      OutlinedButton.icon(
        icon: const Icon(Icons.badge_outlined),
        label: Text('Entrar como ${config.labelMembro.toLowerCase()}'),
        onPressed: () => Navigator.pushNamed(context, '/login', arguments: 'Membro'),
      ),
      const SizedBox(height: 8),
      OutlinedButton.icon(
        icon: const Icon(Icons.person_add_alt_1_outlined),
        label: Text('Registrar ${config.labelMembro.toLowerCase()}'),
        onPressed: () => Navigator.pushNamed(context, '/registro-pescador'),
      ),
      const SizedBox(height: 8),
      OutlinedButton.icon(
        icon: const Icon(Icons.lock_reset_outlined),
        label: Text('Recuperar senha do ${config.labelMembro.toLowerCase()}'),
        onPressed: () => Navigator.pushNamed(context, '/recuperar-senha-pescador'),
      ),
    ];
  }

  Color _corStatus(String status) => switch (status) {
        'Liberado' => Colors.green,
        'Finalizado' => Colors.grey,
        _ => Colors.orange,
      };

  @override
  Widget build(BuildContext context) {
    final configProv = context.watch<ConfigProvider>();
    final config = configProv.config;

    // Show spinner while loading OR while config hasn't been attempted yet
    if (configProv.carregando || (config == null && configProv.erro == null)) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (config == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Torneio')),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.error_outline, size: 64, color: Colors.grey),
              const SizedBox(height: 16),
              Text(configProv.erro ?? 'Torneio não encontrado.'),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Voltar'),
              ),
            ],
          ),
        ),
      );
    }

    final cor = _corStatus(config.status);
    final exibirRanking = config.status == 'Liberado' || config.status == 'Finalizado';
    final rankingMembros =
        (_rankingData?['membrosGanhadores'] as List?)?.cast<Map<String, dynamic>>() ?? const <Map<String, dynamic>>[];
    final posicoesParticipantes = <String, int>{
      for (final membro in rankingMembros)
        if ((membro['membroId'] as String?)?.isNotEmpty ?? false)
          membro['membroId'] as String: (membro['posicao'] as int?) ?? 0,
    };

    final botoesExtras = _buildBotoesExtras(context, config);
    final temBarra = botoesExtras.isNotEmpty;

    return Scaffold(
      appBar: AppBar(
        title: Text(config.nomeTorneio),
      ),
      body: Column(
        children: [
          // ── Conteúdo rolável ──────────────────────────────────────────────
          Expanded(
            child: SafeArea(
              top: false,
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    // Logo
                    if (config.logoUrl != null)
                      Center(
                        child: ClipRRect(
                          borderRadius: BorderRadius.circular(12),
                          child: Image.network(
                            config.logoUrl!, height: 120, fit: BoxFit.contain,
                            errorBuilder: (context, error, stackTrace) => const SizedBox.shrink(),
                          ),
                        ),
                      ),
                    const SizedBox(height: 16),

                    // Info card
                    Card(
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Row(
                          children: [
                            Expanded(
                              child: Text(
                                config.nomeTorneio,
                                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                              ),
                            ),
                            Chip(
                              label: Text(config.status, style: TextStyle(color: cor, fontSize: 12)),
                              backgroundColor: cor.withAlpha(30),
                              side: BorderSide(color: cor.withAlpha(80)),
                            ),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(height: 16),

                    // Status message
                    if (config.status == 'Aberto')
                      _StatusBanner(
                        icon: Icons.lock_clock,
                        message: 'Este torneio ainda não está aberto ao público.',
                        color: Colors.orange,
                      )
                    else if (config.status == 'Finalizado')
                      _StatusBanner(
                        icon: Icons.check_circle,
                        message: 'Este torneio foi encerrado.',
                        color: Colors.grey,
                      ),
                    const SizedBox(height: 16),

                    // Botão Fiscal/Administração (fixo no scroll)
                    FilledButton.icon(
                      icon: const Icon(Icons.login),
                      label: const Text('Fiscal/Administração'),
                      onPressed: () => Navigator.pushNamed(context, '/login'),
                    ),

                    // ── Ranking ──────────────────────────────────────────────
                    if (exibirRanking) ...[
                      const SizedBox(height: 16),
                      if (_rankingCarregando)
                        const Center(child: Padding(
                          padding: EdgeInsets.all(16),
                          child: CircularProgressIndicator(),
                        ))
                      else if (_rankingData != null && (_rankingData!['disponivel'] as bool? ?? false))
                        _RankingSection(
                          data: _rankingData!,
                          config: config,
                        ),
                    ],

                    // ── Patrocinadores ───────────────────────────────────────
                    if (config.exibirParticipantesPublicos && _participantes.isNotEmpty) ...[
                      const SizedBox(height: 24),
                      _ParticipantesSection(
                        participantes: _participantes,
                        labelMembroPlural: config.labelMembroPlural,
                        posicoesRanking: posicoesParticipantes,
                      ),
                    ],
                    if (_patrocinadores.isNotEmpty) ...[
                      const SizedBox(height: 24),
                      PatrocinadoresSection(patrocinadores: _patrocinadores),
                    ],
                  ],
                ),
              ),
            ),
          ),

          // ── Barra de botões fixa (só quando permitirRegistroPublicoMembro) ──
          if (temBarra)
            SafeArea(
              top: false,
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  mainAxisSize: MainAxisSize.min,
                  children: botoesExtras,
                ),
              ),
            ),
        ],
      ),
    );
  }
}

// ── Ranking section widget ────────────────────────────────────────────────────

class _RankingSection extends StatelessWidget {
  final Map<String, dynamic> data;
  final dynamic config;

  const _RankingSection({required this.data, required this.config});

  String _resolverFoto(String? url) => AppConfig.resolverUrl(url) ?? '';

  @override
  Widget build(BuildContext context) {
    final premiacaoPorEquipe = data['premiacaoPorEquipe'] as bool? ?? false;
    final premiacaoPorMembro = data['premiacaoPorMembro'] as bool? ?? false;
    final equipes = (data['equipesGanhadoras'] as List?)?.cast<Map<String, dynamic>>() ?? [];
    final membros = (data['membrosGanhadores'] as List?)?.cast<Map<String, dynamic>>() ?? [];
    final medida = data['medidaCaptura'] as String? ?? 'cm';
    final usarFator = data['usarFatorMultiplicador'] as bool? ?? false;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (premiacaoPorEquipe && equipes.isNotEmpty) ...[
          _SectionTitle(
            icon: Icons.emoji_events,
            color: Colors.amber.shade700,
            title: 'Ranking por ${config.labelEquipePlural}',
          ),
          const SizedBox(height: 8),
          Card(
            child: Column(
              children: equipes.asMap().entries.map((entry) {
                final r = entry.value;
                final pos = r['posicao'] as int? ?? (entry.key + 1);
                final fotoUrl = _resolverFoto(r['fotoUrl'] as String?);
                return ListTile(
                  leading: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      _Medalha(pos),
                      const SizedBox(width: 8),
                      _Avatar(fotoUrl: fotoUrl, icon: Icons.directions_boat),
                    ],
                  ),
                  title: Text(r['nomeEquipe'] as String? ?? '', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 18)),
                  subtitle: Text('${r['qtdCapturas']} ${config.labelCaptura.toLowerCase()}(s)', style: const TextStyle(fontSize: 15)),
                  trailing: Text(
                    '${(r['totalPontos'] as num?)?.toStringAsFixed(2) ?? '0'} pts',
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
                  ),
                );
              }).toList(),
            ),
          ),
          const SizedBox(height: 16),
        ],
        if (premiacaoPorMembro && membros.isNotEmpty) ...[
          _SectionTitle(
            icon: Icons.person_pin,
            color: Colors.blue.shade700,
            title: 'Ranking por ${config.labelMembroPlural}',
          ),
          const SizedBox(height: 8),
          ...membros.asMap().entries.map((entry) {
            final r = entry.value;
            final pos = r['posicao'] as int? ?? (entry.key + 1);
            final fotoUrl = _resolverFoto(r['fotoUrl'] as String?);
            final capturas = (r['capturas'] as List?)?.cast<Map<String, dynamic>>() ?? [];
            return _MembroTile(
              posicao: pos,
              nome: r['nomeMembro'] as String? ?? '',
              nomeEquipe: r['nomeEquipe'] as String? ?? '',
              totalPontos: (r['totalPontos'] as num?)?.toDouble() ?? 0,
              fotoUrl: fotoUrl,
              capturas: capturas,
              medida: medida,
              usarFator: usarFator,
              labelItem: config.labelItem as String,
            );
          }),
        ],
      ],
    );
  }
}

class _SectionTitle extends StatelessWidget {
  final IconData icon;
  final Color color;
  final String title;
  const _SectionTitle({required this.icon, required this.color, required this.title});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(icon, color: color, size: 22),
        const SizedBox(width: 8),
        Text(title, style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
      ],
    );
  }
}

class _Medalha extends StatelessWidget {
  final int posicao;
  const _Medalha(this.posicao);

  @override
  Widget build(BuildContext context) {
    final text = switch (posicao) { 1 => '🥇', 2 => '🥈', 3 => '🥉', _ => '$posicao°' };
    return Text(text, style: const TextStyle(fontSize: 32));
  }
}

class _Avatar extends StatelessWidget {
  final String fotoUrl;
  final IconData icon;
  const _Avatar({required this.fotoUrl, required this.icon});

  @override
  Widget build(BuildContext context) {
    const double size = 56;
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: Colors.grey.shade300,
      ),
      clipBehavior: Clip.antiAlias,
      child: fotoUrl.isNotEmpty
          ? Image.network(
              fotoUrl,
              width: size,
              height: size,
              fit: BoxFit.cover,
              errorBuilder: (ctx, err, st) =>
                  Icon(icon, size: 28, color: Colors.grey.shade600),
            )
          : Icon(icon, size: 28, color: Colors.grey.shade600),
    );
  }
}

class _MembroTile extends StatefulWidget {
  final int posicao;
  final String nome;
  final String nomeEquipe;
  final double totalPontos;
  final String fotoUrl;
  final List<Map<String, dynamic>> capturas;
  final String medida;
  final bool usarFator;
  final String labelItem;

  const _MembroTile({
    required this.posicao,
    required this.nome,
    required this.nomeEquipe,
    required this.totalPontos,
    required this.fotoUrl,
    required this.capturas,
    required this.medida,
    required this.usarFator,
    required this.labelItem,
  });

  @override
  State<_MembroTile> createState() => _MembroTileState();
}

class _MembroTileState extends State<_MembroTile> {
  bool _expandido = false;
  final _expandidoKey = GlobalKey();

  String _resolverFoto(String? url) => AppConfig.resolverUrl(url) ?? '';

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 6),
      child: Column(
        children: [
          ListTile(
            onTap: widget.capturas.isNotEmpty
                ? () {
                    setState(() => _expandido = !_expandido);
                    if (!_expandido) return;
                    WidgetsBinding.instance.addPostFrameCallback((_) {
                      final ctx = _expandidoKey.currentContext;
                      if (ctx != null) {
                        Scrollable.ensureVisible(
                          ctx,
                          duration: const Duration(milliseconds: 300),
                          curve: Curves.easeInOut,
                          alignmentPolicy: ScrollPositionAlignmentPolicy.keepVisibleAtEnd,
                        );
                      }
                    });
                  }
                : null,
            leading: _Medalha(widget.posicao),
            minLeadingWidth: 40,
            title: Row(
              children: [
                GestureDetector(
                  onTap: widget.fotoUrl.isNotEmpty
                      ? () => _abrirFoto(context, widget.fotoUrl)
                      : null,
                  child: _Avatar(fotoUrl: widget.fotoUrl, icon: Icons.person),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    widget.nome,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 18),
                  ),
                ),
              ],
            ),
            subtitle: Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (widget.nomeEquipe.isNotEmpty)
                    Text(widget.nomeEquipe, style: const TextStyle(fontSize: 15)),
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      const Spacer(),
                      Text(
                        '${widget.totalPontos.toStringAsFixed(2)} pts',
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
                      ),
                      if (widget.capturas.isNotEmpty) ...[
                        const SizedBox(width: 4),
                        Icon(_expandido ? Icons.expand_less : Icons.expand_more, size: 22),
                      ],
                    ],
                  ),
                ],
              ),
            ),
          ),
          if (_expandido && widget.capturas.isNotEmpty)
            Padding(
              key: _expandidoKey,
              padding: const EdgeInsets.fromLTRB(8, 0, 8, 10),
              child: Column(
                children: widget.capturas.map((c) {
                  final capFotoUrl = _resolverFoto(c['fotoUrl'] as String?);
                  final fator = (c['fatorMultiplicador'] as num? ?? 1).toDouble();
                  return Padding(
                    padding: const EdgeInsets.symmetric(vertical: 4),
                    child: Row(
                      children: [
                        // Foto da captura
                        GestureDetector(
                          onTap: capFotoUrl.isNotEmpty
                              ? () => _abrirFoto(context, capFotoUrl)
                              : null,
                          child: Container(
                            width: 80,
                            height: 80,
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(8),
                              color: Colors.grey.shade200,
                            ),
                            child: capFotoUrl.isNotEmpty
                                ? ClipRRect(
                                    borderRadius: BorderRadius.circular(8),
                                    child: Image.network(
                                      capFotoUrl,
                                      fit: BoxFit.cover,
                                      errorBuilder: (ctx, e, st) => Icon(
                                        Icons.image_not_supported_outlined,
                                        color: Colors.grey.shade400,
                                        size: 32,
                                      ),
                                    ),
                                  )
                                : Icon(Icons.photo_camera_outlined,
                                    color: Colors.grey.shade400, size: 32),
                          ),
                        ),
                        const SizedBox(width: 12),
                        // Detalhes
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                c['nomeItem'] as String? ?? '',
                                style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w600),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                '${(c['tamanhoMedida'] as num?)?.toStringAsFixed(2) ?? '-'} ${widget.medida}'
                                '${widget.usarFator && fator > 1 ? '  ×${fator.toStringAsFixed(2)}' : ''}',
                                style: TextStyle(fontSize: 15, color: Colors.grey.shade700),
                              ),
                              if (c['dataHora'] != null)
                                Text(
                                  c['dataHora'] as String,
                                  style: TextStyle(fontSize: 14, color: Colors.grey.shade500),
                                ),
                            ],
                          ),
                        ),
                        // Pontuação
                        Text(
                          '${(c['pontuacao'] as num?)?.toStringAsFixed(2) ?? '-'} pts',
                          style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold),
                        ),
                      ],
                    ),
                  );
                }).toList(),
              ),
            ),
        ],
      ),
    );
  }

  void _abrirFoto(BuildContext context, String url) {
    showDialog(
      context: context,
      builder: (_) => Dialog(
        backgroundColor: Colors.black,
        insetPadding: const EdgeInsets.all(12),
        child: Stack(
          children: [
            InteractiveViewer(
              child: Image.network(
                url,
                fit: BoxFit.contain,
                errorBuilder: (ctx, e, st) => const Center(
                  child: Icon(Icons.broken_image, color: Colors.white, size: 64),
                ),
              ),
            ),
            Positioned(
              top: 8, right: 8,
              child: GestureDetector(
                onTap: () => Navigator.pop(context),
                child: Container(
                  decoration: BoxDecoration(
                    color: Colors.black54,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  padding: const EdgeInsets.all(4),
                  child: const Icon(Icons.close, color: Colors.white, size: 22),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}


class _StatusBanner extends StatelessWidget {
  final IconData icon;
  final String message;
  final Color color;
  const _StatusBanner({required this.icon, required this.message, required this.color});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: color.withAlpha(30),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withAlpha(80)),
      ),
      child: Row(
        children: [
          Icon(icon, color: color),
          const SizedBox(width: 8),
          Expanded(child: Text(message, style: TextStyle(color: color))),
        ],
      ),
    );
  }
}

class _ParticipantesSection extends StatelessWidget {
  final List<Membro> participantes;
  final String labelMembroPlural;
  final Map<String, int> posicoesRanking;

  const _ParticipantesSection({
    required this.participantes,
    required this.labelMembroPlural,
    required this.posicoesRanking,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        _SectionTitle(
          icon: Icons.groups_2_outlined,
          color: Colors.green.shade700,
          title: 'Participantes',
        ),
        const SizedBox(height: 8),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(8),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '${participantes.length} ${labelMembroPlural.toLowerCase()}',
                  style: TextStyle(
                    color: Colors.grey.shade700,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 8),
                Wrap(
                  spacing: 6,
                  runSpacing: 6,
                  children: participantes.map((participante) {
                    final posicaoRanking = posicoesRanking[participante.id];
                    return SizedBox(
                      width: 120,
                      height: 180,
                      child: Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          border: Border.all(color: Colors.grey.shade300),
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          crossAxisAlignment: CrossAxisAlignment.center,
                          children: [
                            GestureDetector(
                              onTap: (participante.fotoUrl ?? '').isNotEmpty
                                  ? () => _abrirFoto(context, participante.fotoUrl!)
                                  : null,
                              child: _Avatar(
                                fotoUrl: participante.fotoUrl ?? '',
                                icon: Icons.person,
                              ),
                            ),
                            const SizedBox(height: 8),
                            Text(
                              participante.nome,
                              textAlign: TextAlign.center,
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(fontWeight: FontWeight.w600),
                            ),
                            const SizedBox(height: 4),
                            SizedBox(
                              height: 42,
                              child: Center(
                                child: posicaoRanking != null && posicaoRanking > 0
                                    ? _Medalha(posicaoRanking)
                                    : null,
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  }).toList(),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  void _abrirFoto(BuildContext context, String url) {
    showDialog(
      context: context,
      builder: (_) => Dialog(
        backgroundColor: Colors.black,
        insetPadding: const EdgeInsets.all(12),
        child: Stack(
          children: [
            InteractiveViewer(
              child: Image.network(
                url,
                fit: BoxFit.contain,
                errorBuilder: (ctx, e, st) => const Center(
                  child: Icon(Icons.broken_image, color: Colors.white, size: 64),
                ),
              ),
            ),
            Positioned(
              top: 8,
              right: 8,
              child: GestureDetector(
                onTap: () => Navigator.pop(context),
                child: Container(
                  decoration: BoxDecoration(
                    color: Colors.black54,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  padding: const EdgeInsets.all(4),
                  child: const Icon(Icons.close, color: Colors.white, size: 22),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
