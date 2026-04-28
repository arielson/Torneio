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
  final List<TextEditingController> _valoresParcelasControllers = [];
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
      _atualizarValoresParcelas(
        config.quantidadeParcelas,
        valorTotal: config.valorPorMembro,
        valoresSalvos: config.valoresParcelas,
      );
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
      setState(() => _erro = 'Erro ao carregar a configuracao financeira.');
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  void _atualizarValoresParcelas(
    int quantidade, {
    double? valorTotal,
    List<ValorParcelaFinanceira>? valoresSalvos,
  }) {
    final total = valorTotal ?? double.tryParse(_valorController.text.replaceAll(',', '.')) ?? 0;
    final valoresPadrao = _dividirUniformemente(total, quantidade);
    final valoresExistentes = _valoresParcelasControllers
        .map((controller) => double.tryParse(controller.text.replaceAll(',', '.')) ?? 0)
        .toList();

    for (final controller in _valoresParcelasControllers) {
      controller.dispose();
    }
    _valoresParcelasControllers.clear();

    for (var i = 0; i < quantidade; i++) {
      ValorParcelaFinanceira? salvo;
      if (valoresSalvos != null) {
        for (final item in valoresSalvos) {
          if (item.numeroParcela == i + 1) {
            salvo = item;
            break;
          }
        }
      }

      final valorInicial = salvo?.valor ?? (i < valoresExistentes.length ? valoresExistentes[i] : valoresPadrao[i]);
      _valoresParcelasControllers.add(TextEditingController(text: valorInicial.toStringAsFixed(2)));
    }
  }

  List<double> _dividirUniformemente(double total, int quantidade) {
    if (quantidade <= 0) return const [];
    final base = (total / quantidade * 100).round() / 100;
    final valores = List<double>.filled(quantidade, base);
    final soma = valores.fold<double>(0, (acumulado, item) => acumulado + item);
    final diferenca = ((total - soma) * 100).round() / 100;
    valores[quantidade - 1] = ((valores[quantidade - 1] + diferenca) * 100).round() / 100;
    return valores;
  }

  void _onQuantidadeParcelasChanged(String value) {
    final quantidade = int.tryParse(value.trim()) ?? 0;
    setState(() => _atualizarValoresParcelas(quantidade));
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
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Informe um valor por pescador valido.')),
      );
      return;
    }
    if (parcelas == null || parcelas < 0 || parcelas > 999) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Informe uma quantidade de parcelas entre 0 e 999.')),
      );
      return;
    }
    if (taxaInscricao == null || taxaInscricao < 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Informe uma taxa de inscricao valida.')),
      );
      return;
    }

    final valoresParcelas = <Map<String, dynamic>>[];
    if (parcelas > 0) {
      for (var i = 0; i < _valoresParcelasControllers.length; i++) {
        final valorParcela = double.tryParse(_valoresParcelasControllers[i].text.replaceAll(',', '.'));
        if (valorParcela == null || valorParcela < 0) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Informe um valor valido para a parcela ${i + 1}.')),
          );
          return;
        }
        valoresParcelas.add({
          'numeroParcela': i + 1,
          'valor': valorParcela,
        });
      }
    }

    var confirmarSubstituicao = false;
    if (_configAtual != null &&
        _configAtual!.possuiConfiguracaoAnterior &&
        (_configAtual!.valorPorMembro != valor ||
            _configAtual!.quantidadeParcelas != parcelas ||
            _configAtual!.taxaInscricaoValor != taxaInscricao ||
            _configAtual!.dataPrimeiroVencimento?.toIso8601String() != _primeiroVencimento?.toIso8601String() ||
            _configAtual!.dataVencimentoTaxaInscricao?.toIso8601String() != _vencimentoTaxaInscricao?.toIso8601String() ||
            !_mesmosValoresParcelas(_configAtual!.valoresParcelas, valoresParcelas))) {
      final confirmar = await showDialog<bool>(
        context: context,
        builder: (_) => AlertDialog(
          title: const Text('Substituir configuracao'),
          content: const Text(
            'Ja existe uma configuracao financeira salva. Deseja limpar a configuracao anterior e criar uma nova?',
          ),
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
          'valoresParcelas': valoresParcelas,
        },
        token: auth.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Configuracao financeira atualizada.')),
      );
      await _carregar();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) setState(() => _salvando = false);
    }
  }

  bool _mesmosValoresParcelas(
    List<ValorParcelaFinanceira> atuais,
    List<Map<String, dynamic>> novos,
  ) {
    if (atuais.length != novos.length) return false;
    for (var i = 0; i < atuais.length; i++) {
      final atual = atuais[i];
      final novo = novos[i];
      if (atual.numeroParcela != (novo['numeroParcela'] as int)) return false;
      if (atual.valor != ((novo['valor'] as num).toDouble())) return false;
    }
    return true;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Configuracao')),
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
                          decoration: const InputDecoration(
                            labelText: 'Valor total por pescador',
                            border: OutlineInputBorder(),
                          ),
                        ),
                        const SizedBox(height: 12),
                        TextField(
                          controller: _taxaInscricaoController,
                          keyboardType: const TextInputType.numberWithOptions(decimal: true),
                          decoration: const InputDecoration(
                            labelText: 'Taxa de inscricao opcional',
                            border: OutlineInputBorder(),
                            helperText: 'Use 0 quando nao houver taxa.',
                          ),
                        ),
                        const SizedBox(height: 12),
                        TextField(
                          controller: _parcelasController,
                          keyboardType: TextInputType.number,
                          onChanged: _onQuantidadeParcelasChanged,
                          decoration: const InputDecoration(
                            labelText: 'Quantidade de parcelas',
                            border: OutlineInputBorder(),
                            helperText: 'Use 0 para nao gerar parcelas.',
                          ),
                        ),
                        if (_valoresParcelasControllers.isNotEmpty) ...[
                          const SizedBox(height: 12),
                          const Text(
                            'Valor de cada parcela',
                            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
                          ),
                          const SizedBox(height: 4),
                          const Text(
                            'Ajuste individualmente cada parcela. A soma nao precisa ser igual ao valor total por pescador.',
                            style: TextStyle(color: Colors.black54),
                          ),
                          const SizedBox(height: 12),
                          ...List.generate(
                            _valoresParcelasControllers.length,
                            (index) => Padding(
                              padding: const EdgeInsets.only(bottom: 12),
                              child: Row(
                                children: [
                                  SizedBox(
                                    width: 90,
                                    child: Text('Parcela ${index + 1}'),
                                  ),
                                  const SizedBox(width: 12),
                                  Expanded(
                                    child: TextField(
                                      controller: _valoresParcelasControllers[index],
                                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                                      decoration: const InputDecoration(
                                        border: OutlineInputBorder(),
                                        isDense: true,
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ],
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
                              ? const SizedBox(
                                  width: 18,
                                  height: 18,
                                  child: CircularProgressIndicator(strokeWidth: 2),
                                )
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
    for (final controller in _valoresParcelasControllers) {
      controller.dispose();
    }
    super.dispose();
  }
}
