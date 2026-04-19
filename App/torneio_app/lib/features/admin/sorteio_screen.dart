import 'dart:async';
import 'dart:math';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:uuid/uuid.dart';
import '../../core/constants.dart';
import '../../core/models/sorteio_equipe.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

// ── Agrupamento por embarcação ────────────────────────────────
class _GrupoEquipe {
  final String equipeId;
  final String nomeEquipe;
  final List<String> membros;
  _GrupoEquipe(this.equipeId, this.nomeEquipe, this.membros);
}

List<_GrupoEquipe> _agrupar(List<SorteioEquipe> lista) {
  final map = <String, _GrupoEquipe>{};
  for (final item in lista) {
    if (!map.containsKey(item.equipeId)) {
      map[item.equipeId] = _GrupoEquipe(item.equipeId, item.nomeEquipe, []);
    }
    map[item.equipeId]!.membros.add(item.nomeMembro);
  }
  return map.values.toList()
    ..sort((a, b) => a.nomeEquipe.compareTo(b.nomeEquipe));
}

// ── Dados de simulação (debug) ────────────────────────────────
const _mockEquipesNomes = [
  'Barco Peixe Espada',
  'Barco Marlim Azul',
  'Barco Tubarão Martelo',
  'Barco Arraia Manta',
  'Barco Golfinho Prateado',
  'Barco Orca Negra',
];

const _todosPescadores = [
  'Ana Silva',      'Bruno Costa',    'Carlos Lima',    'Diana Melo',     'Eduardo Neto',
  'Fernanda Gomes', 'Gabriel Souza',  'Helena Alves',   'Igor Martins',   'Juliana Reis',
  'Klaus Weber',    'Laura Dias',     'Marcos Brum',    'Natália Cruz',   'Oscar Faria',
  'Patricia Barros','Quirino Moura',  'Renata Pinto',   'Sérgio Leal',    'Tânia Vaz',
  'Ulisses Torres', 'Vânia Ribeiro',  'Wilson Cunha',   'Ximena Pereira', 'Yago Duarte',
  'Zara Monteiro',  'André Felix',    'Bianca Gama',    'Cássio Rios',    'Débora Sena',
];

List<SorteioEquipe> _gerarMockSorteio() {
  final rng = Random();
  const uuid = Uuid();
  final pescadores = List<String>.from(_todosPescadores)..shuffle(rng);
  final result = <SorteioEquipe>[];
  int idx = 0;
  for (final nome in _mockEquipesNomes) {
    final equipeId = uuid.v4();
    for (int pos = 1; pos <= 5; pos++) {
      result.add(SorteioEquipe(
        id: uuid.v4(),
        equipeId: equipeId,
        nomeEquipe: nome,
        membroId: uuid.v4(),
        nomeMembro: pescadores[idx++],
        posicao: pos,
      ));
    }
  }
  return result;
}

// ── Pré-condições ─────────────────────────────────────────────
class _PreCondicoes {
  final int qtdEquipes;
  final int totalVagas;
  final int qtdMembros;
  final bool valido;
  final String? mensagemErro;
  const _PreCondicoes({
    required this.qtdEquipes,
    required this.totalVagas,
    required this.qtdMembros,
    required this.valido,
    this.mensagemErro,
  });
  factory _PreCondicoes.fromJson(Map<String, dynamic> json) => _PreCondicoes(
        qtdEquipes: json['qtdEquipes'] as int,
        totalVagas: json['totalVagas'] as int,
        qtdMembros: json['qtdMembros'] as int,
        valido: json['valido'] as bool,
        mensagemErro: json['mensagemErro'] as String?,
      );
}

// ── Tela principal ────────────────────────────────────────────
class SorteioAdminScreen extends StatefulWidget {
  const SorteioAdminScreen({super.key});
  @override
  State<SorteioAdminScreen> createState() => _SorteioAdminScreenState();
}

