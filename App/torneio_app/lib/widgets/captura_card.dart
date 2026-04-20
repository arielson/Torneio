import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../core/models/captura.dart';
import 'expandable_network_image.dart';

class CapturaCard extends StatelessWidget {
  final Captura captura;
  final String medidaLabel;
  final bool mostrarFator;

  const CapturaCard({
    super.key,
    required this.captura,
    required this.medidaLabel,
    this.mostrarFator = false,
  });

  @override
  Widget build(BuildContext context) {
    final fmt = DateFormat('dd/MM/yyyy HH:mm');
    final invalida = captura.invalidada;

    Color iconColor;
    IconData iconData;
    Color bgColor;

    if (invalida) {
      iconColor = Colors.red;
      iconData = Icons.cancel;
      bgColor = Colors.red.shade50;
    } else if (captura.pendenteSync) {
      iconColor = Colors.orange;
      iconData = Icons.sync_problem;
      bgColor = Colors.orange.shade50;
    } else {
      iconColor = Colors.green;
      iconData = Icons.check_circle;
      bgColor = Colors.green.shade50;
    }

    return Card(
      margin: const EdgeInsets.symmetric(vertical: 4, horizontal: 0),
      color: invalida ? Colors.red.shade50 : null,
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          ExpandableRectImage(
            imageUrl: captura.fotoUrl,
            fallbackIcon: iconData,
            width: 72,
            borderRadius: const BorderRadius.horizontal(left: Radius.circular(12)),
          ),
          Expanded(
            child: ListTile(
              leading: CircleAvatar(
                backgroundColor: bgColor,
                child: Icon(iconData, color: iconColor),
              ),
              title: Text(
                '${captura.nomeItem} - ${captura.nomeMembro}',
                style: TextStyle(
                  fontWeight: FontWeight.w500,
                  decoration: invalida ? TextDecoration.lineThrough : null,
                  color: invalida ? Colors.red.shade700 : null,
                ),
              ),
              subtitle: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '${captura.tamanhoMedida.toStringAsFixed(1)} $medidaLabel'
                    '${mostrarFator && captura.fatorMultiplicador != 1.0 ? ' x ${captura.fatorMultiplicador.toStringAsFixed(2)}' : ''}'
                    ' = ${captura.pontuacao.toStringAsFixed(2)} pts',
                    style: invalida ? TextStyle(color: Colors.red.shade400) : null,
                  ),
                  Text(
                    fmt.format(captura.dataHora.toLocal()),
                    style: Theme.of(context).textTheme.bodySmall,
                  ),
                  if (invalida && captura.motivoInvalidacao != null)
                    Padding(
                      padding: const EdgeInsets.only(top: 2),
                      child: Text(
                        'Invalidada: ${captura.motivoInvalidacao}',
                        style: TextStyle(
                          color: Colors.red.shade600,
                          fontSize: 11,
                          fontStyle: FontStyle.italic,
                        ),
                      ),
                    ),
                ],
              ),
              isThreeLine: true,
            ),
          ),
        ],
      ),
    );
  }
}
