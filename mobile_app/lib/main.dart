
// CareFlow AI – Mobile App Entry Point
// Component B: Medical Triage (Sujana – IT24103033)
// Component D: Pharmacy Inventory (Amodhya – IT24102599)
// Component C: Appointments & Resource Scheduling
//
// Auth gate: checks SecureStorage on startup.
// → If PatientId exists → HomeScreen
// → Otherwise → LoginScreen

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'screens/login_screen.dart';
import 'screens/home_screen.dart';
import 'screens/my_prescriptions_screen.dart';
import 'screens/appointment_availability_screen.dart';
import 'services/auth_service.dart';
import 'theme/app_theme.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();

  SystemChrome.setSystemUIOverlayStyle(
    const SystemUiOverlayStyle(
      statusBarColor: Colors.transparent,
      statusBarIconBrightness: Brightness.light,
    ),
  );

  runApp(const CareFlowApp());
}

class CareFlowApp extends StatelessWidget {
  const CareFlowApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'CareFlow AI',
      debugShowCheckedModeBanner: false,

      // Use the shared application theme
      theme: AppTheme.theme,

      // Auth gate
      // Checks whether the patient is already logged in.
      home: const _AuthGate(),

      // Application routes
      routes: {
        '/prescriptions': (context) =>
            const MyPrescriptionsScreen(),

        // Component C: Appointment Availability
        '/appointments': (context) =>
            const AppointmentAvailabilityScreen(),
      },
    );
  }
}

// ── Auth Gate ─────────────────────────────────────────────────────────────────
// Checks SecureStorage for a saved PatientId.
// Shows a branded splash, then routes to Home or Login.
class _AuthGate extends StatefulWidget {
  const _AuthGate();

  @override
  State<_AuthGate> createState() => _AuthGateState();
}

class _AuthGateState extends State<_AuthGate> {
  @override
  void initState() {
    super.initState();
    _checkAuth();
  }

  Future<void> _checkAuth() async {
    final loggedIn = await AuthService.isLoggedIn();

    if (!mounted) return;

    Navigator.pushReplacement(
      context,
      MaterialPageRoute(
        builder: (_) =>
            loggedIn ? const HomeScreen() : const LoginScreen(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.navyDark,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 80,
              height: 80,
              decoration: BoxDecoration(
                color: AppTheme.teal,
                borderRadius: BorderRadius.circular(22),
              ),
              child: const Icon(
                Icons.add,
                color: Colors.white,
                size: 48,
              ),
            ),
            const SizedBox(height: 20),

            const Text(
              'CareFlow AI',
              style: TextStyle(
                color: Colors.white,
                fontSize: 28,
                fontWeight: FontWeight.bold,
              ),
            ),

            const SizedBox(height: 6),

            const Text(
              'SMART DIGITAL HOSPITAL',
              style: TextStyle(
                color: AppTheme.teal,
                fontSize: 11,
                fontWeight: FontWeight.w600,
                letterSpacing: 2,
              ),
            ),

            const SizedBox(height: 40),

            const SizedBox(
              width: 28,
              height: 28,
              child: CircularProgressIndicator(
                color: AppTheme.teal,
                strokeWidth: 2.5,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

