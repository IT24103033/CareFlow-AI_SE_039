// Component B – Register Screen
// Mock registration: explains to the patient that their PatientId is assigned
// by the hospital at admission (Component A). Points them back to Login.
// This satisfies the "registration UI screen" deliverable requirement.

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../services/auth_service.dart';
import 'home_screen.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey       = GlobalKey<FormState>();
  final _nameCtrl      = TextEditingController();
  final _phoneCtrl     = TextEditingController();
  final _dobCtrl       = TextEditingController();

  bool _isSubmitting  = false;
  bool _isDone        = false;
  String _generatedId = '';
  String? _errorMessage;

  static const _purple = Color(0xFF6C63FF);
  static const _darkBg = Color(0xFF1A1A2E);
  static const _cardBg = Color(0xFF16213E);
  static const _border = Color(0xFF2D2B55);
  static const _green  = Color(0xFF52C41A);

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
      final dob = _dobCtrl.text.trim().isEmpty ? '2000-01-01' : _dobCtrl.text.trim();
      final patient = await AuthService.registerPatient(
        fullName: _nameCtrl.text.trim(),
        dateOfBirth: dob,
      );

      final realId = patient['id'].toString();

      setState(() {
        _generatedId  = realId;
        _isDone       = true;
        _isSubmitting = false;
      });
    } catch (e) {
      setState(() {
        _isSubmitting = false;
        _errorMessage = 'Registration failed. Make sure the backend is running.\nDetails: $e';
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _darkBg,
      appBar: AppBar(
        backgroundColor: _darkBg,
        title: const Text('Register',
            style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios, color: Colors.white),
          onPressed: () => Navigator.pop(context),
        ),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(2),
          child: Container(height: 2, color: _purple),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(28),
        child: _isDone ? _buildSuccess() : _buildForm(),
      ),
    );
  }

  // ── Success state ──────────────────────────────────────────────────────────
  Widget _buildSuccess() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Center(
          child: Container(
            width: 80, height: 80,
            decoration: BoxDecoration(
              color: _green.withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(24),
            ),
            child: const Center(
                child: Text('✅', style: TextStyle(fontSize: 40))),
          ),
        ),
        const SizedBox(height: 24),
        Center(
          child: const Text('Registration Submitted!',
              style: TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.bold)),
        ),
        const SizedBox(height: 8),
        Center(
          child: const Text(
            'Your request has been sent to the hospital.\nYour Patient ID is:',
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.white54, fontSize: 13),
          ),
        ),
        const SizedBox(height: 20),
        // Patient ID card
        Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: _cardBg,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: _purple.withValues(alpha: 0.5)),
          ),
          child: Row(
            children: [
              Expanded(
                child: Text(_generatedId,
                    style: const TextStyle(
                        color: _purple,
                        fontSize: 13,
                        fontFamily: 'monospace',
                        fontWeight: FontWeight.bold)),
              ),
              IconButton(
                icon: const Icon(Icons.copy, color: Colors.white54, size: 20),
                onPressed: () {
                  Clipboard.setData(ClipboardData(text: _generatedId));
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Patient ID copied!')),
                  );
                },
              ),
            ],
          ),
        ),
        const SizedBox(height: 12),
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: Colors.amber.withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(10),
            border: Border.all(color: Colors.amber.withValues(alpha: 0.4)),
          ),
          child: const Text(
            '⚠️  Save this ID! You will need it to log in. In a real hospital system this would be printed on your admission card.',
            style: TextStyle(color: Colors.amber, fontSize: 12),
          ),
        ),
        const SizedBox(height: 28),
        SizedBox(
          width: double.infinity,
          height: 52,
          child: ElevatedButton(
            onPressed: () => Navigator.pushAndRemoveUntil(
              context,
              MaterialPageRoute(builder: (_) => const HomeScreen()),
              (_) => false,
            ),
            style: ElevatedButton.styleFrom(
              backgroundColor: _purple,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14)),
            ),
            child: const Text('Go to Patient Portal',
                style: TextStyle(
                    color: Colors.white,
                    fontSize: 16,
                    fontWeight: FontWeight.bold)),
          ),
        ),
      ],
    );
  }

  // ── Registration form ──────────────────────────────────────────────────────
  Widget _buildForm() {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Info banner
          Container(
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: _purple.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: _purple.withValues(alpha: 0.3)),
            ),
            child: const Text(
              'ℹ️  New patients are registered by the hospital reception. Fill in your details and a Patient ID will be assigned to you.',
              style: TextStyle(color: Colors.white70, fontSize: 13),
            ),
          ),
          if (_errorMessage != null) ...[
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.red.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(10),
                border: Border.all(color: Colors.red.withValues(alpha: 0.4)),
              ),
              child: Row(
                children: [
                  const Icon(Icons.error_outline, color: Colors.red, size: 20),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(_errorMessage!,
                        style: const TextStyle(color: Colors.red, fontSize: 13)),
                  ),
                ],
              ),
            ),
          ],
          const SizedBox(height: 28),

          _label('Full Name *'),
          const SizedBox(height: 8),
          TextFormField(
            controller: _nameCtrl,
            style: const TextStyle(color: Colors.white),
            decoration: _inputDec('e.g. Kamal Perera', Icons.person_outline),
            validator: (v) {
              if (v == null || v.trim().isEmpty) return 'Full name is required';
              if (v.trim().length < 3) return 'Enter your full name';
              return null;
            },
          ),
          const SizedBox(height: 20),

          _label('Phone Number *'),
          const SizedBox(height: 8),
          TextFormField(
            controller: _phoneCtrl,
            keyboardType: TextInputType.phone,
            style: const TextStyle(color: Colors.white),
            decoration: _inputDec('+94 77 123 4567', Icons.phone_outlined),
            validator: (v) {
              if (v == null || v.trim().isEmpty) return 'Phone number is required';
              if (v.trim().length < 9) return 'Enter a valid phone number';
              return null;
            },
          ),
          const SizedBox(height: 20),

          _label('Date of Birth *'),
          const SizedBox(height: 8),
          TextFormField(
            controller: _dobCtrl,
            style: const TextStyle(color: Colors.white),
            decoration: _inputDec('YYYY-MM-DD', Icons.calendar_today_outlined),
            readOnly: true,
            onTap: () async {
              final picked = await showDatePicker(
                context: context,
                initialDate: DateTime(2000),
                firstDate: DateTime(1920),
                lastDate: DateTime.now(),
                builder: (ctx, child) => Theme(
                  data: Theme.of(ctx).copyWith(
                    colorScheme: const ColorScheme.dark(primary: _purple),
                  ),
                  child: child!,
                ),
              );
              if (picked != null) {
                _dobCtrl.text =
                    '${picked.year}-${picked.month.toString().padLeft(2, '0')}-${picked.day.toString().padLeft(2, '0')}';
              }
            },
            validator: (v) =>
                (v == null || v.isEmpty) ? 'Date of birth is required' : null,
          ),
          const SizedBox(height: 32),

          SizedBox(
            width: double.infinity,
            height: 52,
            child: ElevatedButton(
              onPressed: _isSubmitting ? null : _register,
              style: ElevatedButton.styleFrom(
                backgroundColor: _purple,
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14)),
              ),
              child: _isSubmitting
                  ? const SizedBox(
                      width: 22, height: 22,
                      child: CircularProgressIndicator(
                          strokeWidth: 2, color: Colors.white))
                  : const Text('Submit Registration',
                      style: TextStyle(
                          color: Colors.white,
                          fontSize: 16,
                          fontWeight: FontWeight.bold)),
            ),
          ),
        ],
      ),
    );
  }

  Widget _label(String text) => Text(
        text,
        style: const TextStyle(
            color: Colors.white70, fontSize: 14, fontWeight: FontWeight.w600),
      );

  InputDecoration _inputDec(String hint, IconData icon) => InputDecoration(
        hintText: hint,
        hintStyle: const TextStyle(color: Colors.white24),
        prefixIcon: Icon(icon, color: Colors.white38, size: 20),
        filled: true,
        fillColor: _cardBg,
        enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: _border)),
        focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: _purple, width: 1.5)),
        errorBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: Colors.red)),
        focusedErrorBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: Colors.red, width: 1.5)),
        errorStyle: const TextStyle(color: Colors.redAccent),
      );
}
