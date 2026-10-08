import 'package:flutter/material.dart';

import '../services/api_service.dart';
import '../services/auth_service.dart';

class AppointmentAvailabilityScreen extends StatefulWidget {
  final ApiService? apiService;
  final String? initialPatientId;

  const AppointmentAvailabilityScreen({
    super.key,
    this.apiService,
    this.initialPatientId,
  });

  @override
  State<AppointmentAvailabilityScreen> createState() =>
      _AppointmentAvailabilityScreenState();
}

class _AppointmentAvailabilityScreenState
    extends State<AppointmentAvailabilityScreen> {
  late final ApiService _apiService;

  // ---------------------------------------------------------
  // State
  // ---------------------------------------------------------

  String? selectedSpecialization;
  DateTime selectedDate = DateTime.now();

  String? patientId;

  bool isLoading = false;
  bool isBooking = false;

  List<dynamic> availableDoctors = [];
  List<dynamic> availableSlots = [];

  Map<String, dynamic>? selectedDoctor;

  final List<String> specializations = [
    'Cardiology',
    'Neurology',
    'General Medicine',
    'Pediatrics',
  ];

  // ---------------------------------------------------------
  // Pastel colors
  // ---------------------------------------------------------

  static const Color pageBackground = Color(0xFFF8FBFC);
  static const Color white = Color(0xFFFFFFFF);

  static const Color bluePastel = Color(0xFFDCEFF7);
  static const Color blueLight = Color(0xFFEAF6FA);
  static const Color blueAccent = Color(0xFF7DBDD8);
  static const Color blueDark = Color(0xFF397A96);

  static const Color greenPastel = Color(0xFFDDF3E4);
  static const Color greenLight = Color(0xFFEDF9F1);
  static const Color greenAccent = Color(0xFF86C79A);
  static const Color greenDark = Color(0xFF4E9565);

  static const Color textDark = Color(0xFF334155);
  static const Color textMid = Color(0xFF64748B);
  static const Color textLight = Color(0xFF94A3B8);

  static const Color borderColor = Color(0xFFDCE6EA);

  // ---------------------------------------------------------
  // Init
  // ---------------------------------------------------------

  @override
  void initState() {
    super.initState();

    // Use the injected fake service during testing.
    // Use the real ApiService during normal application use.
    _apiService = widget.apiService ?? ApiService();

    // Use the supplied patient ID during testing.
    // Otherwise load the logged-in patient's ID normally.
    if (widget.initialPatientId != null) {
      patientId = widget.initialPatientId;
    } else {
      _loadPatient();
    }
  }

  // ---------------------------------------------------------
  // Load logged-in patient
  // ---------------------------------------------------------

  Future<void> _loadPatient() async {
    final id = await AuthService.getPatientId();

    if (!mounted) return;

    setState(() {
      patientId = id;
    });
  }

  // ---------------------------------------------------------
  // Date helper
  // ---------------------------------------------------------

  String _formatDate(DateTime date) {
    return '${date.year}-'
        '${date.month.toString().padLeft(2, '0')}-'
        '${date.day.toString().padLeft(2, '0')}';
  }

  String _formatTime(String time) {
    if (time.length >= 5) {
      return time.substring(0, 5);
    }

    return time;
  }

  // ---------------------------------------------------------
  // Select date
  // ---------------------------------------------------------

  Future<void> _selectDate() async {
    final pickedDate = await showDatePicker(
      context: context,
      initialDate: selectedDate,
      firstDate: DateTime.now(),
      lastDate: DateTime(2030, 12, 31),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: blueAccent,
              onPrimary: Colors.white,
              surface: white,
              onSurface: textDark,
            ),
          ),
          child: child!,
        );
      },
    );

    if (pickedDate != null) {
      setState(() {
        selectedDate = pickedDate;
        selectedDoctor = null;
        availableDoctors = [];
        availableSlots = [];
      });
    }
  }

  // ---------------------------------------------------------
  // Search doctors
  // ---------------------------------------------------------

  Future<void> _searchDoctors() async {
    setState(() {
      isLoading = true;
      selectedDoctor = null;
      availableSlots = [];
      availableDoctors = [];
    });

    try {
      final doctors = await _apiService.searchDoctorAvailability(
        specialization: selectedSpecialization,
        date: _formatDate(selectedDate),
      );

      if (!mounted) return;

      setState(() {
        availableDoctors = doctors;
      });
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to search doctors: ${e.toString()}')),
      );
    } finally {
      if (mounted) {
        setState(() {
          isLoading = false;
        });
      }
    }
  }

  // ---------------------------------------------------------
  // Select doctor and load slots
  // ---------------------------------------------------------

  Future<void> _selectDoctor(Map<String, dynamic> doctor) async {
    setState(() {
      selectedDoctor = doctor;
      availableSlots = [];
      isLoading = true;
    });

    try {
      final slots = await _apiService.fetchAvailableSlots(
        doctorId: doctor['doctorId'].toString(),
        date: _formatDate(selectedDate),
        slotDurationMinutes: 30,
      );

      if (!mounted) return;

      setState(() {
        availableSlots = slots;
      });
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Failed to load available slots: ${e.toString()}'),
        ),
      );
    } finally {
      if (mounted) {
        setState(() {
          isLoading = false;
        });
      }
    }
  }

  // ---------------------------------------------------------
  // Book appointment
  // ---------------------------------------------------------

  Future<void> _bookAppointment(Map<String, dynamic> slot) async {
    if (patientId == null || patientId!.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Patient information not found. Please log in again.'),
        ),
      );

      return;
    }

    if (selectedDoctor == null) {
      return;
    }

    final doctorId = selectedDoctor!['doctorId'].toString();

    final startTime = slot['startTime'].toString();
    final endTime = slot['endTime'].toString();

    setState(() {
      isBooking = true;
    });

    try {
      final success = await _apiService.createTentativeAppointment(
        doctorId: doctorId,
        patientId: patientId!,
        appointmentDate: _formatDate(selectedDate),
        startTime: startTime,
        endTime: endTime,
      );

      if (!mounted) return;

      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Appointment created successfully!'),
            backgroundColor: greenDark,
          ),
        );

        // Refresh slots after booking.
        await _selectDoctor(selectedDoctor!);
      }
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Booking failed: ${e.toString()}'),
          backgroundColor: const Color(0xFFD96B6B),
        ),
      );
    } finally {
      if (mounted) {
        setState(() {
          isBooking = false;
        });
      }
    }
  }

  // ---------------------------------------------------------
  // UI
  // ---------------------------------------------------------

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: pageBackground,

      // -----------------------------------------------------
      // App bar
      // -----------------------------------------------------
      appBar: AppBar(
        backgroundColor: white,
        foregroundColor: textDark,
        elevation: 0,
        title: const Text(
          'Find Appointment',
          style: TextStyle(color: textDark, fontWeight: FontWeight.bold),
        ),
      ),

      body: SingleChildScrollView(
        padding: const EdgeInsets.fromLTRB(20, 18, 20, 30),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // -------------------------------------------------
            // Header
            // -------------------------------------------------

            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(22),
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [bluePastel, greenPastel],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(22),
              ),
              child: Row(
                children: [
                  Container(
                    width: 52,
                    height: 52,
                    decoration: BoxDecoration(
                      color: white.withValues(alpha: 0.85),
                      borderRadius: BorderRadius.circular(16),
                    ),
                    child: const Icon(
                      Icons.calendar_month_outlined,
                      color: blueDark,
                      size: 28,
                    ),
                  ),

                  const SizedBox(width: 16),

                  const Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Find Available Doctors',
                          style: TextStyle(
                            color: textDark,
                            fontSize: 21,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        SizedBox(height: 5),
                        Text(
                          'Search for real-time appointment availability.',
                          style: TextStyle(color: textMid, fontSize: 13),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 24),

            // -------------------------------------------------
            // Search card
            // -------------------------------------------------
            Container(
              padding: const EdgeInsets.all(18),
              decoration: BoxDecoration(
                color: white,
                borderRadius: BorderRadius.circular(18),
                border: Border.all(color: borderColor),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.04),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Search Criteria',
                    style: TextStyle(
                      color: textDark,
                      fontSize: 17,
                      fontWeight: FontWeight.bold,
                    ),
                  ),

                  const SizedBox(height: 18),

                  // -------------------------------------------
                  // Specialization
                  // -------------------------------------------
                  const Text(
                    'Specialization',
                    style: TextStyle(
                      color: textDark,
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),

                  const SizedBox(height: 8),

                  Container(
                    decoration: BoxDecoration(
                      color: blueLight,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: borderColor),
                    ),
                    child: DropdownButtonFormField<String>(
                      initialValue: selectedSpecialization,
                      dropdownColor: white,
                      style: const TextStyle(color: textDark, fontSize: 14),
                      decoration: const InputDecoration(
                        prefixIcon: Icon(
                          Icons.medical_services_outlined,
                          color: blueDark,
                          size: 20,
                        ),
                        border: InputBorder.none,
                        contentPadding: EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 4,
                        ),
                      ),
                      hint: const Text(
                        'Select specialization',
                        style: TextStyle(color: textLight, fontSize: 14),
                      ),
                      items: specializations.map((specialization) {
                        return DropdownMenuItem<String>(
                          value: specialization,
                          child: Text(specialization),
                        );
                      }).toList(),
                      onChanged: (value) {
                        setState(() {
                          selectedSpecialization = value;
                          selectedDoctor = null;
                          availableDoctors = [];
                          availableSlots = [];
                        });
                      },
                    ),
                  ),

                  const SizedBox(height: 18),

                  // -------------------------------------------
                  // Date
                  // -------------------------------------------
                  const Text(
                    'Appointment Date',
                    style: TextStyle(
                      color: textDark,
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),

                  const SizedBox(height: 8),

                  InkWell(
                    onTap: _selectDate,
                    borderRadius: BorderRadius.circular(12),
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 15,
                      ),
                      decoration: BoxDecoration(
                        color: greenLight,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: borderColor),
                      ),
                      child: Row(
                        children: [
                          Container(
                            width: 38,
                            height: 38,
                            decoration: BoxDecoration(
                              color: greenPastel,
                              borderRadius: BorderRadius.circular(10),
                            ),
                            child: const Icon(
                              Icons.calendar_today_outlined,
                              color: greenDark,
                              size: 19,
                            ),
                          ),

                          const SizedBox(width: 12),

                          Text(
                            _formatDate(selectedDate),
                            style: const TextStyle(
                              color: textDark,
                              fontSize: 14,
                              fontWeight: FontWeight.w600,
                            ),
                          ),

                          const Spacer(),

                          const Icon(Icons.keyboard_arrow_down, color: textMid),
                        ],
                      ),
                    ),
                  ),

                  const SizedBox(height: 20),

                  // -------------------------------------------
                  // Search button
                  // -------------------------------------------
                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: ElevatedButton.icon(
                      onPressed: isLoading ? null : _searchDoctors,
                      icon: const Icon(Icons.search, size: 21),
                      label: const Text(
                        'Search Available Doctors',
                        style: TextStyle(
                          fontSize: 15,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: blueAccent,
                        foregroundColor: white,
                        disabledBackgroundColor: bluePastel,
                        disabledForegroundColor: textLight,
                        elevation: 0,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(13),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 26),

            // -------------------------------------------------
            // Loading
            // -------------------------------------------------
            if (isLoading)
              const Center(
                child: Padding(
                  padding: EdgeInsets.all(24),
                  child: CircularProgressIndicator(color: blueAccent),
                ),
              ),

            // -------------------------------------------------
            // Doctors
            // -------------------------------------------------
            if (!isLoading && availableDoctors.isNotEmpty) ...[
              const Text(
                'Available Doctors',
                style: TextStyle(
                  color: textDark,
                  fontSize: 19,
                  fontWeight: FontWeight.bold,
                ),
              ),

              const SizedBox(height: 12),

              ...availableDoctors.map((doctor) => _doctorCard(doctor)),
            ],

            // -------------------------------------------------
            // No doctors
            // -------------------------------------------------
            if (!isLoading &&
                availableDoctors.isEmpty &&
                selectedSpecialization != null)
              _emptyState(
                icon: Icons.person_search_outlined,
                title: 'No doctors found',
                message: 'No doctors are available for the selected date.',
              ),

            // -------------------------------------------------
            // Available slots
            // -------------------------------------------------
            if (selectedDoctor != null &&
                !isLoading &&
                availableSlots.isNotEmpty) ...[
              const SizedBox(height: 18),

              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(18),
                decoration: BoxDecoration(
                  color: white,
                  borderRadius: BorderRadius.circular(18),
                  border: Border.all(color: borderColor),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Container(
                          width: 42,
                          height: 42,
                          decoration: BoxDecoration(
                            color: greenPastel,
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: const Icon(
                            Icons.access_time,
                            color: greenDark,
                          ),
                        ),

                        const SizedBox(width: 12),

                        const Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Available Slots',
                              style: TextStyle(
                                color: textDark,
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            SizedBox(height: 3),
                            Text(
                              'Choose a convenient time',
                              style: TextStyle(color: textMid, fontSize: 12),
                            ),
                          ],
                        ),
                      ],
                    ),

                    const SizedBox(height: 16),

                    ...availableSlots.map((slot) => _slotCard(slot)),
                  ],
                ),
              ),
            ],

            // -------------------------------------------------
            // No slots
            // -------------------------------------------------
            if (selectedDoctor != null && !isLoading && availableSlots.isEmpty)
              _emptyState(
                icon: Icons.event_busy_outlined,
                title: 'No available slots',
                message:
                    'There are no available appointment slots for this doctor.',
              ),
          ],
        ),
      ),
    );
  }

  // ---------------------------------------------------------
  // Doctor card
  // ---------------------------------------------------------

  Widget _doctorCard(Map<String, dynamic> doctor) {
    final doctorName = doctor['doctorName']?.toString() ?? 'Doctor';

    final specialization = doctor['specialization']?.toString() ?? '';

    final date = doctor['date']?.toString() ?? '';

    final startTime = doctor['startTime']?.toString() ?? '';

    final endTime = doctor['endTime']?.toString() ?? '';

    final isSelected =
        selectedDoctor?['doctorId'].toString() == doctor['doctorId'].toString();

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(
          color: isSelected ? blueAccent : borderColor,
          width: isSelected ? 2 : 1,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.035),
            blurRadius: 10,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: InkWell(
        borderRadius: BorderRadius.circular(18),
        onTap: () => _selectDoctor(doctor),
        child: Padding(
          padding: const EdgeInsets.all(17),
          child: Row(
            children: [
              // ---------------------------------------------
              // Doctor icon
              // ---------------------------------------------

              Container(
                width: 56,
                height: 56,
                decoration: BoxDecoration(
                  color: bluePastel,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: const Icon(
                  Icons.person_outline,
                  color: blueDark,
                  size: 30,
                ),
              ),

              const SizedBox(width: 15),

              // ---------------------------------------------
              // Doctor information
              // ---------------------------------------------
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      doctorName,
                      style: const TextStyle(
                        color: textDark,
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),

                    const SizedBox(height: 5),

                    Text(
                      specialization,
                      style: const TextStyle(
                        color: blueDark,
                        fontSize: 13,
                        fontWeight: FontWeight.w600,
                      ),
                    ),

                    const SizedBox(height: 7),

                    Row(
                      children: [
                        const Icon(
                          Icons.calendar_today_outlined,
                          color: textLight,
                          size: 13,
                        ),

                        const SizedBox(width: 5),

                        Expanded(
                          child: Text(
                            '$date • ${_formatTime(startTime)} - ${_formatTime(endTime)}',
                            style: const TextStyle(
                              color: textMid,
                              fontSize: 12,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),

              const SizedBox(width: 8),

              // ---------------------------------------------
              // Arrow
              // ---------------------------------------------
              Container(
                width: 34,
                height: 34,
                decoration: BoxDecoration(
                  color: blueLight,
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.arrow_forward_ios,
                  color: blueDark,
                  size: 14,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  // ---------------------------------------------------------
  // Slot card
  // ---------------------------------------------------------

  Widget _slotCard(Map<String, dynamic> slot) {
    final startTime = slot['startTime']?.toString() ?? '';

    final endTime = slot['endTime']?.toString() ?? '';

    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      child: ElevatedButton(
        onPressed: isBooking ? null : () => _bookAppointment(slot),
        style: ElevatedButton.styleFrom(
          backgroundColor: greenAccent,
          foregroundColor: Colors.white,
          disabledBackgroundColor: greenPastel,
          disabledForegroundColor: textLight,
          minimumSize: const Size(double.infinity, 52),
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(13),
          ),
        ),
        child: isBooking
            ? const SizedBox(
                width: 22,
                height: 22,
                child: CircularProgressIndicator(
                  color: Colors.white,
                  strokeWidth: 2,
                ),
              )
            : Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.access_time, size: 19),
                  const SizedBox(width: 8),
                  Text(
                    '${_formatTime(startTime)} - ${_formatTime(endTime)}',
                    style: const TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
      ),
    );
  }

  // ---------------------------------------------------------
  // Empty state
  // ---------------------------------------------------------

  Widget _emptyState({
    required IconData icon,
    required String title,
    required String message,
  }) {
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.only(top: 8),
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: borderColor),
      ),
      child: Column(
        children: [
          Container(
            width: 54,
            height: 54,
            decoration: BoxDecoration(
              color: blueLight,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Icon(icon, color: blueDark, size: 28),
          ),

          const SizedBox(height: 12),

          Text(
            title,
            style: const TextStyle(
              color: textDark,
              fontSize: 16,
              fontWeight: FontWeight.bold,
            ),
          ),

          const SizedBox(height: 5),

          Text(
            message,
            textAlign: TextAlign.center,
            style: const TextStyle(color: textMid, fontSize: 13),
          ),
        ],
      ),
    );
  }
}
