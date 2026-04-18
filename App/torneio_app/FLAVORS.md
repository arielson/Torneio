# Flavors — torneio_app

Cada flavor representa um torneio específico com identidade visual própria.

## Flavors disponíveis

| Flavor          | Slug            | Pacote Android                      | Cor primária |
|-----------------|-----------------|-------------------------------------|--------------|
| `generic`       | (digitado pelo usuário) | `com.example.torneio_app`   | Azul #1976D2 |
| `amigosdapesca` | `amigosdapesca` | `br.net.ari.torneio.amigosdapesca`  | Azul oceano #1565C0 |

## Rodando um flavor

```bash
# Flavor genérico
flutter run --flavor generic -t lib/main.dart

# Amigos da Pesca
flutter run --flavor amigosdapesca -t lib/main_amigosdapesca.dart

# Release
flutter build apk --flavor amigosdapesca -t lib/main_amigosdapesca.dart
flutter build appbundle --flavor amigosdapesca -t lib/main_amigosdapesca.dart
```

## Criando um novo flavor

### 1. Entry point Dart

Crie `lib/main_<slug>.dart` inicializando o `FlavorConfig`:

```dart
void main() {
  WidgetsFlutterBinding.ensureInitialized();
  FlavorConfig.initialize(const FlavorConfig(
    fixedSlug: '<slug>',
    appName: 'Nome do Torneio',
    packageId: 'br.net.ari.torneio.<slug>',
    apiBaseUrl: 'https://torneio.ari.net.br',
    theme: FlavorTheme(
      primaryColor: Color(0xFF...),
      secondaryColor: Color(0xFF...),
      splashBackground: Color(0xFF...),
      logoAsset: 'assets/flavors/<slug>/logo.png',
    ),
  ));
  runApp(const TorneioApp());
}
```

### 2. Android — build.gradle.kts

Adicione em `productFlavors`:

```kotlin
create("<slug>") {
    dimension = "torneio"
    applicationId = "br.net.ari.torneio.<slug>"
    resValue("string", "app_name", "Nome do Torneio")
}
```

### 3. Android — splash nativo

Crie a estrutura:

```
android/app/src/<slug>/res/
├── drawable/launch_background.xml
├── drawable-v21/launch_background.xml
└── values/colors.xml          ← define @color/splash_bg
```

### 4. Assets

Crie `assets/flavors/<slug>/logo.png` e declare no `pubspec.yaml`:

```yaml
flutter:
  assets:
    - assets/flavors/<slug>/
```

### 5. iOS (adicionar manualmente no Xcode)

1. Abra `ios/Runner.xcworkspace` no Xcode.
2. Em **Product → Scheme → Manage Schemes**, duplique o scheme `Runner`.
3. Renomeie para o nome do flavor.
4. Em **Build Settings**, defina `PRODUCT_BUNDLE_IDENTIFIER` para o `packageId` do flavor.
5. Crie `ios/config/<slug>.xcconfig` com variáveis do flavor.
6. Para splash personalizado: adicione `LaunchImage` em **Assets.xcassets** específico do scheme.

> **Nota:** O Xcode não usa os diretórios `productFlavors` do Android.  
> A configuração iOS é feita via schemes e xcconfig.
