// Component B – Home Screen (updated)
// Shows the Patient Portal landing page after login.
// My Triage Records card is now fully wired to SubmitTriageScreen and
// TriageDashboardScreen. Logout support included.

import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import 'submit_triage_screen.dart';
import 'triage_dashboard_screen.dart';
import 'my_prescriptions_screen.dart';
import 'login_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  String _patientName = '';

  static const _purple  = Color(0xFF6C63FF);
  static const _darkBg  = Color(0xFF1A1A2E);
  static const _cardBg  = Color(0xFF16213E);

  @override
  void initState() {
    super.initState();
    _loadName();
  }

  Future<void> _loadName() async {
    final name = await AuthService.getPatientName();
    if (mounted) setState(() => _patientName = name ?? 'Patient');
  }

  Future<void> _logout() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        backgroundColor: _cardBg,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Log Out',
            style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        content: const Text('Are you sure you want to log out?',
            style: TextStyle(color: Colors.white70)),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child:
                const Text('Cancel', style: TextStyle(color: Colors.white54)),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(
                backgroundColor: _purple,
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8))),
            child: const Text('Log Out',
                style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      await AuthService.logout();
      if (!mounted) return;
      Navigator.pushAndRemoveUntil(
        context,
        MaterialPageRoute(builder: (_) => const LoginScreen()),
        (_) => false,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _darkBg,
      appBar: AppBar(
        backgroundColor: _darkBg,
        title: const Row(
          children: [
            Text('🏥', style: TextStyle(fontSize: 22)),
            SizedBox(width: 8),
            Text('CareFlow AI',
                style: TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: 20)),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.white54),
            tooltip: 'Log Out',
            onPressed: _logout,
          ),
        ],
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(2),
          child: Container(height: 2, color: _purple),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Smart Digital Hospital',
                style: TextStyle(color: Colors.white54, fontSize: 14)),
            const SizedBox(height: 4),
            Text(
              'Welcome back, $_patientName 👋',
              style: const TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 28),

            // ── Component B: Triage section ──────────────────────────────────
            _sectionLabel('🩺 Triage (Component B)'),
            const SizedBox(height: 10),
            _MenuCard(
              icon: '🚑',
              title: 'Submit Symptoms',
              subtitle: 'Describe symptoms + attach a photo · AI triages instantly',
              color: _purple,
              onTap: () => Navigator.push(context,
                  MaterialPageRoute(builder: (_) => const SubmitTriageScreen())),
            ),
            const SizedBox(height: 12),
            _MenuCard(
              icon: '📋',
              title: 'My Triage Records',
              subtitle: 'Track status: Pending → In Review → Approved/Rejected',
              color: const Color(0xFF1890FF),
              onTap: () => Navigator.push(context,
                  MaterialPageRoute(
                      builder: (_) => const TriageDashboardScreen())),
            ),
            const SizedBox(height: 24),

            // ── Component D: Pharmacy section ────────────────────────────────
            _sectionLabel('💊 Pharmacy (Component D)'),
            const SizedBox(height: 10),
            _MenuCard(
              icon: '💊',
              title: 'My Prescriptions',
              subtitle: 'View e-prescriptions & AI safety reports',
              color: const Color(0xFF13C2C2),
              onTap: () => Navigator.push(context,
                  MaterialPageRoute(
                      builder: (_) => const MyPrescriptionsScreen())),
            ),
            const SizedBox(height: 24),

            // ── Coming soon ──────────────────────────────────────────────────
            _sectionLabel('📅 Coming Soon'),
            const SizedBox(height: 10),
            _MenuCard(
              icon: '📅',
              title: 'My Appointments',
              subtitle: 'Upcoming appointments — Component C',
              color: const Color(0xFF722ED1),
              onTap: () => ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                    content: Text('Appointments feature coming soon!')),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _sectionLabel(String text) => Text(
        text,
        style: const TextStyle(
            color: Colors.white38,
            fontSize: 12,
            fontWeight: FontWeight.w700,
            letterSpacing: 1),
      );
}

// ── Reusable Menu Card ─────────────────────────────────────────────────────
class _MenuCard extends StatelessWidget {
  final String      icon;
  final String      title;
  final String      subtitle;
  final Color       color;
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
        padding: const EdgeInsets.all(18),
        decoration: BoxDecoration(
          color: const Color(0xFF16213E),
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: const Color(0xFF2D2B55)),
          boxShadow: [
            BoxShadow(
                color: Colors.black.withValues(alpha: 0.15),
                blurRadius: 8,
                offset: const Offset(0, 3)),
          ],
        ),
        child: Row(
          children: [
            Container(
              width: 52,
              height: 52,
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(14),
              ),
              child: Center(
                  child: Text(icon, style: const TextStyle(fontSize: 26))),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title,
                      style: const TextStyle(
                          color: Colors.white,
                          fontSize: 15,
                          fontWeight: FontWeight.bold)),
                  const SizedBox(height: 4),
                  Text(subtitle,
                      style: const TextStyle(
                          color: Colors.white54, fontSize: 12)),
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
