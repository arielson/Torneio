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
    if (patrocinadores.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Patrocinadores',
          style: Theme.of(context).textTheme.titleMedium,
        ),
        const SizedBox(height: 8),
        SizedBox(
          height: 210,
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
      width: 240,
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
          Center(
            child: ExpandableRectImage(
              imageUrl: patrocinador.fotoUrl,
              fallbackIcon: Icons.campaign_outlined,
              width: 210,
              height: 128,
              borderRadius: BorderRadius.circular(14),
            ),
          ),
          const SizedBox(height: 12),
          Wrap(
            alignment: WrapAlignment.center,
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
              if (_temValor(patrocinador.facebook))
                _DestinoChip(
                  icon: Icons.facebook,
                  label: 'Facebook',
                  onTap: () => _abrirFacebook(patrocinador.facebook!),
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
    final normalizado = bruto
        .replaceAll('https://instagram.com/', '')
        .replaceAll('https://www.instagram.com/', '')
        .replaceAll('http://instagram.com/', '')
        .replaceAll('http://www.instagram.com/', '')
        .replaceAll('@', '')
        .split('/')
        .first
        .trim();
    final web = Uri.parse('https://www.instagram.com/$normalizado/');
    await launchUrl(web, mode: LaunchMode.externalApplication);
  }

  Future<void> _abrirFacebook(String value) async {
    final bruto = value.trim();
    final url = bruto.startsWith('http://') || bruto.startsWith('https://')
        ? bruto
        : 'https://facebook.com/${bruto.replaceAll('facebook.com/', '').replaceAll('www.facebook.com/', '').replaceAll('@', '').split('/').first.trim()}';
    await launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
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
      backgroundColor: const Color(0xFFF5F5F5),
      side: BorderSide(color: Colors.grey.shade300),
      avatar: Icon(icon, size: 16, color: Colors.black87),
      label: Text(
        label,
        style: const TextStyle(
          color: Colors.black87,
          fontWeight: FontWeight.w600,
        ),
      ),
      onPressed: onTap,
      visualDensity: VisualDensity.compact,
    );
  }
}
