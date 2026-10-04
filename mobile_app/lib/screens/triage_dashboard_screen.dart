// Component B – Triage Status Dashboard (Redesigned)
// CareFlow AI healthcare aesthetic: navy header + white cards.

import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/triage_record.dart';
import '../services/triage_repository.dart';
import '../theme/app_theme.dart';

class TriageDashboardScreen extends StatefulWidget {
  final TriageRepository repository;
  const TriageDashboardScreen({
    super.key,
    this.repository = const ApiTriageRepository(),
  });

  @override
  State<TriageDashboardScreen> createState() => _TriageDashboardScreenState();
}

class _TriageDashboardScreenState extends State<TriageDashboardScreen> {
  List<TriageRecord> _records = [];
  bool _isLoading = true;
  String? _error;
  String? _patientName;
  int _loadVersion = 0;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final version = ++_loadVersion;
    setState(() {
      _isLoading = true;
      _error = null;
    });
    try {
      final history = await widget.repository.loadHistory();
      if (!mounted || version != _loadVersion) return;
      setState(() {
        _records = history.records;
        _patientName = history.patientName;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted || version != _loadVersion) return;
      setState(() {
        _error =
            'Could not load records.\nMake sure the backend is running.\n\n$e';
        _isLoading = false;
      });
    }
  }

  // ── Status helpers ─────────────────────────────────────────────────────────
  Color _statusColor(String status) {
    switch (status.toLowerCase()) {
      case 'approved':
        return AppTheme.success;
      case 'rejected':
        return AppTheme.danger;
      case 'assessmentfailed':
        return AppTheme.danger;
      case 'inreview':
        return const Color(0xFF3182CE);
      default:
        return AppTheme.pending;
    }
  }

  String _statusLabel(String status) {
    switch (status.toLowerCase()) {
      case 'inreview':
        return 'In Review';
      case 'assessmentfailed':
        return 'Assessment unavailable';
      case 'revisionrequested':
        return 'Revision requested';
      case 'approved':
        return 'Approved';
      case 'rejected':
        return 'Rejected';
      default:
        return 'Pending';
    }
  }

