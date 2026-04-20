import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/grupo.dart';
import '../../core/models/membro.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class GruposAdminScreen extends StatefulWidget {
  const GruposAdminScreen({super.key});

  @override
  State<GruposAdminScreen> createState() => _GruposAdminScreenState();
}

class _GruposAdminScreenState extends State<GruposAdminScreen> {
  final _api = ApiService();
  List<Grupo> _grupos = [];
  bool _carregando = true;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    setState(() { _carregando = true; _erro = null; });
    try {
      final auth = context.read<AuthProvider>().usuario;
      if (auth?.slug == null) return;
      final data = await _api.get(ApiConstants.grupos(auth!.slug!), token: auth.token) as List<dynamic>;
      setState(() { _grupos = data.map((j) => Grupo.fromJson(j as Map<String, dynamic>)).toList()..sort((a, b) => a.nome.compareTo(b.nome)); });
    } catch (e) {
      setState(() => _erro = e.toString());
    } finally {
      setState(() => _carregando = false);
    }
  }

  Future<void> _criarGrupo() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null) return;
    final ctrl = TextEditingController();
    final nome = await showDialog<String>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Novo grupo'),
        content: TextField(controller: ctrl, decoration: const InputDecoration(labelText: 'Nome do grupo'), autofocus: true),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(context, ctrl.text.trim()), child: const Text('Criar')),
        ],
      ),
    );
    if (nome == null || nome.isEmpty || !mounted) return;
    try {
      await _api.post(ApiConstants.grupos(auth!.slug!), {'nome': nome}, token: auth.token);
      await _carregar();
    } catch (e) {
      if (mounted) _mostrarErro(e.toString());
    }
  }

  Future<void> _removerGrupo(Grupo grupo) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null) return;
    final conf = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Remover grupo'),
        content: Text('Remover "${grupo.nome}"?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Remover'),
          ),
        ],
      ),
    );
    if (conf != true || !mounted) return;
    try {
      await _api.delete('${ApiConstants.grupos(auth!.slug!)}/${grupo.id}', token: auth.token);
      await _carregar();
    } catch (e) {
      if (mounted) _mostrarErro(e.toString());
    }
  }

  void _abrirDetalhes(Grupo grupo) async {
    await Navigator.push(context, MaterialPageRoute(builder: (_) => _GrupoDetalhesScreen(grupo: grupo)));
    _carregar();
  }

  void _mostrarErro(String msg) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(msg), backgroundColor: Colors.red));
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelEquipe = config?.labelEquipe ?? 'Equipe';

    return Scaffold(
      appBar: AppBar(
        title: const Text('Grupos'),
        actions: [
          IconButton(icon: const Icon(Icons.refresh), onPressed: _carregar),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _criarGrupo,
        tooltip: 'Novo grupo',
        child: const Icon(Icons.add),
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : _erro != null
              ? Center(child: Text(_erro!, style: const TextStyle(color: Colors.red)))
              : _grupos.isEmpty
                  ? Center(
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.people_outline, size: 64, color: Colors.grey[400]),
                          const SizedBox(height: 12),
                          Text('Nenhum grupo cadastrado', style: TextStyle(color: Colors.grey[600])),
                          const SizedBox(height: 8),
                          Text(
                            'Grupos são equipes pre-formadas.\nCada grupo sorteia uma ${labelEquipe.toLowerCase()}.',
                            textAlign: TextAlign.center,
                            style: TextStyle(color: Colors.grey[500], fontSize: 13),
                          ),
                        ],
                      ),
                    )
                  : RefreshIndicator(
                      onRefresh: _carregar,
                      child: ListView.separated(
                        padding: const EdgeInsets.all(12),
                        itemCount: _grupos.length,
                        separatorBuilder: (context, index) => const SizedBox(height: 8),
                        itemBuilder: (_, i) {
                          final g = _grupos[i];
                          return Card(
                            child: ListTile(
                              leading: CircleAvatar(
                                backgroundColor: Colors.deepOrange.withAlpha(30),
                                child: Text('${i + 1}', style: const TextStyle(color: Colors.deepOrange, fontWeight: FontWeight.bold)),
                              ),
                              title: Text(g.nome, style: const TextStyle(fontWeight: FontWeight.w600)),
                              subtitle: Text('${g.membros.length} membro${g.membros.length != 1 ? 's' : ''}'),
                              trailing: Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  IconButton(icon: const Icon(Icons.edit_outlined, size: 20), onPressed: () => _abrirDetalhes(g)),
                                  IconButton(icon: const Icon(Icons.delete_outline, size: 20, color: Colors.red), onPressed: () => _removerGrupo(g)),
                                ],
                              ),
                              onTap: () => _abrirDetalhes(g),
                            ),
                          );
                        },
                      ),
                    ),
    );
  }
}

// ── Tela de detalhes de um grupo ─────────────────────────────────────────────

class _GrupoDetalhesScreen extends StatefulWidget {
  final Grupo grupo;
  const _GrupoDetalhesScreen({required this.grupo});

  @override
  State<_GrupoDetalhesScreen> createState() => _GrupoDetalhesScreenState();
}

