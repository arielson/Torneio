class Grupo {
  final String id;
  final String nome;
  final List<GrupoMembroItem> membros;

  const Grupo({
    required this.id,
    required this.nome,
    required this.membros,
  });

  factory Grupo.fromJson(Map<String, dynamic> json) => Grupo(
        id: json['id'] as String,
        nome: json['nome'] as String,
        membros: (json['membros'] as List<dynamic>? ?? [])
            .map((m) => GrupoMembroItem.fromJson(m as Map<String, dynamic>))
            .toList(),
      );
}

class GrupoMembroItem {
  final String id;
  final String membroId;
  final String nomeMembro;

  const GrupoMembroItem({
    required this.id,
    required this.membroId,
    required this.nomeMembro,
  });

  factory GrupoMembroItem.fromJson(Map<String, dynamic> json) => GrupoMembroItem(
        id: json['id'] as String,
        membroId: json['membroId'] as String,
        nomeMembro: json['nomeMembro'] as String? ?? '',
      );
}
