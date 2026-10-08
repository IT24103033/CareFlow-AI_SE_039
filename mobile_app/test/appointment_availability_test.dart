import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile_app/screens/appointment_availability_screen.dart';
import 'package:mobile_app/services/api_service.dart';

class FakeApiService extends ApiService {
  Future<List<dynamic>> Function({String? specialization, String? date})?
  onSearchDoctors;

  Future<List<dynamic>> Function({
    required String doctorId,
    required String date,
    int slotDurationMinutes,
  })?
  onFetchSlots;

  Future<bool> Function({
    required String doctorId,
    required String patientId,
    required String appointmentDate,
    required String startTime,
    required String endTime,
  })?
  onCreateAppointment;

  @override
  Future<List<dynamic>> searchDoctorAvailability({
    String? specialization,
    String? date,
  }) {
    return onSearchDoctors!(specialization: specialization, date: date);
  }

  @override
  Future<List<dynamic>> fetchAvailableSlots({
    required String doctorId,
    required String date,
    int slotDurationMinutes = 30,
  }) {
    return onFetchSlots!(
      doctorId: doctorId,
      date: date,
      slotDurationMinutes: slotDurationMinutes,
    );
  }

  @override
  Future<bool> createTentativeAppointment({
    required String doctorId,
    required String patientId,
    required String appointmentDate,
    required String startTime,
    required String endTime,
  }) {
    return onCreateAppointment!(
      doctorId: doctorId,
      patientId: patientId,
      appointmentDate: appointmentDate,
      startTime: startTime,
      endTime: endTime,
    );
  }
}

Widget buildScreen(
  FakeApiService api, {
  String patientId = 'test-patient-001',
}) {
  return MaterialApp(
    home: AppointmentAvailabilityScreen(
      apiService: api,
      initialPatientId: patientId,
    ),
  );
}

Future<void> tapVisible(WidgetTester tester, Finder finder) async {
  await tester.ensureVisible(finder);
  await tester.pumpAndSettle();
  await tester.tap(finder);
  await tester.pumpAndSettle();
}

Future<void> selectCardiology(WidgetTester tester) async {
  final dropdown = find.byType(DropdownButtonFormField<String>);

  expect(dropdown, findsOneWidget);

  await tester.tap(dropdown);
  await tester.pumpAndSettle();

  final cardiologyOption = find.text('Cardiology').last;

  expect(cardiologyOption, findsOneWidget);

  await tester.tap(cardiologyOption);
  await tester.pumpAndSettle();
}

Future<void> searchDoctors(WidgetTester tester) async {
  final searchButton = find.text('Search Available Doctors');

  await tester.ensureVisible(searchButton);
  await tester.pumpAndSettle();

  await tester.tap(searchButton);
  await tester.pumpAndSettle();
}

Future<void> selectDoctor(WidgetTester tester) async {
  final doctor = find.text('Dr. Sarah Perera');

  expect(doctor, findsOneWidget);

  await tester.ensureVisible(doctor);
  await tester.pumpAndSettle();

  await tester.tap(doctor);

  // Only pump once here.
  // Some tests intentionally keep the slot request pending
  // so that the loading state can be verified.
  await tester.pump();
}