class _SorteioAdminScreenState extends State<SorteioAdminScreen> {
  final ApiService _api = ApiService();
  bool _carregando = true;
  bool _processando = false;
  String? _erro;
  List<SorteioEquipe> _resultado = const [];
  _PreCondicoes? _preCondicoes;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    setState(() { _carregando = true; _erro = null; });
    try {
      final results = await Future.wait([
        _api.get(ApiConstants.sorteio(auth!.slug!), token: auth.token),
        _api.get(ApiConstants.sorteioPreCondicoes(auth.slug!), token: auth.token),
      ]);
      final rawLista = results[0];
      final lista = rawLista is List
          ? rawLista.map((e) => SorteioEquipe.fromJson(e as Map<String, dynamic>)).toList()
          : <SorteioEquipe>[];
      final pre = _PreCondicoes.fromJson(results[1] as Map<String, dynamic>);
      setState(() {
        _resultado = lista;
        _preCondicoes = pre;
      });
    } on ApiException catch (e) {
      setState(() => _erro = e.message);
    } catch (_) {
      setState(() => _erro = 'Erro ao carregar dados.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _sortear() async {
    final pre = _preCondicoes;
    if (pre != null && !pre.valido) return;
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    final config = context.read<ConfigProvider>().config;

    setState(() => _processando = true);

    final apiFuture = _api
        .post(ApiConstants.sorteio(auth!.slug!), {}, token: auth.token)
        .then<List<SorteioEquipe>>((data) {
          if (data is! List) return <SorteioEquipe>[];
          return data.map((e) => SorteioEquipe.fromJson(e as Map<String, dynamic>)).toList();
        });

    if (!mounted) return;

    // Dialog retorna: null = erro; [] = cancelado; [..] = confirmado pelo usuário
    final resultado = await showDialog<List<SorteioEquipe>>(
      context: context,
      barrierDismissible: false,
      barrierColor: const Color(0xEA080810),
      builder: (_) => _SorteioAnimacaoDialog(
        chamadaApi: () => apiFuture,
        labelEquipe: config?.labelEquipe ?? 'Equipe',
        labelMembro: config?.labelMembro ?? 'Membro',
      ),
    );

    if (!mounted) return;

    if (resultado == null) {
      // Erro na API durante a animação
      setState(() => _processando = false);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Erro ao realizar sorteio.'),
            backgroundColor: Colors.red),
      );
      return;
    }

    if (resultado.isEmpty) {
      // Usuário cancelou
      setState(() => _processando = false);
      return;
    }

