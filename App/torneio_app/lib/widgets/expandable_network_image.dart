import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';

class ExpandableAvatar extends StatelessWidget {
  final String? imageUrl;
  final IconData fallbackIcon;
  final double radius;

  const ExpandableAvatar({
    super.key,
    required this.imageUrl,
    required this.fallbackIcon,
    this.radius = 24,
  });

  @override
  Widget build(BuildContext context) {
    if (imageUrl == null || imageUrl!.isEmpty) {
      return CircleAvatar(radius: radius, child: Icon(fallbackIcon));
    }

    return GestureDetector(
      onTap: () => showExpandedImage(context, imageUrl!),
      child: CircleAvatar(
        radius: radius,
        backgroundColor: Colors.grey.shade200,
        child: ClipOval(
          child: SizedBox.expand(
            child: CachedNetworkImage(
              imageUrl: imageUrl!,
              fit: BoxFit.cover,
              errorWidget: (ctx, url, err) => Icon(fallbackIcon),
            ),
          ),
        ),
      ),
    );
  }
}

class ExpandableRectImage extends StatelessWidget {
  final String? imageUrl;
  final IconData fallbackIcon;
  final double width;
  final double? height;
  final BorderRadius borderRadius;

  const ExpandableRectImage({
    super.key,
    required this.imageUrl,
    required this.fallbackIcon,
    this.width = 72,
    this.height,
    this.borderRadius = const BorderRadius.all(Radius.circular(12)),
  });

  @override
  Widget build(BuildContext context) {
    final content = imageUrl != null && imageUrl!.isNotEmpty
        ? CachedNetworkImage(
            imageUrl: imageUrl!,
            fit: BoxFit.cover,
            errorWidget: (ctx, url, err) => _RectPlaceholder(icon: fallbackIcon),
          )
        : _RectPlaceholder(icon: fallbackIcon);

    final child = ClipRRect(
      borderRadius: borderRadius,
      child: SizedBox(
        width: width,
        height: height,
        child: content,
      ),
    );

    if (imageUrl == null || imageUrl!.isEmpty) {
      return child;
    }

    return GestureDetector(
      onTap: () => showExpandedImage(context, imageUrl!),
      child: child,
    );
  }
}

Future<void> showExpandedImage(BuildContext context, String imageUrl) {
  return showDialog<void>(
    context: context,
    builder: (_) => Dialog(
      insetPadding: const EdgeInsets.all(16),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Flexible(
            child: InteractiveViewer(
              minScale: 0.8,
              maxScale: 4,
              child: CachedNetworkImage(
                imageUrl: imageUrl,
                fit: BoxFit.contain,
                errorWidget: (ctx, url, err) => const Padding(
                  padding: EdgeInsets.all(32),
                  child: Icon(Icons.broken_image, size: 64, color: Colors.grey),
                ),
              ),
            ),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Fechar'),
          ),
        ],
      ),
    ),
  );
}

class _RectPlaceholder extends StatelessWidget {
  final IconData icon;

  const _RectPlaceholder({required this.icon});

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.grey.shade100,
      child: Icon(icon, color: Colors.grey, size: 28),
    );
  }
}
