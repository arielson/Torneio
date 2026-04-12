class UsuarioAutenticado {
  final String id;
  final String nome;
  final String perfil; // "AdminGeral" | "AdminTorneio" | "Fiscal"
  final String? torneioId;
  final String? slug;
  final String token;
  final DateTime expiraEm;

  const UsuarioAutenticado({
    required this.id,
    required this.nome,
    required this.perfil,
    this.torneioId,
    this.slug,
    required this.token,
    required this.expiraEm,
  });

  bool get isFiscal => perfil == 'Fiscal';
  bool get isAdminTorneio => perfil == 'AdminTorneio';
  bool get isAdminGeral => perfil == 'AdminGeral';
  bool get tokenExpirado => DateTime.now().isAfter(expiraEm);
}
