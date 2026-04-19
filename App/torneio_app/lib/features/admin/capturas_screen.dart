import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/captura.dart';
import '../../core/models/equipe.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class CapturasAdminScreen extends StatefulWidget {
  const CapturasAdminScreen({super.key});

  @override
  State<CapturasAdminScreen> createState() => _CapturasAdminScreenState();
}

class _CapturasAdminScreenState extends State<CapturasAdminScreen> {
  final ApiService _api = ApiService();

  List<Captura> _capturas = const [];
  List<Equipe> _equipes = const [];
  String? _equipeFiltroId;   // null = todas
  bool _carregando = true;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregarTudo();
  }

  Future<void> _carregarTudo() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      // Equipes para filtro
      final eqData = await _api.get(ApiConstants.equipes(auth!.slug!), token: auth.token);
      final equipes = eqData is List
          ? eqData.map((e) => Equipe.fromJson(e as Map<String, dynamic>)).toList()
          : <Equipe>[];
      equipes.sort((a, b) => a.nome.compareTo(b.nome));

      await _carregarCapturas(slug: auth.slug!, token: auth.token);

      if (mounted) setState(() => _equipes = equipes);
    } on ApiException catch (e) {
      if (mounted) setState(() => _erro = e.message);
    } catch (_) {
      if (mounted) setState(() => _erro = 'Erro ao carregar dados.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _carregarCapturas({String? slug, String? token}) async {
    final auth = context.read<AuthProvider>().usuario;
    final s = slug ?? auth?.slug;
    final t = token ?? auth?.token;
    if (s == null || t == null) return;

    final url = _equipeFiltroId != null
        ? '${ApiConstants.capturas(s)}?equipeId=$_equipeFiltroId'
        : ApiConstants.capturas(s);

    final data = await _api.get(url, token: t);
    final lista = data is List
        ? data.map((e) => Captura.fromJson(e as Map<String, dynamic>)).toList()
        : <Captura>[];
    lista.sort((a, b) => b.dataHora.compareTo(a.dataHora));

    if (mounted) setState(() => _capturas = lista);
  }

  Future<void> _filtrarPorEquipe(String? equipeId) async {
    setState(() => _equipeFiltroId = equipeId);
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    setState(() => _carregando = true);
    try {
      await _carregarCapturas();
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _remover(Captura captura) async {
    final config = context.read<ConfigProvider>().config;
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final labelCaptura = config?.labelCaptura ?? 'Captura';

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: Text('Remover ${labelCaptura.toLowerCase()}'),
        content: Text(
          '${captura.nomeItem} — ${captura.nomeMembro}\n'
          '${captura.tamanhoMedida} ${config?.medidaCaptura ?? ''}\n\n'
          'Esta ação não pode ser desfeita.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Remover'),
          ),
        ],
      ),
    );

    if (confirmar != true || !mounted) return;

    try {
      await _api.delete(
        '${ApiConstants.capturas(auth!.slug!)}/${captura.id}',
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('$labelCaptura removida.')),
      );
      await _carregarCapturas();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  void _verFoto(String url) {
    if (url.isEmpty) return;
    showDialog(
      context: context,
      builder: (_) => Dialog(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ClipRRect(
              borderRadius: const BorderRadius.vertical(top: Radius.circular(12)),
              child: Image.network(
                url,
                fit: BoxFit.contain,
                errorBuilder: (context, e, st) => const Padding(
                  padding: EdgeInsets.all(32),
                  child: Icon(Icons.broken_image, size: 64, color: Colors.grey),
                ),
              ),
            ),
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Fechar'),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelCaptura = config?.labelCaptura ?? 'Captura';
    final labelCapturaPlural = config?.labelCapturaPlural ?? 'Capturas';
    final labelEquipePlural = config?.labelEquipePlural ?? 'Equipes';
    final medida = config?.medidaCaptura ?? '';
    final usarFator = config?.usarFatorMultiplicador ?? false;

    return Scaffold(
      appBar: AppBar(
        title: Text(labelCapturaPlural),
        bottom: _equipes.isNotEmpty
            ? PreferredSize(
                preferredSize: const Size.fromHeight(48),
                child: _FiltroEquipes(
                  equipes: _equipes,
                  selecionado: _equipeFiltroId,
                  labelTodas: 'Todas as $labelEquipePlural',
                  onSelecionado: _filtrarPorEquipe,
                ),
              )
            : null,
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregarTudo,
              child: _erro != null
                  ? ListView(children: [
                      Padding(
                        padding: const EdgeInsets.all(24),
                        child: Text(_erro!, textAlign: TextAlign.center),
                      )
                    ])
                  : _capturas.isEmpty
                      ? ListView(children: [
                          Padding(
                            padding: const EdgeInsets.all(24),
                            child: Text(
                              'Nenhuma ${labelCaptura.toLowerCase()} registrada.',
                              textAlign: TextAlign.center,
                              style: const TextStyle(color: Colors.grey),
                            ),
                          )
                        ])
                      : Column(
                          children: [
                            // Totalizador
                            _Totalizador(
                              capturas: _capturas,
                              medida: medida,
                              usarFator: usarFator,
                              labelCaptura: labelCapturaPlural,
                            ),
                            Expanded(
                              child: ListView.separated(
                                padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
                                itemCount: _capturas.length,
                                separatorBuilder: (context, i) => const SizedBox(height: 8),
                                itemBuilder: (_, i) => _CapturaCard(
                                  captura: _capturas[i],
                                  medida: medida,
                                  usarFator: usarFator,
                                  onRemover: () => _remover(_capturas[i]),
                                  onVerFoto: () => _verFoto(_capturas[i].fotoUrl),
                                ),
                              ),
                            ),
                          ],
                        ),
            ),
    );
  }
}

// ── Filtro de equipes ─────────────────────────────────────────────────────────

class _FiltroEquipes extends StatelessWidget {
  final List<Equipe> equipes;
  final String? selecionado;
  final String labelTodas;
  final ValueChanged<String?> onSelecionado;

  const _FiltroEquipes({
    required this.equipes,
    required this.selecionado,
    required this.labelTodas,
    required this.onSelecionado,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 44,
      child: ListView(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
        children: [
          _Chip(
            label: labelTodas,
            selecionado: selecionado == null,
            onTap: () => onSelecionado(null),
          ),
          ...equipes.map((e) => _Chip(
                label: e.nome,
                selecionado: selecionado == e.id,
                onTap: () => onSelecionado(e.id),
              )),
        ],
      ),
    );
  }
}

class _Chip extends StatelessWidget {
  final String label;
  final bool selecionado;
  final VoidCallback onTap;

  const _Chip({required this.label, required this.selecionado, required this.onTap});

  @override
  Widget build(BuildContext context) {
    final cor = Theme.of(context).colorScheme.primary;
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: GestureDetector(
        onTap: onTap,
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
          decoration: BoxDecoration(
            color: selecionado ? Colors.white : cor,
            border: Border.all(color: selecionado ? Colors.grey.shade400 : cor),
            borderRadius: BorderRadius.circular(20),
          ),
          child: Text(
            label,
            style: TextStyle(
              fontSize: 12,
              fontWeight: selecionado ? FontWeight.normal : FontWeight.w600,
              color: selecionado ? Colors.grey.shade800 : Colors.white,
            ),
          ),
        ),
      ),
    );
  }
}

