// Component B – Submit Triage Screen
// The core symptom submission form.
// Features:
//  • Strict form validation (symptoms cannot be blank, must be ≥ 20 chars)
//  • Camera / image picker integration (mandatory device feature)
//  • Loading spinner while the Planning Agent runs on the backend
//  • Success state shows AI plan (SuggestedSpecialist, UrgencyLevel, Rationale)
//  • Error state with user-friendly message

import 'dart:io';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../services/auth_service.dart';
import '../services/triage_service.dart';

class SubmitTriageScreen extends StatefulWidget {
  const SubmitTriageScreen({super.key});

  @override
  State<SubmitTriageScreen> createState() => _SubmitTriageScreenState();
}

class _SubmitTriageScreenState extends State<SubmitTriageScreen> {
  final _formKey      = GlobalKey<FormState>();
  final _symptomsCtrl = TextEditingController();

  bool    _isLoading    = false;
  String? _errorMessage;
  Map<String, dynamic>? _result;  // API response after submission
  File?   _pickedImage;           // Optional photo from camera / gallery

  static const _purple  = Color(0xFF6C63FF);
  static const _darkBg  = Color(0xFF1A1A2E);
  static const _cardBg  = Color(0xFF16213E);
  static const _border  = Color(0xFF2D2B55);

  final _picker = ImagePicker();

  @override
  void dispose() {
    _symptomsCtrl.dispose();
    super.dispose();
  }

  // ── Camera / Gallery picker ────────────────────────────────────────────────
  Future<void> _pickImage(ImageSource source) async {
    try {
      final picked = await _picker.pickImage(
        source: source,
        maxWidth: 1024,
        maxHeight: 1024,
        imageQuality: 80,
      );
      if (picked != null) {
        setState(() => _pickedImage = File(picked.path));
      }
    } catch (e) {
      setState(() =>
          _errorMessage = 'Camera/gallery access denied. Check app permissions.');
    }
  }

