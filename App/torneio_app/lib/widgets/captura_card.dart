import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../core/models/captura.dart';

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
    return Card(
      margin: const EdgeInsets.symmetric(vertical: 4, horizontal: 0),
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: captura.pendenteSync ? Colors.orange.shade100 : Colors.green.shade100,
          child: Icon(
            captura.pendenteSync ? Icons.sync_problem : Icons.check_circle,
            color: captura.pendenteSync ? Colors.orange : Colors.green,
          ),
        ),
        title: Text(
          '${captura.nomeItem} — ${captura.nomeMembro}',
          style: const TextStyle(fontWeight: FontWeight.w500),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              '${captura.tamanhoMedida.toStringAsFixed(1)} $medidaLabel'
              '${mostrarFator && captura.fatorMultiplicador != 1.0 ? ' × ${captura.fatorMultiplicador.toStringAsFixed(2)}' : ''}'
              ' = ${captura.pontuacao.toStringAsFixed(2)} pts',
            ),
            Text(
              fmt.format(captura.dataHora.toLocal()),
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
        isThreeLine: true,
      ),
    );
  }
}
