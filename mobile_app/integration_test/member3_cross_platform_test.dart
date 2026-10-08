import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:mobile_app/screens/login_screen.dart';
import 'package:mobile_app/screens/home_screen.dart';
import 'package:mobile_app/screens/submit_triage_screen.dart';
import 'package:mobile_app/screens/my_appointments_screen.dart';
import 'package:mobile_app/services/auth_service.dart';
import 'package:mobile_app/services/api_service.dart';
import 'package:mobile_app/services/triage_repository.dart';
import 'package:mobile_app/services/triage_service.dart';
import 'package:mobile_app/theme/app_theme.dart';

// Records the real production repository response; no stubbed data or network interception.
class RecordingRepository extends ApiTriageRepository {
  Map<String, dynamic>? submitted;
  @override
  Future<Map<String, dynamic>> submit({required String symptoms, File? imageFile}) async {
    submitted = await super.submit(symptoms: symptoms, imageFile: imageFile);
    return submitted!;
  }
}

Future<void> waitUntil(WidgetTester tester, bool Function() condition, String reason) async {
  final deadline = DateTime.now().add(const Duration(minutes: 3));
  while (!condition() && DateTime.now().isBefore(deadline)) {
    await tester.pump(const Duration(milliseconds: 300));
    await Future<void>.delayed(const Duration(milliseconds: 300));
  }
  expect(condition(), isTrue, reason: reason);
}

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();
  const phase = String.fromEnvironment('E2E_PHASE', defaultValue: 'submit');
  const username = String.fromEnvironment('PATIENT_USERNAME');
  const password = String.fromEnvironment('PATIENT_PASSWORD');
  const runId = String.fromEnvironment('RUN_ID');
  const caseId = String.fromEnvironment('CASE_ID');
  const appointmentId = String.fromEnvironment('APPOINTMENT_ID');
  testWidgets('M3 cross-platform $phase: real Flutter UI and real API', (tester) async {
    expect(username, isNotEmpty, reason: 'Set PATIENT_USERNAME in private config');
    expect(password, isNotEmpty);
    await AuthService.logout();
    await tester.pumpWidget(MaterialApp(theme: AppTheme.theme, home: const LoginScreen()));
    await tester.pumpAndSettle();
    await tester.enterText(find.byType(TextField).at(0), username);
    await tester.enterText(find.byType(TextField).at(1), password);
    await tester.testTextInput.receiveAction(TextInputAction.done);
    await tester.tap(find.widgetWithText(ElevatedButton, 'Login'));
    await waitUntil(tester, () => find.byType(HomeScreen).evaluate().isNotEmpty, 'Patient UI login must succeed');
    final patientId = await AuthService.getPatientId();
    expect(patientId, isNotNull);

    if (phase == 'submit') {
      expect(runId, isNotEmpty);
      final repo = RecordingRepository();
      // Mount the production submission screen after real UI login; navigation is outside this test.
      await tester.pumpWidget(MaterialApp(theme: AppTheme.theme, home: SubmitTriageScreen(repository: repo)));
      await tester.pumpAndSettle();
      await tester.enterText(find.byType(TextFormField).first,
          'Persistent general tiredness and low energy for several days. Synthetic test reference $runId.');
      await tester.testTextInput.receiveAction(TextInputAction.done);
      final submit = find.widgetWithText(ElevatedButton, 'Submit for AI Triage');
      await tester.ensureVisible(submit);
      await tester.tap(submit);
      await waitUntil(tester, () => repo.submitted != null, 'Real AI assessment must return');
      await tester.pumpAndSettle();
      final result = repo.submitted!;
      expect(result['aiAgentStatus'], 'Completed');
      expect(result['analysisMethod'], 'GeminiAI');
      expect(result['approvalStatus'], 'Pending');
      expect(result['safetyVerdict'], 'Safe');
      expect(result['schedulingOutcome'], 'ActionRequired', reason: 'Need matching doctor availability');
      final slots = result['availableSlots'] as List;
      expect(slots, isNotEmpty);
      final slot = slots.first as Map;
      final choice = find.widgetWithText(ElevatedButton, '${slot['date']} ${slot['startTime']}');
      await tester.ensureVisible(choice);
      await tester.tap(choice);
      await waitUntil(tester, () => find.text('Appointment booked successfully!').evaluate().isNotEmpty, 'Slot booking through UI must succeed');
      final saved = await TriageService.getTriageById(result['id']);
      expect(saved['tentativeAppointmentId'], isNotNull);
      final appointments = await ApiService().getPatientAppointments(patientId!);
      final booking = appointments.singleWhere((a) => a['id'] == saved['tentativeAppointmentId']);
      expect(booking['status'], 'Tentative');
      expect(booking['approvalStatus'], 'Pending');
      // Only non-secret identifiers are emitted for the React stage.
      debugPrint('M3_HANDOFF:${jsonEncode({'caseId': result['id'], 'appointmentId': booking['id'], 'doctorId': booking['doctorId'], 'runId': runId})}');
    } else {
      expect(phase, 'verify');
      expect(caseId, isNotEmpty);
      expect(appointmentId, isNotEmpty);
      final record = await TriageService.getTriageById(caseId);
      expect(record['triageStatus'], 'Approved');
      expect(record['approvalStatus'], 'Approved');
      expect(record['tentativeAppointmentId'], appointmentId);
      final appointments = await ApiService().getPatientAppointments(patientId!);
      final booking = appointments.singleWhere((a) => a['id'] == appointmentId);
      expect(booking['status'], 'Confirmed');
      expect(booking['approvalStatus'], 'Approved');
      await tester.pumpWidget(MaterialApp(theme: AppTheme.theme, home: const MyAppointmentsScreen()));
      await waitUntil(tester, () => find.text('Doctor: ${booking['doctorName']}').evaluate().isNotEmpty, 'Real appointment list must load');
      final card = find.ancestor(of: find.text('Time: ${booking['startTime']} - ${booking['endTime']}'), matching: find.byType(Card));
      expect(find.descendant(of: card, matching: find.text('Confirmed')), findsWidgets);
      debugPrint('M3_VERIFIED:${jsonEncode({'caseId': caseId, 'appointmentId': appointmentId, 'status': 'Confirmed'})}');
    }
  }, timeout: const Timeout(Duration(minutes: 6)));
}
