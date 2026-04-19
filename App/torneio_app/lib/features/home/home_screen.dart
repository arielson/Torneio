import 'dart:async';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../core/models/torneio_resumo.dart';
import '../../core/models/banner_app.dart';
import '../../core/providers/home_provider.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final _searchController = TextEditingController();
  Timer? _debounce;
  int _bannerIndex = 0;
  late final PageController _pageController;
  Timer? _bannerTimer;

  @override
  void initState() {
    super.initState();
    _pageController = PageController();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<HomeProvider>().carregarHome().then((_) => _iniciarCarrossel());
    });
  }

  void _iniciarCarrossel() {
    _bannerTimer?.cancel();
    final banners = context.read<HomeProvider>().banners;
    if (banners.length <= 1) return;
    _bannerTimer = Timer.periodic(const Duration(seconds: 4), (_) {
      if (!mounted) return;
      final next = (_bannerIndex + 1) % banners.length;
      _pageController.animateToPage(
        next,
        duration: const Duration(milliseconds: 400),
        curve: Curves.easeInOut,
      );
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    _debounce?.cancel();
    _bannerTimer?.cancel();
    _pageController.dispose();
    super.dispose();
  }

  void _onSearch(String q) {
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 400), () {
      context.read<HomeProvider>().buscar(q);
    });
  }

  void _abrirTorneio(String slug) {
    Navigator.pushNamed(context, '/torneio', arguments: slug);
  }

  Future<void> _abrirBanner(BannerApp banner) async {
    switch (banner.tipoDestino) {
      case 'Torneio':
        if (banner.torneioSlug.isNotEmpty) {
          Navigator.pushNamed(context, '/torneio', arguments: banner.torneioSlug);
        }
      case 'Site':
        final url = banner.destino;
        if (url != null && url.isNotEmpty) {
          await launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
        }
      case 'WhatsApp':
        final phone = banner.destino;
        if (phone != null && phone.isNotEmpty) {
          final native = Uri.parse('whatsapp://send?phone=$phone');
          final web = Uri.parse('https://wa.me/$phone');
          if (!await launchUrl(native, mode: LaunchMode.externalApplication)) {
            await launchUrl(web, mode: LaunchMode.externalApplication);
          }
        }
      case 'Instagram':
        final handle = banner.destino;
        if (handle != null && handle.isNotEmpty) {
          final native = Uri.parse('instagram://user?username=$handle');
          final web = Uri.parse('https://instagram.com/$handle');
          if (!await launchUrl(native, mode: LaunchMode.externalApplication)) {
            await launchUrl(web, mode: LaunchMode.externalApplication);
          }
        }
      case 'Email':
        final email = banner.destino;
        if (email != null && email.isNotEmpty) {
          await launchUrl(Uri.parse('mailto:$email'));
        }
      // 'Nenhum' ou qualquer outro: não faz nada
    }
  }

  Color _corStatus(String status) => switch (status) {
        'Liberado' => Colors.green,
        'Finalizado' => Colors.grey,
        _ => Colors.orange,
      };

  @override
  Widget build(BuildContext context) {
    final prov = context.watch<HomeProvider>();
    final torneios = prov.torneiosExibidos;
    final banners = prov.banners;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Torneios'),
        centerTitle: false,
      ),
      body: prov.carregando
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: () => prov.carregarHome().then((_) => _iniciarCarrossel()),
              child: CustomScrollView(
                slivers: [
                  // Search bar
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
                      child: SearchBar(
                        controller: _searchController,
                        hintText: 'Buscar torneio por nome ou slug...',
                        leading: const Icon(Icons.search),
                        trailing: [
                          if (_searchController.text.isNotEmpty)
                            IconButton(
                              icon: const Icon(Icons.clear),
                              onPressed: () {
                                _searchController.clear();
                                prov.limparBusca();
                              },
                            ),
                        ],
                        onChanged: _onSearch,
                      ),
                    ),
                  ),

                  // Banner carousel
                  if (banners.isNotEmpty && !prov.buscaAtiva)
                    SliverToBoxAdapter(
                      child: Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                        child: Column(
                          children: [
                            SizedBox(
                              height: 160,
                              child: PageView.builder(
                                controller: _pageController,
                                itemCount: banners.length,
                                onPageChanged: (i) => setState(() => _bannerIndex = i),
                                itemBuilder: (ctx, i) => _BannerCard(
                                  banner: banners[i],
                                  onTap: () => _abrirBanner(banners[i]),
                                ),
                              ),
                            ),
                            if (banners.length > 1) ...[
                              const SizedBox(height: 8),
                              Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: List.generate(
                                  banners.length,
                                  (i) => AnimatedContainer(
                                    duration: const Duration(milliseconds: 300),
                                    margin: const EdgeInsets.symmetric(horizontal: 3),
                                    width: i == _bannerIndex ? 16 : 6,
                                    height: 6,
                                    decoration: BoxDecoration(
                                      borderRadius: BorderRadius.circular(3),
                                      color: i == _bannerIndex
                                          ? Theme.of(context).colorScheme.primary
                                          : Colors.grey.shade300,
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ],
                        ),
                      ),
                    ),

                  // Section title
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 8, 16, 4),
                      child: Text(
                        prov.buscaAtiva ? 'Resultados da busca' : 'Ultimos torneios',
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                      ),
                    ),
                  ),

                  if (prov.buscando)
                    const SliverToBoxAdapter(
                      child: Padding(padding: EdgeInsets.all(24), child: Center(child: CircularProgressIndicator())),
                    )
                  else if (torneios.isEmpty)
                    SliverToBoxAdapter(
                      child: Padding(
                        padding: const EdgeInsets.all(24),
                        child: Center(
                          child: Text(
                            prov.buscaAtiva ? 'Nenhum torneio encontrado.' : 'Nenhum torneio disponivel.',
                            style: const TextStyle(color: Colors.grey),
                          ),
                        ),
                      ),
                    )
                  else
                    SliverPadding(
                      padding: const EdgeInsets.fromLTRB(16, 0, 16, 24),
                      sliver: SliverList(
                        delegate: SliverChildBuilderDelegate(
                          (ctx, i) => _TorneioCard(
                            torneio: torneios[i],
                            corStatus: _corStatus,
                            onTap: () => _abrirTorneio(torneios[i].slug),
                          ),
                          childCount: torneios.length,
                        ),
                      ),
                    ),
                ],
              ),
            ),
    );
  }
}