class _GrupoDetalhesScreenState extends State<_GrupoDetalhesScreen> {
  final _api = ApiService();
  late Grupo _grupo;
  List<Membro> _membrosDisponiveis = [];
  bool _carregando = true;

  @override
  void initState() {
    super.initState();
    _grupo = widget.grupo;
    _carregar();
  }

  Future<void> _carregar() async {
    setState(() => _carregando = true);
    try {
      final auth = context.read<AuthProvider>().usuario!;
      final slug = auth.slug!;
      final results = await Future.wait([
        _api.get('${ApiConstants.grupos(slug)}/${_grupo.id}', token: auth.token),
        _api.get(ApiConstants.membros(slug), token: auth.token),
      ]);

      final grupoJson = results[0] as Map<String, dynamic>;
      final todosMembros = (results[1] as List<dynamic>)
          .map((j) => Membro.fromJson(j as Map<String, dynamic>))
          .toList();

      final grupoAtual = Grupo.fromJson(grupoJson);
      final idsNoGrupo = grupoAtual.membros.map((m) => m.membroId).toSet();
      setState(() {
        _grupo = grupoAtual;
        _membrosDisponiveis = todosMembros
            .where((m) => !idsNoGrupo.contains(m.id))
            .toList()
          ..sort((a, b) => a.nome.compareTo(b.nome));
        _carregando = false;
      });
    } catch (e) {
      setState(() => _carregando = false);
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString()), backgroundColor: Colors.red));
    }
  }

  Future<void> _adicionarMembro() async {
    if (_membrosDisponiveis.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Todos os membros já estão em grupos.')));
      return;
    }

    Membro? selecionado;
    await showDialog(
      context: context,
      builder: (_) => StatefulBuilder(builder: (ctx, ss) => AlertDialog(
        title: const Text('Adicionar membro'),
        content: SizedBox(
          width: double.maxFinite,
          height: 300,
          child: ListView.builder(
            itemCount: _membrosDisponiveis.length,
            itemBuilder: (_, i) => ListTile(
              title: Text(_membrosDisponiveis[i].nome),
              selected: selecionado?.id == _membrosDisponiveis[i].id,
              onTap: () { ss(() => selecionado = _membrosDisponiveis[i]); },
            ),
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Cancelar')),
          FilledButton(
            onPressed: selecionado == null ? null : () => Navigator.pop(ctx),
            child: const Text('Adicionar'),
          ),
        ],
      )),
    );

    if (selecionado == null || !mounted) return;
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null) return;
    try {
      await _api.post(
        ApiConstants.grupoMembros(auth!.slug!, _grupo.id),
        {'membroId': selecionado!.id},
        token: auth.token,
      );
      await _carregar();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString()), backgroundColor: Colors.red));
    }
  }

  Future<void> _removerMembro(GrupoMembroItem m) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null) return;
    final conf = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Remover membro'),
        content: Text('Remover "${m.nomeMembro}" do grupo?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Remover'),
          ),
        ],
      ),
    );
    if (conf != true || !mounted) return;
    try {
      await _api.delete('${ApiConstants.grupoMembros(auth!.slug!, _grupo.id)}/${m.id}', token: auth.token);
      await _carregar();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString()), backgroundColor: Colors.red));
    }
  }

  Future<void> _renomearGrupo() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null) return;
    final ctrl = TextEditingController(text: _grupo.nome);
    final nome = await showDialog<String>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Renomear grupo'),
        content: TextField(controller: ctrl, decoration: const InputDecoration(labelText: 'Novo nome'), autofocus: true),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(context, ctrl.text.trim()), child: const Text('Salvar')),
        ],
      ),
    );
    if (nome == null || nome.isEmpty || !mounted) return;
    try {
      await _api.put('${ApiConstants.grupos(auth!.slug!)}/${_grupo.id}', {'nome': nome}, token: auth.token);
      await _carregar();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString()), backgroundColor: Colors.red));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(_grupo.nome),
        actions: [
          IconButton(icon: const Icon(Icons.edit_outlined), tooltip: 'Renomear', onPressed: _renomearGrupo),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _adicionarMembro,
        tooltip: 'Adicionar membro',
        child: const Icon(Icons.person_add),
      ),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : _grupo.membros.isEmpty
              ? const Center(child: Text('Nenhum membro neste grupo.'))
              : ListView.separated(
                  padding: const EdgeInsets.all(12),
                  itemCount: _grupo.membros.length,
                  separatorBuilder: (context, index) => const Divider(height: 1),
                  itemBuilder: (_, i) {
                    final m = _grupo.membros[i];
                    return ListTile(
                      leading: CircleAvatar(child: Text(m.nomeMembro.isNotEmpty ? m.nomeMembro[0].toUpperCase() : '?')),
                      title: Text(m.nomeMembro),
                      trailing: IconButton(
                        icon: const Icon(Icons.remove_circle_outline, color: Colors.red),
                        onPressed: () => _removerMembro(m),
                      ),
                    );
                  },
                ),
    );
  }
}
