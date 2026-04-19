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
  String? _equipeId;
  String? _fotoPath;
  int? _fonteFoto; // 0 = Camera, 1 = Galeria
  bool _salvando = false;

  Future<void> _escolherFoto(ImageSource source) async {
    final picker = ImagePicker();
    final foto = await picker.pickImage(
      source: source,
      maxWidth: 1280,
      maxHeight: 1280,
      imageQuality: 85,
    );
    if (foto != null) {
      setState(() {
        _fotoPath = foto.path;
        _fonteFoto = source == ImageSource.camera ? 0 : 1;
      });
    }
  }

  void _mostrarOpcoesFoto() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (_) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const SizedBox(height: 8),
            Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey.shade300,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            const SizedBox(height: 12),
            ListTile(
              leading: const Icon(Icons.camera_alt, color: Colors.blue),
              title: const Text('Tirar foto'),
              onTap: () {
                Navigator.pop(context);
                _escolherFoto(ImageSource.camera);
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_library, color: Colors.green),
              title: const Text('Escolher da galeria'),
              onTap: () {
                Navigator.pop(context);
                _escolherFoto(ImageSource.gallery);
              },
            ),
            const SizedBox(height: 8),
          ],
        ),
      ),
    );
  }

  Future<void> _salvar({bool forcarOffline = false}) async {
    if (!_formKey.currentState!.validate()) return;
    if (_fotoPath == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Adicione uma foto da captura.')),
      );
      return;
    }

    final auth = context.read<AuthProvider>();
    final config = context.read<ConfigProvider>().config!;
    final capProv = context.read<CapturaProvider>();
    final fiscalId = auth.usuario?.id ?? '';

    final minhasEquipes =
        capProv.equipes.where((e) => e.fiscalId == fiscalId).toList();

    final equipeIdFinal = _equipeId ??
        (minhasEquipes.length == 1 ? minhasEquipes.first.id : null);

    if (equipeIdFinal == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Selecione uma embarcação.')),
      );
      return;
    }

    setState(() => _salvando = true);

    final req = RegistrarCapturaRequest(
      torneioId: config.id,
      itemId: _itemId!,
      membroId: _membroId!,
      equipeId: equipeIdFinal,
      tamanhoMedida: double.parse(_tamanhoController.text.replaceAll(',', '.')),
      fotoUrl: _fotoPath!,
      dataHora: DateTime.now(),
      fonteFoto: _fonteFoto,
    );

    final ok = await capProv.registrarCaptura(
      slug: config.slug,
      token: auth.usuario!.token,
      req: req,
      forcarOffline: forcarOffline,
    );

    if (!mounted) return;
    setState(() => _salvando = false);

    if (ok) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            forcarOffline || capProv.pendentesSync > 0
                ? 'Captura salva para sincronizar depois.'
                : 'Captura registrada com sucesso!',
          ),
          backgroundColor:
              forcarOffline || capProv.pendentesSync > 0 ? Colors.orange : Colors.green,
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
    final auth = context.watch<AuthProvider>();
    final membros = capProv.membros;
    final itens = capProv.itens;
    final fiscalId = auth.usuario?.id ?? '';
    final minhasEquipes =
        capProv.equipes.where((e) => e.fiscalId == fiscalId).toList();
    final multiEquipe = minhasEquipes.length > 1;

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
              // Equipe — só exibe quando o fiscal tem mais de uma
              if (multiEquipe) ...[
                DropdownButtonFormField<String>(
                  initialValue: _equipeId,
                  decoration: InputDecoration(
                    labelText: config?.labelEquipe ?? 'Embarcação',
                    border: const OutlineInputBorder(),
                  ),
                  items: minhasEquipes
                      .map((e) => DropdownMenuItem(value: e.id, child: Text(e.nome)))
                      .toList(),
                  onChanged: (v) => setState(() => _equipeId = v),
                  validator: (v) => v == null
                      ? 'Selecione uma ${config?.labelEquipe ?? "embarcação"}'
                      : null,
                ),
                const SizedBox(height: 16),
              ],

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
                validator: (v) =>
                    v == null ? 'Selecione um ${config?.labelMembro ?? "membro"}' : null,
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
                            i.comprimento != null
                                ? '${i.nome} (mín. ${i.comprimento!.toStringAsFixed(1)} ${config?.medidaCaptura ?? "cm"})'
                                : i.nome,
                          ),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => _itemId = v),
                validator: (v) =>
                    v == null ? 'Selecione um ${config?.labelItem ?? "item"}' : null,
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
                onTap: _mostrarOpcoesFoto,
                child: Container(
                  height: 180,
                  decoration: BoxDecoration(
                    border: Border.all(color: Colors.grey),
                    borderRadius: BorderRadius.circular(8),
                    color: Colors.grey.shade100,
                  ),
                  child: _fotoPath != null
                      ? Stack(
                          fit: StackFit.expand,
                          children: [
                            ClipRRect(
                              borderRadius: BorderRadius.circular(8),
                              child: Image.file(File(_fotoPath!), fit: BoxFit.cover),
                            ),
                            Positioned(
                              top: 6,
                              right: 6,
                              child: _FotoOrigemBadge(fonteFoto: _fonteFoto),
                            ),
                          ],
                        )
                      : const Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(Icons.add_a_photo, size: 48, color: Colors.grey),
                            SizedBox(height: 8),
                            Text(
                              'Câmera ou galeria',
                              style: TextStyle(color: Colors.grey),
                            ),
                          ],
                        ),
                ),
              ),
              if (_fotoPath != null)
                Padding(
                  padding: const EdgeInsets.only(top: 4),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      TextButton.icon(
                        icon: const Icon(Icons.camera_alt, size: 16),
                        label: const Text('Câmera'),
                        onPressed: () => _escolherFoto(ImageSource.camera),
                      ),
                      TextButton.icon(
                        icon: const Icon(Icons.photo_library, size: 16),
                        label: const Text('Galeria'),
                        onPressed: () => _escolherFoto(ImageSource.gallery),
                      ),
                    ],
                  ),
                ),
              const SizedBox(height: 24),

              FilledButton.icon(
                icon: const Icon(Icons.send),
                label: Text(_salvando ? 'Salvando...' : 'Registrar agora'),
                onPressed: _salvando ? null : () => _salvar(forcarOffline: false),
              ),
              const SizedBox(height: 8),
              OutlinedButton.icon(
                icon: const Icon(Icons.schedule),
                label: Text(_salvando ? 'Salvando...' : 'Sincronizar depois'),
                onPressed: _salvando ? null : () => _salvar(forcarOffline: true),
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

class _FotoOrigemBadge extends StatelessWidget {
  final int? fonteFoto;
  const _FotoOrigemBadge({this.fonteFoto});

  @override
  Widget build(BuildContext context) {
    final isGaleria = fonteFoto == 1;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: Colors.black54,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            isGaleria ? Icons.photo_library : Icons.camera_alt,
            color: Colors.white,
            size: 12,
          ),
          const SizedBox(width: 4),
          Text(
            isGaleria ? 'Galeria' : 'Câmera',
            style: const TextStyle(color: Colors.white, fontSize: 11),
          ),
        ],
      ),
    );
  }
}