void main() {
  testWidgets('appointment screen renders the main search controls', (
    tester,
  ) async {
    final api = FakeApiService();

    await tester.pumpWidget(buildScreen(api));

    expect(find.text('Find Appointment'), findsOneWidget);

    expect(find.text('Find Available Doctors'), findsOneWidget);

    expect(find.text('Search Criteria'), findsOneWidget);

    expect(find.text('Specialization'), findsOneWidget);

    expect(find.text('Appointment Date'), findsOneWidget);

    expect(find.text('Search Available Doctors'), findsOneWidget);
  });

  testWidgets(
    'specialization dropdown displays all supported specializations',
    (tester) async {
      final api = FakeApiService();

      await tester.pumpWidget(buildScreen(api));

      final dropdown = find.byType(DropdownButtonFormField<String>);

      await tester.tap(dropdown);
      await tester.pumpAndSettle();

      expect(find.text('Cardiology'), findsOneWidget);

      expect(find.text('Neurology'), findsOneWidget);

      expect(find.text('General Medicine'), findsOneWidget);

      expect(find.text('Pediatrics'), findsOneWidget);
    },
  );

  testWidgets('selecting a specialization updates the dropdown', (
    tester,
  ) async {
    final api = FakeApiService();

    await tester.pumpWidget(buildScreen(api));

    await selectCardiology(tester);

    expect(find.text('Cardiology'), findsOneWidget);
  });

  testWidgets('date selector opens the date picker', (tester) async {
    final api = FakeApiService();

    await tester.pumpWidget(buildScreen(api));

    final today = DateTime.now();

    final formattedToday =
        '${today.year}-'
        '${today.month.toString().padLeft(2, '0')}-'
        '${today.day.toString().padLeft(2, '0')}';

    final dateFinder = find.text(formattedToday);

    expect(dateFinder, findsOneWidget);

    await tester.ensureVisible(dateFinder);

    await tester.pumpAndSettle();

    await tester.tap(dateFinder);

    await tester.pumpAndSettle();

    expect(find.byType(DatePickerDialog), findsOneWidget);

    final cancelButton = find.text('Cancel');

    expect(cancelButton, findsOneWidget);

    await tester.tap(cancelButton);

    await tester.pumpAndSettle();

    expect(find.byType(DatePickerDialog), findsNothing);
  });

  testWidgets('search displays loading state while waiting for doctors', (
    tester,
  ) async {
    final api = FakeApiService();

    final pending = Completer<List<dynamic>>();

    api.onSearchDoctors = ({String? specialization, String? date}) {
      return pending.future;
    };

    await tester.pumpWidget(buildScreen(api));

    await selectCardiology(tester);

    final searchButton = find.text('Search Available Doctors');

    await tester.ensureVisible(searchButton);

    await tester.pumpAndSettle();

    await tester.tap(searchButton);

    await tester.pump();

    expect(find.byType(CircularProgressIndicator), findsOneWidget);

    final button = tester.widget<ElevatedButton>(
      find.byType(ElevatedButton).first,
    );

    expect(button.onPressed, isNull);

    pending.complete([
      {
        'doctorId': 'doctor-001',
        'doctorName': 'Dr. Sarah Perera',
        'specialization': 'Cardiology',
        'date': '2026-10-10',
        'startTime': '09:00:00',
        'endTime': '12:00:00',
      },
    ]);

    await tester.pumpAndSettle();

    expect(find.text('Dr. Sarah Perera'), findsOneWidget);

    expect(find.text('Cardiology'), findsAtLeastNWidgets(1));
  });

  testWidgets('successful doctor search displays available doctors', (
    tester,
  ) async {
    final api = FakeApiService();

    api.onSearchDoctors = ({String? specialization, String? date}) async {
      return [
        {
          'doctorId': 'doctor-001',
          'doctorName': 'Dr. Sarah Perera',
          'specialization': 'Cardiology',
          'date': date ?? '2026-10-10',
          'startTime': '09:00:00',
          'endTime': '12:00:00',
        },
      ];
    };

    await tester.pumpWidget(buildScreen(api));

    await selectCardiology(tester);

    await searchDoctors(tester);

    expect(find.text('Available Doctors'), findsOneWidget);

    expect(find.text('Dr. Sarah Perera'), findsOneWidget);

    expect(find.textContaining('09:00 - 12:00'), findsOneWidget);
  });

  testWidgets('doctor search with no results displays empty state', (
    tester,
  ) async {
    final api = FakeApiService();

    api.onSearchDoctors = ({String? specialization, String? date}) async {
      return [];
    };

    await tester.pumpWidget(buildScreen(api));

    await selectCardiology(tester);

    await searchDoctors(tester);

    expect(find.text('No doctors found'), findsOneWidget);

    expect(
      find.text('No doctors are available for the selected date.'),
      findsOneWidget,
    );
  });

  testWidgets('doctor search failure displays an error message', (
    tester,
  ) async {
    final api = FakeApiService();

    api.onSearchDoctors = ({String? specialization, String? date}) async {
      throw Exception('Server unavailable');
    };

    await tester.pumpWidget(buildScreen(api));

    await selectCardiology(tester);

    await searchDoctors(tester);

    expect(find.textContaining('Failed to search doctors'), findsOneWidget);
  });

  testWidgets('selecting a doctor loads available appointment slots', (
    tester,
  ) async {
    final api = FakeApiService();

    api.onSearchDoctors = ({String? specialization, String? date}) async {
      return [
        {
          'doctorId': 'doctor-001',
          'doctorName': 'Dr. Sarah Perera',
          'specialization': 'Cardiology',
          'date': date ?? '2026-10-10',
          'startTime': '09:00:00',
          'endTime': '12:00:00',
        },
      ];
    };

    final pendingSlots = Completer<List<dynamic>>();

    api.onFetchSlots =
        ({
          required String doctorId,
          required String date,
          int slotDurationMinutes = 30,
        }) {
          return pendingSlots.future;
        };

    await tester.pumpWidget(buildScreen(api));

    await selectCardiology(tester);

    await searchDoctors(tester);

    // Do NOT call pumpAndSettle inside selectDoctor
    // because the slot API is intentionally pending.
    await selectDoctor(tester);

    expect(find.byType(CircularProgressIndicator), findsOneWidget);

    pendingSlots.complete([
      {'startTime': '09:00:00', 'endTime': '09:30:00'},
      {'startTime': '10:00:00', 'endTime': '10:30:00'},
    ]);

    await tester.pumpAndSettle();

    expect(find.text('Available Slots'), findsOneWidget);

    expect(find.text('09:00 - 09:30'), findsOneWidget);

    expect(find.text('10:00 - 10:30'), findsOneWidget);
  });

  testWidgets('booking without patient information is rejected safely', (
    tester,
  ) async {
    final api = FakeApiService();

    api.onSearchDoctors = ({String? specialization, String? date}) async {
      return [
        {
          'doctorId': 'doctor-001',
          'doctorName': 'Dr. Sarah Perera',
          'specialization': 'Cardiology',
          'date': date ?? '2026-10-10',
          'startTime': '09:00:00',
          'endTime': '12:00:00',
        },
      ];
    };

    api.onFetchSlots =
        ({
          required String doctorId,
          required String date,
          int slotDurationMinutes = 30,
        }) async {
          return [
            {'startTime': '09:00:00', 'endTime': '09:30:00'},
          ];
        };

    await tester.pumpWidget(buildScreen(api, patientId: ''));

    await selectCardiology(tester);

    await searchDoctors(tester);

    await selectDoctor(tester);

    // The fake slot request completes immediately,
    // so settle here before looking for the slot.
    await tester.pumpAndSettle();

    final slot = find.text('09:00 - 09:30');

    await tester.ensureVisible(slot);

    await tester.pumpAndSettle();

    await tester.tap(slot);

    await tester.pumpAndSettle();

    expect(
      find.text('Patient information not found. Please log in again.'),
      findsOneWidget,
    );
  });

  testWidgets('successful booking displays confirmation and refreshes slots', (
    tester,
  ) async {
    final api = FakeApiService();

    var slotRequests = 0;

    api.onSearchDoctors = ({String? specialization, String? date}) async {
      return [
        {
          'doctorId': 'doctor-001',
          'doctorName': 'Dr. Sarah Perera',
          'specialization': 'Cardiology',
          'date': date ?? '2026-10-10',
          'startTime': '09:00:00',
          'endTime': '12:00:00',
        },
      ];
    };

    api.onFetchSlots =
        ({
          required String doctorId,
          required String date,
          int slotDurationMinutes = 30,
        }) async {
          slotRequests++;

          return [
            {'startTime': '09:00:00', 'endTime': '09:30:00'},
          ];
        };

    api.onCreateAppointment =
        ({
          required String doctorId,
          required String patientId,
          required String appointmentDate,
          required String startTime,
          required String endTime,
        }) async {
          expect(doctorId, 'doctor-001');

          expect(patientId, 'patient-001');

          expect(startTime, '09:00:00');

          expect(endTime, '09:30:00');

          return true;
        };

    await tester.pumpWidget(buildScreen(api, patientId: 'patient-001'));

    await selectCardiology(tester);

    await searchDoctors(tester);

    await selectDoctor(tester);

    // Fake slot API completes immediately.
    await tester.pumpAndSettle();

    expect(slotRequests, 1);

    final slot = find.text('09:00 - 09:30');

    await tester.ensureVisible(slot);

    await tester.pumpAndSettle();

    await tester.tap(slot);

    await tester.pumpAndSettle();

    expect(find.text('Appointment created successfully!'), findsOneWidget);

    expect(slotRequests, 2);
  });

  testWidgets('booking failure displays booking error', (tester) async {
    final api = FakeApiService();

    api.onSearchDoctors = ({String? specialization, String? date}) async {
      return [
        {
          'doctorId': 'doctor-001',
          'doctorName': 'Dr. Sarah Perera',
          'specialization': 'Cardiology',
          'date': date ?? '2026-10-10',
          'startTime': '09:00:00',
          'endTime': '12:00:00',
        },
      ];
    };

    api.onFetchSlots =
        ({
          required String doctorId,
          required String date,
          int slotDurationMinutes = 30,
        }) async {
          return [
            {'startTime': '09:00:00', 'endTime': '09:30:00'},
          ];
        };

    api.onCreateAppointment =
        ({
          required String doctorId,
          required String patientId,
          required String appointmentDate,
          required String startTime,
          required String endTime,
        }) async {
          throw Exception('Booking service unavailable');
        };

    await tester.pumpWidget(buildScreen(api, patientId: 'patient-001'));

    await selectCardiology(tester);

    await searchDoctors(tester);

    await selectDoctor(tester);

    // Fake slot API completes immediately.
    await tester.pumpAndSettle();

    final slot = find.text('09:00 - 09:30');

    await tester.ensureVisible(slot);

    await tester.pumpAndSettle();

    await tester.tap(slot);

    await tester.pumpAndSettle();

    expect(find.textContaining('Booking failed'), findsOneWidget);
  });
}
