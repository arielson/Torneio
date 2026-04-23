import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/patrocinador.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';
import '../../widgets/expandable_network_image.dart';
import 'patrocinador_form_screen.dart';

class PatrocinadoresAdminScreen extends StatefulWidget {
  const PatrocinadoresAdminScreen({super.key});

  @override
  State<PatrocinadoresAdminScreen> createState() => _PatrocinadoresAdminScreenState();
}

class _PatrocinadoresAdminScreenState extends State<PatrocinadoresAdminScreen> {
  final ApiService _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<Patrocinador> _patrocinadores = const [];

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
      final data = await _api.get(ApiConstants.patrocinadores(auth!.slug!), token: auth.token);
      final lista = data is List
          ? data.map((e) => Patrocinador.fromJson(e as Map<String, dynamic>)).toList()
          : <Patrocinador>[];
      setState(() {
        _patrocinadores = lista..sort((a, b) => a.nome.compareTo(b.nome));
      });
    } on ApiException catch (e) {
      setState(() => _erro = e.message);
    } catch (_) {
      setState(() => _erro = 'Erro ao carregar dados.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _abrirFormulario({Patrocinador? patrocinador}) async {
    await Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => PatrocinadorFormScreen(patrocinador: patrocinador)),
    );
    if (mounted) await _carregar();
  }

  Future<void> _remover(Patrocinador patrocinador) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Remover patrocinador'),
        content: Text('Deseja remover ${patrocinador.nome}?'),
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
        '${ApiConstants.patrocinadores(auth!.slug!)}/${patrocinador.id}',
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Patrocinador removido com sucesso.')),
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
    return Scaffold(
      appBar: AppBar(title: const Text('Patrocinadores')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: const Text('Novo patrocinador'),
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
                  : _patrocinadores.isEmpty
                      ? ListView(
                          children: const [
                            Padding(
                              padding: EdgeInsets.all(24),
                              child: Text(
                                'Nenhum patrocinador cadastrado.',
                                textAlign: TextAlign.center,
                              ),
                            ),
                          ],
                        )
                      : ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: _patrocinadores.length,
                          separatorBuilder: (context, index) => const SizedBox(height: 12),
                          itemBuilder: (context, index) {
                            final patrocinador = _patrocinadores[index];
                            final contatos = <String>[
                              if ((patrocinador.instagram ?? '').trim().isNotEmpty)
                                'Instagram: ${patrocinador.instagram}',
                              if ((patrocinador.facebook ?? '').trim().isNotEmpty)
                                'Facebook: ${patrocinador.facebook}',
                              if ((patrocinador.site ?? '').trim().isNotEmpty)
                                'Site: ${patrocinador.site}',
                              if ((patrocinador.zap ?? '').trim().isNotEmpty)
                                'Zap: ${patrocinador.zap}',
                            ];

                            return Card(
                              child: ListTile(
                                leading: ExpandableRectImage(
                                  imageUrl: patrocinador.fotoUrl,
                                  fallbackIcon: Icons.campaign_outlined,
                                  width: 56,
                                  height: 56,
                                ),
                                title: Text(patrocinador.nome),
                                subtitle: Text(
                                  contatos.isEmpty ? 'Sem canais informados' : contatos.join(' - '),
                                ),
                                trailing: Wrap(
                                  spacing: 8,
                                  children: [
                                    IconButton(
                                      onPressed: () => _abrirFormulario(patrocinador: patrocinador),
                                      icon: const Icon(Icons.edit),
                                      tooltip: 'Editar',
                                    ),
                                    IconButton(
                                      onPressed: () => _remover(patrocinador),
                                      icon: const Icon(Icons.delete_outline),
                                      tooltip: 'Remover',
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
