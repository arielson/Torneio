import 'dart:io';
import 'package:flutter/material.dart';

class AdminPhotoPicker extends StatelessWidget {
  final String titulo;
  final String? fotoAtualUrl;
  final String? fotoLocalPath;
  final VoidCallback onTap;

  const AdminPhotoPicker({
    super.key,
    required this.titulo,
    required this.fotoAtualUrl,
    required this.fotoLocalPath,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(titulo, style: Theme.of(context).textTheme.titleSmall),
        const SizedBox(height: 8),
        GestureDetector(
          onTap: onTap,
          child: Container(
            height: 220,
            decoration: BoxDecoration(
              border: Border.all(color: Colors.grey),
              borderRadius: BorderRadius.circular(8),
              color: Colors.grey.shade100,
            ),
            child: fotoLocalPath != null
                ? ClipRRect(
                    borderRadius: BorderRadius.circular(8),
                    child: Image.file(File(fotoLocalPath!), fit: BoxFit.cover),
                  )
                : fotoAtualUrl != null
                    ? ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: Image.network(fotoAtualUrl!, fit: BoxFit.cover),
                      )
                    : const Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.add_a_photo_outlined, size: 48, color: Colors.grey),
                          SizedBox(height: 8),
                          Text(
                            'Toque para selecionar a foto',
                            style: TextStyle(color: Colors.grey),
                          ),
                        ],
                      ),
          ),
        ),
        TextButton.icon(
          onPressed: onTap,
          icon: Icon(
            fotoLocalPath != null || fotoAtualUrl != null ? Icons.refresh : Icons.upload,
          ),
          label: Text(
            fotoLocalPath != null || fotoAtualUrl != null
                ? 'Alterar foto'
                : 'Selecionar foto',
          ),
        ),
      ],
    );
  }
}
