import 'dart:async';
import 'dart:math';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:uuid/uuid.dart';
import '../../core/constants.dart';
import '../../core/models/equipe.dart';
import '../../core/models/grupo.dart';
import '../../core/models/membro.dart';
import '../../core/models/sorteio_equipe.dart';
import '../../core/models/sorteio_grupo.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

const bool kVideoDemoSorteio = bool.fromEnvironment('VIDEO_DEMO_SORTEIO', defaultValue: false);

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

// ── Resultado da seleção de participantes ─────────────────────
class _SelecaoParticipantes {
  final List<Equipe> equipes;
  final List<Membro> membros;
  const _SelecaoParticipantes({required this.equipes, required this.membros});
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

  // Abre o diálogo de seleção de participantes e retorna a seleção confirmada
  Future<_SelecaoParticipantes?> _abrirSelecao({
    List<Equipe>? mockEquipes,
    List<Membro>? mockMembros,
  }) async {
    final isMock = mockEquipes != null && mockMembros != null;
    final authNullable = context.read<AuthProvider>().usuario;
    if (!isMock && (authNullable?.slug == null || authNullable?.token == null)) return null;
    final config = context.read<ConfigProvider>().config;

    return showDialog<_SelecaoParticipantes>(
      context: context,
      barrierDismissible: false,
      builder: (_) => _SelecionarParticipantesDialog(
        slug: authNullable?.slug ?? 'mock',
        token: authNullable?.token ?? '',
        labelEquipe: config?.labelEquipe ?? 'Equipe',
        labelEquipePlural: config?.labelEquipePlural ?? 'Equipes',
        labelMembro: config?.labelMembro ?? 'Membro',
        labelMembroPlural: config?.labelMembroPlural ?? 'Membros',
        mockEquipes: mockEquipes,
        mockMembros: mockMembros,
      ),
    );
  }

  Future<void> _sortear() async {
    final pre = _preCondicoes;
    if (pre != null && !pre.valido) return;
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;
    final config = context.read<ConfigProvider>().config;

    // Abre o diálogo de seleção
    if (!mounted) return;
    final selecao = await _abrirSelecao();
    if (selecao == null || !mounted) return; // cancelado

    setState(() => _processando = true);

    final filtro = {
      'equipeIds': selecao.equipes.map((e) => e.id).toList(),
      'membroIds': selecao.membros.map((m) => m.id).toList(),
    };

    final apiFuture = _api
        .post(ApiConstants.sorteio(auth!.slug!), filtro, token: auth.token)
        .then<List<SorteioEquipe>>((data) {
          if (data is! List) return <SorteioEquipe>[];
          return data.map((e) => SorteioEquipe.fromJson(e as Map<String, dynamic>)).toList();
        });

    if (!mounted) return;

    final resultado = await showDialog<List<SorteioEquipe>>(
      context: context,
      barrierDismissible: false,
      barrierColor: const Color(0xEA080810),
      builder: (_) => _SorteioAnimacaoDialog(
        chamadaApi: () => apiFuture,
        labelEquipe: config?.labelEquipe ?? 'Equipe',
        labelMembro: config?.labelMembro ?? 'Membro',
        nomesEquipes: selecao.equipes.map((e) => e.nome).toList(),
        nomesMembros: selecao.membros.map((m) => m.nome).toList(),
      ),
    );

    if (!mounted) return;

    if (resultado == null) {
      setState(() => _processando = false);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Erro ao realizar sorteio.'),
            backgroundColor: Colors.red),
      );
      return;
    }

    if (resultado.isEmpty) {
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

    // Monta equipes e membros fictícios para a seleção na simulação
    const uuid = Uuid();
    final mockEquipes = _mockEquipesNomes
        .map((nome) => Equipe(
              id: uuid.v4(),
              torneioId: 'mock',
              nome: nome,
              capitao: '',
              qtdVagas: 5,
              qtdMembros: 0,
              fiscalIds: const ['mock'],
            ))
        .toList();
    final mockMembros = _todosPescadores
        .map((nome) => Membro(id: uuid.v4(), nome: nome))
        .toList();

    // A simulação também passa pela seleção de participantes (com dados fictícios)
    if (!mounted) return;
    final selecao = await _abrirSelecao(
      mockEquipes: mockEquipes,
      mockMembros: mockMembros,
    );
    if (selecao == null || !mounted) return;

    await showDialog<void>(
      context: context,
      barrierDismissible: false,
      barrierColor: const Color(0xEA080810),
      builder: (_) => _SorteioAnimacaoDialog(
        chamadaApi: () async => _gerarMockSorteio(),
        labelEquipe: config?.labelEquipe ?? 'Equipe',
        labelMembro: config?.labelMembro ?? 'Membro',
        nomesEquipes: selecao.equipes.isNotEmpty
            ? selecao.equipes.map((e) => e.nome).toList()
            : const [..._mockEquipesNomes],
        nomesMembros: selecao.membros.isNotEmpty
            ? selecao.membros.map((m) => m.nome).toList()
            : List<String>.from(_todosPescadores)..shuffle(),
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

    if (config?.modoSorteio == 'GrupoEquipe') {
      return const _SorteioGrupoEquipeScreen();
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
          if (kDebugMode || kVideoDemoSorteio)
            IconButton(
              onPressed: _processando ? null : _simular,
              icon: const Icon(Icons.bug_report_outlined),
              tooltip: 'Simular sorteio (demo)',
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
                        if (pre != null && !pre.valido) ...[
                          _AlertaPreCondicoes(
                            pre: pre,
                            labelEquipePlural: labelEquipePlural,
                            labelMembroPlural: labelMembroPlural,
                          ),
                          const SizedBox(height: 16),
                        ],
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
                          ...grupos.map((g) => _CardEquipeSorteada(grupo: g)),
                        ],
                      ],
                    ),
            ),
    );
  }
}

