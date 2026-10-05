// Component D: Pharmacy Inventory & E-Prescriptions
// Patient-facing screen showing their e-prescriptions with AI safety status.
// Added: Pickup QR codes + Medication readiness notifications (Amodhya)

import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:qr_flutter/qr_flutter.dart';
import '../models/prescription_model.dart';
import '../services/prescription_service.dart';
import '../services/medication_notification_service.dart';
import '../services/auth_service.dart';
import '../theme/app_theme.dart';

// ── Status chip colours
Color _statusColor(String status) {
  switch (status) {
    case 'Issued':    return AppTheme.success;
    case 'Dispensed': return AppTheme.blueDark;
    case 'Cancelled': return AppTheme.danger;
    default:          return AppTheme.pending; // Draft
  }
}

Color _aiColor(String? status) {
  switch (status) {
    case 'Safe':    return AppTheme.success;
    case 'Warning': return AppTheme.warning;
    case 'Blocked': return AppTheme.danger;
    default:        return AppTheme.textMid;
  }
}

String _aiIcon(String? status) {
  switch (status) {
    case 'Safe':    return '✅';
    case 'Warning': return '⚠️';
    case 'Blocked': return '🚫';
    default:        return '🔍';
  }
}

// ── Screen ───────────────────────────────────────────────────────────────────
class MyPrescriptionsScreen extends StatefulWidget {
  const MyPrescriptionsScreen({super.key});

  @override
  State<MyPrescriptionsScreen> createState() => _MyPrescriptionsScreenState();
}

class _MyPrescriptionsScreenState extends State<MyPrescriptionsScreen> {
  List<Prescription> _prescriptions = [];
  bool   _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _fetchPrescriptions();
  }

  Future<void> _fetchPrescriptions() async {
    setState(() { _loading = true; _error = null; });
    try {
      final patientId = await AuthService.getPatientId();
      if (patientId == null || patientId.isEmpty) {
        setState(() { _error = 'Could not find your Patient ID. Please login again.'; _loading = false; });
        return;
      }
      final token = await AuthService.getAccessToken();
      if (token == null || token.isEmpty) {
        setState(() { _error = 'Session expired. Please log out and log in again.'; _loading = false; });
        return;
      }
      final results = await PrescriptionService.getPrescriptionsByPatient(patientId);
      setState(() { _prescriptions = results; _loading = false; });
    } on Exception catch (e) {
      final msg = e.toString();
      String errorText;
      if (msg.contains('401') || msg.contains('Unauthorized')) {
        errorText = 'Session expired. Please log out and log in again to refresh your session.';
      } else if (msg.contains('403')) {
        errorText = 'Access denied. Your account may not have permission to view prescriptions.';
      } else if (msg.contains('SocketException') || msg.contains('Connection refused') || msg.contains('Failed host lookup')) {
        errorText = 'Cannot reach the server. Please check your network connection and try again.';
      } else {
        errorText = 'Could not load prescriptions. Please try again.\n\nDetails: $msg';
      }
      setState(() { _error = errorText; _loading = false; });
    }
  }


  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.pageWhite,
      appBar: AppBar(
        backgroundColor: AppTheme.cardWhite,
        foregroundColor: AppTheme.textDark,
        elevation: 0,
        title: const Text(
          '💊 My Prescriptions',
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 20),
        ),
        centerTitle: false,
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(1),
          child: Container(height: 1, color: AppTheme.borderGray),
        ),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator(color: AppTheme.teal))
          : _error != null
              ? _emptyState(icon: '⚠️', title: 'Error Loading Data', subtitle: _error!)
              : _prescriptions.isEmpty
                  ? _emptyState(
                      icon: '📋',
                      title: 'No prescriptions found',
                      subtitle: 'You currently have no active prescriptions.',
                    )
                  : RefreshIndicator(
                      onRefresh: _fetchPrescriptions,
                      color: AppTheme.teal,
                      child: ListView.builder(
                        padding: const EdgeInsets.all(16),
                        itemCount: _prescriptions.length,
                        itemBuilder: (context, i) => _PrescriptionCard(prescription: _prescriptions[i]),
                      ),
                    ),
    );
  }

  Widget _emptyState({required String icon, required String title, required String subtitle}) {
    return Center(
      child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
        Text(icon, style: const TextStyle(fontSize: 52)),
        const SizedBox(height: 16),
        Text(title, style: const TextStyle(color: AppTheme.textDark, fontSize: 17, fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        Text(subtitle, style: const TextStyle(color: AppTheme.textMid, fontSize: 13), textAlign: TextAlign.center),
      ]),
    );
  }
}

