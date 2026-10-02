// CareFlow AI – Design System
// Light healthcare theme with pastel blue and green colors.

import 'package:flutter/material.dart';

class AppTheme {
  // Main colors
  static const Color pageWhite = Color(0xFFF8FBFC);
  static const Color cardWhite = Color(0xFFFFFFFF);

  // Pastel blue
  static const Color bluePastel = Color(0xFFDCEFF7);
  static const Color blueLight = Color(0xFFEAF6FA);
  static const Color blueAccent = Color(0xFF7DBDD8);
  static const Color blueDark = Color(0xFF397A96);

  // Pastel green
  static const Color greenPastel = Color(0xFFDDF3E4);
  static const Color greenLight = Color(0xFFEDF9F1);
  static const Color greenAccent = Color(0xFF86C79A);
  static const Color greenDark = Color(0xFF4E9565);

  // Text
  static const Color textDark = Color(0xFF334155);
  static const Color textMid = Color(0xFF64748B);
  static const Color textLight = Color(0xFF94A3B8);

  // Form
  static const Color inputBg = Color(0xFFF7FAFB);
  static const Color borderGray = Color(0xFFDCE6EA);

  // Status
  static const Color success = Color(0xFF5EAA73);
  static const Color warning = Color(0xFFD9A441);
  static const Color danger = Color(0xFFD96B6B);
  static const Color pending = Color(0xFFD9A441);

  // Compatibility colors used by other existing screens
  static const Color navyDark = blueDark;
  static const Color navyMid = Color(0xFF6FAFC8);
  static const Color teal = Color(0xFF79BFA0);
  static const Color tealLight = Color(0xFFE8F6EE);

  static List<BoxShadow> cardShadow = [
    BoxShadow(
      color: Colors.black.withValues(alpha: 0.06),
      blurRadius: 18,
      offset: const Offset(0, 6),
    ),
  ];

  static List<BoxShadow> subtleShadow = [
    BoxShadow(
      color: Colors.black.withValues(alpha: 0.04),
      blurRadius: 8,
      offset: const Offset(0, 2),
    ),
  ];

  static InputDecoration inputDecoration({
    required String hint,
    IconData? icon,
    Widget? suffixIcon,
  }) {
    return InputDecoration(
      hintText: hint,
      hintStyle: const TextStyle(
        color: textLight,
        fontSize: 14,
      ),
      prefixIcon: icon != null
          ? const Icon(
              Icons.search,
              color: textLight,
              size: 20,
            )
          : null,
      suffixIcon: suffixIcon,
      filled: true,
      fillColor: inputBg,
      contentPadding: const EdgeInsets.symmetric(
        horizontal: 16,
        vertical: 14,
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(
          color: borderGray,
        ),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(
          color: blueAccent,
          width: 1.5,
        ),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(
          color: danger,
        ),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(
          color: danger,
          width: 1.5,
        ),
      ),
      errorStyle: const TextStyle(
        color: danger,
      ),
    );
  }

  static ThemeData get theme {
    return ThemeData(
      useMaterial3: true,

      scaffoldBackgroundColor: pageWhite,

      colorScheme: ColorScheme.fromSeed(
        seedColor: blueAccent,
        primary: blueAccent,
        secondary: greenAccent,
        surface: cardWhite,
      ),

      appBarTheme: const AppBarTheme(
        backgroundColor: cardWhite,
        foregroundColor: textDark,
        elevation: 0,
        centerTitle: false,
      ),

      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: blueAccent,
          foregroundColor: Colors.white,
          minimumSize: const Size(
            double.infinity,
            52,
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14),
          ),
          textStyle: const TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.w600,
          ),
          elevation: 0,
        ),
      ),

      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: blueDark,
          side: const BorderSide(
            color: blueAccent,
          ),
          minimumSize: const Size(
            double.infinity,
            52,
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14),
          ),
          textStyle: const TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),

      snackBarTheme: SnackBarThemeData(
        backgroundColor: textDark,
        contentTextStyle: const TextStyle(
          color: Colors.white,
        ),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }
}