// ── Diálogo de seleção de participantes ───────────────────────
class _SelecionarParticipantesDialog extends StatefulWidget {
  final String slug;
  final String token;
  final String labelEquipe;
  final String labelEquipePlural;
  final String labelMembro;
  final String labelMembroPlural;
  final List<Equipe>? mockEquipes;
  final List<Membro>? mockMembros;

  const _SelecionarParticipantesDialog({
    required this.slug,
    required this.token,
    required this.labelEquipe,
    required this.labelEquipePlural,
    required this.labelMembro,
    required this.labelMembroPlural,
    this.mockEquipes,
    this.mockMembros,
  });

  @override
  State<_SelecionarParticipantesDialog> createState() =>
      _SelecionarParticipantesDialogState();
}

class _SelecionarParticipantesDialogState
    extends State<_SelecionarParticipantesDialog> {
  final ApiService _api = ApiService();

  bool _carregando = true;
  String? _erro;
  List<Equipe> _equipes = const [];
  List<Membro> _membros = const [];
  final Set<String> _equipesSel = {};
  final Set<String> _membrosSel = {};

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    setState(() { _carregando = true; _erro = null; });
    try {
      final List<Equipe> equipes;
      final List<Membro> membros;

      if (widget.mockEquipes != null && widget.mockMembros != null) {
        // Modo simulação: usa dados fictícios sem chamar a API
        equipes = List<Equipe>.from(widget.mockEquipes!)
          ..sort((a, b) => a.nome.compareTo(b.nome));
        membros = List<Membro>.from(widget.mockMembros!)
          ..sort((a, b) => a.nome.compareTo(b.nome));
      } else {
        final results = await Future.wait([
          _api.get(ApiConstants.equipes(widget.slug), token: widget.token),
          _api.get(ApiConstants.membros(widget.slug), token: widget.token),
        ]);

        equipes = (results[0] as List)
            .map((e) => Equipe.fromJson(e as Map<String, dynamic>))
            .toList()
          ..sort((a, b) => a.nome.compareTo(b.nome));

        membros = (results[1] as List)
            .map((m) => Membro.fromJson(m as Map<String, dynamic>))
            .toList()
          ..sort((a, b) => a.nome.compareTo(b.nome));
      }

      setState(() {
        _equipes = equipes;
        _membros = membros;
        // Seleciona todos por padrão
        _equipesSel
          ..clear()
          ..addAll(equipes.map((e) => e.id));
        _membrosSel
          ..clear()
          ..addAll(membros.map((m) => m.id));
      });
    } catch (_) {
      setState(() => _erro = 'Erro ao carregar participantes.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  int get _totalVagasSelecionadas => _equipes
      .where((e) => _equipesSel.contains(e.id))
      .fold(0, (s, e) => s + e.qtdVagas);

  String? get _erroValidacao {
    final qtdEq = _equipesSel.length;
    final qtdMb = _membrosSel.length;
    final vagas = _totalVagasSelecionadas;
    if (qtdEq < 2) {
      return 'Selecione pelo menos 2 ${widget.labelEquipePlural.toLowerCase()}.';
    }
    if (qtdMb < vagas) {
      final faltam = vagas - qtdMb;
      return '${widget.labelMembroPlural} insuficientes: $vagas vaga${vagas != 1 ? 's' : ''}, $qtdMb selecionado${qtdMb != 1 ? 's' : ''}. Faltam $faltam.';
    }
    if (qtdMb > vagas) {
      final sobram = qtdMb - vagas;
      return '$sobram ${widget.labelMembroPlural.toLowerCase()} ficariam de fora: $vagas vaga${vagas != 1 ? 's' : ''}, $qtdMb selecionado${qtdMb != 1 ? 's' : ''}. Remova $sobram ou selecione mais ${widget.labelEquipePlural.toLowerCase()}.';
    }
    return null;
  }

  void _toggleEquipe(String id) =>
      setState(() => _equipesSel.contains(id) ? _equipesSel.remove(id) : _equipesSel.add(id));

  void _toggleMembro(String id) =>
      setState(() => _membrosSel.contains(id) ? _membrosSel.remove(id) : _membrosSel.add(id));

  void _selecionarTodasEquipes(bool sel) => setState(() {
        if (sel) {
          _equipesSel.addAll(_equipes.map((e) => e.id));
        } else {
          _equipesSel.clear();
        }
      });

  void _selecionarTodosMembros(bool sel) => setState(() {
        if (sel) {
          _membrosSel.addAll(_membros.map((m) => m.id));
        } else {
          _membrosSel.clear();
        }
      });

  void _confirmar() {
    final equipes = _equipes.where((e) => _equipesSel.contains(e.id)).toList();
    final membros = _membros.where((m) => _membrosSel.contains(m.id)).toList();
    Navigator.of(context).pop(_SelecaoParticipantes(equipes: equipes, membros: membros));
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      insetPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 24),
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 560),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Cabeçalho
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 20, 16, 0),
              child: Row(
                children: [
                  const Icon(Icons.people_alt_outlined),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      'Selecionar participantes',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close),
                    onPressed: () => Navigator.of(context).pop(null),
                  ),
                ],
              ),
            ),
            const Divider(),

            if (_carregando)
              const Padding(
                padding: EdgeInsets.all(32),
                child: CircularProgressIndicator(),
              )
            else if (_erro != null)
              Padding(
                padding: const EdgeInsets.all(24),
                child: Text(_erro!, style: const TextStyle(color: Colors.red)),
              )
            else
              Flexible(
                child: SingleChildScrollView(
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // ── Equipes ──────────────────────────────
                      _SecaoSelecao(
                        titulo: widget.labelEquipePlural,
                        icone: Icons.water,
                        cor: Theme.of(context).colorScheme.primary,
                        labelSelTodos: 'Todas',
                        labelDeselTodos: 'Nenhuma',
                        onSelTodos: () => _selecionarTodasEquipes(true),
                        onDeselTodos: () => _selecionarTodasEquipes(false),
                        resumo: '${_equipesSel.length} selecionada${_equipesSel.length != 1 ? 's' : ''} · $_totalVagasSelecionadas vaga${_totalVagasSelecionadas != 1 ? 's' : ''}',
                        itens: _equipes.map((e) {
                          return _ItemSelecao(
                            id: e.id,
                            nome: e.nome,
                            subtitulo: '${e.qtdVagas} vaga${e.qtdVagas != 1 ? 's' : ''}',
                            selecionado: _equipesSel.contains(e.id),
                            onToggle: () => _toggleEquipe(e.id),
                          );
                        }).toList(),
                      ),
                      const SizedBox(height: 16),

                      // ── Membros ──────────────────────────────
                      _SecaoSelecao(
                        titulo: widget.labelMembroPlural,
                        icone: Icons.person,
                        cor: Colors.teal,
                        labelSelTodos: 'Todos',
                        labelDeselTodos: 'Nenhum',
                        onSelTodos: () => _selecionarTodosMembros(true),
                        onDeselTodos: () => _selecionarTodosMembros(false),
                        resumo: '${_membrosSel.length} selecionado${_membrosSel.length != 1 ? 's' : ''}',
                        itens: _membros.map((m) {
                          return _ItemSelecao(
                            id: m.id,
                            nome: m.nome,
                            selecionado: _membrosSel.contains(m.id),
                            onToggle: () => _toggleMembro(m.id),
                          );
                        }).toList(),
                      ),
                      const SizedBox(height: 12),

                      // ── Validação ────────────────────────────
                      if (_erroValidacao != null)
                        Container(
                          padding: const EdgeInsets.all(10),
                          decoration: BoxDecoration(
                            color: Colors.orange.shade50,
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(color: Colors.orange.shade200),
                          ),
                          child: Row(
                            children: [
                              Icon(Icons.warning_amber_rounded,
                                  color: Colors.orange.shade700, size: 18),
                              const SizedBox(width: 8),
                              Expanded(
                                child: Text(
                                  _erroValidacao!,
                                  style: TextStyle(
                                      color: Colors.orange.shade900,
                                      fontSize: 13),
                                ),
                              ),
                            ],
                          ),
                        )
                      else
                        Container(
                          padding: const EdgeInsets.all(10),
                          decoration: BoxDecoration(
                            color: Colors.green.shade50,
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(color: Colors.green.shade200),
                          ),
                          child: Row(
                            children: [
                              Icon(Icons.check_circle_outline,
                                  color: Colors.green.shade700, size: 18),
                              const SizedBox(width: 8),
                              Expanded(
                                child: Text(
                                  '${_equipesSel.length} ${widget.labelEquipePlural.toLowerCase()} · $_totalVagasSelecionadas vagas · ${_membrosSel.length} ${widget.labelMembroPlural.toLowerCase()} — pronto para sortear.',
                                  style: TextStyle(
                                      color: Colors.green.shade900,
                                      fontSize: 13),
                                ),
                              ),
                            ],
                          ),
                        ),
                      const SizedBox(height: 8),
                    ],
                  ),
                ),
              ),

            const Divider(),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 8, 20, 16),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  TextButton(
                    onPressed: () => Navigator.of(context).pop(null),
                    child: const Text('Cancelar'),
                  ),
                  const SizedBox(width: 8),
                  FilledButton.icon(
                    onPressed: (_carregando || _erroValidacao != null) ? null : _confirmar,
                    icon: const Icon(Icons.shuffle),
                    label: const Text('Sortear'),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ── Seção de seleção (equipes ou membros) ─────────────────────
class _SecaoSelecao extends StatelessWidget {
  final String titulo;
  final IconData icone;
  final Color cor;
  final String labelSelTodos;
  final String labelDeselTodos;
  final VoidCallback onSelTodos;
  final VoidCallback onDeselTodos;
  final String resumo;
  final List<_ItemSelecao> itens;

  const _SecaoSelecao({
    required this.titulo,
    required this.icone,
    required this.cor,
    required this.labelSelTodos,
    required this.labelDeselTodos,
    required this.onSelTodos,
    required this.onDeselTodos,
    required this.resumo,
    required this.itens,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(icone, color: cor, size: 18),
            const SizedBox(width: 6),
            Text(titulo,
                style: TextStyle(
                    fontWeight: FontWeight.bold, color: cor)),
            const Spacer(),
            TextButton(
              style: TextButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 8),
                  minimumSize: Size.zero,
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                  visualDensity: VisualDensity.compact),
              onPressed: onSelTodos,
              child: Text(labelSelTodos,
                  style: const TextStyle(fontSize: 12)),
            ),
            Text('/', style: TextStyle(color: Colors.grey.shade400, fontSize: 12)),
            TextButton(
              style: TextButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 8),
                  minimumSize: Size.zero,
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                  visualDensity: VisualDensity.compact),
              onPressed: onDeselTodos,
              child: Text(labelDeselTodos,
                  style: const TextStyle(fontSize: 12)),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Container(
          decoration: BoxDecoration(
            border: Border.all(color: Colors.grey.shade300),
            borderRadius: BorderRadius.circular(8),
          ),
          constraints: const BoxConstraints(maxHeight: 180),
          child: ListView(
            shrinkWrap: true,
            children: itens,
          ),
        ),
        const SizedBox(height: 4),
        Text(resumo,
            style: const TextStyle(fontSize: 12, color: Colors.grey)),
      ],
    );
  }
}

