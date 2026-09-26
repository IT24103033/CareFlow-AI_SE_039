// Component B – Triage API Service
// Handles all HTTP calls to the ASP.NET Core backend /api/triage endpoints.

import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;

class TriageService {
  // ── Change this to your machine's IP when running on a real Android device ──
  // iOS Simulator  → localhost works fine
  // Android Emulator → use 10.0.2.2 instead of localhost
  // Real device (USB) → use your computer's local IP e.g. 192.168.1.5
  static const String _baseUrl = 'http://localhost:5241';

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

    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/triage'),
          headers: {'Content-Type': 'application/json'},
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
  // GET /api/triage  — filters client-side by patientId
  static Future<List<Map<String, dynamic>>> getMyTriageRecords(
      String patientId) async {
    final response = await http
        .get(Uri.parse('$_baseUrl/api/triage'),
            headers: {'Content-Type': 'application/json'})
        .timeout(const Duration(seconds: 20));

    if (response.statusCode == 200) {
      final List<dynamic> all = jsonDecode(response.body);
      return all
          .cast<Map<String, dynamic>>()
          .where((r) => r['patientId'] == patientId)
          .toList();
    }
    throw HttpException(
        'Fetch records failed (${response.statusCode}): ${response.body}');
  }

  // ── Get a single triage record ─────────────────────────────────────────────
  // GET /api/triage/{id}
  static Future<Map<String, dynamic>> getTriageById(String id) async {
    final response = await http
        .get(Uri.parse('$_baseUrl/api/triage/$id'),
            headers: {'Content-Type': 'application/json'})
        .timeout(const Duration(seconds: 20));

    if (response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    }
    throw HttpException(
        'Fetch by ID failed (${response.statusCode}): ${response.body}');
  }
}
