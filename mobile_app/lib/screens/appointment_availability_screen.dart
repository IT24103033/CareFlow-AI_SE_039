// Component C – Appointment Availability Screen
// Restyled to match CareFlow AI design system:
// Navy header + white content cards + teal accents.

import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class AppointmentAvailabilityScreen extends StatefulWidget {
  const AppointmentAvailabilityScreen({super.key});

  @override
  State<AppointmentAvailabilityScreen> createState() =>
      _AppointmentAvailabilityScreenState();
}

class _AppointmentAvailabilityScreenState
    extends State<AppointmentAvailabilityScreen> {
  String? selectedSpecialization;
  DateTime selectedDate = DateTime(2026, 9, 25);
  String? _selectedSlot;

  final List<String> specializations = [
    'Cardiology',
    'Neurology',
    'General Medicine',
    'Pediatrics',
  ];

  // Temporary sample slots — will be replaced with API data.
  final List<String> availableSlots = [
    '09:00 - 09:30',
    '09:30 - 10:00',
    '10:00 - 10:30',
    '10:30 - 11:00',
    '11:00 - 11:30',
    '11:30 - 12:00',
  ];

  Future<void> _selectDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: selectedDate,
      firstDate: DateTime(2026, 1, 1),
      lastDate: DateTime(2030, 12, 31),
      builder: (context, child) => Theme(
        data: Theme.of(context).copyWith(
          colorScheme: const ColorScheme.light(
            primary: AppTheme.navyDark,
            onPrimary: Colors.white,
            secondary: AppTheme.teal,
          ),
        ),
        child: child!,
      ),
    );
    if (picked != null) setState(() => selectedDate = picked);
  }

  String get _formattedDate =>
      '${selectedDate.year}-'
      '${selectedDate.month.toString().padLeft(2, '0')}-'
      '${selectedDate.day.toString().padLeft(2, '0')}';

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
                    child: const Row(
                      children: [
                        Icon(Icons.event_available_outlined,
                            color: AppTheme.teal, size: 20),
                        SizedBox(width: 10),
                        Expanded(
                          child: Text(
                            'Select a specialization and date to view available doctor slots.',
                            style: TextStyle(
                                color: Color(0xFF285E61), fontSize: 12),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 24),

                  // ── Specialization ─────────────────────────────────────
                  _fieldLabel('Specialization'),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<String>(
                    initialValue: selectedSpecialization,
                    decoration: AppTheme.inputDecoration(
                      hint: 'Select specialization',
                      icon: Icons.medical_services_outlined,
                    ),
                    items: specializations.map((s) {
                      return DropdownMenuItem(value: s, child: Text(s));
                    }).toList(),
                    onChanged: (v) =>
                        setState(() => selectedSpecialization = v),
                  ),
                  const SizedBox(height: 20),

                  // ── Date picker ────────────────────────────────────────
                  _fieldLabel('Appointment Date'),
                  const SizedBox(height: 8),
                  InkWell(
                    onTap: _selectDate,
                    borderRadius: BorderRadius.circular(10),
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 16, vertical: 14),
                      decoration: BoxDecoration(
                        color: AppTheme.inputBg,
                        borderRadius: BorderRadius.circular(10),
                        border: Border.all(color: AppTheme.borderGray),
                      ),
                      child: Row(
                        children: [
                          const Icon(Icons.calendar_month_outlined,
                              color: AppTheme.textLight, size: 20),
                          const SizedBox(width: 12),
                          Text(
                            _formattedDate,
                            style: const TextStyle(
                                color: AppTheme.textDark, fontSize: 14),
                          ),
                          const Spacer(),
                          const Icon(Icons.arrow_drop_down,
                              color: AppTheme.textLight),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 28),

                  // ── Doctor card ────────────────────────────────────────
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppTheme.cardWhite,
                      borderRadius: BorderRadius.circular(14),
                      boxShadow: AppTheme.subtleShadow,
                    ),
                    child: Row(
                      children: [
                        CircleAvatar(
                          radius: 26,
                          backgroundColor:
                              AppTheme.teal.withValues(alpha: 0.12),
                          child: const Icon(Icons.person_outline,
                              color: AppTheme.teal, size: 28),
                        ),
                        const SizedBox(width: 14),
                        const Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Dr. Nimal Perera',
                                style: TextStyle(
                                  color: AppTheme.textDark,
                                  fontSize: 16,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              SizedBox(height: 3),
                              Text(
                                'Cardiology',
                                style: TextStyle(
                                    color: AppTheme.textMid, fontSize: 13),
                              ),
                            ],
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: AppTheme.tealLight,
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: const Text(
                            'Available',
                            style: TextStyle(
                              color: AppTheme.teal,
                              fontSize: 11,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 28),

                  // ── Available slots ────────────────────────────────────
                  Row(
                    children: [
                      const Icon(Icons.access_time_outlined,
                          color: AppTheme.textMid, size: 18),
                      const SizedBox(width: 6),
                      const Text(
                        'Available Slots',
                        style: TextStyle(
                          color: AppTheme.textDark,
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const Spacer(),
                      Text(
                        '${availableSlots.length} slots',
                        style: const TextStyle(
                            color: AppTheme.textLight, fontSize: 12),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),

                  // Slot grid — 2 columns
                  GridView.builder(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    gridDelegate:
                        const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      crossAxisSpacing: 10,
                      mainAxisSpacing: 10,
                      childAspectRatio: 3.0,
                    ),
                    itemCount: availableSlots.length,
                    itemBuilder: (context, index) {
                      final slot = availableSlots[index];
                      final isSelected = _selectedSlot == slot;
                      return GestureDetector(
                        onTap: () => setState(() => _selectedSlot = slot),
                        child: AnimatedContainer(
                          duration: const Duration(milliseconds: 180),
                          decoration: BoxDecoration(
                            color: isSelected
                                ? AppTheme.navyDark
                                : AppTheme.cardWhite,
                            borderRadius: BorderRadius.circular(10),
                            border: Border.all(
                              color: isSelected
                                  ? AppTheme.navyDark
                                  : AppTheme.borderGray,
                            ),
                            boxShadow:
                                isSelected ? AppTheme.subtleShadow : null,
                          ),
                          alignment: Alignment.center,
                          child: Text(
                            slot,
                            style: TextStyle(
                              color: isSelected
                                  ? Colors.white
                                  : AppTheme.textDark,
                              fontSize: 13,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      );
                    },
                  ),
                  const SizedBox(height: 28),

                  // ── Confirm button ─────────────────────────────────────
                  if (_selectedSlot != null) ...[
                    Container(
                      padding: const EdgeInsets.all(14),
                      decoration: BoxDecoration(
                        color: AppTheme.tealLight,
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: Row(
                        children: [
                          const Icon(Icons.check_circle_outline,
                              color: AppTheme.teal, size: 18),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              'Selected: $_selectedSlot on $_formattedDate',
                              style: const TextStyle(
                                  color: AppTheme.teal,
                                  fontSize: 13,
                                  fontWeight: FontWeight.w600),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 16),
                    SizedBox(
                      width: double.infinity,
                      height: 52,
                      child: ElevatedButton(
                        onPressed: () {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(
                                  'Appointment requested for $_selectedSlot'),
                            ),
                          );
                        },
                        child: const Text('Confirm Appointment'),
                      ),
                    ),
                  ],
                ],
              ),
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
                    icon: const Icon(Icons.arrow_back_ios,
                        color: Colors.white, size: 20),
                    onPressed: () => Navigator.pop(context),
                  ),
                  const Expanded(
                    child: Text(
                      'Find Appointment',
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
            const Padding(
              padding: EdgeInsets.fromLTRB(20, 8, 20, 0),
              child: Text(
                'Search available slots and book a consultation.',
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

  Widget _fieldLabel(String text) => Text(
        text,
        style: const TextStyle(
          color: AppTheme.textDark,
          fontSize: 13,
          fontWeight: FontWeight.w600,
        ),
      );
}
