class ChecklistTorneioItem {
  final String id;
  final String torneioId;
  final String item;
  final DateTime? data;
  final String? responsavel;
  final bool concluido;

  const ChecklistTorneioItem({
    required this.id,
    required this.torneioId,
    required this.item,
    this.data,
    this.responsavel,
    required this.concluido,
  });

  factory ChecklistTorneioItem.fromJson(Map<String, dynamic> json) => ChecklistTorneioItem(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        item: json['item'] as String? ?? '',
        data: json['data'] != null ? DateTime.tryParse(json['data'] as String) : null,
        responsavel: json['responsavel'] as String?,
        concluido: json['concluido'] as bool? ?? false,
      );
}
