import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile_app/main.dart';
import 'package:mobile_app/screens/login_screen.dart';

void main() {
  testWidgets('App opens login when no patient session is stored', (tester) async {
    // Keep CI independent of native keychain services and personal sessions.
    FlutterSecureStorage.setMockInitialValues({});
    await tester.pumpWidget(const CareFlowApp());
    await tester.pumpAndSettle();
    expect(find.byType(MaterialApp), findsOneWidget);
    expect(find.byType(LoginScreen), findsOneWidget);
  });
}
