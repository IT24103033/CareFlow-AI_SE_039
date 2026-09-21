// CareFlow AI – Mobile App Entry Point
// Component D: Pharmacy Inventory & E-Prescriptions (Amodhya)

import 'package:flutter/material.dart';
import 'screens/my_prescriptions_screen.dart';

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
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.dark(
          primary:   const Color(0xFF6C63FF),
          secondary: const Color(0xFF8B5CF6),
          surface:   const Color(0xFF16213E),
          background: const Color(0xFF1A1A2E),
        ),
        scaffoldBackgroundColor: const Color(0xFF1A1A2E),
        fontFamily: 'Roboto',
        appBarTheme: const AppBarTheme(
          backgroundColor: Color(0xFF1A1A2E),
          foregroundColor: Colors.white,
          elevation: 0,
        ),
      ),
      // ── Routing ────────────────────────────────────────────────────────────
      home: const CareFlowHome(),
      routes: {
        '/prescriptions': (context) => const MyPrescriptionsScreen(),
      },
    );
  }
}

// ── Home Screen ───────────────────────────────────────────────────────────────
class CareFlowHome extends StatelessWidget {
  const CareFlowHome({super.key});

  @override
  Widget build(BuildContext context) {
    const purple = Color(0xFF6C63FF);
    const darkBg = Color(0xFF1A1A2E);
    const cardBg = Color(0xFF16213E);

    return Scaffold(
      backgroundColor: darkBg,
      appBar: AppBar(
        title: const Row(
          children: [
            Text('🏥', style: TextStyle(fontSize: 22)),
            SizedBox(width: 8),
            Text('CareFlow AI',
              style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 20)),
          ],
        ),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(2),
          child: Container(height: 2, color: purple),
        ),
      ),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Smart Digital Hospital',
              style: TextStyle(color: Colors.white70, fontSize: 14),
            ),
            const SizedBox(height: 8),
            const Text(
              'Patient Portal',
              style: TextStyle(color: Colors.white, fontSize: 26, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 28),

            // ── Menu Cards ──────────────────────────────────────────────────
            _MenuCard(
              icon: '💊',
              title: 'My Prescriptions',
              subtitle: 'View e-prescriptions & AI safety reports',
              color: purple,
              onTap: () => Navigator.pushNamed(context, '/prescriptions'),
            ),
            const SizedBox(height: 16),
            // Placeholder cards for other components
            _MenuCard(
              icon: '📋',
              title: 'My Triage Records',
              subtitle: 'Symptom submissions and triage status',
              color: const Color(0xFF1890FF),
              onTap: () {},
            ),
            const SizedBox(height: 16),
            _MenuCard(
              icon: '📅',
              title: 'My Appointments',
              subtitle: 'Upcoming appointments and schedules',
              color: const Color(0xFF13C2C2),
              onTap: () {},
            ),
          ],
        ),
      ),
    );
  }
}

class _MenuCard extends StatelessWidget {
  final String   icon;
  final String   title;
  final String   subtitle;
  final Color    color;
  final VoidCallback onTap;

  const _MenuCard({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      borderRadius: BorderRadius.circular(14),
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: const Color(0xFF16213E),
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: const Color(0xFF2D2B55)),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.15), blurRadius: 8, offset: const Offset(0, 3)),
          ],
        ),
        child: Row(
          children: [
            Container(
              width: 52, height: 52,
              decoration: BoxDecoration(
                color: color.withOpacity(0.15),
                borderRadius: BorderRadius.circular(14),
              ),
              child: Center(child: Text(icon, style: const TextStyle(fontSize: 26))),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title, style: const TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.bold)),
                  const SizedBox(height: 4),
                  Text(subtitle, style: const TextStyle(color: Colors.white54, fontSize: 12)),
                ],
              ),
            ),
            Icon(Icons.arrow_forward_ios, color: color, size: 16),
          ],
        ),
      ),
    );
  }
}
