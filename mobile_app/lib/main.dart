// CareFlow AI – Mobile App Entry Point
// Component B: Medical Triage (Sujana – IT24103033)
// Component D: Pharmacy Inventory (Amodhya – IT24102599)
// Component C: Appointments & Resource Scheduling

import 'package:flutter/material.dart';

import 'screens/LoginScreen.dart';
import 'screens/home_screen.dart';
import 'screens/appointment_availability_screen.dart';
import 'screens/my_prescriptions_screen.dart';
import 'services/auth_service.dart';
import 'theme/app_theme.dart';

void main() {
  runApp(const CareFlowApp());
}

class CareFlowApp extends StatelessWidget {
  const CareFlowApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'CareFlow AI',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.theme,
      home: const _AuthGate(),
      routes: {
        '/login': (context) => const LoginScreen(),
        '/home': (context) => const HomeScreen(),
        '/appointments': (context) =>
            const AppointmentAvailabilityScreen(),
        '/prescriptions': (context) =>
            const MyPrescriptionsScreen(),
      },
    );
  }
}

class _AuthGate extends StatefulWidget {
  const _AuthGate({super.key});

  @override
  State<_AuthGate> createState() => _AuthGateState();
}

class _AuthGateState extends State<_AuthGate> {
  bool _loading = true;
  bool _loggedIn = false;

  @override
  void initState() {
    super.initState();
    _checkLogin();
  }

  Future<void> _checkLogin() async {
    final loggedIn = await AuthService.isLoggedIn();

    if (!mounted) return;

    setState(() {
      _loggedIn = loggedIn;
      _loading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(
        body: Center(
          child: CircularProgressIndicator(),
        ),
      );
    }

    if (_loggedIn) {
      return const HomeScreen();
    }

    return const LoginScreen();
  }
}