// ── Prescription Card ─────────────────────────────────────────────────────────
class _PrescriptionCard extends StatefulWidget {
  final Prescription prescription;
  const _PrescriptionCard({required this.prescription});

  @override
  State<_PrescriptionCard> createState() => _PrescriptionCardState();
}

class _PrescriptionCardState extends State<_PrescriptionCard> {
  bool _expanded = false;

  // ── QR pickup sheet ──────────────────────────────────────────────────────
  void _showPickupQr(BuildContext context, Prescription px) {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.transparent,
      isScrollControlled: true,
      builder: (_) => _PickupQrSheet(prescription: px),
    );
  }

  // ── Medication readiness notification ────────────────────────────────────
  Future<void> _sendReadyNotification(Prescription px) async {
    final summary = px.items
        .map((i) => '${i.medicineName} ×${i.quantity}')
        .join(', ');
    await MedicationNotificationService.instance.notifyMedicationReady(
      prescriptionId: px.id,
      medicineSummary: summary,
    );
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: const Text('🔔 Medication readiness notification sent!'),
          backgroundColor: AppTheme.teal,
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          duration: const Duration(seconds: 2),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final px = widget.prescription;
    final createdDate = DateTime.tryParse(px.createdAt);
    final dateStr     = createdDate != null
        ? '${createdDate.day}/${createdDate.month}/${createdDate.year}'
        : px.createdAt;

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: AppTheme.cardWhite,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppTheme.borderGray),
        boxShadow: AppTheme.subtleShadow,
      ),
      child: Column(
        children: [
          // ── Card Header ────────────────────────────────────────────────────
          InkWell(
            borderRadius: BorderRadius.circular(14),
            onTap: () => setState(() => _expanded = !_expanded),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Left icon
                  Container(
                    width: 44, height: 44,
                    decoration: BoxDecoration(color: AppTheme.tealLight, borderRadius: BorderRadius.circular(12)),
                    child: const Center(child: Text('💊', style: TextStyle(fontSize: 22))),
                  ),
                  const SizedBox(width: 14),
                  // Title block
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(children: [
                          _StatusChip(label: px.status, color: _statusColor(px.status)),
                          const SizedBox(width: 8),
                          if (px.aiSafetyStatus != null)
                            _StatusChip(
                              label: '${_aiIcon(px.aiSafetyStatus)} ${px.aiSafetyStatus!}',
                              color: _aiColor(px.aiSafetyStatus),
                            ),
                        ]),
                        const SizedBox(height: 6),
                        Text(
                          '${px.items.length} medicine${px.items.length != 1 ? 's' : ''} prescribed',
                          style: const TextStyle(color: AppTheme.textDark, fontSize: 15, fontWeight: FontWeight.w600),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Issued $dateStr${px.triageSeverity.isNotEmpty ? " · Triage: ${px.triageSeverity}" : ""}',
                          style: const TextStyle(color: AppTheme.textMid, fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                  // QR pickup button (only for Issued prescriptions)
                  if (px.status == 'Issued') ...[
                    IconButton(
                      tooltip: 'Show pickup QR code',
                      icon: const Icon(Icons.qr_code_2_rounded, color: AppTheme.teal),
                      onPressed: () => _showPickupQr(context, px),
                      padding: EdgeInsets.zero,
                      constraints: const BoxConstraints(),
                    ),
                    const SizedBox(width: 4),
                  ],
                  // Notification bell (only for Issued prescriptions)
                  if (px.status == 'Issued')
                    IconButton(
                      tooltip: 'Send medication ready notification',
                      icon: const Icon(Icons.notifications_active_rounded, color: AppTheme.warning),
                      onPressed: () => _sendReadyNotification(px),
                      padding: EdgeInsets.zero,
                      constraints: const BoxConstraints(),
                    ),
                  const SizedBox(width: 4),
                  // Expand chevron
                  Icon(_expanded ? Icons.expand_less : Icons.expand_more, color: AppTheme.textLight),
                ],
              ),
            ),
          ),

          // ── Expanded Detail ───────────────────────────────────────────────
          if (_expanded)
            Container(
              decoration: const BoxDecoration(
                border: Border(top: BorderSide(color: AppTheme.borderGray)),
              ),
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // AI Safety panel
                  if (px.aiSafetyStatus != null && px.aiSafetyCheckResult != null)
                    _AiSafetyPanel(
                      status: px.aiSafetyStatus!,
                      resultJson: px.aiSafetyCheckResult!,
                    ),

                  // Medicine list
                  const Padding(
                    padding: EdgeInsets.only(bottom: 10),
                    child: Text('Medicines:', style: TextStyle(color: AppTheme.textDark, fontWeight: FontWeight.bold, fontSize: 13)),
                  ),
                  ...px.items.map((item) => _MedicineItemTile(item: item)),

                  // Notes
                  if (px.notes != null && px.notes!.isNotEmpty) ...[
                    const SizedBox(height: 12),
                    const Text('Notes:', style: TextStyle(color: AppTheme.textDark, fontWeight: FontWeight.bold, fontSize: 13)),
                    const SizedBox(height: 4),
                    Text(px.notes!, style: const TextStyle(color: AppTheme.textMid, fontSize: 13)),
                  ],

                  // Notification badge
                  if (px.notificationSent) ...[
                    const SizedBox(height: 12),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                      decoration: BoxDecoration(
                        color: AppTheme.success.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: AppTheme.success.withValues(alpha: 0.3)),
                      ),
                      child: Text(
                        '✅ Notified via ${px.notificationChannel ?? "Email"}',
                        style: const TextStyle(color: AppTheme.success, fontSize: 12, fontWeight: FontWeight.w600),
                      ),
                    ),
                  ],
                ],
              ),
            ),
        ],
      ),
    );
  }
}

