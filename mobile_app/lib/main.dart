import 'package:flutter/material.dart';
import 'screens/PatientHomeScreen.dart';

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
        primarySwatch: Colors.blue,
        useMaterial3: true,
      ),
      home: const PatientHomeScreen(),
      debugShowCheckedModeBanner: false,
    );
  }
}