  IconData _statusIcon(String status) {
    switch (status.toLowerCase()) {
      case 'approved':
        return Icons.check_circle_outline;
      case 'rejected':
        return Icons.cancel_outlined;
      case 'inreview':
        return Icons.manage_search_outlined;
      case 'assessmentfailed':
        return Icons.error_outline;
      case 'revisionrequested':
        return Icons.edit_note;
      default:
        return Icons.hourglass_top_outlined;
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

  Map<String, dynamic>? _parseAiPlan(String? raw) {
    if (raw == null) return null;
    try {
      return jsonDecode(raw) as Map<String, dynamic>;
    } catch (_) {
      return null;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.pageWhite,
      body: Column(
        children: [
          _buildHeader(),
          Expanded(
            child: RefreshIndicator(
              color: AppTheme.teal,
              backgroundColor: AppTheme.cardWhite,
              onRefresh: _load,
              child: _buildBody(),
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
              padding: const EdgeInsets.fromLTRB(8, 8, 8, 0),
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
                      'My Triage Records',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 19,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  IconButton(
                    icon: const Icon(
                      Icons.refresh_outlined,
                      color: Color(0xFFAEC0D8),
                    ),
                    onPressed: _load,
                  ),
                ],
              ),
            ),
            if (_patientName != null)
              Padding(
                padding: const EdgeInsets.fromLTRB(20, 6, 20, 0),
                child: Text(
                  '${_records.length} submission${_records.length == 1 ? '' : 's'} for $_patientName',
                  style: const TextStyle(
                    color: Color(0xFFAEC0D8),
                    fontSize: 13,
                  ),
                ),
              ),
            Container(
              height: 24,
              margin: const EdgeInsets.only(top: 14),
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

  Widget _buildBody() {
    if (_isLoading) {
      return const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            CircularProgressIndicator(color: AppTheme.teal),
            SizedBox(height: 16),
            Text(
              'Loading your records...',
              style: TextStyle(color: AppTheme.textMid),
            ),
          ],
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.cloud_off_outlined,
                color: AppTheme.textLight,
                size: 56,
              ),
              const SizedBox(height: 16),
              const Text(
                'Something went wrong',
                style: TextStyle(
                  color: AppTheme.textDark,
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                _error!,
                textAlign: TextAlign.center,
                style: const TextStyle(color: AppTheme.textMid, fontSize: 12),
              ),
              const SizedBox(height: 24),
              ElevatedButton.icon(
                onPressed: _load,
                icon: const Icon(Icons.refresh, size: 18),
                label: const Text('Try Again'),
                style: ElevatedButton.styleFrom(
                  minimumSize: const Size(180, 46),
                ),
              ),
            ],
          ),
        ),
      );
    }

    if (_records.isEmpty) {
      return ListView(
        children: [
          SizedBox(
            height: MediaQuery.of(context).size.height * 0.70,
            child: Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Container(
                      width: 80,
                      height: 80,
                      decoration: BoxDecoration(
                        color: AppTheme.teal.withValues(alpha: 0.08),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(
                        Icons.list_alt_outlined,
                        color: AppTheme.teal,
                        size: 40,
                      ),
                    ),
                    const SizedBox(height: 20),
                    const Text(
                      'No Triage Records Yet',
                      style: TextStyle(
                        color: AppTheme.textDark,
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 8),
                    const Text(
                      'Submit your first symptom report and our AI will triage it for doctor review.',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: AppTheme.textMid, fontSize: 13),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      );
    }

    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 4, 20, 32),
      children: [
        _buildLegend(),
        const SizedBox(height: 16),
        ..._records.map((r) => _buildRecordCard(r)),
      ],
    );
  }

  Widget _buildLegend() {
    final items = [
      ('Pending', AppTheme.pending),
      ('In Review', const Color(0xFF3182CE)),
      ('Approved', AppTheme.success),
      ('Rejected', AppTheme.danger),
      ('Assessment unavailable', AppTheme.danger),
      ('Revision requested', AppTheme.warning),
    ];
    return Wrap(
      runSpacing: 8,
      children: items
          .map(
            (item) => Padding(
              padding: const EdgeInsets.only(right: 14),
              child: Row(
                children: [
                  Container(
                    width: 8,
                    height: 8,
                    decoration: BoxDecoration(
                      color: item.$2,
                      borderRadius: BorderRadius.circular(4),
                    ),
                  ),
                  const SizedBox(width: 4),
                  Text(
                    item.$1,
                    style: const TextStyle(
                      color: AppTheme.textMid,
                      fontSize: 11,
                    ),
                  ),
                ],
              ),
            ),
          )
          .toList(),
    );
  }

  Widget _buildRecordCard(TriageRecord record) {
    final statusColor = _statusColor(record.triageStatus);
    final severityColor = _severityColor(record.severityLevel);
    final aiPlan = _parseAiPlan(record.aiPlan);

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: AppTheme.cardWhite,
        borderRadius: BorderRadius.circular(16),
        border: Border(left: BorderSide(color: statusColor, width: 4)),
        boxShadow: AppTheme.subtleShadow,
      ),
      child: Material(
        color: Colors.transparent,
        borderRadius: BorderRadius.circular(16),
        clipBehavior: Clip.antiAlias,
        child: Theme(
          data: Theme.of(context).copyWith(dividerColor: Colors.transparent),
          child: ExpansionTile(
            tilePadding: const EdgeInsets.symmetric(
              horizontal: 16,
              vertical: 8,
            ),
            childrenPadding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
            leading: Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: statusColor.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(
                _statusIcon(record.triageStatus),
                color: statusColor,
                size: 24,
              ),
            ),
            title: Text(
              record.symptoms.length > 60
                  ? '${record.symptoms.substring(0, 60)}...'
                  : record.symptoms,
              style: const TextStyle(
                color: AppTheme.textDark,
                fontSize: 13,
                fontWeight: FontWeight.w600,
              ),
              maxLines: 2,
            ),
            subtitle: Padding(
              padding: const EdgeInsets.only(top: 6),
              child: Wrap(
                spacing: 8,
                runSpacing: 6,
                children: [
                  _smallChip(_statusLabel(record.triageStatus), statusColor),
                  if (record.severityLevel.isNotEmpty)
                    _smallChip(record.severityLevel, severityColor),
                ],
              ),
            ),
            trailing: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(
                  _formatDate(record.createdAt),
                  style: const TextStyle(
                    color: AppTheme.textLight,
                    fontSize: 10,
                  ),
                ),
                const SizedBox(height: 4),
                const Icon(
                  Icons.keyboard_arrow_down,
                  color: AppTheme.textLight,
                  size: 20,
                ),
              ],
            ),
            children: [
              const Divider(color: AppTheme.borderGray),
              const SizedBox(height: 8),

              _detailRow(
                'Symptoms',
                record.symptoms,
                Icons.description_outlined,
              ),
              const SizedBox(height: 10),

              if (record.doctorNotes != null && record.doctorNotes!.isNotEmpty)
                _detailRow(
                  'Doctor Notes',
                  record.doctorNotes!,
                  Icons.local_hospital_outlined,
                ),

              if (record.triageStatus == 'AssessmentFailed')
                const Text(
                  'Assessment unavailable. No urgency has been assigned; contact staff for follow-up.',
                  style: TextStyle(color: AppTheme.danger),
                ),

              if (record.safetyVerdict != null && record.safetyVerdict!.isNotEmpty) ...[
                const SizedBox(height: 10),
                _detailRow(
                  'Safety & Context Findings',
                  '${record.safetyVerdict!}\n${record.safetySummary ?? ""}'.trim(),
                  Icons.shield_outlined,
                ),
              ],

              if (record.schedulingOutcome != null && record.schedulingOutcome!.isNotEmpty) ...[
                const SizedBox(height: 10),
                _detailRow(
                  'Scheduling Outcome',
                  record.appointmentDetails != null 
                    ? '${record.schedulingOutcome!}\nDoctor: ${record.appointmentDetails!["doctorName"] ?? "Unknown"}\nDate: ${record.appointmentDetails!["appointmentDate"] ?? ""} at ${record.appointmentDetails!["startTime"] ?? ""}\nStatus: ${record.appointmentDetails!["status"] ?? ""}'
                    : record.schedulingOutcome!,
                  Icons.calendar_month_outlined,
                ),
              ],

              if (record.notificationOutcome != null && record.notificationOutcome!.isNotEmpty) ...[
                const SizedBox(height: 10),
                _detailRow(
                  'Execution & Notification',
                  record.notificationOutcome!,
                  Icons.notifications_outlined,
                ),
              ],

              if (aiPlan != null) ...[
                const SizedBox(height: 10),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: AppTheme.tealLight,
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: const [
                          Icon(
                            Icons.psychology_outlined,
                            color: AppTheme.teal,
                            size: 16,
                          ),
                          SizedBox(width: 6),
                          Text(
                            'AI Clinical Plan',
                            style: TextStyle(
                              color: AppTheme.teal,
                              fontWeight: FontWeight.bold,
                              fontSize: 13,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      if (aiPlan['SuggestedSpecialist'] != null ||
                          aiPlan['suggestedSpecialist'] != null)
                        _miniRow(
                          'Specialist',
                          aiPlan['SuggestedSpecialist'] ??
                              aiPlan['suggestedSpecialist'],
                        ),
                      if (aiPlan['UrgencyLevel'] != null ||
                          aiPlan['urgencyLevel'] != null)
                        _miniRow(
                          'Urgency',
                          aiPlan['UrgencyLevel'] ?? aiPlan['urgencyLevel'],
                        ),
                      if (aiPlan['RecommendedAction'] != null ||
                          aiPlan['recommendedAction'] != null)
                        _miniRow(
                          'Action',
                          aiPlan['RecommendedAction'] ??
                              aiPlan['recommendedAction'],
                        ),
                      if (aiPlan['Rationale'] != null ||
                          aiPlan['rationale'] != null)
                        _miniRow(
                          'Rationale',
                          aiPlan['Rationale'] ?? aiPlan['rationale'],
                        ),
                    ],
                  ),
                ),
              ],

              const SizedBox(height: 14),
              _buildTimeline(record.triageStatus),
              if (record.triageStatus == 'RevisionRequested') ...[
                const SizedBox(height: 16),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: () => _showRevisionDialog(record),
                    icon: const Icon(Icons.edit, size: 18),
                    label: const Text('Revise Submission'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppTheme.navyDark,
                      foregroundColor: Colors.white,
                    ),
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }

  // ── Status Timeline ────────────────────────────────────────────────────────
  Widget _buildTimeline(String status) {
    if (status == 'AssessmentFailed') {
      return const Text(
        'Submission saved · Assessment unavailable',
        style: TextStyle(color: AppTheme.danger),
      );
    }
    if (status == 'RevisionRequested') {
      return const Text(
        'Submission saved · Revision requested',
        style: TextStyle(color: AppTheme.warning),
      );
    }
    final steps = ['Pending', 'In Review', 'Complete'];
    final current = status.toLowerCase();
    int activeIndex = 0;
    if (current == 'inreview') activeIndex = 1;
    if (current == 'approved' || current == 'rejected') activeIndex = 2;

    return Row(
      children: List.generate(steps.length, (i) {
        final done = i <= activeIndex;
        final color = done ? AppTheme.teal : AppTheme.borderGray;
        final isLast = i == steps.length - 1;
        return Expanded(
          child: Row(
            children: [
              Column(
                children: [
                  Container(
                    width: 26,
                    height: 26,
                    decoration: BoxDecoration(
                      color: done ? AppTheme.teal : AppTheme.inputBg,
                      shape: BoxShape.circle,
                      border: Border.all(color: color, width: 2),
                    ),
                    child: done
                        ? const Icon(Icons.check, color: Colors.white, size: 14)
                        : null,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    steps[i],
                    style: TextStyle(
                      color: done ? AppTheme.teal : AppTheme.textLight,
                      fontSize: 9,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
              if (!isLast)
                Expanded(
                  child: Container(
                    height: 2,
                    margin: const EdgeInsets.only(bottom: 18),
                    color: i < activeIndex
                        ? AppTheme.teal
                        : AppTheme.borderGray,
                  ),
                ),
            ],
          ),
        );
      }),
    );
  }

  void _showRevisionDialog(TriageRecord record) {
    final TextEditingController symptomsController =
        TextEditingController(text: record.symptoms);
    bool isSubmitting = false;

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Revise Submission'),
              content: SizedBox(
                width: double.maxFinite,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    if (record.doctorNotes != null && record.doctorNotes!.isNotEmpty) ...[
                      const Text(
                        'Doctor Notes:',
                        style: TextStyle(
                            fontWeight: FontWeight.bold, color: AppTheme.danger),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        record.doctorNotes!,
                        style: const TextStyle(fontSize: 13),
                      ),
                      const SizedBox(height: 16),
                    ],
                    const Text('Update your symptoms:', style: TextStyle(fontWeight: FontWeight.bold)),
                    const SizedBox(height: 8),
                    TextField(
                      controller: symptomsController,
                      maxLines: 5,
                      decoration: const InputDecoration(
                        border: OutlineInputBorder(),
                        hintText: 'Describe your symptoms...',
                      ),
                    ),
                  ],
                ),
              ),
              actions: [
                if (!isSubmitting)
                  TextButton(
                    onPressed: () => Navigator.pop(context),
                    child: const Text('Cancel', style: TextStyle(color: AppTheme.textMid)),
                  ),
                ElevatedButton(
                  onPressed: isSubmitting
                      ? null
                      : () async {
                          if (symptomsController.text.trim().length < 20) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(
                                content: Text('Please provide at least 20 characters.'),
                              ),
                            );
                            return;
                          }
                          setDialogState(() => isSubmitting = true);
                          try {
                            await widget.repository.revise(
                              triageId: record.id,
                              updatedSymptoms: symptomsController.text.trim(),
                              expectedUpdatedAt: record.updatedAt,
                            );
                            if (context.mounted) {
                              Navigator.pop(context);
                              _load(); // Reload dashboard
                            }
                          } catch (e) {
                            setDialogState(() => isSubmitting = false);
                            if (context.mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                SnackBar(content: Text('Error: $e')),
                              );
                            }
                          }
                        },
                  style: ElevatedButton.styleFrom(backgroundColor: AppTheme.teal),
                  child: isSubmitting
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                        )
                      : const Text('Submit', style: TextStyle(color: Colors.white)),
                ),
              ],
            );
          },
        );
      },
    );
  }

  // ── Helpers ────────────────────────────────────────────────────────────────
  Widget _smallChip(String label, Color color) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
    decoration: BoxDecoration(
      color: color.withValues(alpha: 0.12),
      borderRadius: BorderRadius.circular(20),
      border: Border.all(color: color.withValues(alpha: 0.4)),
    ),
    child: Text(
      label,
      style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.w600),
    ),
  );

  Widget _detailRow(String label, String value, IconData icon) => Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      Row(
        children: [
          Icon(icon, color: AppTheme.textLight, size: 14),
          const SizedBox(width: 6),
          Text(
            label,
            style: const TextStyle(
              color: AppTheme.textLight,
              fontSize: 11,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
      const SizedBox(height: 3),
      Text(
        value,
        style: const TextStyle(color: AppTheme.textMid, fontSize: 13),
      ),
    ],
  );

  Widget _miniRow(String label, dynamic value) => Padding(
    padding: const EdgeInsets.only(bottom: 6),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 76,
          child: Text(
            '$label:',
            style: const TextStyle(color: Color(0xFF285E61), fontSize: 11),
          ),
        ),
        Expanded(
          child: Text(
            value?.toString() ?? '—',
            style: const TextStyle(color: AppTheme.textDark, fontSize: 12),
          ),
        ),
      ],
    ),
  );

  String _formatDate(DateTime dt) {
    return '${dt.day.toString().padLeft(2, '0')}/${dt.month.toString().padLeft(2, '0')}/${dt.year}  '
        '${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
  }
}
