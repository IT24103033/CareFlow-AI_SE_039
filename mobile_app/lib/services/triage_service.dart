import 'api_config.dart';
// Component B – Triage API Service
// Handles all HTTP calls to the ASP.NET Core backend /api/triage endpoints with JWT authentication.

import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'auth_service.dart';

class TriageService {
  static const String _baseUrl = ApiConfig.origin;

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
    String? attachmentId;
    final token = await AuthService.getAccessToken();

    // 1. Upload image if present
    if (imageFile != null) {
      final uploadUri = Uri.parse('$_baseUrl/api/triage/upload-image');
      final req = http.MultipartRequest('POST', uploadUri);
      if (token != null) req.headers['Authorization'] = 'Bearer $token';
      req.files.add(await http.MultipartFile.fromPath('image', imageFile.path));

      final uploadResp = await req.send().timeout(const Duration(seconds: 120));
        final bodyStr = await uploadResp.stream.bytesToString();
        if (uploadResp.statusCode == 200 || uploadResp.statusCode == 201) {
          final bodyJson = jsonDecode(bodyStr);
          attachmentId = bodyJson['attachmentId'];
        } else {
          throw HttpException('Image upload failed: ${uploadResp.statusCode} - $bodyStr');
        }
    }

    // 2. Submit triage data
    final headers = await _authHeaders();
    final body = <String, dynamic>{
      'patientId': patientId,
      'symptoms': symptoms,
      // ignore: use_null_aware_elements
      if (attachmentId != null) 'attachmentId': attachmentId,
    };

    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/triage'),
          headers: headers,
          body: jsonEncode(body),
        )
        .timeout(const Duration(seconds: 120));

    if (response.statusCode == 201 || response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    }
    
    if (response.statusCode == 400) {
      try {
        final errJson = jsonDecode(response.body);
        if (errJson['error'] == 'EMERGENCY_DETECTED') {
          throw HttpException(errJson['message']);
        }
      } catch (_) {}
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

  // ── Revise a triage request ────────────────────────────────────────────────
  // POST /api/triage/{id}/revise
  static Future<Map<String, dynamic>> reviseTriage({
    required String triageId,
    required String updatedSymptoms,
    required DateTime expectedUpdatedAt,
  }) async {
    final headers = await _authHeaders();
    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/triage/$triageId/revise'),
          headers: headers,
          body: jsonEncode({
            'updatedSymptoms': updatedSymptoms,
            'expectedUpdatedAt': expectedUpdatedAt.toIso8601String(),
          }),
        )
        .timeout(const Duration(seconds: 120));

    if (response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    }
    throw HttpException(
        'Revise triage failed (${response.statusCode}): ${response.body}');
  }
}
