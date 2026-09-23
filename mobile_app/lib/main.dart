import 'package:flutter/material.dart';
import 'screens/LoginScreen.dart';

void main() {
  runApp(const CareFlowMobileApp());
}

class CareFlowMobileApp extends StatelessWidget {
  const CareFlowMobileApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'CareFlow AI',
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF0AB39C),
          brightness: Brightness.light,
        ),
        fontFamily: 'Roboto',
      ),
      home: const LoginScreen(),
      debugShowCheckedModeBanner: false,
    );
  }
}