class _ItemSelecao extends StatelessWidget {
  final String id;
  final String nome;
  final String? subtitulo;
  final bool selecionado;
  final VoidCallback onToggle;

  const _ItemSelecao({
    required this.id,
    required this.nome,
    this.subtitulo,
    required this.selecionado,
    required this.onToggle,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onToggle,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        child: Row(
          children: [
            Checkbox(
              value: selecionado,
              onChanged: (_) => onToggle(),
              visualDensity: VisualDensity.compact,
              materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
            ),
            const SizedBox(width: 6),
            Expanded(
              child: Text(nome, style: const TextStyle(fontSize: 13)),
            ),
            if (subtitulo != null)
              Text(subtitulo!,
                  style: const TextStyle(fontSize: 11, color: Colors.grey)),
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
    required this.nomesEquipes,
    required this.nomesMembros,
    this.isSimulacao = false,
  });
  final Future<List<SorteioEquipe>> Function() chamadaApi;
  final String labelEquipe;
  final String labelMembro;
  final List<String> nomesEquipes;
  final List<String> nomesMembros;
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

  static const _poolEquipesFallback = [
    'Mar Bravo', 'Peixe Espada', 'Águia do Mar', 'Barco Veloz',
    'Corredeira', 'Maré Alta', 'Vento Sul', 'Onda Brava',
  ];
  static const _poolMembrosFallback = [
    'João Silva', 'Pedro Costa', 'Ana Lima', 'Carlos Melo',
    'Maria Santos', 'Lucas Ferreira', 'Beatriz Neves', 'Rafael Torres',
  ];

  List<String> get _poolEquipes =>
      widget.nomesEquipes.isNotEmpty ? widget.nomesEquipes : _poolEquipesFallback;
  List<String> get _poolMembros =>
      widget.nomesMembros.isNotEmpty ? widget.nomesMembros : _poolMembrosFallback;

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
    if (widget.isSimulacao) {
      Navigator.of(context).pop(<SorteioEquipe>[]);
      return;
    }
    Navigator.of(context).pop(_resultadosOriginais);
  }

  void _cancelar() {
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
          builder: (ctx, _) => Transform.translate(
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
            'Serão sorteados até $totalVagas $labelMembroPlural para $qtdEquipes $labelEquipePlural.',
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

// ═══════════════════════════════════════════════════════════════════════════
// MODO GRUPO-EQUIPE — Sorteio de equipes pre-formadas para embarcações
// ═══════════════════════════════════════════════════════════════════════════

class _SorteioGrupoEquipeScreen extends StatefulWidget {
  const _SorteioGrupoEquipeScreen();

  @override
  State<_SorteioGrupoEquipeScreen> createState() => _SorteioGrupoEquipeScreenState();
}

class _SorteioGrupoEquipeScreenState extends State<_SorteioGrupoEquipeScreen> {
  final _api = ApiService();
  bool _carregando = true;
  bool _processando = false;
  String? _erro;
  List<SorteioGrupo> _resultado = const [];
  _SorteioGrupoPreCondicoes? _pre;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null) return;
    setState(() { _carregando = true; _erro = null; });
    try {
      final results = await Future.wait([
        _api.get(ApiConstants.sorteioGrupo(auth!.slug!), token: auth.token),
        _api.get(ApiConstants.sorteioGrupoPreCondicoes(auth.slug!), token: auth.token),
      ]);
      final lista = (results[0] as List<dynamic>)
          .map((e) => SorteioGrupo.fromJson(e as Map<String, dynamic>))
          .toList();
      final pre = _SorteioGrupoPreCondicoes.fromJson(results[1] as Map<String, dynamic>);
      setState(() { _resultado = lista; _pre = pre; });
    } on ApiException catch (e) {
      setState(() => _erro = e.message);
    } catch (_) {
      setState(() => _erro = 'Erro ao carregar dados.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _sortear() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null) return;
    final config = context.read<ConfigProvider>().config;

    if (!mounted) return;
    final selecao = await showDialog<_SelecaoGrupoEquipe>(
      context: context,
      barrierDismissible: false,
      builder: (_) => _SelecionarGruposEquipesDialog(
        slug: auth!.slug!,
        token: auth.token,
        labelEquipe: config?.labelEquipe ?? 'Equipe',
        labelEquipePlural: config?.labelEquipePlural ?? 'Equipes',
      ),
    );
    if (selecao == null || !mounted) return;

    setState(() => _processando = true);

    try {
      final filtro = {
        'grupoIds': selecao.grupos.map((g) => g.id).toList(),
        'equipeIds': selecao.equipes.map((e) => e.id).toList(),
      };

      final data = await _api.post(ApiConstants.sorteioGrupo(auth!.slug!), filtro, token: auth.token);
      final resultado = (data as List<dynamic>)
          .map((e) => SorteioGrupo.fromJson(e as Map<String, dynamic>))
          .toList();

      if (!mounted) return;

      // Mostra diálogo de confirmação com preview
      final confirmar = await showDialog<bool>(
        context: context,
        barrierDismissible: false,
        builder: (_) => _SorteioGrupoPreviewDialog(
          resultado: resultado,
          labelEquipe: config?.labelEquipe ?? 'Equipe',
        ),
      );

      if (confirmar != true || !mounted) {
        setState(() => _processando = false);
        return;
      }

      // Confirmar e salvar
      final payload = resultado.map((r) => {
        'grupoId': r.grupoId,
        'nomeGrupo': r.nomeGrupo,
        'equipeId': r.equipeId,
        'nomeEquipe': r.nomeEquipe,
        'posicao': r.posicao,
      }).toList();

      await _api.post(ApiConstants.sorteioGrupoConfirmar(auth.slug!), payload, token: auth.token);
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
        const SnackBar(content: Text('Erro ao realizar sorteio.'), backgroundColor: Colors.red),
      );
    }
  }

  Future<void> _limpar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null) return;
    final conf = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Limpar sorteio'),
        content: const Text('Deseja remover o resultado atual?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Limpar')),
        ],
      ),
    );
    if (conf != true) return;
    setState(() => _processando = true);
    try {
      await _api.delete(ApiConstants.sorteioGrupo(auth!.slug!), token: auth.token);
      if (!mounted) return;
      setState(() => _resultado = const []);
      await _carregar();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Sorteio limpo.')));
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.message), backgroundColor: Colors.red));
    } finally {
      if (mounted) setState(() => _processando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelEquipePlural = config?.labelEquipePlural ?? 'Equipes';
    final pre = _pre;
    final podeSortear = !_processando && (pre == null || pre.valido) && _resultado.isEmpty;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Sorteio de Grupos'),
        actions: [
          if (kDebugMode && _resultado.isEmpty)
            IconButton(
              onPressed: _processando ? null : _sortear,
              icon: const Icon(Icons.bug_report_outlined),
              tooltip: 'Simular sorteio (DEV)',
            ),
          if (_resultado.isNotEmpty)
            TextButton.icon(
              onPressed: _processando ? null : _limpar,
              icon: const Icon(Icons.delete_outline, color: Colors.white),
              label: const Text('Limpar', style: TextStyle(color: Colors.white)),
            ),
          if (_resultado.isEmpty)
            TextButton.icon(
              onPressed: podeSortear ? _sortear : null,
              icon: const Icon(Icons.shuffle, color: Colors.white),
              label: const Text('Sortear', style: TextStyle(color: Colors.white)),
            ),
        ],
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : _erro != null
              ? Center(child: Text(_erro!, style: const TextStyle(color: Colors.red)))
              : RefreshIndicator(
                  onRefresh: _carregar,
                  child: SingleChildScrollView(
                    physics: const AlwaysScrollableScrollPhysics(),
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Pré-condições
                        if (pre != null && !pre.valido) ...[
                          Container(
                            padding: const EdgeInsets.all(16),
                            decoration: BoxDecoration(
                              color: Colors.orange.shade50,
                              border: Border.all(color: Colors.orange.shade300),
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                              Icon(Icons.warning_amber_rounded, color: Colors.orange.shade700),
                              const SizedBox(width: 8),
                              Expanded(child: Text(pre.mensagemErro ?? '', style: TextStyle(color: Colors.orange.shade900))),
                            ]),
                          ),
                          const SizedBox(height: 16),
                        ],

                        // Resultado
                        if (_resultado.isNotEmpty) ...[
                          Text('${_resultado.length} grupo${_resultado.length != 1 ? 's' : ''} sorteados',
                              style: const TextStyle(fontWeight: FontWeight.w600, color: Colors.green)),
                          const SizedBox(height: 12),
                          ..._resultado.map((r) => _SorteioGrupoCard(item: r, labelEquipe: config?.labelEquipe ?? 'Equipe')),
                        ] else ...[
                          // Estado vazio
                          const SizedBox(height: 32),
                          const Center(child: Icon(Icons.shuffle, size: 64, color: Colors.grey)),
                          const SizedBox(height: 16),
                          const Center(child: Text('Nenhum sorteio realizado ainda.', textAlign: TextAlign.center)),
                          if (pre != null && pre.valido) ...[
                            const SizedBox(height: 8),
                            Center(child: Text(
                              '${pre.qtdGrupos} grupos · ${pre.qtdEquipes} $labelEquipePlural',
                              textAlign: TextAlign.center,
                              style: const TextStyle(color: Colors.grey, fontSize: 13),
                            )),
                          ],
                          const SizedBox(height: 24),
                          Center(child: FilledButton.icon(
                            onPressed: podeSortear ? _sortear : null,
                            icon: const Icon(Icons.shuffle),
                            label: Text(_processando ? 'Processando...' : 'Realizar Sorteio'),
                          )),
                        ],
                      ],
                    ),
                  ),
                ),
    );
  }
}

