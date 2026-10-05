import 'package:flutter/material.dart';
import '../models/patient_profile.dart';
import '../services/api_service.dart';
import '../theme/app_theme.dart';

class MedicalProfileScreen extends StatefulWidget {
  const MedicalProfileScreen({super.key});

  @override
  State<MedicalProfileScreen> createState() => _MedicalProfileScreenState();
}

class _MedicalProfileScreenState extends State<MedicalProfileScreen> {
  PatientProfile? _profile;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final profile = await ApiService().fetchMyMedicalProfile();
      if (!mounted) return;
      setState(() {
        _profile = profile;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.toString().replaceFirst('Exception: ', '');
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.pageWhite,
      appBar: AppBar(
        backgroundColor: AppTheme.navyDark,
        foregroundColor: Colors.white,
        elevation: 0,
        title: const Text(
          'My Medical Profile',
          style: TextStyle(fontWeight: FontWeight.w600, fontSize: 18),
        ),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? _ErrorState(message: _error!, onRetry: _load)
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView(
                    padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
                    children: [
                      _HeroCard(profile: _profile!),
                      const SizedBox(height: 16),
                      _InfoCard(
                        icon: Icons.history_edu_rounded,
                        title: 'Medical History',
                        content: _profile!.medicalHistorySummary.trim().isEmpty
                            ? 'No medical history on record.'
                            : _profile!.medicalHistorySummary,
                      ),
                      const SizedBox(height: 12),
                      _InfoCard(
                        icon: Icons.bloodtype_rounded,
                        title: 'Blood Group',
                        content: _profile!.bloodGroup.isEmpty
                            ? 'Not recorded'
                            : _profile!.bloodGroup,
                      ),
                      if (_profile!.dateOfBirth.isNotEmpty) ...[
                        const SizedBox(height: 12),
                        _InfoCard(
                          icon: Icons.cake_outlined,
                          title: 'Date of Birth',
                          content: _profile!.dateOfBirth,
                        ),
                      ],
                      if ((_profile!.email ?? '').isNotEmpty) ...[
                        const SizedBox(height: 12),
                        _InfoCard(
                          icon: Icons.email_outlined,
                          title: 'Email',
                          content: _profile!.email!,
                        ),
                      ],
                      if ((_profile!.phone ?? '').isNotEmpty) ...[
                        const SizedBox(height: 12),
                        _InfoCard(
                          icon: Icons.phone_outlined,
                          title: 'Phone',
                          content: _profile!.phone!,
                        ),
                      ],
                    ],
                  ),
                ),
    );
  }
}

class _HeroCard extends StatelessWidget {
  final PatientProfile profile;
  const _HeroCard({required this.profile});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: AppTheme.navyDark,
        borderRadius: BorderRadius.circular(16),
        boxShadow: AppTheme.subtleShadow,
      ),
      child: Row(
        children: [
          Container(
            width: 56,
            height: 56,
            decoration: BoxDecoration(
              color: AppTheme.teal.withValues(alpha: 0.25),
              borderRadius: BorderRadius.circular(16),
            ),
            child: const Icon(Icons.person, color: Colors.white, size: 32),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  profile.fullName.isEmpty ? 'Patient' : profile.fullName,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  profile.bloodGroup.isEmpty
                      ? 'Personal medical record'
                      : 'Blood group ${profile.bloodGroup}',
                  style: const TextStyle(color: Color(0xFFAEC0D8), fontSize: 13),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _InfoCard extends StatelessWidget {
  final IconData icon;
  final String title;
  final String content;

  const _InfoCard({
    required this.icon,
    required this.title,
    required this.content,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppTheme.cardWhite,
        borderRadius: BorderRadius.circular(16),
        boxShadow: AppTheme.subtleShadow,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 36,
                height: 36,
                decoration: BoxDecoration(
                  color: AppTheme.teal.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Icon(icon, color: AppTheme.teal, size: 20),
              ),
              const SizedBox(width: 10),
              Text(
                title,
                style: const TextStyle(
                  fontWeight: FontWeight.w600,
                  fontSize: 15,
                  color: AppTheme.textDark,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            content,
            style: const TextStyle(
              fontSize: 14,
              color: AppTheme.textMid,
              height: 1.5,
            ),
          ),
        ],
      ),
    );
  }
}

class _ErrorState extends StatelessWidget {
  final String message;
  final VoidCallback onRetry;
  const _ErrorState({required this.message, required this.onRetry});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(28),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.folder_off_outlined, size: 48, color: AppTheme.textLight),
            const SizedBox(height: 12),
            Text(
              message,
              textAlign: TextAlign.center,
              style: const TextStyle(color: AppTheme.textMid, fontSize: 14),
            ),
            const SizedBox(height: 16),
            ElevatedButton(onPressed: onRetry, child: const Text('Try again')),
          ],
        ),
      ),
    );
  }
}
