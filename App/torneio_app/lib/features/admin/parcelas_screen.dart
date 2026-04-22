import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../core/constants.dart';
import '../../core/models/membro.dart';
import '../../core/models/parcela_torneio.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';

class ParcelasAdminScreen extends StatefulWidget {
  const ParcelasAdminScreen({super.key});

  @override
  State<ParcelasAdminScreen> createState() => _ParcelasAdminScreenState();
}

class _ParcelasAdminScreenState extends State<ParcelasAdminScreen> {
  final _api = ApiService();
  bool _carregando = true;
  String? _erro;
  List<ParcelaTorneio> _parcelas = const [];
  List<Membro> _membros = const [];
  String? _membroIdSelecionado;
  String? _tipoSelecionado;
  bool _somenteNaoPagas = false;
  bool _somenteInadimplentes = false;

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
      final parcelasData = await _api.get(
        ApiConstants.financeiroCobrancas(
          auth!.slug!,
          membroId: _membroIdSelecionado,
          tipo: _tipoSelecionado,
          naoPagas: _somenteNaoPagas,
          inadimplentes: _somenteInadimplentes,
        ),
        token: auth.token,
      );
      final membrosData = await _api.get(ApiConstants.membros(auth.slug!), token: auth.token);

      if (!mounted) return;
      setState(() {
        _parcelas = parcelasData is List
            ? parcelasData.map((e) => ParcelaTorneio.fromJson(e as Map<String, dynamic>)).toList()
            : <ParcelaTorneio>[];
        _membros = membrosData is List
            ? membrosData.map((e) => Membro.fromJson(e as Map<String, dynamic>)).toList()
            : <Membro>[];
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar cobranças.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<DateTime?> _selecionarData(DateTime? inicial) {
    final agora = DateTime.now();
    return showDatePicker(
      context: context,
      initialDate: inicial ?? agora,
      firstDate: DateTime(agora.year - 5),
      lastDate: DateTime(agora.year + 10),
    );
  }

  Future<void> _editarParcela(ParcelaTorneio parcela) async {
    final observacaoController = TextEditingController(text: parcela.observacao ?? '');
    DateTime vencimento = parcela.vencimento;

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => StatefulBuilder(
        builder: (context, setStateDialog) => AlertDialog(
          title: Text('Cobrança ${parcela.numeroParcela}'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              OutlinedButton.icon(
                onPressed: () async {
                  final data = await _selecionarData(vencimento);
                  if (data != null) {
                    setStateDialog(() => vencimento = data);
                  }
                },
                icon: const Icon(Icons.calendar_month_outlined),
                label: Text('Vencimento: ${DateFormat('dd/MM/yyyy').format(vencimento)}'),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: observacaoController,
                maxLines: 3,
                decoration: const InputDecoration(
                  labelText: 'Observação',
                  border: OutlineInputBorder(),
                ),
              ),
            ],
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(dialogContext, false), child: const Text('Cancelar')),
            FilledButton(onPressed: () => Navigator.pop(dialogContext, true), child: const Text('Salvar')),
          ],
        ),
      ),
    );