// ── AI Safety Panel ───────────────────────────────────────────────────────────
class _AiSafetyPanel extends StatelessWidget {
  final String status;
  final String resultJson;
  const _AiSafetyPanel({required this.status, required this.resultJson});

  @override
  Widget build(BuildContext context) {
    final color = _aiColor(status);
    Map<String, dynamic>? data;
    try { data = jsonDecode(resultJson) as Map<String, dynamic>; } catch (_) {}

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: color.withValues(alpha: 0.3)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            '${_aiIcon(status)} AI Safety: $status',
            style: TextStyle(color: color, fontWeight: FontWeight.bold, fontSize: 14),
          ),
          if (data?['Summary'] != null) ...[
            const SizedBox(height: 6),
            Text(data!['Summary'] as String, style: const TextStyle(color: AppTheme.textMid, fontSize: 12)),
          ],
          if ((data?['Errors'] as List?)?.isNotEmpty == true) ...[
            const SizedBox(height: 6),
            ...(data!['Errors'] as List).map((e) => Padding(
              padding: const EdgeInsets.only(top: 3),
              child: Text('• $e', style: const TextStyle(color: AppTheme.danger, fontSize: 12)),
            )),
          ],
          if ((data?['Warnings'] as List?)?.isNotEmpty == true) ...[
            const SizedBox(height: 6),
            ...(data!['Warnings'] as List).map((w) => Padding(
              padding: const EdgeInsets.only(top: 3),
              child: Text('• $w', style: const TextStyle(color: AppTheme.warning, fontSize: 12)),
            )),
          ],
        ],
      ),
    );
  }
}

