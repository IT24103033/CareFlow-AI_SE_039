// Component B – Authentication Service
// Uses flutter_secure_storage to persist session tokens and patient identity.
// On iOS → Keychain, On Android → AES-encrypted SharedPreferences.

import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  static const _storage = FlutterSecureStorage(
    aOptions: AndroidOptions(resetOnError: true),
    iOptions: IOSOptions(
      accessibility: KeychainAccessibility.first_unlock,
    ),
  );

  static const _keyToken       = 'careflow_token';
  static const _keyPatientId   = 'careflow_patient_id';
  static const _keyPatientName = 'careflow_patient_name';
  static const _keyUsername    = 'careflow_username';
  static const _keyEmail       = 'careflow_email';
  static const _keyRole        = 'careflow_role';

  static final String _baseUrl =
      Platform.isAndroid ? 'http://10.0.2.2:5241' : 'http://localhost:5241';

  // ── Save session after login / register ────────────────────────────────────
  static Future<void> saveSession({
    required String token,
    required String patientId,
    required String patientName,
    String? username,
    String? email,
    String? role,
  }) async {
    await _storage.write(key: _keyToken,       value: token);
    await _storage.write(key: _keyPatientId,   value: patientId);
    await _storage.write(key: _keyPatientName, value: patientName);
    if (username != null) {
      await _storage.write(key: _keyUsername,  value: username);
    }
    if (email != null) {
      await _storage.write(key: _keyEmail,     value: email);
    }
    if (role != null) {
      await _storage.write(key: _keyRole,      value: role);
    }
  }

  // ── Read current session ───────────────────────────────────────────────────
  static Future<String?> getAccessToken() => _storage.read(key: _keyToken);
  static Future<String?> getPatientId()   => _storage.read(key: _keyPatientId);
  static Future<String?> getPatientName() => _storage.read(key: _keyPatientName);
  static Future<String?> getUsername()    => _storage.read(key: _keyUsername);
  static Future<String?> getEmail()       => _storage.read(key: _keyEmail);
  static Future<String?> getRole()        => _storage.read(key: _keyRole);

  // ── Check if an authenticated session exists ───────────────────────────────
  static Future<bool> isLoggedIn() async {
    final token = await _storage.read(key: _keyToken);
    final id    = await _storage.read(key: _keyPatientId);
    return token != null && token.isNotEmpty && id != null && id.isNotEmpty;
  }

  // ── Real Login with Username and Password ──────────────────────────────────
  static Future<Map<String, dynamic>> login({
    required String username,
    required String password,
  }) async {
    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/auth/login'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'username': username.trim(),
            'password': password,
          }),
        )
        .timeout(const Duration(seconds: 15));

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body) as Map<String, dynamic>;
      final token = data['token'] as String;
      final user = data['user'] as Map<String, dynamic>;
      final patientId = (user['patientId'] ?? '').toString();
      final fullName = (user['fullName'] ?? user['username'] ?? '').toString();

      await saveSession(
        token: token,
        patientId: patientId,
        patientName: fullName,
        username: user['username']?.toString(),
        email: user['email']?.toString(),
        role: user['role']?.toString(),
      );

      return data;
    }

    String errorMsg = 'Invalid username or password.';
    try {
      final errBody = jsonDecode(response.body);
      if (errBody is Map && errBody.containsKey('message')) {
        errorMsg = errBody['message'].toString();
      } else if (errBody is String && errBody.isNotEmpty) {
        errorMsg = errBody;
      }
    } catch (_) {
      if (response.body.isNotEmpty) {
        errorMsg = response.body;
      }
    }

    throw HttpException(errorMsg);
  }

  // ── Register new patient with backend Auth API ─────────────────────────────
  static Future<Map<String, dynamic>> registerPatient({
    required String username,
    required String email,
    required String password,
    required String fullName,
    required String dateOfBirth,
    String bloodGroup = 'O+',
    String medicalHistorySummary = 'Registered via Mobile App',
  }) async {
    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/auth/register-patient'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'username': username.trim(),
            'email': email.trim().toLowerCase(),
            'password': password,
            'fullName': fullName.trim(),
            'dateOfBirth': dateOfBirth,
            'bloodGroup': bloodGroup,
            'medicalHistorySummary': medicalHistorySummary,
          }),
        )
        .timeout(const Duration(seconds: 15));

    if (response.statusCode == 200 || response.statusCode == 201) {
      final data = jsonDecode(response.body) as Map<String, dynamic>;
      final token = data['token'] as String;
      final user = data['user'] as Map<String, dynamic>;
      final patientId = (user['patientId'] ?? '').toString();
      final name = (user['fullName'] ?? fullName).toString();

      await saveSession(
        token: token,
        patientId: patientId,
        patientName: name,
        username: user['username']?.toString() ?? username,
        email: user['email']?.toString() ?? email.trim().toLowerCase(),
        role: 'Patient',
      );

      return data;
    }

    String errorMsg = 'Registration failed (${response.statusCode})';
    try {
      final errBody = jsonDecode(response.body);
      if (errBody is Map && errBody.containsKey('message')) {
        errorMsg = errBody['message'].toString();
      } else if (errBody is String && errBody.isNotEmpty) {
        errorMsg = errBody;
      }
    } catch (_) {
      if (response.body.isNotEmpty) errorMsg = response.body;
    }

    throw HttpException(errorMsg);
  }

  // ── Clear session on logout ────────────────────────────────────────────────
  static Future<void> logout() async {
    await _storage.deleteAll();
  }
}
