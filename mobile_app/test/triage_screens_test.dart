import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile_app/models/triage_record.dart';
import 'package:mobile_app/screens/submit_triage_screen.dart';
import 'package:mobile_app/screens/triage_dashboard_screen.dart';
import 'package:mobile_app/services/triage_repository.dart';
import 'package:mobile_app/theme/app_theme.dart';

const symptoms = 'Persistent discomfort reported over several days.';

class FakeRepository extends TriageRepository {
  int submissions = 0;
  String? submittedSymptoms;
  Future<Map<String, dynamic>> Function()? onSubmit;
  Future<TriageHistory> Function()? onHistory;

  @override
  Future<Map<String, dynamic>> submit({
    required String symptoms,
    File? imageFile,
  }) {
    submissions++;
    submittedSymptoms = symptoms;
    return onSubmit!();
  }

  @override
  Future<TriageHistory> loadHistory() => onHistory!();
}

Map<String, dynamic> result(String status) => {
  'triageStatus': status,
  'severityLevel': status == 'AssessmentFailed' ? 'Unassessed' : 'Medium',
  'aiPlan': status == 'AssessmentFailed'
      ? null
      : jsonEncode({
          'SuggestedSpecialist': 'General Practitioner',
          'UrgencyLevel': 'Medium',
          'RecommendedAction': 'Review the patient context.',
          'Rationale': 'A clinician must review this assessment.',
        }),
};

TriageHistory history(String status) => TriageHistory(
  patientName: 'Test Patient',
  records: [
    TriageRecord(
      id: 'case-1',
      patientId: 'patient-1',
      symptoms: symptoms,
      severityLevel: status == 'AssessmentFailed' ? 'Unassessed' : 'Medium',
      triageStatus: status,
      aiPlan: result(status)['aiPlan'],
      createdAt: DateTime.utc(2026, 9, 27),
      updatedAt: DateTime.utc(2026, 9, 27),
    ),
  ],
);

Future<void> mount(WidgetTester tester, Widget screen) =>
    tester.pumpWidget(MaterialApp(theme: AppTheme.theme, home: screen));

Future<void> submit(WidgetTester tester, {String text = symptoms}) async {
  await tester.enterText(find.byType(TextFormField).first, text);
  await tester.ensureVisible(find.byType(ElevatedButton).first);
  await tester.pumpAndSettle();
  await tester.tap(find.byType(ElevatedButton).first);
  await tester.pump();
}