  void _showImageSourceSheet() {
    showModalBottomSheet(
      context: context,
      backgroundColor: _cardBg,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (_) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading: const Icon(Icons.camera_alt, color: Color(0xFF6C63FF)),
              title: const Text('Take a Photo',
                  style: TextStyle(color: Colors.white)),
              onTap: () {
                Navigator.pop(context);
                _pickImage(ImageSource.camera);
              },
            ),
            ListTile(
              leading:
                  const Icon(Icons.photo_library, color: Color(0xFF6C63FF)),
              title: const Text('Choose from Gallery',
                  style: TextStyle(color: Colors.white)),
              onTap: () {
                Navigator.pop(context);
                _pickImage(ImageSource.gallery);
              },
            ),
          ],
        ),
      ),
    );
  }

  // ── Submit handler ─────────────────────────────────────────────────────────
  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading    = true;
      _errorMessage = null;
      _result       = null;
    });

    try {
      final patientId = await AuthService.getPatientId();
      if (patientId == null) throw Exception('Not logged in.');

      final data = await TriageService.submitTriage(
        patientId: patientId,
        symptoms:  _symptomsCtrl.text.trim(),
        imageFile: _pickedImage,
      );

      setState(() => _result = data);
      _symptomsCtrl.clear();
      setState(() => _pickedImage = null);
    } catch (e) {
      setState(() => _errorMessage =
          'Submission failed. Make sure the backend is running.\n\nError: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ── Parse AI plan JSON safely ──────────────────────────────────────────────
  Map<String, dynamic>? _parseAiPlan(String? raw) {
    if (raw == null) return null;
    try {
      return jsonDecode(raw) as Map<String, dynamic>;
    } catch (_) {
      return null;
    }
  }

  // ── Severity colour helper ─────────────────────────────────────────────────
  Color _severityColor(String? level) {
    switch ((level ?? '').toLowerCase()) {
      case 'critical': return const Color(0xFFFF4D4F);
      case 'high':     return const Color(0xFFFA8C16);
      case 'medium':   return const Color(0xFFFADB14);
      default:         return const Color(0xFF52C41A);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _darkBg,
      appBar: AppBar(
        backgroundColor: _darkBg,
        title: const Text('Submit Symptoms',
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
        padding: const EdgeInsets.all(24),
        child: _result != null ? _buildSuccess() : _buildForm(),
      ),
    );
  }

  // ── Success state ──────────────────────────────────────────────────────────
  Widget _buildSuccess() {
    final severity = _result!['severityLevel'] as String? ?? '';
    final status   = _result!['triageStatus']  as String? ?? '';
    final aiPlan   = _parseAiPlan(_result!['aiPlan'] as String?);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Center(
          child: Container(
            width: 80, height: 80,
            decoration: BoxDecoration(
              color: const Color(0xFF52C41A).withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(24),
            ),
            child: const Center(
                child: Text('✅', style: TextStyle(fontSize: 40))),
          ),
        ),
        const SizedBox(height: 20),
        Center(
          child: const Text('Submission Received!',
              style: TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.bold)),
        ),
        const SizedBox(height: 4),
        Center(
          child: const Text(
            'Our AI Planning Agent has analysed your symptoms.',
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.white54, fontSize: 13),
          ),
        ),
        const SizedBox(height: 24),

        // ── Status + Severity row ──────────────────────────────────────────
        Row(
          children: [
            _chip('Status: $status', _purple),
            const SizedBox(width: 10),
            if (severity.isNotEmpty)
              _chip('Severity: $severity', _severityColor(severity)),
          ],
        ),
        const SizedBox(height: 20),

        // ── AI Plan card ───────────────────────────────────────────────────
        if (aiPlan != null) ...[
          _sectionTitle('🤖 AI Clinical Plan'),
          const SizedBox(height: 10),
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: _cardBg,
              borderRadius: BorderRadius.circular(14),
              border: Border.all(color: _purple.withValues(alpha: 0.4)),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _planRow('Suggested Specialist',
                    aiPlan['SuggestedSpecialist'] ?? aiPlan['suggestedSpecialist'] ?? '—'),
                _planRow('Urgency Level',
                    aiPlan['UrgencyLevel'] ?? aiPlan['urgencyLevel'] ?? '—'),
                _planRow('Recommended Action',
                    aiPlan['RecommendedAction'] ?? aiPlan['recommendedAction'] ?? '—'),
                _planRow('Rationale',
                    aiPlan['Rationale'] ?? aiPlan['rationale'] ?? '—'),
                if (aiPlan['AnalysisMethod'] != null ||
                    aiPlan['analysisMethod'] != null)
                  _planRow('Analysed By',
                      aiPlan['AnalysisMethod'] ?? aiPlan['analysisMethod']),
              ],
            ),
          ),
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.amber.withValues(alpha: 0.08),
              borderRadius: BorderRadius.circular(10),
              border: Border.all(color: Colors.amber.withValues(alpha: 0.3)),
            ),
            child: const Text(
              '⚕️  This AI plan is for guidance only. A doctor will review and confirm your triage status.',
              style: TextStyle(color: Colors.amber, fontSize: 12),
            ),
          ),
        ],
        const SizedBox(height: 28),

        // ── Submit another button ──────────────────────────────────────────
        SizedBox(
          width: double.infinity,
          height: 52,
          child: OutlinedButton(
            onPressed: () => setState(() => _result = null),
            style: OutlinedButton.styleFrom(
              foregroundColor: _purple,
              side: const BorderSide(color: _purple),
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14)),
            ),
            child: const Text('Submit Another',
                style:
                    TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          ),
        ),
      ],
    );
  }

  // ── Submission form ────────────────────────────────────────────────────────
  Widget _buildForm() {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Intro banner
          Container(
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: _purple.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: _purple.withValues(alpha: 0.3)),
            ),
            child: const Text(
              '🤖 Our AI Planning Agent will analyse your symptoms and suggest a specialist and urgency level for doctor review.',
              style: TextStyle(color: Colors.white70, fontSize: 13),
            ),
          ),
          const SizedBox(height: 24),

          // ── Symptoms field ───────────────────────────────────────────────
          _sectionTitle('📋 Describe Your Symptoms *'),
          const SizedBox(height: 4),
          const Text(
            'Be as detailed as possible — include location, duration, and severity.',
            style: TextStyle(color: Colors.white38, fontSize: 12),
          ),
          const SizedBox(height: 10),
          TextFormField(
            controller: _symptomsCtrl,
            maxLines: 6,
            style: const TextStyle(color: Colors.white),
            decoration: InputDecoration(
              hintText:
                  'e.g. "I have had severe chest pain on the left side for 2 hours. It radiates to my left arm and I feel short of breath..."',
              hintStyle: const TextStyle(color: Colors.white24, fontSize: 13),
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
                  borderSide:
                      const BorderSide(color: Colors.red, width: 1.5)),
              errorStyle: const TextStyle(color: Colors.redAccent),
            ),
            validator: (v) {
              if (v == null || v.trim().isEmpty) {
                return 'Symptoms description is required';
              }
              if (v.trim().length < 20) {
                return 'Please describe your symptoms in more detail (min 20 characters)';
              }
              return null;
            },
          ),
          const SizedBox(height: 24),

          // ── Camera / Image picker (device feature) ───────────────────────
          _sectionTitle('📸 Attach a Photo (Optional)'),
          const SizedBox(height: 4),
          const Text(
            'Photo of rash, injury, or physical symptom helps the AI assess better.',
            style: TextStyle(color: Colors.white38, fontSize: 12),
          ),
          const SizedBox(height: 10),
          GestureDetector(
            onTap: _showImageSourceSheet,
            child: Container(
              width: double.infinity,
              height: _pickedImage != null ? 200 : 100,
              decoration: BoxDecoration(
                color: _cardBg,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                    color: _pickedImage != null
                        ? _purple
                        : _border,
                    style: BorderStyle.solid),
              ),
              clipBehavior: Clip.antiAlias,
              child: _pickedImage != null
                  ? Stack(
                      fit: StackFit.expand,
                      children: [
                        Image.file(_pickedImage!, fit: BoxFit.cover),
                        Positioned(
                          top: 8,
                          right: 8,
                          child: GestureDetector(
                            onTap: () => setState(() => _pickedImage = null),
                            child: Container(
                              padding: const EdgeInsets.all(4),
                              decoration: BoxDecoration(
                                  color: Colors.black54,
                                  borderRadius: BorderRadius.circular(20)),
                              child: const Icon(Icons.close,
                                  color: Colors.white, size: 18),
                            ),
                          ),
                        ),
                      ],
                    )
                  : Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: const [
                        Icon(Icons.add_a_photo_outlined,
                            color: Color(0xFF6C63FF), size: 32),
                        SizedBox(height: 8),
                        Text('Tap to add photo',
                            style: TextStyle(
                                color: Colors.white38, fontSize: 13)),
                      ],
                    ),
            ),
          ),
          const SizedBox(height: 24),

          // ── Error banner ─────────────────────────────────────────────────
          if (_errorMessage != null)
            Container(
              margin: const EdgeInsets.only(bottom: 16),
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.red.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(10),
                border: Border.all(color: Colors.red.withValues(alpha: 0.4)),
              ),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Icon(Icons.error_outline,
                      color: Colors.red, size: 18),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(_errorMessage!,
                        style: const TextStyle(
                            color: Colors.redAccent, fontSize: 12)),
                  ),
                ],
              ),
            ),

          // ── Submit button ────────────────────────────────────────────────
          SizedBox(
            width: double.infinity,
            height: 56,
            child: ElevatedButton(
              onPressed: _isLoading ? null : _submit,
              style: ElevatedButton.styleFrom(
                backgroundColor: _purple,
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14)),
                elevation: 4,
              ),
              child: _isLoading
                  ? const Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        SizedBox(
                          width: 20, height: 20,
                          child: CircularProgressIndicator(
                              strokeWidth: 2, color: Colors.white),
                        ),
                        SizedBox(width: 12),
                        Text('AI is analysing symptoms...',
                            style: TextStyle(color: Colors.white, fontSize: 15)),
                      ],
                    )
                  : const Text('🚀 Submit for AI Triage',
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

  // ── Helpers ────────────────────────────────────────────────────────────────
  Widget _sectionTitle(String text) => Text(
        text,
        style: const TextStyle(
            color: Colors.white, fontSize: 15, fontWeight: FontWeight.bold),
      );

  Widget _chip(String label, Color color) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.15),
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: color.withValues(alpha: 0.5)),
        ),
        child: Text(label,
            style: TextStyle(
                color: color, fontSize: 12, fontWeight: FontWeight.bold)),
      );

  Widget _planRow(String label, String value) => Padding(
        padding: const EdgeInsets.only(bottom: 10),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(label,
                style: const TextStyle(
                    color: Colors.white38,
                    fontSize: 11,
                    fontWeight: FontWeight.w600,
                    letterSpacing: 0.5)),
            const SizedBox(height: 3),
            Text(value,
                style: const TextStyle(color: Colors.white, fontSize: 14)),
          ],
        ),
      );
}