    // Confirmar: persiste no servidor
    try {
      final payload = resultado
          .map((e) => {
                'equipeId':   e.equipeId,
                'nomeEquipe': e.nomeEquipe,
                'membroId':   e.membroId,
                'nomeMembro': e.nomeMembro,
                'posicao':    e.posicao,
              })
          .toList();
      await _api.post(
        ApiConstants.sorteioConfirmar(auth.slug!),
        payload,
        token: auth.token,
      );
      if (!mounted) return;
      setState(() { _resultado = resultado; _processando = false; });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Sorteio confirmado com sucesso.')),
      );
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _processando = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    } catch (_) {
      if (!mounted) return;
      setState(() => _processando = false);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Erro ao confirmar sorteio.'),
            backgroundColor: Colors.red),
      );
    }
  }

  Future<void> _simular() async {
    final config = context.read<ConfigProvider>().config;
    await showDialog<void>(
      context: context,
      barrierDismissible: false,
      barrierColor: const Color(0xEA080810),
      builder: (_) => _SorteioAnimacaoDialog(
        chamadaApi: () async => _gerarMockSorteio(),
        labelEquipe: config?.labelEquipe ?? 'Equipe',
        labelMembro: config?.labelMembro ?? 'Membro',
        isSimulacao: true,
      ),
    );
  }

  Future<void> _limpar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Limpar sorteio'),
        content: const Text('Deseja remover o resultado atual?'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Cancelar')),
          FilledButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Limpar')),
        ],
      ),
    );
    if (confirmar != true) return;
    setState(() => _processando = true);
    try {
      await _api.delete(ApiConstants.sorteio(auth!.slug!), token: auth.token);
      if (!mounted) return;
      setState(() => _resultado = const []);
      await _carregar();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Sorteio limpo com sucesso.')),
      );
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) setState(() => _processando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    if (config?.modoSorteio == 'Nenhum') {
      return Scaffold(
        appBar: AppBar(title: const Text('Sorteio')),
        body: const Center(
          child: Padding(
              padding: EdgeInsets.all(24),
              child: Text('Este torneio não utiliza sorteio.')),
        ),
      );
    }

    final labelEquipePlural = config?.labelEquipePlural ?? 'Equipes';
    final labelMembroPlural = config?.labelMembroPlural ?? 'Membros';
    final pre = _preCondicoes;
    final podeSortear =
        !_processando && (pre == null || pre.valido) && _resultado.isEmpty;

    final grupos = _agrupar(_resultado);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Sorteio'),
        actions: [
          if (kDebugMode)
            IconButton(
              onPressed: _processando ? null : _simular,
              icon: const Icon(Icons.bug_report_outlined),
              tooltip: 'Simular sorteio (DEV)',
            ),
          if (_resultado.isEmpty)
            TextButton.icon(
              onPressed: podeSortear ? _sortear : null,
              icon: const Icon(Icons.shuffle, color: Colors.white),
              label: const Text('Sortear',
                  style: TextStyle(color: Colors.white)),
            ),
          if (_resultado.isNotEmpty)
            TextButton.icon(
              onPressed: _processando ? null : _limpar,
              icon: const Icon(Icons.delete_outline, color: Colors.white),
              label: const Text('Limpar',
                  style: TextStyle(color: Colors.white)),
            ),
        ],
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: _erro != null
                  ? ListView(children: [
                      Padding(
                          padding: const EdgeInsets.all(24),
                          child: Text(_erro!, textAlign: TextAlign.center)),
                    ])
                  : ListView(
                      padding: const EdgeInsets.all(16),
                      children: [
                        // Alerta de pré-condições
                        if (pre != null && !pre.valido) ...[
                          _AlertaPreCondicoes(
                            pre: pre,
                            labelEquipePlural: labelEquipePlural,
                            labelMembroPlural: labelMembroPlural,
                          ),
                          const SizedBox(height: 16),
                        ],
                        // Estado vazio
                        if (_resultado.isEmpty)
                          _EstadoVazio(
                            podeSortear: podeSortear,
                            processando: _processando,
                            totalVagas: pre?.totalVagas ?? 0,
                            qtdEquipes: pre?.qtdEquipes ?? 0,
                            labelEquipePlural: labelEquipePlural,
                            labelMembroPlural: labelMembroPlural,
                            onSortear: _sortear,
                          )
                        else ...[
                          // Cabeçalho do resultado
                          Padding(
                            padding: const EdgeInsets.only(bottom: 12),
                            child: Text(
                              '${grupos.length} $labelEquipePlural · ${_resultado.length} $labelMembroPlural sorteados',
                              style: Theme.of(context)
                                  .textTheme
                                  .titleSmall
                                  ?.copyWith(color: Colors.grey),
                            ),
                          ),
                          // Grade de embarcações
                          ...grupos.map((g) => _CardEquipeSorteada(grupo: g)),
                        ],
                      ],
                    ),
            ),
    );
  }
}

// ── Card de embarcação com seus membros ──────────────────────
class _CardEquipeSorteada extends StatelessWidget {
  final _GrupoEquipe grupo;
  const _CardEquipeSorteada({required this.grupo});

  @override
  Widget build(BuildContext context) {
    final primary = Theme.of(context).colorScheme.primary;
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // Cabeçalho com nome da embarcação
          Container(
            color: primary,
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
            child: Row(
              children: [
                Icon(Icons.water, color: Colors.white.withAlpha(200), size: 18),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    grupo.nomeEquipe,
                    style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: 14),
                  ),
                ),
                Text(
                  '${grupo.membros.length} membros',
                  style: TextStyle(
                      color: Colors.white.withAlpha(180), fontSize: 11),
                ),
              ],
            ),
          ),
          // Lista de pescadores
          ...grupo.membros.asMap().entries.map((e) => Container(
                decoration: BoxDecoration(
                  border: Border(
                    bottom: e.key < grupo.membros.length - 1
                        ? BorderSide(color: Colors.grey.withAlpha(40))
                        : BorderSide.none,
                  ),
                ),
                padding:
                    const EdgeInsets.symmetric(horizontal: 14, vertical: 9),
                child: Text(e.value, style: const TextStyle(fontSize: 13)),
              )),
        ],
      ),
    );
  }
}