class _SorteioGrupoPreCondicoes {
  final int qtdGrupos;
  final int qtdEquipes;
  final bool valido;
  final String? mensagemErro;

  const _SorteioGrupoPreCondicoes({required this.qtdGrupos, required this.qtdEquipes, required this.valido, this.mensagemErro});

  factory _SorteioGrupoPreCondicoes.fromJson(Map<String, dynamic> json) => _SorteioGrupoPreCondicoes(
    qtdGrupos: json['qtdGrupos'] as int? ?? 0,
    qtdEquipes: json['qtdEquipes'] as int? ?? 0,
    valido: json['valido'] as bool? ?? false,
    mensagemErro: json['mensagemErro'] as String?,
  );
}

class _SelecaoGrupoEquipe {
  final List<Grupo> grupos;
  final List<Equipe> equipes;
  const _SelecaoGrupoEquipe({required this.grupos, required this.equipes});
}

// ── Card do resultado ─────────────────────────────────────────────────────

class _SorteioGrupoCard extends StatelessWidget {
  final SorteioGrupo item;
  final String labelEquipe;
  const _SorteioGrupoCard({required this.item, required this.labelEquipe});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
        side: const BorderSide(color: Color(0xFFA6E3A1), width: 2),
      ),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(color: Colors.grey.shade200, borderRadius: BorderRadius.circular(4)),
                child: Text('#${item.posicao}', style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
              ),
              const SizedBox(width: 8),
              Expanded(child: Text(item.nomeGrupo, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15))),
            ]),
            const SizedBox(height: 4),
            Row(children: [
              const Icon(Icons.arrow_forward, size: 16, color: Colors.green),
              const SizedBox(width: 4),
              Text(item.nomeEquipe, style: TextStyle(color: Colors.orange.shade700, fontWeight: FontWeight.w600)),
            ]),
            if (item.nomesMembros.isNotEmpty) ...[
              const SizedBox(height: 4),
              Text(item.nomesMembros.join(', '), style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
            ],
          ],
        ),
      ),
    );
  }
}

