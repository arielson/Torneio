import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/equipe.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';
import 'equipe_form_screen.dart';
import 'reorganizacao_emergencial_screen.dart';

class EquipesAdminScreen extends StatefulWidget {
  const EquipesAdminScreen({super.key});

  @override
  State<EquipesAdminScreen> createState() => _EquipesAdminScreenState();
}

class _EquipesAdminScreenState extends State<EquipesAdminScreen> {
  final ApiService _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<Equipe> _equipes = const [];

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
      final data = await _api.get(
        ApiConstants.equipes(auth!.slug!),
        token: auth.token,
      );

      final lista = data is List
          ? data.map((e) => Equipe.fromJson(e as Map<String, dynamic>)).toList()
          : <Equipe>[];

      setState(() {
        _equipes = lista..sort((a, b) => a.nome.compareTo(b.nome));
      });
    } on ApiException catch (e) {
      setState(() => _erro = e.message);
    } catch (_) {
      setState(() => _erro = 'Erro ao carregar dados.');
    } finally {
      if (mounted) {
        setState(() => _carregando = false);
      }
    }
  }

  Future<void> _abrirFormulario({Equipe? equipe}) async {
    await Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => EquipeFormScreen(equipe: equipe),
      ),
    );

    if (mounted) {
      await _carregar();
    }
  }

  Future<void> _abrirReorganizacaoEmergencial() async {
    await Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => const ReorganizacaoEmergencialScreen()),
    );

    if (mounted) {
      await _carregar();
    }
  }

  Future<void> _remover(Equipe equipe) async {
    final config = context.read<ConfigProvider>().config;
    final auth = context.read<AuthProvider>().usuario;
    if (config == null || auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: Text('Remover ${config.labelEquipe.toLowerCase()}'),
        content: Text('Deseja remover ${equipe.nome}?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Remover'),
          ),
        ],
      ),
    );

    if (confirmar != true) return;

    try {
      await _api.delete(
        '${ApiConstants.equipes(auth!.slug!)}/${equipe.id}',
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('${config.labelEquipe} removida com sucesso.')),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final label = config?.labelEquipe ?? 'Equipe';
    final labelPlural = config?.labelEquipePlural ?? 'Equipes';
    final labelMembro = config?.labelMembro ?? 'Membro';
    final exibirMembros = config?.modoSorteio != 'Nenhum';
    final exibirVagas = config?.modoSorteio != 'Nenhum';

    return Scaffold(
      appBar: AppBar(
        title: Text(labelPlural),
        actions: [
          if (exibirMembros)
            IconButton(
              onPressed: _abrirReorganizacaoEmergencial,
              tooltip: 'Reorganizacao emergencial',
              icon: const Icon(Icons.warning_amber_rounded),
            ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: Text('Nova $label'),
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
                          child: Text(
                            _erro!,
                            textAlign: TextAlign.center,
                          ),
                        ),
                      ],
                    )
                  : _equipes.isEmpty
                      ? ListView(
                          children: [
                            if (exibirMembros)
                              Container(
                                margin: const EdgeInsets.fromLTRB(16, 16, 16, 0),
                                padding: const EdgeInsets.all(12),
                                decoration: BoxDecoration(
                                  color: Colors.orange.shade50,
                                  borderRadius: BorderRadius.circular(12),
                                  border: Border.all(color: Colors.orange.shade200),
                                ),
                                child: const Text(
                                  'Reorganizacao de emergencia deve ser usada apenas em caso critico e exige confirmacao do administrador do torneio.',
                                ),
                              ),
                            Padding(
                              padding: const EdgeInsets.all(24),
                              child: Text(
                                'Nenhuma ${label.toLowerCase()} cadastrada.',
                                textAlign: TextAlign.center,
                              ),
                            ),
                          ],
                        )
                      : ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: _equipes.length,
                          separatorBuilder: (context, index) =>
                              const SizedBox(height: 12),
                          itemBuilder: (context, index) {
                            final equipe = _equipes[index];
                            return Card(
                              child: Padding(
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(
                                      equipe.nome,
                                      style: Theme.of(context).textTheme.titleMedium,
                                    ),
                                    const SizedBox(height: 6),
                                    Text('Capitão: ${equipe.capitao}'),
                                    if (exibirVagas) Text('Vagas: ${equipe.qtdVagas}'),
                                    if (exibirMembros)
                                      Text(
                                        '${config?.labelMembroPlural ?? '${labelMembro}s'}: ${equipe.qtdMembros}/${equipe.qtdVagas}',
                                      ),
                                    const SizedBox(height: 12),
                                    Row(
                                      mainAxisAlignment: MainAxisAlignment.end,
                                      children: [
                                        TextButton.icon(
                                          onPressed: () => _abrirFormulario(equipe: equipe),
                                          icon: const Icon(Icons.edit),
                                          label: const Text('Editar'),
                                        ),
                                        const SizedBox(width: 8),
                                        TextButton.icon(
                                          onPressed: () => _remover(equipe),
                                          icon: const Icon(Icons.delete_outline),
                                          label: const Text('Remover'),
                                        ),
                                      ],
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
            ),
    );
  }
}
