import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../core/models/patrocinador.dart';
import 'expandable_network_image.dart';

class PatrocinadoresSection extends StatelessWidget {
  final List<Patrocinador> patrocinadores;

  const PatrocinadoresSection({
    super.key,
    required this.patrocinadores,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Patrocinadores',
          style: Theme.of(context).textTheme.titleMedium,
        ),
        const SizedBox(height: 8),
        if (patrocinadores.isEmpty)
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: Colors.grey.shade50,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: Colors.grey.shade300),
            ),
            child: const Text(
              'Nenhum patrocinador cadastrado.',
              style: TextStyle(color: Colors.grey),
            ),
          )
        else
          SizedBox(
            height: 172,
            child: ListView.separated(
              scrollDirection: Axis.horizontal,
              itemCount: patrocinadores.length,
              separatorBuilder: (_, _) => const SizedBox(width: 10),
              itemBuilder: (context, index) => _PatrocinadorCard(
                patrocinador: patrocinadores[index],
              ),
            ),
          ),
      ],
    );
  }
}

class _PatrocinadorCard extends StatelessWidget {
  final Patrocinador patrocinador;

  const _PatrocinadorCard({required this.patrocinador});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 220,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: Colors.grey.shade300),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(10),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              ExpandableRectImage(
                imageUrl: patrocinador.fotoUrl,
                fallbackIcon: Icons.campaign_outlined,
                width: 56,
                height: 56,
                borderRadius: BorderRadius.circular(12),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  patrocinador.nome,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              if (_temValor(patrocinador.site))
                _DestinoChip(
                  icon: Icons.language,
                  label: 'Site',
                  onTap: () => _abrirSite(patrocinador.site!),
                ),
              if (_temValor(patrocinador.instagram))
                _DestinoChip(
                  icon: Icons.camera_alt_outlined,
                  label: 'Instagram',
                  onTap: () => _abrirInstagram(patrocinador.instagram!),
                ),
              if (_temValor(patrocinador.zap))
                _DestinoChip(
                  icon: Icons.chat_bubble_outline,
                  label: 'Zap',
                  onTap: () => _abrirZap(patrocinador.zap!),
                ),
            ],
          ),
        ],
      ),
    );
  }

  bool _temValor(String? value) => value != null && value.trim().isNotEmpty;

  Future<void> _abrirSite(String value) async {
    final normalizado = value.trim();
    final url = normalizado.startsWith('http://') || normalizado.startsWith('https://')
        ? normalizado
        : 'https://$normalizado';
    await launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
  }

  Future<void> _abrirInstagram(String value) async {
    final bruto = value.trim();
    final handle = bruto
        .replaceAll('https://instagram.com/', '')
        .replaceAll('https://www.instagram.com/', '')
        .replaceAll('@', '')
        .split('/')
        .first
        .trim();
    final native = Uri.parse('instagram://user?username=$handle');
    final web = Uri.parse('https://instagram.com/$handle');
    if (!await launchUrl(native, mode: LaunchMode.externalApplication)) {
      await launchUrl(web, mode: LaunchMode.externalApplication);
    }
  }

  Future<void> _abrirZap(String value) async {
    final phone = value.replaceAll(RegExp(r'[^0-9]'), '');
    final native = Uri.parse('whatsapp://send?phone=$phone');
    final web = Uri.parse('https://wa.me/$phone');
    if (!await launchUrl(native, mode: LaunchMode.externalApplication)) {
      await launchUrl(web, mode: LaunchMode.externalApplication);
    }
  }
}

class _DestinoChip extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback onTap;

  const _DestinoChip({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return ActionChip(
      avatar: Icon(icon, size: 16),
      label: Text(label),
      onPressed: onTap,
      visualDensity: VisualDensity.compact,
    );
  }
}