class _BannerCard extends StatelessWidget {
  final BannerApp banner;
  final VoidCallback onTap;
  const _BannerCard({required this.banner, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Card(
        clipBehavior: Clip.antiAlias,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        child: Stack(
          fit: StackFit.expand,
          children: [
            Image.network(
              banner.imagemUrl,
              fit: BoxFit.cover,
              errorBuilder: (context, error, stackTrace) => Container(
                color: Colors.grey.shade200,
                child: const Icon(Icons.image, size: 48, color: Colors.grey),
              ),
            ),
            Positioned(
              bottom: 0, left: 0, right: 0,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topCenter,
                    end: Alignment.bottomCenter,
                    colors: [Colors.transparent, Colors.black.withAlpha(180)],
                  ),
                ),
                child: Text(
                  banner.torneioNome,
                  style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _TorneioCard extends StatelessWidget {
  final TorneioResumo torneio;
  final Color Function(String) corStatus;
  final VoidCallback onTap;
  const _TorneioCard({required this.torneio, required this.corStatus, required this.onTap});

  @override
  Widget build(BuildContext context) {
    final cor = corStatus(torneio.status);
    return Card(
      margin: const EdgeInsets.symmetric(vertical: 6),
      child: ListTile(
        onTap: onTap,
        leading: ClipRRect(
          borderRadius: BorderRadius.circular(8),
          child: torneio.logoUrl != null
              ? Image.network(torneio.logoUrl!, width: 48, height: 48, fit: BoxFit.cover,
                  errorBuilder: (context, error, stackTrace) => _DefaultLogo())
              : _DefaultLogo(),
        ),
        title: Text(torneio.nomeTorneio, style: const TextStyle(fontWeight: FontWeight.bold)),
        subtitle: Text(torneio.slug, style: const TextStyle(color: Colors.grey, fontSize: 12)),
        trailing: Chip(
          label: Text(torneio.status, style: TextStyle(color: cor, fontSize: 11)),
          backgroundColor: cor.withAlpha(30),
          side: BorderSide(color: cor.withAlpha(80)),
          padding: EdgeInsets.zero,
          visualDensity: VisualDensity.compact,
        ),
      ),
    );
  }
}

class _DefaultLogo extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      width: 48, height: 48,
      color: Theme.of(context).colorScheme.primaryContainer,
      child: Icon(Icons.emoji_events, color: Theme.of(context).colorScheme.primary),
    );
  }
}
