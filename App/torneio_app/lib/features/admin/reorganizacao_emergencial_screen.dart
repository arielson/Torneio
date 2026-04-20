import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/equipe.dart';
import '../../core/models/membro.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

class ReorganizacaoEmergencialScreen extends StatefulWidget {
  const ReorganizacaoEmergencialScreen({super.key});

  @override
  State<ReorganizacaoEmergencialScreen> createState() => _ReorganizacaoEmergencialScreenState();
}

class _ReorganizacaoEmergencialScreenState extends State<ReorganizacaoEmergencialScreen> {
  final ApiService _api = ApiService();
  final _formKey = GlobalKey<FormState>();
  final _motivoController = TextEditingController();
  final _confirmacaoController = TextEditingController();

  bool _carregando = true;
  bool _salvando = false;
  String? _erro;
  String? _membroId;
  String? _equipeDestinoId;
  List<Equipe> _equipes = const [];
  List<Membro> _membros = const [];

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
        _api.get(ApiConstants.equipes(auth!.slug!), token: auth.token),
        _api.get(ApiConstants.membros(auth.slug!), token: auth.token),
      ]);

      final equipes = (results[0] as List<dynamic>)
          .map((e) => Equipe.fromJson(e as Map<String, dynamic>))
          .toList()
        ..sort((a, b) => a.nome.compareTo(b.nome));
      final membros = (results[1] as List<dynamic>)
          .map((e) => Membro.fromJson(e as Map<String, dynamic>))
          .toList()
        ..sort((a, b) => a.nome.compareTo(b.nome));

      setState(() {
        _equipes = equipes;
        _membros = membros;
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

  String _origemDoMembro(String membroId, String fallback) {
    final origens = _equipes.where((e) => e.membroIds.contains(membroId)).map((e) => e.nome).toList();
    if (origens.isEmpty) return fallback;
    return '$fallback (${origens.join(", ")})';
  }

  Future<void> _confirmarESalvar() async {
    if (!_formKey.currentState!.validate()) return;

    final config = context.read<ConfigProvider>().config;
    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Confirmacao de emergencia'),
        content: Text(
          'Esta reorganizacao manual de ${config?.labelMembroPlural.toLowerCase() ?? "membros"} sera registrada no log. Deseja continuar?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Confirmar'),
          ),
        ],
      ),
    );

    if (confirmar == true) {
      await _salvar();
    }
  }

  Future<void> _salvar() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    setState(() => _salvando = true);

    try {
      await _api.post(
        ApiConstants.reorganizacaoEmergencialEquipe(auth!.slug!),
        {
          'membroId': _membroId,
          'equipeDestinoId': _equipeDestinoId,
          'motivo': _motivoController.text.trim(),
          'confirmacao': _confirmacaoController.text.trim(),
        },
        token: auth.token,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Reorganizacao emergencial registrada com sucesso.'),
          backgroundColor: Colors.green,
        ),
      );
      Navigator.pop(context, true);
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    } catch (_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Erro ao registrar reorganizacao.'), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) {
        setState(() => _salvando = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final labelEquipe = config?.labelEquipe ?? 'Embarcacao';
    final labelMembro = config?.labelMembro ?? 'Pescador';

    return Scaffold(
      appBar: AppBar(title: const Text('Reorganizacao Emergencial')),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: Colors.red.shade50,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.red.shade200),
                    ),
                    child: const Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Icon(Icons.warning_amber_rounded, color: Colors.red),
                            SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                'Medida de emergencia',
                                style: TextStyle(fontWeight: FontWeight.w700, color: Colors.red),
                              ),
                            ),
                          ],
                        ),
                        SizedBox(height: 8),
                        Text(
                          'Use apenas quando houver problema operacional com uma embarcacao. A alteracao sera registrada em log com o motivo informado e confirmacao do administrador.',
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                  if (_erro != null)
                    Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: Text(_erro!, style: const TextStyle(color: Colors.red)),
                    ),
                  Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        DropdownButtonFormField<String>(
                          initialValue: _membroId,
                          decoration: InputDecoration(
                            labelText: labelMembro,
                            border: const OutlineInputBorder(),
                          ),
                          items: _membros
                              .map(
                                (m) => DropdownMenuItem(
                                  value: m.id,
                                  child: Text(_origemDoMembro(m.id, m.nome)),
                                ),
                              )
                              .toList(),
                          onChanged: (value) => setState(() => _membroId = value),
                          validator: (value) => value == null ? 'Selecione um $labelMembro' : null,
                        ),
                        const SizedBox(height: 16),
                        DropdownButtonFormField<String>(
                          initialValue: _equipeDestinoId,
                          decoration: InputDecoration(
                            labelText: '$labelEquipe de destino',
                            border: const OutlineInputBorder(),
                          ),
                          items: _equipes
                              .map(
                                (e) => DropdownMenuItem(
                                  value: e.id,
                                  child: Text('${e.nome} (${e.qtdMembros}/${e.qtdVagas})'),
                                ),
                              )
                              .toList(),
                          onChanged: (value) => setState(() => _equipeDestinoId = value),
                          validator: (value) => value == null ? 'Selecione a $labelEquipe de destino' : null,
                        ),
                        const SizedBox(height: 16),
                        TextFormField(
                          controller: _motivoController,
                          minLines: 3,
                          maxLines: 5,
                          decoration: const InputDecoration(
                            labelText: 'Motivo da emergencia',
                            border: OutlineInputBorder(),
                          ),
                          validator: (value) {
                            if (value == null || value.trim().isEmpty) {
                              return 'Informe o motivo da reorganizacao.';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 16),
                        TextFormField(
                          controller: _confirmacaoController,
                          decoration: const InputDecoration(
                            labelText: 'Confirmacao',
                            hintText: 'Digite REORGANIZAR',
                            border: OutlineInputBorder(),
                          ),
                          validator: (value) {
                            if ((value ?? '').trim().toUpperCase() != 'REORGANIZAR') {
                              return 'Digite REORGANIZAR para confirmar.';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 20),
                        FilledButton.icon(
                          onPressed: _salvando ? null : _confirmarESalvar,
                          style: FilledButton.styleFrom(backgroundColor: Colors.red),
                          icon: const Icon(Icons.warning_amber_rounded),
                          label: Text(_salvando ? 'Registrando...' : 'Executar reorganizacao emergencial'),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
    );
  }

  @override
  void dispose() {
    _motivoController.dispose();
    _confirmacaoController.dispose();
    super.dispose();
  }
}