// ── Totalizador ───────────────────────────────────────────────────────────────

class _Totalizador extends StatelessWidget {
  final List<Captura> capturas;
  final String medida;
  final bool usarFator;
  final String labelCaptura;

  const _Totalizador({
    required this.capturas,
    required this.medida,
    required this.usarFator,
    required this.labelCaptura,
  });

  @override
  Widget build(BuildContext context) {
    final totalMedida = capturas.fold(0.0, (s, c) => s + c.tamanhoMedida);
    final totalPontos = capturas.fold(0.0, (s, c) => s + c.pontuacao);

    return Container(
      margin: const EdgeInsets.fromLTRB(16, 8, 16, 0),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.primaryContainer.withAlpha(80),
        borderRadius: BorderRadius.circular(10),
        border: Border.all(
          color: Theme.of(context).colorScheme.primary.withAlpha(60),
        ),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: [
          _StatItem(
            label: labelCaptura,
            value: '${capturas.length}',
          ),
          _StatItem(
            label: 'Total $medida'.trim(),
            value: _fmt(totalMedida),
          ),
          if (usarFator)
            _StatItem(
              label: 'Pontos',
              value: _fmt(totalPontos),
            ),
        ],
      ),
    );
  }

  String _fmt(double v) => v == v.truncateToDouble() ? v.toInt().toString() : v.toStringAsFixed(2);
}