void main() {
  testWidgets('short symptoms do not call the API', (tester) async {
    final repo = FakeRepository();
    await mount(tester, SubmitTriageScreen(repository: repo));
    await submit(tester, text: 'Too short');
    expect(repo.submissions, 0);
    expect(
      find.text('Please describe in more detail (min 20 characters)'),
      findsOneWidget,
    );
  });

  testWidgets('successful submission displays the assessment after loading', (
    tester,
  ) async {
    final pending = Completer<Map<String, dynamic>>();
    final repo = FakeRepository()..onSubmit = () => pending.future;
    await mount(tester, SubmitTriageScreen(repository: repo));
    await submit(tester);
    expect(repo.submittedSymptoms, symptoms);
    expect(find.text('AI is analysing symptoms...'), findsOneWidget);
    expect(
      tester
          .widget<ElevatedButton>(find.byType(ElevatedButton).first)
          .onPressed,
      isNull,
    );
    pending.complete(result('InReview'));
    await tester.pumpAndSettle();
    expect(find.text('Submission Received!'), findsOneWidget);
    expect(find.text('General Practitioner'), findsOneWidget);
    expect(find.text('Status: InReview'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets(
    'saved assessment failure does not claim successful analysis or low urgency',
    (tester) async {
      final repo = FakeRepository()
        ..onSubmit = () async => result('AssessmentFailed');
      await mount(tester, SubmitTriageScreen(repository: repo));
      await submit(tester);
      await tester.pumpAndSettle();
      expect(
        find.textContaining('assessment could not be completed'),
        findsOneWidget,
      );
      expect(find.text('AI Clinical Plan'), findsNothing);
      expect(find.textContaining('prepared an assessment'), findsNothing);
      expect(find.text('Severity: Low'), findsNothing);
      final badge = tester.widget<Text>(find.text('Severity: Unassessed'));
      expect(badge.style?.color, AppTheme.textMid);
      expect(tester.takeException(), isNull);
    },
  );

  testWidgets('transport failure keeps the form and supports retry', (
    tester,
  ) async {
    final repo = FakeRepository()
      ..onSubmit = () async => throw const SocketException('Offline');
    await mount(tester, SubmitTriageScreen(repository: repo));
    await submit(tester);
    await tester.pumpAndSettle();
    expect(find.textContaining('Submission failed'), findsOneWidget);
    expect(find.text('Submission Received!'), findsNothing);
    repo.onSubmit = () async => result('InReview');
    await submit(tester);
    await tester.pumpAndSettle();
    expect(repo.submissions, 2);
    expect(find.text('Submission Received!'), findsOneWidget);
  });

  for (final fails in [false, true]) {
    testWidgets(
      'leaving during submission handles late ${fails ? 'error' : 'success'}',
      (tester) async {
        final pending = Completer<Map<String, dynamic>>();
        final repo = FakeRepository()..onSubmit = () => pending.future;
        await mount(tester, SubmitTriageScreen(repository: repo));
        await submit(tester);
        await tester.pumpWidget(const SizedBox.shrink());
        if (fails) {
          pending.completeError(const SocketException('Offline'));
        } else {
          pending.complete(result('InReview'));
        }
        await tester.pump();
        expect(tester.takeException(), isNull);
      },
    );
  }

  testWidgets('history shows failure accurately on a narrow phone', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(320, 800);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    final repo = FakeRepository()
      ..onHistory = () async => history('AssessmentFailed');
    await mount(tester, TriageDashboardScreen(repository: repo));
    await tester.pumpAndSettle();
    expect(
      find.text('Assessment unavailable'),
      findsNWidgets(2),
    ); // legend and badge
    expect(find.text('Pending'), findsOneWidget); // legend only
    expect(
      tester.widget<Text>(find.text('Unassessed')).style?.color,
      AppTheme.textMid,
    );
    await tester.tap(find.byType(ExpansionTile));
    await tester.pumpAndSettle();
    expect(find.textContaining('No urgency has been assigned'), findsOneWidget);
    expect(
      find.text('Submission saved · Assessment unavailable'),
      findsOneWidget,
    );
    expect(find.text('AI Clinical Plan'), findsNothing);
    expect(tester.takeException(), isNull);
  });

  testWidgets('history supports empty, error, and retry states', (
    tester,
  ) async {
    final repo = FakeRepository()
      ..onHistory = () async => throw const SocketException('Offline');
    await mount(tester, TriageDashboardScreen(repository: repo));
    await tester.pumpAndSettle();
    expect(find.text('Something went wrong'), findsOneWidget);
    repo.onHistory = () async =>
        const TriageHistory(patientName: 'Test Patient', records: []);
    await tester.tap(find.text('Try Again'));
    await tester.pumpAndSettle();
    expect(find.text('No Triage Records Yet'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  for (final fails in [false, true]) {
    testWidgets('leaving history handles late ${fails ? 'error' : 'success'}', (
      tester,
    ) async {
      final pending = Completer<TriageHistory>();
      final repo = FakeRepository()..onHistory = () => pending.future;
      await mount(tester, TriageDashboardScreen(repository: repo));
      await tester.pumpWidget(const SizedBox.shrink());
      if (fails) {
        pending.completeError(const SocketException('Offline'));
      } else {
        pending.complete(history('InReview'));
      }
      await tester.pump();
      expect(tester.takeException(), isNull);
    });
  }

  testWidgets('an older refresh cannot overwrite a newer review status', (
    tester,
  ) async {
    final repo = FakeRepository()..onHistory = () async => history('InReview');
    await mount(tester, TriageDashboardScreen(repository: repo));
    await tester.pumpAndSettle();
    final older = Completer<TriageHistory>();
    final newer = Completer<TriageHistory>();
    final pending = [older, newer];
    repo.onHistory = () => pending.removeAt(0).future;
    await tester.tap(find.byIcon(Icons.refresh_outlined));
    await tester.pump();
    await tester.tap(find.byIcon(Icons.refresh_outlined));
    await tester.pump();
    newer.complete(history('RevisionRequested'));
    await tester.pumpAndSettle();
    older.complete(history('Approved'));
    await tester.pumpAndSettle();
    expect(find.text('Revision requested'), findsNWidgets(2));
    expect(find.text('Approved'), findsOneWidget); // legend only
    expect(tester.takeException(), isNull);
  });
}
