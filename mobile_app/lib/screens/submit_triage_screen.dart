// Component B – Submit Triage Screen (Redesigned)
// CareFlow AI healthcare aesthetic: navy header, white content card.

import 'dart:async';
import 'dart:io';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../services/triage_repository.dart';
import '../theme/app_theme.dart';
import '../services/ApiService.dart';

class SubmitTriageScreen extends StatefulWidget {
  final TriageRepository repository;
  const SubmitTriageScreen({
    super.key,
    this.repository = const ApiTriageRepository(),
  });

  @override
  State<SubmitTriageScreen> createState() => _SubmitTriageScreenState();
}

class _SubmitTriageScreenState extends State<SubmitTriageScreen> {
  final _formKey = GlobalKey<FormState>();
  final _symptomsCtrl = TextEditingController();

  bool _isLoading = false;
  String? _errorMessage;
  Map<String, dynamic>? _result;
  File? _pickedImage;

  final _picker = ImagePicker();

  @override
  void dispose() {
    _symptomsCtrl.dispose();
    super.dispose();
  }

  Future<void> _pickImage(ImageSource source) async {
    try {
      final picked = await _picker.pickImage(
        source: source,
        maxWidth: 1024,
        maxHeight: 1024,
        imageQuality: 80,
      );
      if (mounted && picked != null) {
        setState(() => _pickedImage = File(picked.path));
      }
    } catch (e) {
      if (!mounted) return;
      setState(
        () => _errorMessage =
            'Camera/gallery access denied. Check app permissions.',
      );
    }
  }

  void _showImageSourceSheet() {
    showModalBottomSheet(
      context: context,
      backgroundColor: AppTheme.cardWhite,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (_) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading: Container(
                width: 38,
                height: 38,
                decoration: BoxDecoration(
                  color: AppTheme.teal.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.camera_alt_outlined,
                  color: AppTheme.teal,
                  size: 20,
                ),
              ),
              title: const Text(
                'Take a Photo',
                style: TextStyle(
                  color: AppTheme.textDark,
                  fontWeight: FontWeight.w600,
                ),
              ),
              onTap: () {
                Navigator.pop(context);
                _pickImage(ImageSource.camera);
              },
            ),
            ListTile(
              leading: Container(
                width: 38,
                height: 38,
                decoration: BoxDecoration(
                  color: AppTheme.navyDark.withValues(alpha: 0.08),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.photo_library_outlined,
                  color: AppTheme.navyDark,
                  size: 20,
                ),
              ),
              title: const Text(
                'Choose from Gallery',
                style: TextStyle(
                  color: AppTheme.textDark,
                  fontWeight: FontWeight.w600,
                ),
              ),
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

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading = true;
      _errorMessage = null;
      _result = null;
    });