// ── Dialog de animação ────────────────────────────────────────
enum _Fase { girando, revelando, erro }

class _SorteioAnimacaoDialog extends StatefulWidget {
  const _SorteioAnimacaoDialog({
    required this.chamadaApi,
    required this.labelEquipe,
    required this.labelMembro,
    this.isSimulacao = false,
  });
  final Future<List<SorteioEquipe>> Function() chamadaApi;
  final String labelEquipe;
  final String labelMembro;
  final bool isSimulacao;

  @override
  State<_SorteioAnimacaoDialog> createState() =>
      _SorteioAnimacaoDialogState();
}

class _SorteioAnimacaoDialogState extends State<_SorteioAnimacaoDialog> {
  _Fase _fase = _Fase.girando;
  Timer? _spinTimer;
  String _slotEquipe = '---';
  String _slotMembro = '---';
  int _spinIdx = 0;

  List<_GrupoEquipe> _grupos = [];
  List<SorteioEquipe> _resultadosOriginais = [];
  List<bool> _visibleCards = [];
  bool _mostrarBotaoFechar = false;
  String _erroMsg = '';

  static const _poolEquipes = [
    'Mar Bravo',      'Peixe Espada',  'Águia do Mar',   'Barco Veloz',
    'Corredeira',     'Maré Alta',     'Vento Sul',       'Onda Brava',
    'Estrela do Mar', 'Boto Cinza',    'Albatroz',        'Marlim Azul',
  ];
  static const _poolMembros = [
    'João Silva',   'Pedro Costa',    'Ana Lima',        'Carlos Melo',
    'Maria Santos', 'Lucas Ferreira', 'Beatriz Neves',   'Rafael Torres',
    'Priya Mendes', 'Sonia Cruz',     'Diego Rocha',     'Camila Duarte',
  ];

  @override
  void initState() {
    super.initState();
    _iniciar();
  }

  Future<void> _iniciar() async {
    _startSpinner();
    try {
      List<SorteioEquipe>? resultados;
      await Future.wait([
        widget.chamadaApi().then((r) => resultados = r),
        Future<void>.delayed(const Duration(milliseconds: 3500)),
      ]);
      _stopSpinner();
      await _startReveal(resultados!);
    } catch (e) {
      _stopSpinner();
      if (!mounted) return;
      setState(() {
        _fase = _Fase.erro;
        _erroMsg = e is ApiException ? e.message : 'Erro ao realizar sorteio.';
      });
      await Future<void>.delayed(const Duration(seconds: 2));
      if (mounted) Navigator.of(context).pop(null);
    }
  }

  void _startSpinner() {
    _spinIdx = 0;
    _spinTimer = Timer.periodic(const Duration(milliseconds: 140), (_) {
      if (!mounted) return;
      setState(() {
        _slotEquipe = _poolEquipes[_spinIdx % _poolEquipes.length];
        _slotMembro = _poolMembros[(_spinIdx + 4) % _poolMembros.length];
        _spinIdx++;
      });
    });
  }

  void _stopSpinner() => _spinTimer?.cancel();

  Future<void> _startReveal(List<SorteioEquipe> resultados) async {
    if (!mounted) return;
    final grupos = _agrupar(resultados);
    setState(() {
      _fase = _Fase.revelando;
      _grupos = grupos;
      _resultadosOriginais = resultados;
      _visibleCards = List.filled(grupos.length, false);
      _mostrarBotaoFechar = false;
    });
    for (int i = 0; i < grupos.length; i++) {
      await Future<void>.delayed(const Duration(milliseconds: 480));
      if (!mounted) return;
      setState(() => _visibleCards[i] = true);
    }
    await Future<void>.delayed(const Duration(milliseconds: 500));
    if (!mounted) return;
    setState(() => _mostrarBotaoFechar = true);
  }

