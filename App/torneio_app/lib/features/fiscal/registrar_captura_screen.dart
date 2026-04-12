import 'dart:io';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../../core/models/captura.dart';
import '../../core/providers/auth_provider.dart';
import '../../core/providers/captura_provider.dart';
import '../../core/providers/config_provider.dart';

class RegistrarCapturaScreen extends StatefulWidget {
  const RegistrarCapturaScreen({super.key});

  @override
  State<RegistrarCapturaScreen> createState() => _RegistrarCapturaScreenState();
}

class _RegistrarCapturaScreenState extends State<RegistrarCapturaScreen> {
  final _formKey = GlobalKey<FormState>();
  final _tamanhoController = TextEditingController();

  String? _membroId;
  String? _itemId;
  String? _fotoPath;
  bool _salvando = false;

  Future<void> _tirarFoto() async {
    final picker = ImagePicker();
    final foto = await picker.pickImage(
      source: ImageSource.camera,
      maxWidth: 1280,
      maxHeight: 1280,
      imageQuality: 85,
    );
    if (foto != null) setState(() => _fotoPath = foto.path);
  }

  Future<void> _salvar() async {
    if (!_formKey.currentState!.validate()) return;
    if (_fotoPath == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Tire uma foto da captura.')),
      );
      return;
    }

    final auth = context.read<AuthProvider>();
    final config = context.read<ConfigProvider>().config!;
    final configProv = context.read<ConfigProvider>();
    final capProv = context.read<CapturaProvider>();

    final anos = configProv.anos.where((a) => a.isLiberado).toList();
    if (anos.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Nenhuma edição ativa no momento.')),
      );
      return;
    }

    final anoAtivo = anos.first;

    // Equipe do fiscal
    final equipe = capProv.equipes.isEmpty
        ? null
        : capProv.equipes
            .where((e) => e.fiscalId == (auth.usuario?.id ?? ''))
            .firstOrNull ?? capProv.equipes.first;

    if (equipe == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Equipe não encontrada.')),
      );
      return;
    }

    setState(() => _salvando = true);

    final req = RegistrarCapturaRequest(
      torneioId: config.id,
      anoTorneioId: anoAtivo.id,
      itemId: _itemId!,
      membroId: _membroId!,
      equipeId: equipe.id,
      tamanhoMedida: double.parse(_tamanhoController.text.replaceAll(',', '.')),
      fotoUrl: _fotoPath!,
      dataHora: DateTime.now(),
    );

    final ok = await capProv.registrarCaptura(
      slug: config.slug,
      token: auth.usuario!.token,
      req: req,
    );

    if (!mounted) return;
    setState(() => _salvando = false);

    if (ok) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            capProv.pendentesSync > 0
                ? 'Captura salva offline. Sincronize quando tiver conexão.'
                : 'Captura registrada com sucesso!',
          ),
          backgroundColor: capProv.pendentesSync > 0 ? Colors.orange : Colors.green,
        ),
      );
      Navigator.pop(context);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Erro ao registrar captura.'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final config = context.watch<ConfigProvider>().config;
    final capProv = context.watch<CapturaProvider>();
    final membros = capProv.membros;
    final itens = capProv.itens;

    return Scaffold(
      appBar: AppBar(
        title: Text('Registrar ${config?.labelCaptura ?? "Captura"}'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Membro
              DropdownButtonFormField<String>(
                initialValue: _membroId,
                decoration: InputDecoration(
                  labelText: config?.labelMembro ?? 'Membro',
                  border: const OutlineInputBorder(),
                ),
                items: membros
                    .map((m) => DropdownMenuItem(value: m.id, child: Text(m.nome)))
                    .toList(),
                onChanged: (v) => setState(() => _membroId = v),
                validator: (v) => v == null ? 'Selecione um ${config?.labelMembro ?? "membro"}' : null,
              ),
              const SizedBox(height: 16),

              // Item
              DropdownButtonFormField<String>(
                initialValue: _itemId,
                decoration: InputDecoration(
                  labelText: config?.labelItem ?? 'Item',
                  border: const OutlineInputBorder(),
                ),
                items: itens
                    .map((i) => DropdownMenuItem(
                          value: i.id,
                          child: Text(
                            '${i.nome} (mín. ${i.comprimento.toStringAsFixed(1)} ${config?.medidaCaptura ?? "cm"})',
                          ),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => _itemId = v),
                validator: (v) => v == null ? 'Selecione um ${config?.labelItem ?? "item"}' : null,
              ),
              const SizedBox(height: 16),

              // Tamanho
              TextFormField(
                controller: _tamanhoController,
                decoration: InputDecoration(
                  labelText: 'Medida (${config?.medidaCaptura ?? "cm"})',
                  border: const OutlineInputBorder(),
                  suffixText: config?.medidaCaptura ?? 'cm',
                ),
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
                validator: (v) {
                  if (v == null || v.trim().isEmpty) return 'Informe a medida';
                  final d = double.tryParse(v.replaceAll(',', '.'));
                  if (d == null || d <= 0) return 'Medida inválida';
                  return null;
                },
              ),
              const SizedBox(height: 24),

              // Foto
              Text(
                'Foto da ${config?.labelCaptura ?? "captura"}',
                style: Theme.of(context).textTheme.titleSmall,
              ),
              const SizedBox(height: 8),
              GestureDetector(
                onTap: _tirarFoto,
                child: Container(
                  height: 180,
                  decoration: BoxDecoration(
                    border: Border.all(color: Colors.grey),
                    borderRadius: BorderRadius.circular(8),
                    color: Colors.grey.shade100,
                  ),
                  child: _fotoPath != null
                      ? ClipRRect(
                          borderRadius: BorderRadius.circular(8),
                          child: Image.file(File(_fotoPath!), fit: BoxFit.cover),
                        )
                      : const Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(Icons.camera_alt, size: 48, color: Colors.grey),
                            SizedBox(height: 8),
                            Text('Toque para fotografar', style: TextStyle(color: Colors.grey)),
                          ],
                        ),
                ),
              ),
              if (_fotoPath != null)
                TextButton.icon(
                  icon: const Icon(Icons.refresh),
                  label: const Text('Tirar outra foto'),
                  onPressed: _tirarFoto,
                ),
              const SizedBox(height: 24),

              FilledButton.icon(
                icon: const Icon(Icons.save),
                label: Text(_salvando ? 'Salvando...' : 'Registrar'),
                onPressed: _salvando ? null : _salvar,
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _tamanhoController.dispose();
    super.dispose();
  }
}
