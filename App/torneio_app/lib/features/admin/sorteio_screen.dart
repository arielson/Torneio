import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/sorteio_equipe.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/config_provider.dart';
import '../../core/services/api_service.dart';

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
      final data = await _api.get(ApiConstants.sorteio(auth!.slug!), token: auth.token);
      final lista = data is List
          ? data.map((e) => SorteioEquipe.fromJson(e as Map<String, dynamic>)).toList()
          : <SorteioEquipe>[];
      setState(() {
        _resultado = lista..sort((a, b) => a.posicao.compareTo(b.posicao));
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
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    setState(() => _processando = true);

    try {
      final data = await _api.post(ApiConstants.sorteio(auth!.slug!), {}, token: auth.token);
      final lista = data is List
          ? data.map((e) => SorteioEquipe.fromJson(e as Map<String, dynamic>)).toList()
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
    final labelMembro = config?.labelMembro ?? 'Membro';

    return Scaffold(
      appBar: AppBar(
        title: const Text('Sorteio'),
        actions: [
          if (_resultado.isEmpty)
            TextButton.icon(
              onPressed: _processando ? null : _sortear,
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
                  : _resultado.isEmpty
                      ? ListView(
                          padding: const EdgeInsets.all(24),
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
                              onPressed: _processando ? null : _sortear,
                              icon: const Icon(Icons.shuffle),
                              label: Text(_processando ? 'Processando...' : 'Realizar sorteio'),
                            ),
                          ],
                        )
                      : ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: _resultado.length,
                          separatorBuilder: (context, index) =>
                              const SizedBox(height: 12),
                          itemBuilder: (context, index) {
                            final item = _resultado[index];
                            return Card(
                              child: ListTile(
                                leading: CircleAvatar(
                                  child: Text(item.posicao.toString()),
                                ),
                                title: Text(item.nomeEquipe),
                                subtitle: Text('$labelMembro: ${item.nomeMembro}'),
                                trailing: TextButton.icon(
                                  onPressed: _processando ? null : () => _ajustarPosicao(item),
                                  icon: const Icon(Icons.swap_vert),
                                  label: const Text('Posição'),
                                ),
                              ),
                            );
                          },
                        ),
            ),
    );
  }
}
