class Item {
  final String id;
  final String nome;
  final double comprimento;
  final double fatorMultiplicador;
  final String? fotoUrl;

  const Item({
    required this.id,
    required this.nome,
    required this.comprimento,
    required this.fatorMultiplicador,
    this.fotoUrl,
  });

  factory Item.fromJson(Map<String, dynamic> json) => Item(
        id: json['id'] as String,
        nome: json['nome'] as String,
        comprimento: (json['comprimento'] as num).toDouble(),
        fatorMultiplicador: (json['fatorMultiplicador'] as num).toDouble(),
        fotoUrl: json['fotoUrl'] as String?,
      );
}
