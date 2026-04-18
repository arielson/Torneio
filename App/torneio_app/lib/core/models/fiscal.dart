class Fiscal {
  final String id;
  final String torneioId;
  final String nome;
  final String? fotoUrl;
  final String usuario;

  const Fiscal({
    required this.id,
    required this.torneioId,
    required this.nome,
    this.fotoUrl,
    required this.usuario,
  });

  factory Fiscal.fromJson(Map<String, dynamic> json) => Fiscal(
        id: json['id'] as String,
        torneioId: json['torneioId'] as String,
        nome: json['nome'] as String,
        fotoUrl: json['fotoUrl'] as String?,
        usuario: json['usuario'] as String,
      );
}