// ── Diálogo de seleção de Grupos e Equipes ───────────────────────────────

class _SelecionarGruposEquipesDialog extends StatefulWidget {
  final String slug;
  final String token;
  final String labelEquipe;
  final String labelEquipePlural;

  const _SelecionarGruposEquipesDialog({
    required this.slug,
    required this.token,
    required this.labelEquipe,
    required this.labelEquipePlural,
  });

  @override
  State<_SelecionarGruposEquipesDialog> createState() => _SelecionarGruposEquipesDialogState();
}

class _SelecionarGruposEquipesDialogState extends State<_SelecionarGruposEquipesDialog> {
  final _api = ApiService();
  List<Grupo> _grupos = [];
  List<Equipe> _equipes = [];
  final Set<String> _gruposSel = {};
  final Set<String> _equipesSel = {};
  bool _carregando = true;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    try {
      final results = await Future.wait([
        _api.get(ApiConstants.grupos(widget.slug), token: widget.token),
        _api.get(ApiConstants.equipes(widget.slug), token: widget.token),
      ]);
      final grupos = (results[0] as List<dynamic>).map((j) => Grupo.fromJson(j as Map<String, dynamic>)).toList()
        ..sort((a, b) => a.nome.compareTo(b.nome));
      final equipes = (results[1] as List<dynamic>).map((j) => Equipe.fromJson(j as Map<String, dynamic>)).toList()
        ..sort((a, b) => a.nome.compareTo(b.nome));
      setState(() {
        _grupos = grupos;
        _equipes = equipes;
        _gruposSel.addAll(grupos.map((g) => g.id));
        _equipesSel.addAll(equipes.map((e) => e.id));
        _carregando = false;
      });
    } catch (_) {
      setState(() => _carregando = false);
    }
  }

  String? get _erroValidacao {
    final qtdG = _gruposSel.length;
    final qtdE = _equipesSel.length;
    if (qtdG < 2) return 'Selecione pelo menos 2 grupos.';
    if (qtdE < 2) return 'Selecione pelo menos 2 ${widget.labelEquipePlural.toLowerCase()}.';
    if (qtdG != qtdE) {
      final diff = qtdG - qtdE;
      if (diff > 0) return '$diff grupo${diff != 1 ? 's' : ''} a mais do que ${widget.labelEquipePlural.toLowerCase()}s.';
      return '${-diff} ${widget.labelEquipe.toLowerCase()}${-diff != 1 ? 's' : ''} a mais do que grupos.';
    }
    return null;
  }

  void _confirmar() {
    final grupos = _grupos.where((g) => _gruposSel.contains(g.id)).toList();
    final equipes = _equipes.where((e) => _equipesSel.contains(e.id)).toList();
    Navigator.pop(context, _SelecaoGrupoEquipe(grupos: grupos, equipes: equipes));
  }

  @override
  Widget build(BuildContext context) {
    final erro = _erroValidacao;
    return Dialog(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Selecionar participantes', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
            const SizedBox(height: 12),
            if (_carregando)
              const CircularProgressIndicator()
            else
              ConstrainedBox(
                constraints: const BoxConstraints(maxHeight: 420),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Grupos
                    Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                      Row(children: [
                        const Text('Grupos', style: TextStyle(fontWeight: FontWeight.w600)),
                        const Spacer(),
                        GestureDetector(onTap: () => setState(() => _gruposSel..addAll(_grupos.map((g) => g.id))), child: const Text('Todos', style: TextStyle(fontSize: 11, color: Colors.blue))),
                        const Text(' · ', style: TextStyle(fontSize: 11)),
                        GestureDetector(onTap: () => setState(() => _gruposSel.clear()), child: const Text('Nenhum', style: TextStyle(fontSize: 11, color: Colors.blue))),
                      ]),
                      const SizedBox(height: 4),
                      Expanded(child: SingleChildScrollView(child: Column(children: _grupos.map((g) => CheckboxListTile(
                        dense: true,
                        title: Text(g.nome, style: const TextStyle(fontSize: 13)),
                        subtitle: Text('${g.membros.length} membro${g.membros.length != 1 ? 's' : ''}', style: const TextStyle(fontSize: 11)),
                        value: _gruposSel.contains(g.id),
                        onChanged: (v) => setState(() => v! ? _gruposSel.add(g.id) : _gruposSel.remove(g.id)),
                      )).toList()))),
                    ])),
                    const VerticalDivider(),
                    // Equipes
                    Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                      Row(children: [
                        Text(widget.labelEquipePlural, style: const TextStyle(fontWeight: FontWeight.w600)),
                        const Spacer(),
                        GestureDetector(onTap: () => setState(() => _equipesSel..addAll(_equipes.map((e) => e.id))), child: const Text('Todas', style: TextStyle(fontSize: 11, color: Colors.blue))),
                        const Text(' · ', style: TextStyle(fontSize: 11)),
                        GestureDetector(onTap: () => setState(() => _equipesSel.clear()), child: const Text('Nenhuma', style: TextStyle(fontSize: 11, color: Colors.blue))),
                      ]),
                      const SizedBox(height: 4),
                      Expanded(child: SingleChildScrollView(child: Column(children: _equipes.map((e) => CheckboxListTile(
                        dense: true,
                        title: Text(e.nome, style: const TextStyle(fontSize: 13)),
                        value: _equipesSel.contains(e.id),
                        onChanged: (v) => setState(() => v! ? _equipesSel.add(e.id) : _equipesSel.remove(e.id)),
                      )).toList()))),
                    ])),
                  ],
                ),
              ),
            const SizedBox(height: 8),
            if (!_carregando) ...[
              if (erro != null)
                Text(erro, style: const TextStyle(color: Colors.red, fontSize: 12), textAlign: TextAlign.center)
              else
                Text(
                  '${_gruposSel.length} grupos · ${_equipesSel.length} ${widget.labelEquipePlural.toLowerCase()} — pronto.',
                  style: const TextStyle(color: Colors.green, fontSize: 12),
                  textAlign: TextAlign.center,
                ),
              const SizedBox(height: 8),
              Row(mainAxisAlignment: MainAxisAlignment.end, children: [
                TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancelar')),
                const SizedBox(width: 8),
                FilledButton.icon(
                  onPressed: erro == null ? _confirmar : null,
                  icon: const Icon(Icons.shuffle, size: 16),
                  label: const Text('Sortear'),
                ),
              ]),
            ],
          ],
        ),
      ),
    );
  }
}

