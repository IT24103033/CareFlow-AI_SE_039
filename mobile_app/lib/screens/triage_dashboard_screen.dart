// Component B – Triage Status Dashboard
// Shows the patient's triage submission history with status tracking.
// Status workflow: Pending → InReview → Approved / Rejected
// Features:
//  • Pull-to-refresh
//  • Loading spinner, empty state, error state
//  • Expandable AI plan per record
//  • Status colour coding + timeline indicator

import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/triage_record.dart';
import '../services/auth_service.dart';
import '../services/triage_service.dart';

class TriageDashboardScreen extends StatefulWidget {
  const TriageDashboardScreen({super.key});

  @override
  State<TriageDashboardScreen> createState() => _TriageDashboardScreenState();
}

class _TriageDashboardScreenState extends State<TriageDashboardScreen> {
  List<TriageRecord> _records    = [];
  bool               _isLoading  = true;
  String?            _error;
  String?            _patientName;

  static const _purple  = Color(0xFF6C63FF);
  static const _darkBg  = Color(0xFF1A1A2E);
  static const _cardBg  = Color(0xFF16213E);

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error     = null;
    });
    try {
      final id   = await AuthService.getPatientId();
      final name = await AuthService.getPatientName();
      if (id == null) throw Exception('Not logged in.');

      final raw = await TriageService.getMyTriageRecords(id);
      setState(() {
        _records     = raw.map(TriageRecord.fromJson).toList();
        _patientName = name;
        _isLoading   = false;
      });
    } catch (e) {
      setState(() {
        _error     = 'Could not load records.\nMake sure the backend is running.\n\n$e';
        _isLoading = false;
      });
    }
  }

  // ── Status helpers ─────────────────────────────────────────────────────────
  Color _statusColor(String status) {
    switch (status.toLowerCase()) {
      case 'approved':  return const Color(0xFF52C41A);
      case 'rejected':  return const Color(0xFFFF4D4F);
      case 'inreview':  return const Color(0xFF1890FF);
      default:          return const Color(0xFFFAAD14); // Pending
    }
  }

  String _statusLabel(String status) {
    switch (status.toLowerCase()) {
      case 'inreview':  return 'In Review';
      case 'approved':  return 'Approved ✅';
      case 'rejected':  return 'Rejected ❌';
      default:          return 'Pending ⏳';
    }
  }

  Color _severityColor(String? level) {
    switch ((level ?? '').toLowerCase()) {
      case 'critical': return const Color(0xFFFF4D4F);
      case 'high':     return const Color(0xFFFA8C16);
      case 'medium':   return const Color(0xFFFADB14);
      default:         return const Color(0xFF52C41A);
    }
  }

  Map<String, dynamic>? _parseAiPlan(String? raw) {
    if (raw == null) return null;
    try { return jsonDecode(raw) as Map<String, dynamic>; } catch (_) { return null; }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _darkBg,
      appBar: AppBar(
        backgroundColor: _darkBg,
        title: const Text('My Triage Records',
            style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios, color: Colors.white),
          onPressed: () => Navigator.pop(context),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh, color: Colors.white),
            onPressed: _load,
          ),
        ],
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(2),
          child: Container(height: 2, color: _purple),
        ),
      ),
      body: RefreshIndicator(
        color: _purple,
        backgroundColor: _cardBg,
        onRefresh: _load,
        child: _buildBody(),
      ),
    );
  }

  Widget _buildBody() {
    // ── Loading ──────────────────────────────────────────────────────────────
    if (_isLoading) {
      return const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            CircularProgressIndicator(color: Color(0xFF6C63FF)),
            SizedBox(height: 16),
            Text('Loading your records...',
                style: TextStyle(color: Colors.white54)),
          ],
        ),
      );
    }

    // ── Error ────────────────────────────────────────────────────────────────
    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Text('😕', style: TextStyle(fontSize: 48)),
              const SizedBox(height: 16),
              const Text('Something went wrong',
                  style: TextStyle(
                      color: Colors.white,
                      fontSize: 18,
                      fontWeight: FontWeight.bold)),
              const SizedBox(height: 8),
              Text(_error!,
                  textAlign: TextAlign.center,
                  style:
                      const TextStyle(color: Colors.white54, fontSize: 12)),
              const SizedBox(height: 24),
              ElevatedButton.icon(
                onPressed: _load,
                icon: const Icon(Icons.refresh),
                label: const Text('Try Again'),
                style: ElevatedButton.styleFrom(
                    backgroundColor: _purple,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12))),
              ),
            ],
          ),
        ),
      );
    }

    // ── Empty state ──────────────────────────────────────────────────────────
    if (_records.isEmpty) {
      return ListView(   // wrapping in ListView lets pull-to-refresh work
        children: [
          SizedBox(
            height: MediaQuery.of(context).size.height * 0.75,
            child: Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Text('📋', style: TextStyle(fontSize: 56)),
                    const SizedBox(height: 20),
                    const Text('No Triage Records Yet',
                        style: TextStyle(
                            color: Colors.white,
                            fontSize: 20,
                            fontWeight: FontWeight.bold)),
                    const SizedBox(height: 8),
                    const Text(
                      'Submit your first symptom report and our AI will triage it for doctor review.',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: Colors.white54, fontSize: 13),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      );
    }

    // ── Record list ──────────────────────────────────────────────────────────
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        // Patient summary header
        if (_patientName != null)
          Padding(
            padding: const EdgeInsets.only(bottom: 16),
            child: Text(
              'Hello, $_patientName 👋  — ${_records.length} submission${_records.length == 1 ? '' : 's'}',
              style: const TextStyle(
                  color: Colors.white70,
                  fontSize: 14,
                  fontWeight: FontWeight.w500),
            ),
          ),

        // Status legend
        _buildLegend(),
        const SizedBox(height: 16),

        // Records
        ..._records.map((r) => _buildRecordCard(r)),
      ],
    );
  }

  Widget _buildLegend() {
    final items = [
      ('Pending', const Color(0xFFFAAD14)),
      ('In Review', const Color(0xFF1890FF)),
      ('Approved', const Color(0xFF52C41A)),
      ('Rejected', const Color(0xFFFF4D4F)),
    ];
    return Row(
      children: items
          .map((item) => Padding(
                padding: const EdgeInsets.only(right: 10),
                child: Row(
                  children: [
                    Container(
                      width: 8, height: 8,
                      decoration: BoxDecoration(
                          color: item.$2,
                          borderRadius: BorderRadius.circular(4)),
                    ),
                    const SizedBox(width: 4),
                    Text(item.$1,
                        style: const TextStyle(
                            color: Colors.white54, fontSize: 11)),
                  ],
                ),
              ))
          .toList(),
    );
  }

  Widget _buildRecordCard(TriageRecord record) {
    final statusColor   = _statusColor(record.triageStatus);
    final severityColor = _severityColor(record.severityLevel);
    final aiPlan        = _parseAiPlan(record.aiPlan);

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: _cardBg,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: statusColor.withValues(alpha: 0.35)),
        boxShadow: [
          BoxShadow(
              color: Colors.black.withValues(alpha: 0.2),
              blurRadius: 8,
              offset: const Offset(0, 3)),
        ],
      ),
      child: Theme(
        data: Theme.of(context).copyWith(dividerColor: Colors.transparent),
        child: ExpansionTile(
          tilePadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          childrenPadding:
              const EdgeInsets.fromLTRB(16, 0, 16, 16),
          leading: Container(
            width: 44, height: 44,
            decoration: BoxDecoration(
              color: statusColor.withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Center(
              child: Text(
                record.triageStatus.toLowerCase() == 'approved'
                    ? '✅'
                    : record.triageStatus.toLowerCase() == 'rejected'
                        ? '❌'
                        : record.triageStatus.toLowerCase() == 'inreview'
                            ? '🔍'
                            : '⏳',
                style: const TextStyle(fontSize: 22),
              ),
            ),
          ),
          title: Text(
            record.symptoms.length > 60
                ? '${record.symptoms.substring(0, 60)}...'
                : record.symptoms,
            style: const TextStyle(
                color: Colors.white, fontSize: 13, fontWeight: FontWeight.w600),
            maxLines: 2,
          ),
          subtitle: Padding(
            padding: const EdgeInsets.only(top: 6),
            child: Row(
              children: [
                _smallChip(_statusLabel(record.triageStatus), statusColor),
                const SizedBox(width: 8),
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
                style: const TextStyle(color: Colors.white38, fontSize: 11),
              ),
              const SizedBox(height: 4),
              const Icon(Icons.keyboard_arrow_down,
                  color: Colors.white38, size: 20),
            ],
          ),
          children: [
            const Divider(color: Color(0xFF2D2B55)),
            const SizedBox(height: 8),

            // Full symptoms
            _detailRow('📋 Symptoms', record.symptoms),
            const SizedBox(height: 10),

            // Doctor notes (if available)
            if (record.doctorNotes != null && record.doctorNotes!.isNotEmpty)
              _detailRow('🩺 Doctor Notes', record.doctorNotes!),

            // AI Plan
            if (aiPlan != null) ...[
              const SizedBox(height: 10),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: _purple.withValues(alpha: 0.08),
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: _purple.withValues(alpha: 0.3)),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('🤖 AI Plan',
                        style: TextStyle(
                            color: Color(0xFF6C63FF),
                            fontWeight: FontWeight.bold,
                            fontSize: 13)),
                    const SizedBox(height: 8),
                    if (aiPlan['SuggestedSpecialist'] != null ||
                        aiPlan['suggestedSpecialist'] != null)
                      _miniRow(
                          'Specialist',
                          aiPlan['SuggestedSpecialist'] ??
                              aiPlan['suggestedSpecialist']),
                    if (aiPlan['UrgencyLevel'] != null ||
                        aiPlan['urgencyLevel'] != null)
                      _miniRow(
                          'Urgency',
                          aiPlan['UrgencyLevel'] ?? aiPlan['urgencyLevel']),
                    if (aiPlan['RecommendedAction'] != null ||
                        aiPlan['recommendedAction'] != null)
                      _miniRow(
                          'Action',
                          aiPlan['RecommendedAction'] ??
                              aiPlan['recommendedAction']),
                    if (aiPlan['Rationale'] != null ||
                        aiPlan['rationale'] != null)
                      _miniRow(
                          'Rationale',
                          aiPlan['Rationale'] ?? aiPlan['rationale']),
                  ],
                ),
              ),
            ],

            // Status timeline
            const SizedBox(height: 14),
            _buildTimeline(record.triageStatus),
          ],
        ),
      ),
    );
  }

  // ── Status Timeline ────────────────────────────────────────────────────────
  Widget _buildTimeline(String status) {
    final steps = ['Pending', 'InReview', 'Approved / Rejected'];
    final current = status.toLowerCase();
    int activeIndex = 0;
    if (current == 'inreview') activeIndex = 1;
    if (current == 'approved' || current == 'rejected') activeIndex = 2;

    return Row(
      children: List.generate(steps.length, (i) {
        final done   = i <= activeIndex;
        final color  = done ? _purple : const Color(0xFF2D2B55);
        final isLast = i == steps.length - 1;
        return Expanded(
          child: Row(
            children: [
              Column(
                children: [
                  Container(
                    width: 24, height: 24,
                    decoration: BoxDecoration(
                      color: done ? _purple : _cardBg,
                      shape: BoxShape.circle,
                      border: Border.all(color: color, width: 2),
                    ),
                    child: done
                        ? const Icon(Icons.check,
                            color: Colors.white, size: 14)
                        : null,
                  ),
                  const SizedBox(height: 4),
                  Text(steps[i],
                      style: TextStyle(
                          color: done ? Colors.white70 : Colors.white24,
                          fontSize: 9)),
                ],
              ),
              if (!isLast)
                Expanded(
                  child: Container(
                    height: 2,
                    margin: const EdgeInsets.only(bottom: 16),
                    color: i < activeIndex ? _purple : const Color(0xFF2D2B55),
                  ),
                ),
            ],
          ),
        );
      }),
    );
  }

  // ── Helpers ────────────────────────────────────────────────────────────────
  Widget _smallChip(String label, Color color) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.15),
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: color.withValues(alpha: 0.4)),
        ),
        child: Text(label,
            style: TextStyle(
                color: color, fontSize: 11, fontWeight: FontWeight.w600)),
      );

  Widget _detailRow(String label, String value) => Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label,
              style: const TextStyle(
                  color: Colors.white38,
                  fontSize: 11,
                  fontWeight: FontWeight.w600)),
          const SizedBox(height: 3),
          Text(value,
              style: const TextStyle(color: Colors.white70, fontSize: 13)),
        ],
      );

  Widget _miniRow(String label, dynamic value) => Padding(
        padding: const EdgeInsets.only(bottom: 6),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SizedBox(
              width: 72,
              child: Text('$label:',
                  style: const TextStyle(
                      color: Colors.white38, fontSize: 11)),
            ),
            Expanded(
              child: Text(value?.toString() ?? '—',
                  style: const TextStyle(
                      color: Colors.white70, fontSize: 12)),
            ),
          ],
        ),
      );

  String _formatDate(DateTime dt) {
    return '${dt.day.toString().padLeft(2, '0')}/${dt.month.toString().padLeft(2, '0')}/${dt.year}  '
        '${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
  }
}
