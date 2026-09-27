// CareFlow AI – Design System
// Matches the professional healthcare UI from the reference design:
// Navy header + white card forms + teal accents

import 'package:flutter/material.dart';

class AppTheme {
  // ── Palette ────────────────────────────────────────────────────────────────
  static const Color navyDark    = Color(0xFF0D1B3E); // deep navy (header/bg)
  static const Color navyMid     = Color(0xFF1A2D5A); // mid navy (card headers, buttons)
  static const Color teal        = Color(0xFF00A896); // CareFlow teal accent
  static const Color tealLight   = Color(0xFFE8F7F5); // teal tint (info banners)
  static const Color pageWhite   = Color(0xFFF4F6FA); // page background
  static const Color cardWhite   = Color(0xFFFFFFFF); // form cards
  static const Color inputBg     = Color(0xFFF7F9FC); // input fill
  static const Color borderGray  = Color(0xFFDDE3EE); // input border
  static const Color textDark    = Color(0xFF1A2040); // primary text
  static const Color textMid     = Color(0xFF4A5568); // secondary text
  static const Color textLight   = Color(0xFF8FA3BF); // hint / disabled text
  static const Color success     = Color(0xFF38A169);
  static const Color warning     = Color(0xFFDD6B20);
  static const Color danger      = Color(0xFFE53E3E);
  static const Color pending     = Color(0xFFD69E2E);

  // ── Shadow ─────────────────────────────────────────────────────────────────
  static List<BoxShadow> cardShadow = [
    BoxShadow(
      color: const Color(0xFF0D1B3E).withValues(alpha: 0.10),
      blurRadius: 20,
      offset: const Offset(0, 8),
    ),
  ];

  static List<BoxShadow> subtleShadow = [
    BoxShadow(
      color: Colors.black.withValues(alpha: 0.06),
      blurRadius: 8,
      offset: const Offset(0, 2),
    ),
  ];

  // ── Input decoration ───────────────────────────────────────────────────────
  static InputDecoration inputDecoration({
    required String hint,
    IconData? icon,
    Widget? suffixIcon,
  }) =>
      InputDecoration(
        hintText: hint,
        hintStyle: const TextStyle(color: textLight, fontSize: 14),
        prefixIcon: icon != null
            ? Icon(icon, color: textLight, size: 20)
            : null,
        suffixIcon: suffixIcon,
        filled: true,
        fillColor: inputBg,
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: borderGray),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: teal, width: 1.5),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: danger),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: danger, width: 1.5),
        ),
        errorStyle: const TextStyle(color: danger),
      );

  // ── ThemeData ──────────────────────────────────────────────────────────────
  static ThemeData get theme => ThemeData(
        useMaterial3: true,
        scaffoldBackgroundColor: pageWhite,
        colorScheme: ColorScheme.fromSeed(
          seedColor: navyDark,
          primary: navyDark,
          secondary: teal,
          surface: cardWhite,
        ),
        appBarTheme: const AppBarTheme(
          backgroundColor: navyDark,
          foregroundColor: Colors.white,
          elevation: 0,
          centerTitle: false,
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: navyMid,
            foregroundColor: Colors.white,
            minimumSize: const Size(double.infinity, 52),
            shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(30)),
            textStyle: const TextStyle(
                fontSize: 16, fontWeight: FontWeight.w600),
            elevation: 2,
          ),
        ),
        outlinedButtonTheme: OutlinedButtonThemeData(
          style: OutlinedButton.styleFrom(
            foregroundColor: navyMid,
            side: const BorderSide(color: navyMid),
            minimumSize: const Size(double.infinity, 52),
            shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(30)),
            textStyle: const TextStyle(
                fontSize: 16, fontWeight: FontWeight.w600),
          ),
        ),
        snackBarTheme: SnackBarThemeData(
          backgroundColor: navyDark,
          contentTextStyle: const TextStyle(color: Colors.white),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
          behavior: SnackBarBehavior.floating,
        ),
      );
}
