// Component B – Authentication Service
// Uses flutter_secure_storage v11.x to persist the PatientId across app sessions.
// v11 supports Swift Package Manager — required for Xcode on macOS 26+.
// On iOS → Keychain, On Android → AES-encrypted SharedPreferences.

import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  // v11 API: options are set via constructor parameters
  static const _storage = FlutterSecureStorage(
    aOptions: AndroidOptions(resetOnError: true),
    iOptions: IOSOptions(
      accessibility: KeychainAccessibility.first_unlock,
    ),
  );

  static const _keyPatientId   = 'careflow_patient_id';
  static const _keyPatientName = 'careflow_patient_name';

  // ── Save session after login / register ────────────────────────────────────
  static Future<void> saveSession({
    required String patientId,
    required String patientName,
  }) async {
    await _storage.write(key: _keyPatientId,   value: patientId);
    await _storage.write(key: _keyPatientName, value: patientName);
  }

  // ── Read current session ───────────────────────────────────────────────────
  static Future<String?> getPatientId()   => _storage.read(key: _keyPatientId);
  static Future<String?> getPatientName() => _storage.read(key: _keyPatientName);

  // ── Check if a session exists ──────────────────────────────────────────────
  static Future<bool> isLoggedIn() async {
    final id = await _storage.read(key: _keyPatientId);
    return id != null && id.isNotEmpty;
  }

  static const String _baseUrl = 'http://localhost:5241';

  // ── Register new patient with backend DB ────────────────────────────────────
  static Future<Map<String, dynamic>> registerPatient({
    required String fullName,
    required String dateOfBirth,
    String bloodGroup = 'O+',
    String medicalHistorySummary = 'Registered via Mobile App',
  }) async {
    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/patientprofiles'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'fullName': fullName,
            'dateOfBirth': dateOfBirth,
            'bloodGroup': bloodGroup,
            'medicalHistorySummary': medicalHistorySummary,
          }),
        )
        .timeout(const Duration(seconds: 15));

    if (response.statusCode == 200 || response.statusCode == 201) {
      final data = jsonDecode(response.body) as Map<String, dynamic>;
      final id = data['id'].toString();
      await saveSession(patientId: id, patientName: fullName);
      return data;
    }
    throw HttpException(
        'Registration failed (${response.statusCode}): ${response.body}');
  }

  // ── Find existing patient by name ───────────────────────────────────────────
  static Future<List<Map<String, dynamic>>> searchPatients(String name) async {
    final response = await http
        .get(
          Uri.parse('$_baseUrl/api/patientprofiles/search?name=${Uri.encodeComponent(name)}'),
          headers: {'Content-Type': 'application/json'},
        )
        .timeout(const Duration(seconds: 15));

    if (response.statusCode == 200) {
      final List<dynamic> list = jsonDecode(response.body);
      return list.cast<Map<String, dynamic>>();
    }
    return [];
  }

  // ── Clear session on logout ────────────────────────────────────────────────
  static Future<void> logout() async {
    await _storage.deleteAll();
  }
}