// ── Diálogo de preview/confirmação do resultado ───────────────────────────

class _SorteioGrupoPreviewDialog extends StatelessWidget {
  final List<SorteioGrupo> resultado;
  final String labelEquipe;

  const _SorteioGrupoPreviewDialog({required this.resultado, required this.labelEquipe});

  @override
  Widget build(BuildContext context) {
    return Dialog(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Resultado do sorteio', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
            const SizedBox(height: 4),
            const Text('Confirme para salvar.', style: TextStyle(fontSize: 12, color: Colors.grey)),
            const SizedBox(height: 12),
            ConstrainedBox(
              constraints: const BoxConstraints(maxHeight: 350),
              child: SingleChildScrollView(
                child: Column(
                  children: resultado.map((r) => ListTile(
                    dense: true,
                    leading: CircleAvatar(radius: 14, child: Text('${r.posicao}', style: const TextStyle(fontSize: 11))),
                    title: Text(r.nomeGrupo, style: const TextStyle(fontWeight: FontWeight.w600)),
                    subtitle: Row(children: [
                      const Icon(Icons.arrow_forward, size: 14, color: Colors.green),
                      const SizedBox(width: 4),
                      Text(r.nomeEquipe, style: TextStyle(color: Colors.orange.shade700)),
                    ]),
                  )).toList(),
                ),
              ),
            ),
            const SizedBox(height: 12),
            Row(mainAxisAlignment: MainAxisAlignment.end, children: [
              TextButton(
                onPressed: () => Navigator.pop(context, false),
                child: const Text('Sortear novamente'),
              ),
              const SizedBox(width: 8),
              FilledButton.icon(
                onPressed: () => Navigator.pop(context, true),
                icon: const Icon(Icons.check, size: 16),
                label: const Text('Confirmar'),
              ),
            ]),
          ],
        ),
      ),
    );
  }
}