  void _confirmar() {
    // Simulação: fecha sem salvar (retorna lista vazia = cancelado)
    if (widget.isSimulacao) {
      Navigator.of(context).pop(<SorteioEquipe>[]);
      return;
    }
    Navigator.of(context).pop(_resultadosOriginais);
  }

  void _cancelar() {
    // Fecha sem salvar
    Navigator.of(context).pop(<SorteioEquipe>[]);
  }

  @override
  void dispose() {
    _spinTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Material(
      color: const Color(0xEA080812),
      child: SafeArea(
        child: switch (_fase) {
          _Fase.girando   => _buildGirando(),
          _Fase.revelando => _buildRevelando(),
          _Fase.erro      => _buildErro(),
        },
      ),
    );
  }

  Widget _buildGirando() {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        const Text('🎰  SORTEIO',
            style: TextStyle(
              color: Colors.white,
              fontSize: 28,
              fontWeight: FontWeight.w700,
              letterSpacing: 2,
              shadows: [Shadow(color: Color(0xFF4FC3F7), blurRadius: 18)],
            )),
        const SizedBox(height: 32),
        _SlotBox(
            label: widget.labelEquipe,
            value: _slotEquipe,
            cor: const Color(0xFF4FC3F7)),
        const SizedBox(height: 12),
        _SlotBox(
            label: widget.labelMembro,
            value: _slotMembro,
            cor: const Color(0xFF81C784)),
        const SizedBox(height: 32),
        Text(
          'Sorteando ${widget.labelMembro.toLowerCase()}s para cada ${widget.labelEquipe.toLowerCase()}...',
          style: const TextStyle(color: Color(0x99FFFFFF), fontSize: 13),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 20),
        const _BouncingDots(),
      ],
    );
  }

  Widget _buildRevelando() {
    return Column(
      children: [
        const SizedBox(height: 36),
        const Text('📋  Resultado do Sorteio',
            style: TextStyle(
              color: Colors.white,
              fontSize: 22,
              fontWeight: FontWeight.w700,
              shadows: [Shadow(color: Color(0xFF4FC3F7), blurRadius: 14)],
            )),
        const SizedBox(height: 16),
        Expanded(
          child: ListView.builder(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            itemCount: _grupos.length,
            itemBuilder: (_, i) {
              final visible = i < _visibleCards.length && _visibleCards[i];
              return AnimatedOpacity(
                opacity: visible ? 1.0 : 0.0,
                duration: const Duration(milliseconds: 450),
                child: AnimatedSlide(
                  offset:
                      visible ? Offset.zero : const Offset(0, 0.2),
                  duration: const Duration(milliseconds: 450),
                  curve: Curves.easeOut,
                  child: _RevealBoatCard(
                      grupo: _grupos[i],
                      labelMembro: widget.labelMembro),
                ),
              );
            },
          ),
        ),
        // Botões confirmar / cancelar
        AnimatedOpacity(
          opacity: _mostrarBotaoFechar ? 1.0 : 0.0,
          duration: const Duration(milliseconds: 400),
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 8, 20, 28),
            child: widget.isSimulacao
                ? SizedBox(
                    width: double.infinity,
                    child: FilledButton.icon(
                      onPressed: _mostrarBotaoFechar ? _cancelar : null,
                      icon: const Icon(Icons.close),
                      label: const Text('Fechar (simulação)'),
                      style: FilledButton.styleFrom(
                        backgroundColor: Colors.white24,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                      ),
                    ),
                  )
                : Row(
                    children: [
                      Expanded(
                        child: FilledButton.icon(
                          onPressed: _mostrarBotaoFechar ? _confirmar : null,
                          icon: const Icon(Icons.check_circle_outline),
                          label: const Text('Confirmar Sorteio'),
                          style: FilledButton.styleFrom(
                            backgroundColor: const Color(0xFF4FC3F7),
                            foregroundColor: Colors.black87,
                            padding: const EdgeInsets.symmetric(vertical: 14),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      OutlinedButton.icon(
                        onPressed: _mostrarBotaoFechar ? _cancelar : null,
                        icon: const Icon(Icons.close,
                            color: Colors.white70),
                        label: const Text('Cancelar',
                            style: TextStyle(color: Colors.white70)),
                        style: OutlinedButton.styleFrom(
                          side: const BorderSide(color: Colors.white30),
                          padding: const EdgeInsets.symmetric(
                              vertical: 14, horizontal: 20),
                        ),
                      ),
                    ],
                  ),
          ),
        ),
      ],
    );
  }

  Widget _buildErro() {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        const Icon(Icons.error_outline, color: Color(0xFFEF9A9A), size: 56),
        const SizedBox(height: 16),
        Text(_erroMsg,
            textAlign: TextAlign.center,
            style: const TextStyle(color: Color(0xFFEF9A9A), fontSize: 15)),
      ],
    );
  }
}

