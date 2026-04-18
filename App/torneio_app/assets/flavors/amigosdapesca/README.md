# Assets — Flavor: Amigos da Pesca

Coloque nesta pasta os assets visuais do flavor.

## Arquivos esperados

| Arquivo    | Dimensão recomendada | Uso                                      |
|------------|----------------------|------------------------------------------|
| `logo.png` | 400 × 400 px         | Exibido na tela de splash Flutter        |

## Notas

- O `logo.png` é referenciado em `FlavorConfig.theme.logoAsset` como
  `'assets/flavors/amigosdapesca/logo.png'`.
- Enquanto o arquivo não existir, o app exibe um ícone padrão (troféu).
- Após adicionar o arquivo, rode `flutter pub get` e reinicie o app.

## Splash nativo Android

Para usar o logo no splash nativo (antes do Flutter carregar):

1. Exporte o logo como PNG em 4 densidades:
   - `mipmap-mdpi/launch_image.png` (48×48)
   - `mipmap-hdpi/launch_image.png` (72×72)
   - `mipmap-xhdpi/launch_image.png` (96×96)
   - `mipmap-xxhdpi/launch_image.png` (144×144)
2. Coloque-os em `android/app/src/amigosdapesca/res/`.
3. Descomente o bloco `<item><bitmap ...>` nos arquivos `launch_background.xml`.
