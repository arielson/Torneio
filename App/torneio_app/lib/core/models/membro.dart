class Membro {
  final String id;
  final String nome;
  final String? fotoUrl;

  const Membro({required this.id, required this.nome, this.fotoUrl});

  factory Membro.fromJson(Map<String, dynamic> json) => Membro(
        id: json['id'] as String,
        nome: json['nome'] as String,
        fotoUrl: json['fotoUrl'] as String?,
      );
}