// ── Card de barco no reveal ───────────────────────────────────
class _RevealBoatCard extends StatelessWidget {
  const _RevealBoatCard({required this.grupo, required this.labelMembro});
  final _GrupoEquipe grupo;
  final String labelMembro;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: Colors.white.withAlpha(15),
        border: Border.all(color: const Color(0xFF4FC3F7).withAlpha(80)),
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
              color: const Color(0xFF4FC3F7).withAlpha(20), blurRadius: 12)
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // Cabeçalho da embarcação
          Container(
            padding:
                const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
            decoration: BoxDecoration(
              color: const Color(0xFF4FC3F7).withAlpha(38),
              borderRadius: const BorderRadius.vertical(top: Radius.circular(11)),
              border: Border(
                  bottom: BorderSide(
                      color: const Color(0xFF4FC3F7).withAlpha(60))),
            ),
            child: Row(
              children: [
                const Icon(Icons.water,
                    color: Color(0xFF4FC3F7), size: 16),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    grupo.nomeEquipe,
                    style: const TextStyle(
                        color: Color(0xFF4FC3F7),
                        fontWeight: FontWeight.bold,
                        fontSize: 14),
                  ),
                ),
                Text(
                  '${grupo.membros.length} ${labelMembro.toLowerCase()}s',
                  style: const TextStyle(
                      color: Color(0x994FC3F7), fontSize: 11),
                ),
              ],
            ),
          ),
          // Lista de pescadores
          ...grupo.membros.asMap().entries.map((e) => Container(
                padding: const EdgeInsets.symmetric(
                    horizontal: 14, vertical: 8),
                decoration: BoxDecoration(
                  border: Border(
                    bottom: e.key < grupo.membros.length - 1
                        ? BorderSide(
                            color: Colors.white.withAlpha(18))
                        : BorderSide.none,
                  ),
                ),
                child: Text(
                  e.value,
                  style: const TextStyle(
                      color: Colors.white, fontSize: 13),
                ),
              )),
        ],
      ),
    );
  }
}

// ── Slot box (fase girando) ───────────────────────────────────
class _SlotBox extends StatelessWidget {
  const _SlotBox(
      {required this.label, required this.value, required this.cor});
  final String label;
  final String value;
  final Color cor;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.symmetric(horizontal: 28),
      padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 20),
      decoration: BoxDecoration(
        color: Colors.white.withAlpha(15),
        border: Border.all(color: cor.withAlpha(90), width: 1.5),
        borderRadius: BorderRadius.circular(14),
        boxShadow: [BoxShadow(color: cor.withAlpha(35), blurRadius: 22)],
      ),
      child: Column(
        children: [
          Text(label.toUpperCase(),
              style: TextStyle(
                  color: cor.withAlpha(170),
                  fontSize: 10,
                  letterSpacing: 1.4)),
          const SizedBox(height: 6),
          Text(value,
              style: TextStyle(
                  color: cor,
                  fontSize: 20,
                  fontWeight: FontWeight.bold),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              textAlign: TextAlign.center),
        ],
      ),
    );
  }
}

