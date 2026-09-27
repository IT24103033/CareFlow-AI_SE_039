// Component B – Register Screen (Redesigned)
// CareFlow AI healthcare aesthetic.

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../services/auth_service.dart';
import '../theme/app_theme.dart';
import 'home_screen.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey   = GlobalKey<FormState>();
  final _nameCtrl  = TextEditingController();
  final _phoneCtrl = TextEditingController();
  final _dobCtrl   = TextEditingController();

  bool    _isSubmitting = false;
  bool    _isDone       = false;
  String  _generatedId  = '';
  String? _errorMessage;

  @override
  void dispose() {
    _nameCtrl.dispose();
    _phoneCtrl.dispose();
    _dobCtrl.dispose();
    super.dispose();
  }

  Future<void> _register() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _isSubmitting = true;
      _errorMessage = null;
    });

    try {
      final dob = _dobCtrl.text.trim().isEmpty
          ? '2000-01-01'
          : _dobCtrl.text.trim();
      final patient = await AuthService.registerPatient(
        fullName:    _nameCtrl.text.trim(),
        dateOfBirth: dob,
      );

      setState(() {
        _generatedId  = patient['id'].toString();
        _isDone       = true;
        _isSubmitting = false;
      });
    } catch (e) {
      setState(() {
        _isSubmitting = false;
        _errorMessage =
            'Registration failed. Make sure the backend is running.\nDetails: $e';
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.navyDark,
      body: Column(
        children: [
          // ── Navy header ────────────────────────────────────────────────────
          _buildHeader(),

          // ── White card body ────────────────────────────────────────────────
          Expanded(
            child: Container(
              decoration: const BoxDecoration(
                color: AppTheme.pageWhite,
                borderRadius:
                    BorderRadius.vertical(top: Radius.circular(28)),
              ),
              child: SingleChildScrollView(
                padding: const EdgeInsets.fromLTRB(24, 28, 24, 32),
                child: _isDone ? _buildSuccess() : _buildForm(),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildHeader() {
    return SafeArea(
      bottom: false,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(24, 16, 24, 28),
        child: Row(
          children: [
            IconButton(
              icon: const Icon(Icons.arrow_back_ios,
                  color: Colors.white, size: 20),
              onPressed: () => Navigator.pop(context),
              padding: EdgeInsets.zero,
            ),
            const SizedBox(width: 4),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: const [
                Text('Create Account',
                    style: TextStyle(
                        color: Colors.white,
                        fontSize: 22,
                        fontWeight: FontWeight.bold)),
                Text('Join CareFlow AI — it\'s free',
                    style: TextStyle(
                        color: Color(0xFFAEC0D8), fontSize: 13)),
              ],
            ),
          ],
        ),
      ),
    );
  }

  // ── Success state ──────────────────────────────────────────────────────────
  Widget _buildSuccess() {
    return Column(
      children: [
        const SizedBox(height: 12),
        Container(
          width: 80,
          height: 80,
          decoration: BoxDecoration(
            color: AppTheme.success.withValues(alpha: 0.12),
            shape: BoxShape.circle,
          ),
          child: const Center(
            child: Icon(Icons.check_circle_outline,
                color: AppTheme.success, size: 44),
          ),
        ),
        const SizedBox(height: 20),
        const Text('Registration Successful!',
            style: TextStyle(
                color: AppTheme.textDark,
                fontSize: 22,
                fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        const Text(
          'Your Patient ID has been assigned.\nSave it carefully — you will need it to sign in.',
          textAlign: TextAlign.center,
          style: TextStyle(color: AppTheme.textMid, fontSize: 13),
        ),
        const SizedBox(height: 24),

        // Patient ID card
        Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: AppTheme.cardWhite,
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: AppTheme.teal.withValues(alpha: 0.4)),
            boxShadow: AppTheme.cardShadow,
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text('YOUR PATIENT ID',
                  style: TextStyle(
                      color: AppTheme.textLight,
                      fontSize: 11,
                      fontWeight: FontWeight.w600,
                      letterSpacing: 1)),
              const SizedBox(height: 8),
              Row(
                children: [
                  Expanded(
                    child: Text(
                      _generatedId,
                      style: const TextStyle(
                          color: AppTheme.navyDark,
                          fontSize: 13,
                          fontFamily: 'monospace',
                          fontWeight: FontWeight.bold),
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.copy_outlined,
                        color: AppTheme.teal, size: 20),
                    onPressed: () {
                      Clipboard.setData(ClipboardData(text: _generatedId));
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                            content: Text('Patient ID copied to clipboard!')),
                      );
                    },
                  ),
                ],
              ),
            ],
          ),
        ),
        const SizedBox(height: 12),

        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: const Color(0xFFFFF3CD),
            borderRadius: BorderRadius.circular(10),
            border: Border.all(color: const Color(0xFFFFD700).withValues(alpha: 0.5)),
          ),
          child: Row(
            children: const [
              Icon(Icons.warning_amber_rounded,
                  color: Color(0xFF856404), size: 18),
              SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Save this ID! In a real hospital system it would be printed on your admission card.',
                  style: TextStyle(color: Color(0xFF856404), fontSize: 12),
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 32),

        ElevatedButton(
          onPressed: () => Navigator.pushAndRemoveUntil(
            context,
            MaterialPageRoute(builder: (_) => const HomeScreen()),
            (_) => false,
          ),
          child: const Text('Go to Patient Portal'),
        ),
      ],
    );
  }

  // ── Registration form ──────────────────────────────────────────────────────
  Widget _buildForm() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Info banner
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: AppTheme.tealLight,
            borderRadius: BorderRadius.circular(10),
          ),
          child: Row(
            children: const [
              Icon(Icons.info_outline, color: AppTheme.teal, size: 18),
              SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Fill in your details below. A Patient ID will be generated and assigned to your account.',
                  style: TextStyle(color: Color(0xFF285E61), fontSize: 12),
                ),
              ),
            ],
          ),
        ),

        if (_errorMessage != null) ...[
          const SizedBox(height: 14),
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: AppTheme.danger.withValues(alpha: 0.08),
              borderRadius: BorderRadius.circular(10),
              border: Border.all(
                  color: AppTheme.danger.withValues(alpha: 0.3)),
            ),
            child: Row(
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
        const SizedBox(height: 24),

        Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _fieldLabel('Full Name *'),
              const SizedBox(height: 6),
              TextFormField(
                controller: _nameCtrl,
                style: const TextStyle(
                    color: AppTheme.textDark, fontSize: 14),
                decoration: AppTheme.inputDecoration(
                    hint: 'e.g. Kamal Perera',
                    icon: Icons.person_outline),
                validator: (v) {
                  if (v == null || v.trim().isEmpty)
                    return 'Full name is required';
                  if (v.trim().length < 3) return 'Enter your full name';
                  return null;
                },
              ),
              const SizedBox(height: 16),

              _fieldLabel('Phone Number *'),
              const SizedBox(height: 6),
              TextFormField(
                controller: _phoneCtrl,
                keyboardType: TextInputType.phone,
                style: const TextStyle(
                    color: AppTheme.textDark, fontSize: 14),
                decoration: AppTheme.inputDecoration(
                    hint: '+94 77 123 4567',
                    icon: Icons.phone_outlined),
                validator: (v) {
                  if (v == null || v.trim().isEmpty)
                    return 'Phone number is required';
                  if (v.trim().length < 9)
                    return 'Enter a valid phone number';
                  return null;
                },
              ),
              const SizedBox(height: 16),

              _fieldLabel('Date of Birth *'),
              const SizedBox(height: 6),
              TextFormField(
                controller: _dobCtrl,
                style: const TextStyle(
                    color: AppTheme.textDark, fontSize: 14),
                decoration: AppTheme.inputDecoration(
                    hint: 'YYYY-MM-DD',
                    icon: Icons.calendar_today_outlined),
                readOnly: true,
                onTap: () async {
                  final picked = await showDatePicker(
                    context: context,
                    initialDate: DateTime(2000),
                    firstDate: DateTime(1920),
                    lastDate: DateTime.now(),
                    builder: (ctx, child) => Theme(
                      data: Theme.of(ctx).copyWith(
                        colorScheme: ColorScheme.light(
                          primary: AppTheme.navyMid,
                          onPrimary: Colors.white,
                        ),
                      ),
                      child: child!,
                    ),
                  );
                  if (picked != null) {
                    _dobCtrl.text =
                        '${picked.year}-${picked.month.toString().padLeft(2, '0')}-${picked.day.toString().padLeft(2, '0')}';
                  }
                },
                validator: (v) => (v == null || v.isEmpty)
                    ? 'Date of birth is required'
                    : null,
              ),
              const SizedBox(height: 32),

              SizedBox(
                width: double.infinity,
                height: 52,
                child: ElevatedButton(
                  onPressed: _isSubmitting ? null : _register,
                  child: _isSubmitting
                      ? const SizedBox(
                          width: 22,
                          height: 22,
                          child: CircularProgressIndicator(
                              strokeWidth: 2, color: Colors.white))
                      : const Text('Create My Account'),
                ),
              ),
            ],
          ),
        ),
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
}