class _StatItem extends StatelessWidget {
  final String label;
  final String value;
  const _StatItem({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Text(value,
            style: Theme.of(context)
                .textTheme
                .titleMedium
                ?.copyWith(fontWeight: FontWeight.bold)),
        Text(label,
            style: Theme.of(context)
                .textTheme
                .labelSmall
                ?.copyWith(color: Colors.grey)),
      ],
    );
  }
}

// ── Card de captura ───────────────────────────────────────────────────────────

class _CapturaCard extends StatelessWidget {
  final Captura captura;
  final String medida;
  final bool usarFator;
  final VoidCallback onRemover;
  final VoidCallback onVerFoto;

  const _CapturaCard({
    required this.captura,
    required this.medida,
    required this.usarFator,
    required this.onRemover,
    required this.onVerFoto,
  });

  @override
  Widget build(BuildContext context) {
    final dateFmt = DateFormat('dd/MM/yyyy HH:mm');
    final temFoto = captura.fotoUrl.isNotEmpty;

    return Card(
      child: IntrinsicHeight(
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Foto
            GestureDetector(
              onTap: temFoto ? onVerFoto : null,
              child: ClipRRect(
                borderRadius: const BorderRadius.horizontal(left: Radius.circular(12)),
                child: SizedBox(
                  width: 72,
                  child: temFoto
                      ? Image.network(
                          captura.fotoUrl,
                          fit: BoxFit.cover,
                          errorBuilder: (context, e, st) => _FotoPlaceholder(),
                        )
                      : _FotoPlaceholder(),
                ),
              ),
            ),

            // Conteúdo
            Expanded(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(12, 10, 4, 10),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            captura.nomeItem,
                            style: const TextStyle(fontWeight: FontWeight.bold),
                          ),
                        ),
                        if (captura.pendenteSync)
                          Tooltip(
                            message: 'Pendente de sincronização',
                            child: Icon(Icons.sync_problem,
                                size: 16, color: Colors.orange.shade700),
                          ),
                      ],
                    ),
                    const SizedBox(height: 2),
                    Text(captura.nomeMembro,
                        style: const TextStyle(color: Colors.grey, fontSize: 12)),
                    Text(captura.nomeEquipe,
                        style: const TextStyle(color: Colors.grey, fontSize: 12)),
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        _MedidaBadge(
                          valor: captura.tamanhoMedida,
                          unidade: medida,
                        ),
                        if (usarFator && captura.fatorMultiplicador > 1) ...[
                          const SizedBox(width: 6),
                          _PontosBadge(pontos: captura.pontuacao),
                        ],
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      dateFmt.format(captura.dataHora.toLocal()),
                      style: const TextStyle(fontSize: 11, color: Colors.grey),
                    ),
                  ],
                ),
              ),
            ),

            // Ações
            Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                if (temFoto)
                  IconButton(
                    icon: const Icon(Icons.photo_outlined, size: 20),
                    tooltip: 'Ver foto',
                    onPressed: onVerFoto,
                  ),
                IconButton(
                  icon: Icon(Icons.delete_outline,
                      size: 20, color: Colors.red.shade400),
                  tooltip: 'Remover',
                  onPressed: onRemover,
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _FotoPlaceholder extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.grey.shade100,
      child: const Icon(Icons.photo_camera_outlined, color: Colors.grey, size: 28),
    );
  }
}

class _MedidaBadge extends StatelessWidget {
  final double valor;
  final String unidade;
  const _MedidaBadge({required this.valor, required this.unidade});

  @override
  Widget build(BuildContext context) {
    final texto = valor == valor.truncateToDouble()
        ? '${valor.toInt()} $unidade'
        : '${valor.toStringAsFixed(2)} $unidade';
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.primary.withAlpha(20),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
            color: Theme.of(context).colorScheme.primary.withAlpha(60)),
      ),
      child: Text(
        texto.trim(),
        style: TextStyle(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: Theme.of(context).colorScheme.primary,
        ),
      ),
    );
  }
}

class _PontosBadge extends StatelessWidget {
  final double pontos;
  const _PontosBadge({required this.pontos});

  @override
  Widget build(BuildContext context) {
    final texto = pontos == pontos.truncateToDouble()
        ? '${pontos.toInt()} pts'
        : '${pontos.toStringAsFixed(2)} pts';
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: Colors.amber.withAlpha(30),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.amber.withAlpha(100)),
      ),
      child: Text(
        texto,
        style: const TextStyle(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: Colors.amber,
        ),
      ),
    );
  }
}
