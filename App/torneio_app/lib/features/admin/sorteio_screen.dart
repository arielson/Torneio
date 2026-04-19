import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/sorteio_equipe.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

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

    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final results = await Future.wait([
        _api.get(ApiConstants.sorteio(auth!.slug!), token: auth.token),
        _api.get(ApiConstants.sorteioPreCondicoes(auth.slug!), token: auth.token),
      ]);

      final lista = results[0] is List
          ? (results[0] as List)
              .map((e) => SorteioEquipe.fromJson(e as Map<String, dynamic>))
              .toList()
          : <SorteioEquipe>[];

      final pre = _PreCondicoes.fromJson(results[1] as Map<String, dynamic>);

      setState(() {
        _resultado = lista..sort((a, b) => a.posicao.compareTo(b.posicao));
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

    setState(() => _processando = true);

    try {
      final data = await _api.post(ApiConstants.sorteio(auth!.slug!), {}, token: auth.token);
      final lista = data is List
          ? data
              .map((e) => SorteioEquipe.fromJson(e as Map<String, dynamic>))
              .toList()
          : <SorteioEquipe>[];
      if (!mounted) return;
      setState(() {
        _resultado = lista..sort((a, b) => a.posicao.compareTo(b.posicao));
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Sorteio realizado com sucesso.')),
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

  Future<void> _limpar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Limpar sorteio'),
        content: const Text('Deseja remover o resultado atual do sorteio?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Limpar'),
          ),
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

  Future<void> _ajustarPosicao(SorteioEquipe item) async {
    final auth = context.read<AuthProvider>().usuario;
    final messenger = ScaffoldMessenger.of(context);
    if (auth?.slug == null || auth?.token == null) return;

    final controller = TextEditingController(text: item.posicao.toString());
    final novaPosicao = await showDialog<int>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Ajustar posição'),
        content: TextField(
          controller: controller,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(
            labelText: 'Nova posição',
            border: OutlineInputBorder(),
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, int.tryParse(controller.text)),
            child: const Text('Salvar'),
          ),
        ],
      ),
    );
    controller.dispose();

    if (novaPosicao == null || novaPosicao <= 0) return;

    setState(() => _processando = true);

    try {
      await _api.put(
        '${ApiConstants.sorteio(auth!.slug!)}/${item.id}/posicao',
        {'posicao': novaPosicao},
        token: auth.token,
      );
      if (!mounted) return;
      await _carregar();
      messenger.showSnackBar(
        const SnackBar(content: Text('Posição ajustada com sucesso.')),
      );
    } on ApiException catch (e) {
      if (!mounted) return;
      messenger.showSnackBar(
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
            child: Text('Este torneio não utiliza sorteio.'),
          ),
        ),
      );
    }

    final labelEquipePlural = config?.labelEquipePlural ?? 'Equipes';
    final labelMembroPlural = config?.labelMembroPlural ?? 'Membros';
    final labelMembro = config?.labelMembro ?? 'Membro';
    final pre = _preCondicoes;
    final podeSortear = !_processando && (pre == null || pre.valido) && _resultado.isEmpty;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Sorteio'),
        actions: [
          if (_resultado.isEmpty)
            TextButton.icon(
              onPressed: podeSortear ? _sortear : null,
              icon: const Icon(Icons.shuffle, color: Colors.white),
              label: const Text('Sortear', style: TextStyle(color: Colors.white)),
            ),
          if (_resultado.isNotEmpty)
            TextButton.icon(
              onPressed: _processando ? null : _limpar,
              icon: const Icon(Icons.delete_outline, color: Colors.white),
              label: const Text('Limpar', style: TextStyle(color: Colors.white)),
            ),
        ],
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: _erro != null
                  ? ListView(
                      children: [
                        Padding(
                          padding: const EdgeInsets.all(24),
                          child: Text(_erro!, textAlign: TextAlign.center),
                        ),
                      ],
                    )
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
                            onSortear: _sortear,
                          )
                        else
                          ..._resultado.map((item) => _CardResultado(
                                item: item,
                                labelMembro: labelMembro,
                                processando: _processando,
                                onAjustar: () => _ajustarPosicao(item),
                              )),
                      ],
                    ),
            ),
    );
  }
}

class _AlertaPreCondicoes extends StatelessWidget {
  final _PreCondicoes pre;
  final String labelEquipePlural;
  final String labelMembroPlural;

  const _AlertaPreCondicoes({
    required this.pre,
    required this.labelEquipePlural,
    required this.labelMembroPlural,
  });

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
          Row(
            children: [
              Icon(Icons.warning_amber_rounded, color: Colors.orange.shade700, size: 20),
              const SizedBox(width: 8),
              Text(
                'Sorteio indisponível',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: Colors.orange.shade800,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            pre.mensagemErro ?? '',
            style: TextStyle(color: Colors.orange.shade900),
          ),
          const SizedBox(height: 8),
          DefaultTextStyle(
            style: TextStyle(color: Colors.orange.shade700, fontSize: 12),
            child: Row(
              children: [
                Text('$labelEquipePlural: ${pre.qtdEquipes}'),
                const Text('  ·  '),
                Text('Vagas: ${pre.totalVagas}'),
                const Text('  ·  '),
                Text('$labelMembroPlural: ${pre.qtdMembros}'),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _EstadoVazio extends StatelessWidget {
  final bool podeSortear;
  final bool processando;
  final VoidCallback onSortear;

  const _EstadoVazio({
    required this.podeSortear,
    required this.processando,
    required this.onSortear,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        const SizedBox(height: 32),
        const Icon(Icons.shuffle, size: 64, color: Colors.grey),
        const SizedBox(height: 16),
        const Text(
          'Nenhum sorteio realizado ainda.',
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 16),
        FilledButton.icon(
          onPressed: podeSortear ? onSortear : null,
          icon: const Icon(Icons.shuffle),
          label: Text(processando ? 'Processando...' : 'Realizar sorteio'),
        ),
      ],
    );
  }
}

class _CardResultado extends StatelessWidget {
  final SorteioEquipe item;
  final String labelMembro;
  final bool processando;
  final VoidCallback onAjustar;

  const _CardResultado({
    required this.item,
    required this.labelMembro,
    required this.processando,
    required this.onAjustar,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Card(
        child: ListTile(
          leading: CircleAvatar(child: Text(item.posicao.toString())),
          title: Text(item.nomeEquipe),
          subtitle: Text('$labelMembro: ${item.nomeMembro}'),
          trailing: TextButton.icon(
            onPressed: processando ? null : onAjustar,
            icon: const Icon(Icons.swap_vert),
            label: const Text('Posição'),
          ),
        ),
      ),
    );
  }
}