    try {
      final data = await widget.repository.submit(
        symptoms: _symptomsCtrl.text.trim(),
        imageFile: _pickedImage,
      );
      if (!mounted) return;

      setState(() => _result = data);
      _symptomsCtrl.clear();
      setState(() => _pickedImage = null);
    } catch (e) {
      if (!mounted) return;
      setState(() {
        String msg = e.toString();
        if (e is TimeoutException) {
          _errorMessage = 'The request timed out. Your submission may still be processing. Check your triage history before submitting again.';
        } else if (msg.contains('Stop.')) {
          // Clean up "Exception:" prefix if present
          _errorMessage = msg.replaceFirst('HttpException: ', '').replaceFirst('Exception: ', '');
        } else {
          _errorMessage = 'Submission failed. Check your connection and try again.\n\nError: $e';
        }
      });
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _bookSlot(Map<String, dynamic> slot) async {
    setState(() => _isLoading = true);
    try {
      final apiService = ApiService(); // Use default instance or passed repo if available
      final slotData = {
        'appointmentDate': slot['date'],
        'startTime': slot['startTime'],
        'endTime': slot['endTime'],
      };
      final updatedResult = await apiService.bookTriageSlot(_result!['id'], slotData);
      if (!mounted) return;
      setState(() {
        _result = updatedResult;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Appointment booked successfully!'), backgroundColor: AppTheme.success),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to book: $e'), backgroundColor: AppTheme.danger),
      );
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Map<String, dynamic>? _parseAiPlan(String? raw) {
    if (raw == null) return null;
    try {
      return jsonDecode(raw) as Map<String, dynamic>;
    } catch (_) {
      return null;
    }
  }

  Color _severityColor(String? level) {
    switch ((level ?? '').toLowerCase()) {
      case 'critical':
        return AppTheme.danger;
      case 'high':
        return AppTheme.warning;
      case 'medium':
        return AppTheme.pending;
      case 'low':
        return AppTheme.success;
      default:
        return AppTheme.textMid;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.pageWhite,
      body: Column(
        children: [
          // ── Navy header ──────────────────────────────────────────────────
          _buildHeader(),

          // ── Scrollable content ───────────────────────────────────────────
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.fromLTRB(20, 8, 20, 32),
              child: _result != null ? _buildSuccess() : _buildForm(),
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
            Padding(
              padding: const EdgeInsets.fromLTRB(8, 8, 16, 0),
              child: Row(
                children: [
                  IconButton(
                    icon: const Icon(
                      Icons.arrow_back_ios,
                      color: Colors.white,
                      size: 20,
                    ),
                    onPressed: () => Navigator.pop(context),
                  ),
                  const Expanded(
                    child: Text(
                      'Submit Symptoms',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 19,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 8, 20, 0),
              child: const Text(
                'Describe your condition — our AI will suggest the right specialist.',
                style: TextStyle(color: Color(0xFFAEC0D8), fontSize: 13),
              ),
            ),
            Container(
              height: 24,
              margin: const EdgeInsets.only(top: 16),
              decoration: const BoxDecoration(
                color: AppTheme.pageWhite,
                borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
              ),
            ),
          ],
        ),
      ),
    );
  }

  // ── Success state ──────────────────────────────────────────────────────────
  Widget _buildSuccess() {
    final severity = _result!['severityLevel'] as String? ?? '';
    final status = _result!['triageStatus'] as String? ?? '';
    final aiPlan = _parseAiPlan(_result!['aiPlan'] as String?);
    final assessmentFailed = status == 'AssessmentFailed';

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Center(
          child: Container(
            width: 76,
            height: 76,
            decoration: BoxDecoration(
              color: AppTheme.success.withValues(alpha: 0.12),
              shape: BoxShape.circle,
            ),
            child: const Icon(
              Icons.check_circle_outline,
              color: AppTheme.success,
              size: 44,
            ),
          ),
        ),
        const SizedBox(height: 16),
        const Center(
          child: Text(
            'Submission Received!',
            style: TextStyle(
              color: AppTheme.textDark,
              fontSize: 22,
              fontWeight: FontWeight.bold,
            ),
          ),
        ),
        const SizedBox(height: 6),
        Center(
          child: Text(
            assessmentFailed
                ? 'Your submission was saved, but the assessment could not be completed. No urgency has been assigned; contact staff for follow-up.'
                : 'Our AI Planning Agent has prepared an assessment for doctor review.',
            textAlign: TextAlign.center,
            style: TextStyle(color: AppTheme.textMid, fontSize: 13),
          ),
        ),
        const SizedBox(height: 20),

        // Status + Severity chips
        Wrap(
          spacing: 10,
          runSpacing: 8,
          children: [
            _chip('Status: $status', AppTheme.teal),
            if (severity.isNotEmpty)
              _chip('Severity: $severity', _severityColor(severity)),
          ],
        ),
        const SizedBox(height: 20),

        // AI Plan card
        if (aiPlan != null) ...[
          _cardSection(
            title: 'AI Clinical Plan',
            icon: Icons.psychology_outlined,
            iconColor: AppTheme.teal,
            child: Column(
              children: [
                _planRow(
                  Icons.person_search_outlined,
                  'Suggested Specialist',
                  aiPlan['SuggestedSpecialist'] ??
                      aiPlan['suggestedSpecialist'] ??
                      '—',
                ),
                _planRow(
                  Icons.schedule_outlined,
                  'Urgency Level',
                  aiPlan['UrgencyLevel'] ?? aiPlan['urgencyLevel'] ?? '—',
                ),
                _planRow(
                  Icons.checklist_outlined,
                  'Recommended Action',
                  aiPlan['RecommendedAction'] ??
                      aiPlan['recommendedAction'] ??
                      '—',
                ),
                _planRow(
                  Icons.lightbulb_outline,
                  'Rationale',
                  aiPlan['Rationale'] ?? aiPlan['rationale'] ?? '—',
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
              border: Border.all(
                color: const Color(0xFFFFD700).withValues(alpha: 0.5),
              ),
            ),
            child: Row(
              children: const [
                Icon(Icons.info_outline, color: Color(0xFF856404), size: 18),
                SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'This AI plan is for guidance only. A doctor will review and confirm your triage status.',
                    style: TextStyle(color: Color(0xFF856404), fontSize: 12),
                  ),
                ),
              ],
            ),
          ),
        ],
        const SizedBox(height: 20),

        // Agent Action Card
        if (_result!['schedulingOutcome'] != null && _result!['schedulingOutcome'] != 'Pending') ...[
          _cardSection(
            title: 'Appointment Agent',
            icon: Icons.calendar_today_outlined,
            iconColor: AppTheme.navyDark,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                _planRow(
                  _result!['schedulingOutcome'] == 'Booked' ? Icons.check_circle_outline : Icons.info_outline,
                  'Booking Status',
                  _result!['schedulingOutcome'] == 'ActionRequired' ? 'Please select a slot below' : _result!['schedulingOutcome'],
                ),
                if (_result!['appointmentDetails'] != null)
                  _planRow(
                    Icons.event_available,
                    'Details',
                    'Dr. ${_result!['appointmentDetails']['doctorName']}\n${_result!['appointmentDetails']['appointmentDate']} at ${_result!['appointmentDetails']['startTime']}',
                  ),
                if (_result!['schedulingOutcome'] == 'ActionRequired' && _result!['availableSlots'] != null) ...[
                  const SizedBox(height: 12),
                  const Text('Available Times:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: AppTheme.navyMid)),
                  const SizedBox(height: 8),
                  Wrap(
                    spacing: 8.0,
                    runSpacing: 8.0,
                    children: (_result!['availableSlots'] as List).map<Widget>((slot) {
                      return ElevatedButton(
                        onPressed: () => _bookSlot(slot),
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppTheme.tealLight,
                          foregroundColor: AppTheme.navyDark,
                          elevation: 0,
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                        ),
                        child: Text('${slot['date']} ${slot['startTime']}'),
                      );
                    }).toList(),
                  ),
                ]
              ],
            ),
          ),
          const SizedBox(height: 28),
        ] else ...[
          const SizedBox(height: 8),
        ],

        OutlinedButton.icon(
          onPressed: () => setState(() => _result = null),
          icon: const Icon(Icons.add_circle_outline, size: 18),
          label: const Text('Submit Another'),
          style: OutlinedButton.styleFrom(
            foregroundColor: AppTheme.navyMid,
            side: const BorderSide(color: AppTheme.navyMid),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(30),
            ),
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
          // AI info banner
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: AppTheme.tealLight,
              borderRadius: BorderRadius.circular(10),
            ),
            child: Row(
              children: const [
                Icon(Icons.psychology_outlined, color: AppTheme.teal, size: 20),
                SizedBox(width: 10),
                Expanded(
                  child: Text(
                    'Our AI Planning Agent will analyse your symptoms and suggest the right specialist.',
                    style: TextStyle(color: Color(0xFF285E61), fontSize: 12),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),

          // Symptoms field
          _fieldLabel('Describe Your Symptoms *'),
          const SizedBox(height: 4),
          const Text(
            'Include location, duration, and severity for better results.',
            style: TextStyle(color: AppTheme.textLight, fontSize: 12),
          ),
          const SizedBox(height: 8),
          TextFormField(
            controller: _symptomsCtrl,
            maxLines: 6,
            maxLength: 4000,
            style: const TextStyle(color: AppTheme.textDark, fontSize: 14),
            decoration:
                AppTheme.inputDecoration(
                  hint:
                      'e.g. "I have had severe chest pain on the left side for 2 hours. It radiates to my left arm and I feel short of breath..."',
                ).copyWith(
                  hintStyle: const TextStyle(
                    color: AppTheme.textLight,
                    fontSize: 13,
                  ),
                ),
            validator: (v) {
              if (v == null || v.trim().isEmpty) {
                return 'Symptoms description is required';
              }
              if (v.trim().length < 20) {
                return 'Please describe in more detail (min 20 characters)';
              }
              return null;
            },
          ),
          const SizedBox(height: 24),

          // Photo picker
          _fieldLabel('Attach a Photo (Optional)'),
          const SizedBox(height: 4),
          const Text(
            'Photos are attached for doctor review. The AI currently assesses your written symptoms.',
            style: TextStyle(color: AppTheme.textLight, fontSize: 12),
          ),
          const SizedBox(height: 8),
          GestureDetector(
            onTap: _showImageSourceSheet,
            child: Container(
              width: double.infinity,
              height: _pickedImage != null ? 200 : 100,
              decoration: BoxDecoration(
                color: AppTheme.inputBg,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: _pickedImage != null
                      ? AppTheme.teal
                      : AppTheme.borderGray,
                ),
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
                                borderRadius: BorderRadius.circular(20),
                              ),
                              child: const Icon(
                                Icons.close,
                                color: Colors.white,
                                size: 18,
                              ),
                            ),
                          ),
                        ),
                      ],
                    )
                  : Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: const [
                        Icon(
                          Icons.add_a_photo_outlined,
                          color: AppTheme.teal,
                          size: 30,
                        ),
                        SizedBox(height: 8),
                        Text(
                          'Tap to add photo',
                          style: TextStyle(
                            color: AppTheme.textLight,
                            fontSize: 13,
                          ),
                        ),
                      ],
                    ),
            ),
          ),
          const SizedBox(height: 24),

          // Error banner
          if (_errorMessage != null) ...[
            Container(
              margin: const EdgeInsets.only(bottom: 16),
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: AppTheme.danger.withValues(alpha: 0.08),
                borderRadius: BorderRadius.circular(10),
                border: Border.all(
                  color: AppTheme.danger.withValues(alpha: 0.3),
                ),
              ),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Icon(
                    Icons.error_outline,
                    color: AppTheme.danger,
                    size: 18,
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      _errorMessage!,
                      style: const TextStyle(
                        color: AppTheme.danger,
                        fontSize: 12,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],

          // Submit button
          SizedBox(
            width: double.infinity,
            height: 52,
            child: ElevatedButton(
              onPressed: _isLoading ? null : _submit,
              child: _isLoading
                  ? const Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        ),
                        SizedBox(width: 12),
                        Text('AI is analysing symptoms...'),
                      ],
                    )
                  : const Text('Submit for AI Triage'),
            ),
          ),
        ],
      ),
    );
  }

  // ── Helpers ────────────────────────────────────────────────────────────────
  Widget _fieldLabel(String text) => Text(
    text,
    style: const TextStyle(
      color: AppTheme.textDark,
      fontSize: 13,
      fontWeight: FontWeight.w600,
    ),
  );

  Widget _chip(String label, Color color) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
    decoration: BoxDecoration(
      color: color.withValues(alpha: 0.12),
      borderRadius: BorderRadius.circular(20),
      border: Border.all(color: color.withValues(alpha: 0.4)),
    ),
    child: Text(
      label,
      style: TextStyle(color: color, fontSize: 12, fontWeight: FontWeight.w600),
    ),
  );

  Widget _cardSection({
    required String title,
    required IconData icon,
    required Color iconColor,
    required Widget child,
  }) => Container(
    padding: const EdgeInsets.all(16),
    decoration: BoxDecoration(
      color: AppTheme.cardWhite,
      borderRadius: BorderRadius.circular(14),
      boxShadow: AppTheme.subtleShadow,
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(icon, color: iconColor, size: 18),
            const SizedBox(width: 8),
            Text(
              title,
              style: const TextStyle(
                color: AppTheme.textDark,
                fontSize: 14,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        const Divider(height: 20, color: AppTheme.borderGray),
        child,
      ],
    ),
  );

  Widget _planRow(IconData icon, String label, String value) => Padding(
    padding: const EdgeInsets.only(bottom: 12),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, color: AppTheme.textLight, size: 16),
        const SizedBox(width: 10),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: const TextStyle(
                  color: AppTheme.textLight,
                  fontSize: 11,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                value,
                style: const TextStyle(color: AppTheme.textDark, fontSize: 13),
              ),
            ],
          ),
        ),
      ],
    ),
  );
}
