// Component D: Pharmacy Inventory & E-Prescriptions
// Patient-facing screen showing their e-prescriptions with AI safety status.

import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/prescription_model.dart';
import '../services/prescription_service.dart';

// ── Colour & style constants ─────────────────────────────────────────────────
const _purple      = Color(0xFF6C63FF);
const _darkBg      = Color(0xFF1A1A2E);
const _cardBg      = Color(0xFF16213E);
const _cardBorder  = Color(0xFF2D2B55);

// Status chip colours
Color _statusColor(String status) {
  switch (status) {
    case 'Issued':    return const Color(0xFF1890FF);
    case 'Dispensed': return const Color(0xFF52C41A);
    case 'Cancelled': return const Color(0xFFFF4D4F);
    default:          return const Color(0xFFFA8C16); // Draft
  }
}

Color _aiColor(String? status) {
  switch (status) {
    case 'Safe':    return const Color(0xFF52C41A);
    case 'Warning': return const Color(0xFFFA8C16);
    case 'Blocked': return const Color(0xFFFF4D4F);
    default:        return Colors.grey;
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
  final TextEditingController _patientIdController = TextEditingController();
  List<Prescription> _prescriptions = [];
  bool   _loading = false;
  String? _error;
  bool   _searched = false;

  Future<void> _fetchPrescriptions() async {
    final patientId = _patientIdController.text.trim();
    if (patientId.isEmpty) {
      setState(() => _error = 'Please enter your Patient ID.');
      return;
    }
    setState(() { _loading = true; _error = null; _searched = true; });
    try {
      final results = await PrescriptionService.getPrescriptionsByPatient(patientId);
      setState(() { _prescriptions = results; _loading = false; });
    } catch (e) {
      setState(() { _error = 'Could not load prescriptions. Check your Patient ID or try again.'; _loading = false; });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _darkBg,
      appBar: AppBar(
        backgroundColor: _darkBg,
        elevation: 0,
        title: const Text(
          '💊 My Prescriptions',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 20),
        ),
        centerTitle: false,
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(2),
          child: Container(height: 2, color: _purple),
        ),
      ),
      body: Column(
        children: [
          // ── Patient ID Search Bar ──────────────────────────────────────────
          Container(
            color: _cardBg,
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Enter your Patient ID to view your prescriptions',
                  style: TextStyle(color: Colors.white70, fontSize: 13),
                ),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _patientIdController,
                        style: const TextStyle(color: Colors.white, fontSize: 13),
                        decoration: InputDecoration(
                          hintText: 'Paste your Patient GUID...',
                          hintStyle: const TextStyle(color: Colors.white38),
                          filled: true,
                          fillColor: _darkBg,
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                            borderSide: const BorderSide(color: _cardBorder),
                          ),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                            borderSide: const BorderSide(color: _cardBorder),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                            borderSide: const BorderSide(color: _purple, width: 2),
                          ),
                          contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                        ),
                        onSubmitted: (_) => _fetchPrescriptions(),
                      ),
                    ),
                    const SizedBox(width: 10),
                    ElevatedButton(
                      onPressed: _loading ? null : _fetchPrescriptions,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: _purple,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      child: _loading
                          ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                          : const Text('Search', style: TextStyle(fontWeight: FontWeight.bold)),
                    ),
                  ],
                ),
                if (_error != null)
                  Padding(
                    padding: const EdgeInsets.only(top: 10),
                    child: Text(_error!, style: const TextStyle(color: Color(0xFFFF4D4F), fontSize: 13)),
                  ),
              ],
            ),
          ),

          // ── Results ──────────────────────────────────────────────────────────
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator(color: _purple))
                : !_searched
                    ? _emptyState(
                        icon: '🔍',
                        title: 'Search for your prescriptions',
                        subtitle: 'Enter your Patient ID above to view your e-prescriptions.',
                      )
                    : _prescriptions.isEmpty
                        ? _emptyState(
                            icon: '📋',
                            title: 'No prescriptions found',
                            subtitle: 'You have no prescriptions linked to this Patient ID.',
                          )
                        : ListView.builder(
                            padding: const EdgeInsets.all(16),
                            itemCount: _prescriptions.length,
                            itemBuilder: (context, i) => _PrescriptionCard(prescription: _prescriptions[i]),
                          ),
          ),
        ],
      ),
    );
  }

  Widget _emptyState({required String icon, required String title, required String subtitle}) {
    return Center(
      child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
        Text(icon, style: const TextStyle(fontSize: 52)),
        const SizedBox(height: 16),
        Text(title, style: const TextStyle(color: Colors.white, fontSize: 17, fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        Text(subtitle, style: const TextStyle(color: Colors.white54, fontSize: 13), textAlign: TextAlign.center),
      ]),
    );
  }

  @override
  void dispose() {
    _patientIdController.dispose();
    super.dispose();
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
        color: _cardBg,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: _cardBorder),
        boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.15), blurRadius: 8, offset: const Offset(0, 3))],
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
                    decoration: BoxDecoration(color: _purple.withOpacity(0.15), borderRadius: BorderRadius.circular(12)),
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
                          style: const TextStyle(color: Colors.white, fontSize: 15, fontWeight: FontWeight.w600),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Issued $dateStr${px.triageSeverity.isNotEmpty ? " · Triage: ${px.triageSeverity}" : ""}',
                          style: const TextStyle(color: Colors.white54, fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                  // Expand chevron
                  Icon(_expanded ? Icons.expand_less : Icons.expand_more, color: Colors.white38),
                ],
              ),
            ),
          ),

          // ── Expanded Detail ───────────────────────────────────────────────
          if (_expanded)
            Container(
              decoration: const BoxDecoration(
                border: Border(top: BorderSide(color: _cardBorder)),
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
                    child: Text('Medicines:', style: TextStyle(color: Colors.white70, fontWeight: FontWeight.bold, fontSize: 13)),
                  ),
                  ...px.items.map((item) => _MedicineItemTile(item: item)),

                  // Notes
                  if (px.notes != null && px.notes!.isNotEmpty) ...[
                    const SizedBox(height: 12),
                    Text('Notes:', style: const TextStyle(color: Colors.white70, fontWeight: FontWeight.bold, fontSize: 13)),
                    const SizedBox(height: 4),
                    Text(px.notes!, style: const TextStyle(color: Colors.white60, fontSize: 13)),
                  ],

                  // Notification badge
                  if (px.notificationSent) ...[
                    const SizedBox(height: 12),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                      decoration: BoxDecoration(
                        color: const Color(0xFF52C41A).withOpacity(0.1),
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: const Color(0xFF52C41A).withOpacity(0.3)),
                      ),
                      child: Text(
                        '✅ Notified via ${px.notificationChannel ?? "Email"}',
                        style: const TextStyle(color: Color(0xFF52C41A), fontSize: 12, fontWeight: FontWeight.w600),
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
        color: color.withOpacity(0.08),
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: color.withOpacity(0.3)),
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
            Text(data!['Summary'] as String, style: const TextStyle(color: Colors.white70, fontSize: 12)),
          ],
          if ((data?['Errors'] as List?)?.isNotEmpty == true) ...[
            const SizedBox(height: 6),
            ...(data!['Errors'] as List).map((e) => Padding(
              padding: const EdgeInsets.only(top: 3),
              child: Text('• $e', style: const TextStyle(color: Color(0xFFFF4D4F), fontSize: 12)),
            )),
          ],
          if ((data?['Warnings'] as List?)?.isNotEmpty == true) ...[
            const SizedBox(height: 6),
            ...(data!['Warnings'] as List).map((w) => Padding(
              padding: const EdgeInsets.only(top: 3),
              child: Text('• $w', style: const TextStyle(color: Color(0xFFFA8C16), fontSize: 12)),
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
        color: _darkBg,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: _cardBorder),
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
                  style: const TextStyle(color: Colors.white, fontSize: 14, fontWeight: FontWeight.w600)),
                const SizedBox(height: 2),
                Text(item.dosage,
                  style: const TextStyle(color: Colors.white60, fontSize: 12)),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text('×${item.quantity}', style: const TextStyle(color: _purple, fontWeight: FontWeight.bold)),
              Text('${item.durationDays}d', style: const TextStyle(color: Colors.white38, fontSize: 11)),
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
        color: color.withOpacity(0.15),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withOpacity(0.4)),
      ),
      child: Text(label,
        style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.bold, letterSpacing: 0.3)),
    );
  }
}
