// Component B – Home Screen (Redesigned)
// CareFlow AI healthcare aesthetic: navy sidebar/header + white card grid.

import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import '../theme/app_theme.dart';
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
        backgroundColor: AppTheme.cardWhite,
        shape:
            RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Sign Out',
            style: TextStyle(
                color: AppTheme.textDark, fontWeight: FontWeight.bold)),
        content: const Text('Are you sure you want to sign out?',
            style: TextStyle(color: AppTheme.textMid)),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel',
                style: TextStyle(color: AppTheme.textMid)),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(
                backgroundColor: AppTheme.danger,
                minimumSize: Size.zero,
                padding:
                    const EdgeInsets.symmetric(horizontal: 18, vertical: 10),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8))),
            child: const Text('Sign Out',
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
      backgroundColor: AppTheme.pageWhite,
      body: CustomScrollView(
        slivers: [
          // ── Navy header ──────────────────────────────────────────────────
          SliverToBoxAdapter(child: _buildHeader()),

          // ── Body ─────────────────────────────────────────────────────────
          SliverPadding(
            padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
            sliver: SliverList(
              delegate: SliverChildListDelegate([
                _sectionLabel('TRIAGE SERVICES'),
                const SizedBox(height: 12),
                _MenuCard(
                  icon: Icons.medical_services_outlined,
                  iconColor: AppTheme.teal,
                  title: 'Submit Symptoms',
                  subtitle:
                      'Describe symptoms & attach a photo · AI triages instantly',
                  onTap: () => Navigator.push(context,
                      MaterialPageRoute(
                          builder: (_) => const SubmitTriageScreen())),
                ),
                const SizedBox(height: 12),
                _MenuCard(
                  icon: Icons.list_alt_outlined,
                  iconColor: const Color(0xFF3182CE),
                  title: 'My Triage Records',
                  subtitle:
                      'Track status: Pending → In Review → Approved / Rejected',
                  onTap: () => Navigator.push(context,
                      MaterialPageRoute(
                          builder: (_) => const TriageDashboardScreen())),
                ),
                const SizedBox(height: 28),

                _sectionLabel('PHARMACY'),
                const SizedBox(height: 12),
                _MenuCard(
                  icon: Icons.local_pharmacy_outlined,
                  iconColor: const Color(0xFF805AD5),
                  title: 'My Prescriptions',
                  subtitle: 'View e-prescriptions & AI safety reports',
                  onTap: () => Navigator.push(context,
                      MaterialPageRoute(
                          builder: (_) => const MyPrescriptionsScreen())),
                ),
                const SizedBox(height: 28),

                _sectionLabel('COMING SOON'),
                const SizedBox(height: 12),
                _MenuCard(
                  icon: Icons.calendar_month_outlined,
                  iconColor: const Color(0xFFD69E2E),
                  title: 'My Appointments',
                  subtitle: 'Upcoming appointments — Component C',
                  badge: 'Soon',
                  onTap: () =>
                      ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                        content:
                            Text('Appointments feature coming soon!')),
                  ),
                ),
              ]),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildHeader() {
    return Container(
      color: AppTheme.navyDark,
      child: SafeArea(
        bottom: false,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Top row: logo + logout
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 16, 0),
              child: Row(
                children: [
                  // Logo
                  Container(
                    width: 36,
                    height: 36,
                    decoration: BoxDecoration(
                      color: AppTheme.teal,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: const Icon(Icons.add,
                        color: Colors.white, size: 22),
                  ),
                  const SizedBox(width: 10),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: const [
                      Text('CareFlow AI',
                          style: TextStyle(
                              color: Colors.white,
                              fontSize: 16,
                              fontWeight: FontWeight.bold)),
                      Text('SMART DIGITAL HOSPITAL',
                          style: TextStyle(
                              color: AppTheme.teal,
                              fontSize: 9,
                              fontWeight: FontWeight.w600,
                              letterSpacing: 1.5)),
                    ],
                  ),
                  const Spacer(),
                  IconButton(
                    icon: const Icon(Icons.logout_outlined,
                        color: Color(0xFFAEC0D8)),
                    tooltip: 'Sign Out',
                    onPressed: _logout,
                  ),
                ],
              ),
            ),

            // Greeting
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 24, 20, 0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Hello, $_patientName 👋',
                    style: const TextStyle(
                        color: Colors.white,
                        fontSize: 26,
                        fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 4),
                  const Text('How can we help you today?',
                      style: TextStyle(
                          color: Color(0xFFAEC0D8), fontSize: 14)),
                ],
              ),
            ),

            // Quick stats row
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 20, 20, 0),
              child: Row(
                children: [
                  _statPill(Icons.shield_outlined, 'HIPAA Compliant'),
                  const SizedBox(width: 10),
                  _statPill(Icons.speed_outlined, 'AI-Powered Triage'),
                ],
              ),
            ),

            // Curved bottom
            Container(
              height: 28,
              margin: const EdgeInsets.only(top: 20),
              decoration: const BoxDecoration(
                color: AppTheme.pageWhite,
                borderRadius:
                    BorderRadius.vertical(top: Radius.circular(28)),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _statPill(IconData icon, String label) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
        decoration: BoxDecoration(
          color: Colors.white.withValues(alpha: 0.10),
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
              color: Colors.white.withValues(alpha: 0.15)),
        ),
        child: Row(
          children: [
            Icon(icon, color: AppTheme.teal, size: 14),
            const SizedBox(width: 6),
            Text(label,
                style: const TextStyle(
                    color: Colors.white70,
                    fontSize: 11,
                    fontWeight: FontWeight.w500)),
          ],
        ),
      );

  Widget _sectionLabel(String text) => Text(
        text,
        style: const TextStyle(
            color: AppTheme.textLight,
            fontSize: 11,
            fontWeight: FontWeight.w700,
            letterSpacing: 1.5),
      );
}

// ── Reusable Menu Card ─────────────────────────────────────────────────────
class _MenuCard extends StatelessWidget {
  final IconData     icon;
  final Color        iconColor;
  final String       title;
  final String       subtitle;
  final VoidCallback onTap;
  final String?      badge;

  const _MenuCard({
    required this.icon,
    required this.iconColor,
    required this.title,
    required this.subtitle,
    required this.onTap,
    this.badge,
  });

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppTheme.cardWhite,
      borderRadius: BorderRadius.circular(16),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            boxShadow: AppTheme.subtleShadow,
          ),
          child: Row(
            children: [
              // Icon bubble
              Container(
                width: 50,
                height: 50,
                decoration: BoxDecoration(
                  color: iconColor.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Icon(icon, color: iconColor, size: 26),
              ),
              const SizedBox(width: 14),

              // Text
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Text(title,
                            style: const TextStyle(
                                color: AppTheme.textDark,
                                fontSize: 15,
                                fontWeight: FontWeight.w600)),
                        if (badge != null) ...[
                          const SizedBox(width: 8),
                          Container(
                            padding: const EdgeInsets.symmetric(
                                horizontal: 7, vertical: 2),
                            decoration: BoxDecoration(
                              color: AppTheme.pending
                                  .withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(20),
                            ),
                            child: Text(badge!,
                                style: const TextStyle(
                                    color: AppTheme.pending,
                                    fontSize: 10,
                                    fontWeight: FontWeight.w700)),
                          ),
                        ],
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(subtitle,
                        style: const TextStyle(
                            color: AppTheme.textMid, fontSize: 12)),
                  ],
                ),
              ),

              // Arrow
              const Icon(Icons.arrow_forward_ios,
                  color: AppTheme.textLight, size: 15),
            ],
          ),
        ),
      ),
    );
  }
}