// ── Bouncing dots ─────────────────────────────────────────────
class _BouncingDots extends StatefulWidget {
  const _BouncingDots();
  @override
  State<_BouncingDots> createState() => _BouncingDotsState();
}

class _BouncingDotsState extends State<_BouncingDots>
    with TickerProviderStateMixin {
  late final List<AnimationController> _ctrls;

  @override
  void initState() {
    super.initState();
    _ctrls = List.generate(3, (i) {
      final c = AnimationController(
          vsync: this, duration: const Duration(milliseconds: 560));
      Future<void>.delayed(Duration(milliseconds: i * 185), () {
        if (mounted) c.repeat(reverse: true);
      });
      return c;
    });
  }

  @override
  void dispose() {
    for (final c in _ctrls) { c.dispose(); }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: List.generate(
        3,
        (i) => AnimatedBuilder(
          animation: _ctrls[i],
          builder: (_, _) => Transform.translate(
            offset: Offset(0, -9 * _ctrls[i].value),
            child: Container(
              margin: const EdgeInsets.symmetric(horizontal: 5),
              width: 9, height: 9,
              decoration: BoxDecoration(
                color: Color.lerp(
                    const Color(0xFF4FC3F7), Colors.white, _ctrls[i].value),
                shape: BoxShape.circle,
              ),
            ),
          ),
        ),
      ),
    );
  }
}

// ── Widgets auxiliares da tela principal ──────────────────────
class _AlertaPreCondicoes extends StatelessWidget {
  final _PreCondicoes pre;
  final String labelEquipePlural;
  final String labelMembroPlural;
  const _AlertaPreCondicoes(
      {required this.pre,
      required this.labelEquipePlural,
      required this.labelMembroPlural});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.orange.shade50,
        border: Border.all(color: Colors.orange.shade300),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(children: [
            Icon(Icons.warning_amber_rounded,
                color: Colors.orange.shade700, size: 20),
            const SizedBox(width: 8),
            Text('Sorteio indisponível',
                style: TextStyle(
                    fontWeight: FontWeight.bold,
                    color: Colors.orange.shade800)),
          ]),
          const SizedBox(height: 8),
          Text(pre.mensagemErro ?? '',
              style: TextStyle(color: Colors.orange.shade900)),
          const SizedBox(height: 8),
          DefaultTextStyle(
            style:
                TextStyle(color: Colors.orange.shade700, fontSize: 12),
            child: Row(children: [
              Text('$labelEquipePlural: ${pre.qtdEquipes}'),
              const Text('  ·  '),
              Text('Vagas: ${pre.totalVagas}'),
              const Text('  ·  '),
              Text('$labelMembroPlural: ${pre.qtdMembros}'),
            ]),
          ),
        ],
      ),
    );
  }
}

class _EstadoVazio extends StatelessWidget {
  final bool podeSortear;
  final bool processando;
  final int totalVagas;
  final int qtdEquipes;
  final String labelEquipePlural;
  final String labelMembroPlural;
  final VoidCallback onSortear;
  const _EstadoVazio({
    required this.podeSortear,
    required this.processando,
    required this.totalVagas,
    required this.qtdEquipes,
    required this.labelEquipePlural,
    required this.labelMembroPlural,
    required this.onSortear,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        const SizedBox(height: 32),
        const Icon(Icons.shuffle, size: 64, color: Colors.grey),
        const SizedBox(height: 16),
        const Text('Nenhum sorteio realizado ainda.',
            textAlign: TextAlign.center),
        if (totalVagas > 0) ...[
          const SizedBox(height: 6),
          Text(
            'Serão sorteados $totalVagas $labelMembroPlural para $qtdEquipes $labelEquipePlural.',
            textAlign: TextAlign.center,
            style: const TextStyle(color: Colors.grey, fontSize: 13),
          ),
        ],
        const SizedBox(height: 20),
        FilledButton.icon(
          onPressed: podeSortear ? onSortear : null,
          icon: const Icon(Icons.shuffle),
          label: Text(processando ? 'Processando...' : 'Realizar Sorteio'),
        ),
      ],
    );
  }
}
