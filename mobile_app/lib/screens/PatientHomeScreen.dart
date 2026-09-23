import 'package:flutter/material.dart';
import '../models/PatientProfile.dart';
import '../services/ApiService.dart';
import '../models/Ward.dart';
import 'MobileWardStatus.dart';
import 'LoginScreen.dart';

class PatientHomeScreen extends StatelessWidget {
  final PatientProfile patient;
  const PatientHomeScreen({super.key, required this.patient});

  @override
  Widget build(BuildContext context) {
    return _PatientScaffold(patient: patient);
  }
}

class _PatientScaffold extends StatefulWidget {
  final PatientProfile patient;
  const _PatientScaffold({required this.patient});

  @override
  State<_PatientScaffold> createState() => _PatientScaffoldState();
}

class _PatientScaffoldState extends State<_PatientScaffold> {
  int _selectedIndex = 0;
  final _primaryTeal = const Color(0xFF0AB39C);

  void _handleLogout() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Logout'),
        content: const Text('Are you sure you want to log out?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Cancel')),
          TextButton(
            onPressed: () {
              Navigator.pop(ctx);
              Navigator.of(context).pushReplacement(
                MaterialPageRoute(builder: (_) => const LoginScreen()),
              );
            },
            child: const Text('Logout', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final screens = [
      _ProfileView(patient: widget.patient),
      const MobileWardStatus(),
    ];

    return Scaffold(
      backgroundColor: const Color(0xFFF0FAFA),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _selectedIndex,
        onTap: (i) => setState(() => _selectedIndex = i),
        selectedItemColor: _primaryTeal,
        unselectedItemColor: Colors.grey,
        backgroundColor: Colors.white,
        elevation: 8,
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.person_outline), activeIcon: Icon(Icons.person), label: 'My Profile'),
          BottomNavigationBarItem(icon: Icon(Icons.bed_outlined), activeIcon: Icon(Icons.bed), label: 'Wards'),
        ],
      ),
      body: screens[_selectedIndex],
    );
  }
}

class _ProfileView extends StatelessWidget {
  final PatientProfile patient;
  final _primaryTeal = const Color(0xFF0AB39C);

  const _ProfileView({required this.patient});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF0FAFA),
      appBar: AppBar(
        title: const Text('My Medical Profile',
            style: TextStyle(fontWeight: FontWeight.w600, fontSize: 18)),
        backgroundColor: const Color(0xFF0AB39C),
        foregroundColor: Colors.white,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout_rounded),
            tooltip: 'Logout',
            onPressed: () {
              showDialog(
                context: context,
                builder: (ctx) => AlertDialog(
                  title: const Text('Logout'),
                  content: const Text('Are you sure you want to log out?'),
                  actions: [
                    TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Cancel')),
                    TextButton(
                      onPressed: () {
                        Navigator.pop(ctx);
                        Navigator.of(context).pushReplacement(
                          MaterialPageRoute(builder: (_) => const LoginScreen()),
                        );
                      },
                      child: const Text('Logout', style: TextStyle(color: Colors.red)),
                    ),
                  ],
                ),
              );
            },
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            // Hero card
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [const Color(0xFF0AB39C), const Color(0xFF08907D)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(16),
              ),
              child: Row(
                children: [
                  CircleAvatar(
                    radius: 32,
                    backgroundColor: Colors.white.withOpacity(0.2),
                    child: const Icon(Icons.person, size: 36, color: Colors.white),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(patient.fullName,
                            style: const TextStyle(
                                color: Colors.white,
                                fontSize: 20,
                                fontWeight: FontWeight.bold)),
                        const SizedBox(height: 4),
                        Text('Blood Group: ${patient.bloodGroup}',
                            style: const TextStyle(color: Colors.white70, fontSize: 14)),
                      ],
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 20),

            // Medical history card
            _InfoCard(
              icon: Icons.history_edu_rounded,
              title: 'Medical History',
              content: patient.medicalHistorySummary.isNotEmpty
                  ? patient.medicalHistorySummary
                  : 'No medical history on record.',
            ),

            const SizedBox(height: 16),

            // Blood group card
            _InfoCard(
              icon: Icons.bloodtype_rounded,
              title: 'Blood Group',
              content: patient.bloodGroup,
            ),
          ],
        ),
      ),
    );
  }
}

class _InfoCard extends StatelessWidget {
  final IconData icon;
  final String title;
  final String content;

  const _InfoCard({required this.icon, required this.title, required this.content});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          )
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, color: const Color(0xFF0AB39C), size: 20),
              const SizedBox(width: 8),
              Text(title,
                  style: const TextStyle(
                      fontWeight: FontWeight.w600,
                      fontSize: 15,
                      color: Color(0xFF212529))),
            ],
          ),
          const SizedBox(height: 10),
          Text(content, style: const TextStyle(fontSize: 14, color: Colors.black54, height: 1.5)),
        ],
      ),
    );
  }
}