// ── Medicine Item Tile ─────────────────────────────────────────────────────────
class _MedicineItemTile extends StatelessWidget {
  final PrescriptionItem item;
  const _MedicineItemTile({required this.item});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: AppTheme.inputBg,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: AppTheme.borderGray),
      ),
      child: Row(
        children: [
          const Text('💊', style: TextStyle(fontSize: 18)),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(item.medicineName,
                  style: const TextStyle(color: AppTheme.textDark, fontSize: 14, fontWeight: FontWeight.w600)),
                const SizedBox(height: 2),
                Text(item.dosage,
                  style: const TextStyle(color: AppTheme.textMid, fontSize: 12)),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text('×${item.quantity}', style: const TextStyle(color: AppTheme.teal, fontWeight: FontWeight.bold)),
              Text('${item.durationDays}d', style: const TextStyle(color: AppTheme.textLight, fontSize: 11)),
            ],
          ),
        ],
      ),
    );
  }
}

// ── Status Chip ───────────────────────────────────────────────────────────────
class _StatusChip extends StatelessWidget {
  final String label;
  final Color  color;
  const _StatusChip({required this.label, required this.color});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withValues(alpha: 0.4)),
      ),
      child: Text(label,
        style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.bold, letterSpacing: 0.3)),
    );
  }
}

// ── Pickup QR Sheet ───────────────────────────────────────────────────────────
class _PickupQrSheet extends StatelessWidget {
  final Prescription prescription;
  const _PickupQrSheet({required this.prescription});

  String get _qrData {
    final px = prescription;
    return 'cf-rx:${px.id}|patient:${px.patientId}|items:${px.items.length}|status:${px.status}';
  }

  @override
  Widget build(BuildContext context) {
    final summaryLines = prescription.items
        .map((i) => '${i.medicineName}  ×${i.quantity}  (${i.dosage})')
        .toList();

    return Container(
      decoration: const BoxDecoration(
        color: AppTheme.cardWhite,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      padding: const EdgeInsets.fromLTRB(24, 12, 24, 32),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // Drag handle
          Container(
            width: 40, height: 4,
            margin: const EdgeInsets.only(bottom: 20),
            decoration: BoxDecoration(
              color: AppTheme.borderGray,
              borderRadius: BorderRadius.circular(2),
            ),
          ),

          const Text(
            '📦 Pharmacy Pickup QR',
            style: TextStyle(
                color: AppTheme.textDark, fontSize: 18, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 6),
          const Text(
            'Show this QR code to the pharmacist to collect your medication.',
            textAlign: TextAlign.center,
            style: TextStyle(color: AppTheme.textMid, fontSize: 13),
          ),
          const SizedBox(height: 20),

          // QR code
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: AppTheme.borderGray),
            ),
            child: QrImageView(
              data: _qrData,
              version: QrVersions.auto,
              size: 220,
              backgroundColor: Colors.white,
              eyeStyle: const QrEyeStyle(
                eyeShape: QrEyeShape.square,
                color: AppTheme.navyDark,
              ),
              dataModuleStyle: const QrDataModuleStyle(
                dataModuleShape: QrDataModuleShape.circle,
                color: AppTheme.teal,
              ),
            ),
          ),
          const SizedBox(height: 20),

          // Prescription summary under QR
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: AppTheme.inputBg,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppTheme.teal.withValues(alpha: 0.3)),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '${prescription.items.length} medicine${prescription.items.length != 1 ? "s" : ""}',
                  style: const TextStyle(
                      color: AppTheme.teal, fontWeight: FontWeight.bold, fontSize: 13),
                ),
                const SizedBox(height: 6),
                ...summaryLines.map(
                  (l) => Padding(
                    padding: const EdgeInsets.only(top: 3),
                    child: Text('• $l',
                        style: const TextStyle(
                            color: AppTheme.textMid, fontSize: 12)),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          SizedBox(
            width: double.infinity,
            child: TextButton(
              onPressed: () => Navigator.pop(context),
              style: TextButton.styleFrom(foregroundColor: AppTheme.textMid),
              child: const Text('Close'),
            ),
          ),
        ],
      ),
    );
  }
}
