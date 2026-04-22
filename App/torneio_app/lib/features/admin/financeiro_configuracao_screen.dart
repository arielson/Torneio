import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../core/constants.dart';
import '../../core/models/financeiro_config.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/services/api_service.dart';

class FinanceiroConfiguracaoScreen extends StatefulWidget {
  const FinanceiroConfiguracaoScreen({super.key});

  @override
  State<FinanceiroConfiguracaoScreen> createState() => _FinanceiroConfiguracaoScreenState();
}

class _FinanceiroConfiguracaoScreenState extends State<FinanceiroConfiguracaoScreen> {
  final _api = ApiService();
  final _valorController = TextEditingController();
  final _taxaInscricaoController = TextEditingController();
  final _parcelasController = TextEditingController();
  bool _carregando = true;
  bool _salvando = false;
  String? _erro;
  DateTime? _primeiroVencimento;
  DateTime? _vencimentoTaxaInscricao;
  FinanceiroConfig? _configAtual;

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
      final configData = await _api.get(ApiConstants.financeiroConfig(auth!.slug!), token: auth.token);
      final config = FinanceiroConfig.fromJson(configData as Map<String, dynamic>);
      _valorController.text = config.valorPorMembro.toStringAsFixed(2);
      _taxaInscricaoController.text = config.taxaInscricaoValor.toStringAsFixed(2);
      _parcelasController.text = config.quantidadeParcelas.toString();
      if (!mounted) return;
      setState(() {
        _configAtual = config;
        _primeiroVencimento = config.dataPrimeiroVencimento;
        _vencimentoTaxaInscricao = config.dataVencimentoTaxaInscricao;
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _erro = 'Erro ao carregar a configuração financeira.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _selecionarPrimeiroVencimento() async {
    final agora = DateTime.now();
    final selecionada = await showDatePicker(
      context: context,
      initialDate: _primeiroVencimento ?? agora,
      firstDate: DateTime(agora.year - 5),
      lastDate: DateTime(agora.year + 10),
    );
    if (selecionada != null && mounted) {
      setState(() => _primeiroVencimento = selecionada);
    }
  }

  Future<void> _selecionarVencimentoTaxa() async {
    final agora = DateTime.now();
    final selecionada = await showDatePicker(
      context: context,
      initialDate: _vencimentoTaxaInscricao ?? agora,
      firstDate: DateTime(agora.year - 5),
      lastDate: DateTime(agora.year + 10),
    );
    if (selecionada != null && mounted) {
      setState(() => _vencimentoTaxaInscricao = selecionada);
    }
  }

  Future<void> _salvarConfiguracao() async {
    final auth = context.read<AuthProvider>().usuario;
    if (auth?.slug == null || auth?.token == null) return;

    final valor = double.tryParse(_valorController.text.replaceAll(',', '.'));
    final taxaInscricao = double.tryParse(_taxaInscricaoController.text.replaceAll(',', '.'));
    final parcelas = int.tryParse(_parcelasController.text.trim());

    if (valor == null || valor < 0) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Informe um valor por pescador valido.')));
      return;
    }
    if (parcelas == null || parcelas < 0 || parcelas > 999) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Informe uma quantidade de parcelas entre 0 e 999.')));
      return;
    }
    if (taxaInscricao == null || taxaInscricao < 0) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Informe uma taxa de inscrição válida.')));
      return;
    }

    var confirmarSubstituicao = false;
    if (_configAtual != null &&
        _configAtual!.possuiConfiguracaoAnterior &&
        (_configAtual!.valorPorMembro != valor ||
            _configAtual!.quantidadeParcelas != parcelas ||
            _configAtual!.taxaInscricaoValor != taxaInscricao ||
            _configAtual!.dataPrimeiroVencimento?.toIso8601String() != _primeiroVencimento?.toIso8601String() ||
            _configAtual!.dataVencimentoTaxaInscricao?.toIso8601String() != _vencimentoTaxaInscricao?.toIso8601String())) {
      final confirmar = await showDialog<bool>(
        context: context,
        builder: (_) => AlertDialog(
          title: const Text('Substituir configuração'),
          content: const Text('Já existe uma configuração financeira salva. Deseja limpar a configuração anterior e criar uma nova?'),
          actions: [
            TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
            FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Confirmar')),
          ],
        ),
      );
      if (confirmar != true) return;
      confirmarSubstituicao = true;
    }

    setState(() => _salvando = true);
    try {
      await _api.put(
        ApiConstants.financeiroConfig(auth!.slug!),
        {
          'valorPorMembro': valor,
          'quantidadeParcelas': parcelas,
          'dataPrimeiroVencimento': _primeiroVencimento?.toIso8601String(),
          'taxaInscricaoValor': taxaInscricao,
          'dataVencimentoTaxaInscricao': _vencimentoTaxaInscricao?.toIso8601String(),
          'confirmarSubstituicao': confirmarSubstituicao,
        },
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Configuração financeira atualizada.')));
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.message), backgroundColor: Colors.red));
    } finally {
      if (mounted) setState(() => _salvando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Configuração')),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : ListView(
              padding: const EdgeInsets.all(16),
              children: [
                if (_erro != null)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 16),
                    child: Text(_erro!, textAlign: TextAlign.center),
                  ),
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        TextField(
                          controller: _valorController,
                          keyboardType: const TextInputType.numberWithOptions(decimal: true),
                          decoration: const InputDecoration(labelText: 'Valor total por pescador', border: OutlineInputBorder()),
                        ),
                        const SizedBox(height: 12),
                        TextField(
                          controller: _taxaInscricaoController,
                          keyboardType: const TextInputType.numberWithOptions(decimal: true),
                          decoration: const InputDecoration(
                            labelText: 'Taxa de inscrição opcional',
                            border: OutlineInputBorder(),
                            helperText: 'Use 0 quando nao houver taxa.',
                          ),
                        ),
                        const SizedBox(height: 12),
                        TextField(
                          controller: _parcelasController,
                          keyboardType: TextInputType.number,
                          decoration: const InputDecoration(
                            labelText: 'Quantidade de parcelas',
                            border: OutlineInputBorder(),
                            helperText: 'Use 0 para nao gerar parcelas.',
                          ),
                        ),
                        const SizedBox(height: 12),
                        OutlinedButton.icon(
                          onPressed: _selecionarPrimeiroVencimento,
                          icon: const Icon(Icons.calendar_month_outlined),
                          label: Text(
                            _primeiroVencimento == null
                                ? 'Selecionar primeiro vencimento'
                                : 'Primeiro vencimento: ${DateFormat('dd/MM/yyyy').format(_primeiroVencimento!)}',
                          ),
                        ),
                        const SizedBox(height: 12),
                        OutlinedButton.icon(
                          onPressed: _selecionarVencimentoTaxa,
                          icon: const Icon(Icons.confirmation_number_outlined),
                          label: Text(
                            _vencimentoTaxaInscricao == null
                                ? 'Selecionar vencimento da taxa'
                                : 'Vencimento da taxa: ${DateFormat('dd/MM/yyyy').format(_vencimentoTaxaInscricao!)}',
                          ),
                        ),
                        const SizedBox(height: 16),
                        FilledButton.icon(
                          onPressed: _salvando ? null : _salvarConfiguracao,
                          icon: _salvando
                              ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                              : const Icon(Icons.save),
                          label: Text(_salvando ? 'Salvando...' : 'Salvar'),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
    );
  }

  @override
  void dispose() {
    _valorController.dispose();
    _taxaInscricaoController.dispose();
    _parcelasController.dispose();
    super.dispose();
  }
}
