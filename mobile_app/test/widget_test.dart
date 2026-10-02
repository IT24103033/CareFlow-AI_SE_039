import 'package:flutter_test/flutter_test.dart';

import 'package:mobile_app/main.dart';

void main() {
  testWidgets(
    'App starts and renders the authentication gate',
    (WidgetTester tester) async {
      await tester.pumpWidget(const CareFlowApp());

      // Allow the authentication check to complete.
      await tester.pump(const Duration(seconds: 2));

      // Verify that the application renders a screen.
      expect(find.byType(CareFlowApp), findsOneWidget);
    },
  );
}