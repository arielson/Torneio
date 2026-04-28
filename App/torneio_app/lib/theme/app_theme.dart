import 'package:flutter/material.dart';
import '../core/models/torneio_config.dart';

class AppTheme {
  AppTheme._();

  static const _defaultPrimary = Color(0xFF106962);
  static const _defaultSecondary = Color(0xFF0E5B55);

  static ThemeData get theme => _build(_defaultPrimary);

  static ThemeData fromConfig(TorneioConfig? config) =>
      _build(_parseHex(config?.corPrimaria) ?? _defaultPrimary);

  static Color? _parseHex(String? hex) {
    if (hex == null || hex.isEmpty) return null;
    final cleaned = hex.startsWith('#') ? hex.substring(1) : hex;
    if (cleaned.length != 6) return null;
    final value = int.tryParse('FF$cleaned', radix: 16);
    return value != null ? Color(value) : null;
  }

  static ThemeData _build(Color primary) {
    final colorScheme = ColorScheme(
      brightness: Brightness.light,
      primary: primary,
      onPrimary: Colors.white,
      secondary: _defaultSecondary,
      onSecondary: Colors.white,
      error: const Color(0xFFB00020),
      onError: Colors.white,
      surface: Colors.white,
      onSurface: const Color(0xFF1C1B1F),
    );

    return ThemeData(
      useMaterial3: true,
      colorScheme: colorScheme,
      appBarTheme: AppBarTheme(
        backgroundColor: primary,
        foregroundColor: Colors.white,
        elevation: 2,
        centerTitle: false,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primary,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
        ),
      ),
      floatingActionButtonTheme: FloatingActionButtonThemeData(
        backgroundColor: primary,
        foregroundColor: Colors.white,
      ),
      progressIndicatorTheme: ProgressIndicatorThemeData(
        color: primary,
      ),
      inputDecorationTheme: InputDecorationTheme(
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: BorderSide(color: primary, width: 2),
        ),
      ),
      chipTheme: ChipThemeData(
        selectedColor: primary,
        labelStyle: const TextStyle(color: Colors.white),
      ),
    );
  }
}
