// Component B – Login Screen (Redesigned)
// CareFlow AI healthcare aesthetic:
// Navy header, white card form, teal accents, trust badges.

import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import '../theme/app_theme.dart';
import 'register_screen.dart';
import 'home_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen>
    with SingleTickerProviderStateMixin {
  final _formKey       = GlobalKey<FormState>();
  final _patientIdCtrl = TextEditingController();
  final _nameCtrl      = TextEditingController();

  bool    _isLoading    = false;
  String? _errorMessage;

  late AnimationController _animCtrl;
  late Animation<double>   _slideAnim;
  late Animation<double>   _fadeAnim;

  // ── UUID validator ─────────────────────────────────────────────────────────
  bool _isValidUuid(String value) {
    final uuidRegex = RegExp(
      r'^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$',
      caseSensitive: false,
    );
    return uuidRegex.hasMatch(value);
  }

  @override
  void initState() {
    super.initState();
    _animCtrl = AnimationController(
        vsync: this, duration: const Duration(milliseconds: 500));
    _slideAnim = Tween<double>(begin: 40, end: 0).animate(
        CurvedAnimation(parent: _animCtrl, curve: Curves.easeOut));
    _fadeAnim = CurvedAnimation(parent: _animCtrl, curve: Curves.easeIn);
    _animCtrl.forward();
  }

  @override
  void dispose() {
    _animCtrl.dispose();
    _patientIdCtrl.dispose();
    _nameCtrl.dispose();
    super.dispose();
  }

  // ── Login handler ──────────────────────────────────────────────────────────
  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading    = true;
      _errorMessage = null;
    });

    try {
      String patientId   = _patientIdCtrl.text.trim();
      String patientName = _nameCtrl.text.trim();

      if (patientId.isEmpty) {
        final matches = await AuthService.searchPatients(patientName);
        if (matches.isEmpty) {
          setState(() => _errorMessage =
              'No patient profile found for "$patientName". Please register first.');
          return;
        }
        patientId   = matches.first['id'].toString();
        patientName = matches.first['fullName']?.toString() ?? patientName;
      }

      await AuthService.saveSession(
        patientId:   patientId,
        patientName: patientName,
      );

      if (!mounted) return;
      Navigator.pushReplacement(
          context, MaterialPageRoute(builder: (_) => const HomeScreen()));
    } catch (e) {
      setState(() => _errorMessage =
          'Login failed. Make sure the backend is running.\nDetails: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.navyDark,
      body: FadeTransition(
        opacity: _fadeAnim,
        child: Column(
          children: [
            // ── Navy header ─────────────────────────────────────────────────
            _buildHeader(),

            // ── White card form ─────────────────────────────────────────────
            Expanded(
              child: Container(
                decoration: const BoxDecoration(
                  color: AppTheme.pageWhite,
                  borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
                ),
                child: SingleChildScrollView(
                  padding: const EdgeInsets.fromLTRB(24, 28, 24, 24),
                  child: AnimatedBuilder(
                    animation: _slideAnim,
                    builder: (context, child) => Transform.translate(
                      offset: Offset(0, _slideAnim.value),
                      child: child,
                    ),
                    child: _buildCard(),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  // ── Header ─────────────────────────────────────────────────────────────────
  Widget _buildHeader() {
    return SafeArea(
      bottom: false,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(24, 20, 24, 32),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Logo row
            Row(
              children: [
                Container(
                  width: 44,
                  height: 44,
                  decoration: BoxDecoration(
                    color: AppTheme.teal,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Center(
                    child: Icon(Icons.add, color: Colors.white, size: 26),
                  ),
                ),
                const SizedBox(width: 12),
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: const [
                    Text('CareFlow AI',
                        style: TextStyle(
                            color: Colors.white,
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                            letterSpacing: 0.3)),
                    Text('SMART DIGITAL HOSPITAL',
                        style: TextStyle(
                            color: AppTheme.teal,
                            fontSize: 10,
                            fontWeight: FontWeight.w600,
                            letterSpacing: 1.5)),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 28),
            const Text('Welcome back',
                style: TextStyle(
                    color: Colors.white,
                    fontSize: 30,
                    fontWeight: FontWeight.bold)),
            const SizedBox(height: 6),
            const Text('Access your patient health portal',
                style: TextStyle(color: Color(0xFFAEC0D8), fontSize: 14)),
          ],
        ),
      ),
    );
  }

  // ── White form card ─────────────────────────────────────────────────────────
  Widget _buildCard() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // ── Secure badge ────────────────────────────────────────────────────
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
          decoration: BoxDecoration(
            color: AppTheme.tealLight,
            borderRadius: BorderRadius.circular(10),
          ),
          child: Row(
            children: const [
              Icon(Icons.verified_user_outlined,
                  color: AppTheme.teal, size: 18),
              SizedBox(width: 8),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Secure Health Login',
                        style: TextStyle(
                            color: AppTheme.teal,
                            fontWeight: FontWeight.w600,
                            fontSize: 13)),
                    Text('End-to-end encrypted · HIPAA compliant',
                        style: TextStyle(
                            color: Color(0xFF2C7A7B), fontSize: 11)),
                  ],
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 24),

        // ── Form ──────────────────────────────────────────────────────────
        Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _fieldLabel('Your Name'),
              const SizedBox(height: 6),
              TextFormField(
                controller: _nameCtrl,
                style: const TextStyle(color: AppTheme.textDark, fontSize: 14),
                decoration: AppTheme.inputDecoration(
                  hint: 'e.g. Kamal Perera',
                  icon: Icons.person_outline,
                ),
                validator: (v) {
                  if (v == null || v.trim().isEmpty) return 'Name is required';
                  if (v.trim().length < 2) return 'Enter a valid name';
                  return null;
                },
              ),
              const SizedBox(height: 16),

              _fieldLabel('Patient ID (optional — leave blank to look up by name)'),
              const SizedBox(height: 6),
              TextFormField(
                controller: _patientIdCtrl,
                style: const TextStyle(color: AppTheme.textDark, fontSize: 14),
                decoration: AppTheme.inputDecoration(
                  hint: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
                  icon: Icons.badge_outlined,
                ),
                validator: (v) {
                  if (v != null &&
                      v.trim().isNotEmpty &&
                      !_isValidUuid(v.trim())) {
                    return 'Enter a valid UUID or leave blank';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 8),

              // Forgot?
              Align(
                alignment: Alignment.centerRight,
                child: TextButton(
                  onPressed: () {},
                  style: TextButton.styleFrom(
                      padding: EdgeInsets.zero,
                      minimumSize: Size.zero,
                      tapTargetSize: MaterialTapTargetSize.shrinkWrap),
                  child: const Text('Forgot patient ID?',
                      style: TextStyle(
                          color: AppTheme.teal,
                          fontWeight: FontWeight.w600,
                          fontSize: 13)),
                ),
              ),

              // ── Error banner ─────────────────────────────────────────────
              if (_errorMessage != null) ...[
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: AppTheme.danger.withValues(alpha: 0.08),
                    borderRadius: BorderRadius.circular(10),
                    border: Border.all(
                        color: AppTheme.danger.withValues(alpha: 0.3)),
                  ),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Icon(Icons.error_outline,
                          color: AppTheme.danger, size: 18),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(_errorMessage!,
                            style: const TextStyle(
                                color: AppTheme.danger, fontSize: 12)),
                      ),
                    ],
                  ),
                ),
              ],

              const SizedBox(height: 22),

              // ── Sign in button ───────────────────────────────────────────
              SizedBox(
                width: double.infinity,
                height: 52,
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _login,
                  child: _isLoading
                      ? const SizedBox(
                          width: 22,
                          height: 22,
                          child: CircularProgressIndicator(
                              strokeWidth: 2, color: Colors.white))
                      : const Text('Sign In Securely'),
                ),
              ),
              const SizedBox(height: 20),

              // ── Register link ────────────────────────────────────────────
              Center(
                child: GestureDetector(
                  onTap: () => Navigator.push(context,
                      MaterialPageRoute(
                          builder: (_) => const RegisterScreen())),
                  child: RichText(
                    text: const TextSpan(
                      text: 'New patient? ',
                      style:
                          TextStyle(color: AppTheme.textMid, fontSize: 13),
                      children: [
                        TextSpan(
                          text: 'Create Account',
                          style: TextStyle(
                              color: AppTheme.teal,
                              fontWeight: FontWeight.w700),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),

        const SizedBox(height: 32),

        // ── Trust badges ──────────────────────────────────────────────────
        _buildTrustBadges(),
      ],
    );
  }

  Widget _fieldLabel(String text) => Text(
        text,
        style: const TextStyle(
            color: AppTheme.textDark,
            fontSize: 13,
            fontWeight: FontWeight.w600),
      );

  Widget _buildTrustBadges() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        _badge(Icons.verified_user, 'HIPAA'),
        const SizedBox(width: 24),
        _badge(Icons.lock_outline, '256-bit\nSSL'),
        const SizedBox(width: 24),
        _badge(Icons.workspace_premium_outlined, 'ISO\n27001'),
      ],
    );
  }

  Widget _badge(IconData icon, String label) => Column(
        children: [
          Icon(icon, color: AppTheme.teal, size: 20),
          const SizedBox(height: 4),
          Text(label,
              textAlign: TextAlign.center,
              style: const TextStyle(
                  color: AppTheme.textLight,
                  fontSize: 10,
                  fontWeight: FontWeight.w600)),
        ],
      );
}
