// Component B – Triage API Service
// Handles all HTTP calls to the ASP.NET Core backend /api/triage endpoints with JWT authentication.

import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'auth_service.dart';

class TriageService {
  static final String _baseUrl =
      Platform.isAndroid ? 'http://10.0.2.2:5241' : 'http://localhost:5241';

  static Future<Map<String, String>> _authHeaders() async {
    final token = await AuthService.getAccessToken();
    final headers = {'Content-Type': 'application/json'};
    if (token != null && token.isNotEmpty) {
      headers['Authorization'] = 'Bearer $token';
    }
    return headers;
  }

  // ── Submit a new triage request ────────────────────────────────────────────
  // POST /api/triage
  static Future<Map<String, dynamic>> submitTriage({
    required String patientId,
    required String symptoms,
    File? imageFile,
  }) async {
    String fullSymptoms = symptoms;
    if (imageFile != null) {
      final bytes  = await imageFile.readAsBytes();
      final b64    = base64Encode(bytes);
      // Embed a short preview of the base64 so the doctor can see an image was attached
      fullSymptoms += '\n\n[PHOTO_ATTACHED: ${b64.substring(0, 50)}...]';
    }

    final headers = await _authHeaders();
    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/triage'),
          headers: headers,
          body: jsonEncode({'patientId': patientId, 'symptoms': fullSymptoms}),
        )
        .timeout(const Duration(seconds: 30));

    if (response.statusCode == 201 || response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    }
    throw HttpException(
        'Submit triage failed (${response.statusCode}): ${response.body}');
  }

  // ── Get all triage records for this patient ────────────────────────────────
  // GET /api/triage — Server derives patient identity from Bearer token and returns only caller's records.
  static Future<List<Map<String, dynamic>>> getMyTriageRecords(
      String patientId) async {
    final headers = await _authHeaders();
    final response = await http
        .get(Uri.parse('$_baseUrl/api/triage'),
            headers: headers)
        .timeout(const Duration(seconds: 20));

    if (response.statusCode == 200) {
      final List<dynamic> all = jsonDecode(response.body);
      return all.cast<Map<String, dynamic>>();
    }
    throw HttpException(
        'Fetch records failed (${response.statusCode}): ${response.body}');
  }

  // ── Get a single triage record ─────────────────────────────────────────────
  // GET /api/triage/{id}
  static Future<Map<String, dynamic>> getTriageById(String id) async {
    final headers = await _authHeaders();
    final response = await http
        .get(Uri.parse('$_baseUrl/api/triage/$id'),
            headers: headers)
        .timeout(const Duration(seconds: 20));

    if (response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    }
    throw HttpException(
        'Fetch by ID failed (${response.statusCode}): ${response.body}');
  }
}