    if (confirmar != true) return;
    if (!mounted) return;

    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    try {
      await _api.put(
        ApiConstants.financeiroCobranca(auth!.slug!, parcela.id),
        {
          'vencimento': vencimento.toIso8601String(),
          'observacao': observacaoController.text.trim(),
        },
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Cobrança atualizada.')),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  Future<void> _editarPagamento(ParcelaTorneio parcela) async {
    bool pago = parcela.pago;
    DateTime? dataPagamento = parcela.dataPagamento ?? DateTime.now();

    final confirmar = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => StatefulBuilder(
        builder: (context, setStateDialog) => AlertDialog(
          title: Text('Pagamento da cobranca ${parcela.numeroParcela}'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              SwitchListTile(
                value: pago,
                title: const Text('Pago'),
                contentPadding: EdgeInsets.zero,
                onChanged: (value) => setStateDialog(() => pago = value),
              ),
              if (pago)
                OutlinedButton.icon(
                  onPressed: () async {
                    final data = await _selecionarData(dataPagamento);
                    if (data != null) {
                      setStateDialog(() => dataPagamento = data);
                    }
                  },
                  icon: const Icon(Icons.event_available_outlined),
                  label: Text(
                    'Data: ${DateFormat('dd/MM/yyyy').format(dataPagamento ?? DateTime.now())}',
                  ),
                ),
              if (!pago)
                const Padding(
                  padding: EdgeInsets.only(top: 8),
                  child: Text(
                    'Ao desmarcar, a data de pagamento sera limpa para manter o estado consistente.',
                  ),
                ),
            ],
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(dialogContext, false), child: const Text('Cancelar')),
            FilledButton(onPressed: () => Navigator.pop(dialogContext, true), child: const Text('Salvar')),
          ],
        ),
      ),
    );

    if (confirmar != true) return;
    if (!mounted) return;

    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    try {
      await _api.put(
        ApiConstants.financeiroPagamentoCobranca(auth!.slug!, parcela.id),
        {
          'pago': pago,
          'dataPagamento': pago ? dataPagamento?.toIso8601String() : null,
        },
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(pago ? 'Cobrança marcada como paga.' : 'Pagamento removido.')),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  Future<void> _anexarComprovante(ParcelaTorneio parcela) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final result = await FilePicker.platform.pickFiles(
      type: FileType.custom,
      allowedExtensions: ['jpg', 'jpeg', 'png', 'pdf', 'doc', 'docx'],
    );
    final path = result?.files.single.path;
    if (path == null) return;

    try {
      await _api.postMultipart(
        ApiConstants.financeiroComprovanteCobranca(auth!.slug!, parcela.id),
        fields: const {},
        files: {'arquivo': path},
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Comprovante anexado com sucesso.')),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  Future<void> _abrirComprovante(String? url) async {
    if (url == null || url.trim().isEmpty) return;
    await launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
  }

  Future<void> _gerarParcelas({required bool somenteNovos, List<String> membroIds = const []}) async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    try {
      await _api.post(
        ApiConstants.financeiroGerarParcelas(auth!.slug!),
        {
          'somenteNovos': somenteNovos,
          'membroIds': membroIds,
        },
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            somenteNovos
                ? 'Parcelas geradas para pescadores novos.'
                : membroIds.isEmpty
                    ? 'Parcelas regeneradas para todos os pescadores.'
                    : 'Parcelas regeneradas para os pescadores selecionados.',
          ),
        ),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    }
  }

  Future<void> _selecionarPescadoresParaRegenerar() async {
    final resultado = await showDialog<List<String>>(
      context: context,
      builder: (dialogContext) {
        final escolhidos = <String>{};
        return StatefulBuilder(
          builder: (context, setStateDialog) => AlertDialog(
            title: const Text('Regenerar parcelas'),
            content: SizedBox(
              width: 420,
              child: SingleChildScrollView(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: _membros
                      .map(
                        (membro) => CheckboxListTile(
                          value: escolhidos.contains(membro.id),
                          title: Text(membro.nome),
                          controlAffinity: ListTileControlAffinity.leading,
                          onChanged: (value) {
                            setStateDialog(() {
                              if (value == true) {
                                escolhidos.add(membro.id);
                              } else {
                                escolhidos.remove(membro.id);
                              }
                            });
                          },
                        ),
                      )
                      .toList(),
                ),
              ),
            ),
            actions: [
              TextButton(onPressed: () => Navigator.pop(dialogContext), child: const Text('Cancelar')),
              FilledButton(
                onPressed: () => Navigator.pop(dialogContext, escolhidos.toList()),
                child: const Text('Regenerar'),
              ),
            ],
          ),
        );
      },
    );

    if (resultado == null || resultado.isEmpty) return;
    await _gerarParcelas(somenteNovos: false, membroIds: resultado);
  }

  @override
  Widget build(BuildContext context) {
    String tipoLabel(String? tipoParcela) => switch (tipoParcela) {
          'Mensalidade' => 'Parcela',
          'TaxaInscricao' => 'Inscrição',
          'ProdutoExtra' => 'Produto extra',
          _ => tipoParcela ?? '-',
        };

    return Scaffold(
      appBar: AppBar(title: const Text('Cobranças')),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _carregar,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Wrap(
                        spacing: 8,
                        runSpacing: 8,
                        children: [
                          OutlinedButton.icon(
                            onPressed: () => _gerarParcelas(somenteNovos: false),
                            icon: const Icon(Icons.refresh),
                            label: const Text('Regenerar parcelas'),
                          ),
                          OutlinedButton.icon(
                            onPressed: () => _gerarParcelas(somenteNovos: true),
                            icon: const Icon(Icons.person_add_alt_1),
                            label: const Text('Gerar para novos'),
                          ),
                          OutlinedButton.icon(
                            onPressed: _selecionarPescadoresParaRegenerar,
                            icon: const Icon(Icons.people_alt_outlined),
                            label: const Text('Regenerar selecionados'),
                          ),
                        ],
                      ),
                    ),
                  ),
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        children: [
                          DropdownButtonFormField<String?>(
                            initialValue: _membroIdSelecionado,
                            decoration: const InputDecoration(
                              labelText: 'Pescador',
                              border: OutlineInputBorder(),
                            ),
                            items: [
                              const DropdownMenuItem<String?>(
                                value: null,
                                child: Text('Todos'),
                              ),
                              ..._membros.map(
                                (m) => DropdownMenuItem<String?>(
                                  value: m.id,
                                  child: Text(m.nome),
                                ),
                              ),
                            ],
                            onChanged: (value) => setState(() => _membroIdSelecionado = value),
                          ),
                          const SizedBox(height: 12),
                          DropdownButtonFormField<String?>(
                            initialValue: _tipoSelecionado,
                            decoration: const InputDecoration(
                              labelText: 'Tipo',
                              border: OutlineInputBorder(),
                            ),
                            items: const [
                              DropdownMenuItem<String?>(
                                value: null,
                                child: Text('Todos'),
                              ),
                              DropdownMenuItem<String?>(
                                value: 'TaxaInscricao',
                                child: Text('Inscrição'),
                              ),
                              DropdownMenuItem<String?>(
                                value: 'Mensalidade',
                                child: Text('Parcela'),
                              ),
                              DropdownMenuItem<String?>(
                                value: 'ProdutoExtra',
                                child: Text('Produto extra'),
                              ),
                            ],
                            onChanged: (value) => setState(() => _tipoSelecionado = value),
                          ),
                          SwitchListTile(
                            value: _somenteNaoPagas,
                            contentPadding: EdgeInsets.zero,
                            title: const Text('Somente não pagas'),
                            onChanged: (value) => setState(() => _somenteNaoPagas = value),
                          ),
                          SwitchListTile(
                            value: _somenteInadimplentes,
                            contentPadding: EdgeInsets.zero,
                            title: const Text('Somente inadimplentes'),
                            onChanged: (value) => setState(() => _somenteInadimplentes = value),
                          ),
                          Align(
                            alignment: Alignment.centerRight,
                            child: FilledButton.icon(
                              onPressed: _carregar,
                              icon: const Icon(Icons.search),
                              label: const Text('Filtrar'),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  if (_erro != null)
                    Padding(
                      padding: const EdgeInsets.only(top: 16),
                      child: Text(_erro!, textAlign: TextAlign.center),
                    ),
                  if (_erro == null && _parcelas.isEmpty)
                    const Padding(
                      padding: EdgeInsets.only(top: 24),
                      child: Text('Nenhuma cobranca encontrada.', textAlign: TextAlign.center),
                    ),
                  ..._parcelas.map(
                    (parcela) => Card(
                      margin: const EdgeInsets.only(top: 12),
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              children: [
                                Expanded(
                                  child: Text(
                                    '${parcela.nomeMembro} - ${parcela.descricao}',
                                    style: Theme.of(context).textTheme.titleMedium,
                                  ),
                                ),
                                _StatusParcelaChip(parcela: parcela),
                              ],
                            ),
                            const SizedBox(height: 8),
                            Text('Tipo: ${tipoLabel(parcela.tipoParcela)}'),
                            Text('Valor: ${parcela.valorFormatado}'),
                            if (parcela.tipoParcela == 'Mensalidade')
                              Text('Parcela da cobranca: ${parcela.numeroParcela}'),
                            Text('Vencimento: ${DateFormat('dd/MM/yyyy').format(parcela.vencimento)}'),
                            if (parcela.dataPagamento != null)
                              Text('Data de pagamento: ${DateFormat('dd/MM/yyyy').format(parcela.dataPagamento!)}'),
                            if ((parcela.observacao ?? '').trim().isNotEmpty)
                              Padding(
                                padding: const EdgeInsets.only(top: 6),
                                child: Text('Observação: ${parcela.observacao}'),
                              ),
                            if ((parcela.comprovanteNomeArquivo ?? '').trim().isNotEmpty)
                              Padding(
                                padding: const EdgeInsets.only(top: 6),
                                child: Text('Comprovante: ${parcela.comprovanteNomeArquivo}'),
                              ),
                            const SizedBox(height: 12),
                            Wrap(
                              spacing: 8,
                              runSpacing: 8,
                              children: [
                                OutlinedButton.icon(
                                  onPressed: () => _editarParcela(parcela),
                                  icon: const Icon(Icons.edit_calendar_outlined),
                                  label: const Text('Editar'),
                                ),
                                OutlinedButton.icon(
                                  onPressed: () => _editarPagamento(parcela),
                                  icon: const Icon(Icons.payments_outlined),
                                  label: Text(parcela.pago ? 'Revisar pagamento' : 'Marcar pagamento'),
                                ),
                                OutlinedButton.icon(
                                  onPressed: () => _anexarComprovante(parcela),
                                  icon: const Icon(Icons.attach_file_outlined),
                                  label: const Text('Comprovante'),
                                ),
                                if ((parcela.comprovanteUrl ?? '').trim().isNotEmpty)
                                  TextButton.icon(
                                    onPressed: () => _abrirComprovante(parcela.comprovanteUrl),
                                    icon: const Icon(Icons.open_in_new),
                                    label: const Text('Visualizar'),
                                  ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}

class _StatusParcelaChip extends StatelessWidget {
  final ParcelaTorneio parcela;

  const _StatusParcelaChip({required this.parcela});

  @override
  Widget build(BuildContext context) {
    if (parcela.pago) {
      return const Chip(label: Text('Pago'), backgroundColor: Color(0xFFDFF5E1));
    }
    if (parcela.inadimplente) {
      return const Chip(label: Text('Inadimplente'), backgroundColor: Color(0xFFFDE2E1));
    }
    return const Chip(label: Text('Em aberto'), backgroundColor: Color(0xFFFFF1D6));
  }
}
