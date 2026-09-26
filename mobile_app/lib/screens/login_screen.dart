// Component B – Login Screen
// Patients enter their PatientId (UUID) to start a session.
// The ID is stored securely via flutter_secure_storage (Keychain/Keystore).
// NOTE: The backend has no JWT /auth/login endpoint, so we store PatientId
// directly as the session identifier. This matches the CreateTriageRequestDto.

import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import 'register_screen.dart';
import 'home_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen>
    with SingleTickerProviderStateMixin {
  final _formKey        = GlobalKey<FormState>();
  final _patientIdCtrl  = TextEditingController();
  final _nameCtrl       = TextEditingController();

  bool _isLoading = false;
  String? _errorMessage;

  late AnimationController _animCtrl;
  late Animation<double>   _fadeAnim;

  // ── Brand colours (shared with the rest of the app) ───────────────────────
  static const _purple  = Color(0xFF6C63FF);
  static const _darkBg  = Color(0xFF1A1A2E);
  static const _cardBg  = Color(0xFF16213E);
  static const _border  = Color(0xFF2D2B55);

  @override
  void initState() {
    super.initState();
    _animCtrl = AnimationController(
        vsync: this, duration: const Duration(milliseconds: 700));
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

  // ── Validate UUID format ───────────────────────────────────────────────────
  bool _isValidUuid(String value) {
    final uuidRegex = RegExp(
      r'^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$',
      caseSensitive: false,
    );
    return uuidRegex.hasMatch(value);
  }

  // ── Login handler ──────────────────────────────────────────────────────────
  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading    = true;
      _errorMessage = null;
    });

    try {
      String patientId = _patientIdCtrl.text.trim();
      String patientName = _nameCtrl.text.trim();

      if (patientId.isEmpty) {
        // Auto-lookup by name in the database
        final matches = await AuthService.searchPatients(patientName);
        if (matches.isEmpty) {
          setState(() => _errorMessage =
              'No patient profile found for "$patientName". Please register first.');
          return;
        }
        patientId = matches.first['id'].toString();
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
      setState(() => _errorMessage = 'Login failed. Make sure the backend is running.\nDetails: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _darkBg,
      body: FadeTransition(
        opacity: _fadeAnim,
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 40),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // ── Logo / Header ─────────────────────────────────────────
                  Center(
                    child: Container(
                      width: 80, height: 80,
                      decoration: BoxDecoration(
                        color: _purple.withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(24),
                        border: Border.all(color: _purple.withValues(alpha: 0.4)),
                      ),
                      child: const Center(
                        child: Text('🏥', style: TextStyle(fontSize: 40)),
                      ),
                    ),
                  ),
                  const SizedBox(height: 28),
                  Center(
                    child: Column(
                      children: const [
                        Text('CareFlow AI',
                            style: TextStyle(
                                color: Colors.white,
                                fontSize: 28,
                                fontWeight: FontWeight.bold)),
                        SizedBox(height: 6),
                        Text('Patient Portal',
                            style: TextStyle(color: Colors.white54, fontSize: 14)),
                      ],
                    ),
                  ),
                  const SizedBox(height: 40),

                  // ── Patient Name field ────────────────────────────────────
                  _label('Your Name'),
                  const SizedBox(height: 8),
                  TextFormField(
                    controller: _nameCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: _inputDecoration('e.g. Kamal Perera', Icons.person_outline),
                    validator: (v) {
                      if (v == null || v.trim().isEmpty) return 'Name is required';
                      if (v.trim().length < 2) return 'Enter a valid name';
                      return null;
                    },
                  ),
                  const SizedBox(height: 20),

                  // ── Patient ID field ──────────────────────────────────────
                  _label('Patient ID (Optional if Name matches profile)'),
                  const SizedBox(height: 4),
                  const Text(
                    'Enter your UUID from hospital card, or leave blank to look up by Name',
                    style: TextStyle(color: Colors.white38, fontSize: 12),
                  ),
                  const SizedBox(height: 8),
                  TextFormField(
                    controller: _patientIdCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: _inputDecoration(
                        'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx (optional)',
                        Icons.badge_outlined),
                    validator: (v) {
                      if (v != null && v.trim().isNotEmpty && !_isValidUuid(v.trim())) {
                        return 'Enter a valid UUID or leave blank to look up by Name';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 12),

                  // ── Error banner ──────────────────────────────────────────
                  if (_errorMessage != null)
                    Container(
                      margin: const EdgeInsets.only(bottom: 12),
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.red.withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(10),
                        border: Border.all(color: Colors.red.withValues(alpha: 0.4)),
                      ),
                      child: Row(
                        children: [
                          const Icon(Icons.error_outline, color: Colors.red, size: 18),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(_errorMessage!,
                                style: const TextStyle(color: Colors.red, fontSize: 13)),
                          ),
                        ],
                      ),
                    ),

                  // ── Login button ──────────────────────────────────────────
                  const SizedBox(height: 8),
                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: ElevatedButton(
                      onPressed: _isLoading ? null : _login,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: _purple,
                        foregroundColor: Colors.white,
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(14)),
                        elevation: 4,
                      ),
                      child: _isLoading
                          ? const SizedBox(
                              width: 22, height: 22,
                              child: CircularProgressIndicator(
                                  strokeWidth: 2, color: Colors.white))
                          : const Text('Sign In',
                              style: TextStyle(
                                  fontSize: 16, fontWeight: FontWeight.bold)),
                    ),
                  ),
                  const SizedBox(height: 24),

                  // ── Register link ─────────────────────────────────────────
                  Center(
                    child: GestureDetector(
                      onTap: () => Navigator.push(context,
                          MaterialPageRoute(builder: (_) => const RegisterScreen())),
                      child: RichText(
                        text: const TextSpan(
                          text: 'New patient? ',
                          style: TextStyle(color: Colors.white54, fontSize: 14),
                          children: [
                            TextSpan(
                              text: 'Register here',
                              style: TextStyle(
                                  color: Color(0xFF6C63FF),
                                  fontWeight: FontWeight.bold),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  // ── Helpers ────────────────────────────────────────────────────────────────
  Widget _label(String text) => Text(
        text,
        style: const TextStyle(
            color: Colors.white70, fontSize: 14, fontWeight: FontWeight.w600),
      );

  InputDecoration _inputDecoration(String hint, IconData icon) =>
      InputDecoration(
        hintText: hint,
        hintStyle: const TextStyle(color: Colors.white24),
        prefixIcon: Icon(icon, color: Colors.white38, size: 20),
        filled: true,
        fillColor: _cardBg,
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _border),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _purple, width: 1.5),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: Colors.red),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: Colors.red, width: 1.5),
        ),
        errorStyle: const TextStyle(color: Colors.redAccent),
      );
}
