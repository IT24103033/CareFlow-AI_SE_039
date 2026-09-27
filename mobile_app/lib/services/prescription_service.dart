// Component D: Pharmacy Inventory & E-Prescriptions — Flutter HTTP service
// Communicates with the ASP.NET Core API to fetch patient prescriptions with JWT authentication.

import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import '../models/prescription_model.dart';
import 'auth_service.dart';

class PrescriptionService {
  static final String _baseUrl = Platform.isAndroid
      ? 'http://10.0.2.2:5241/api'
      : 'http://localhost:5241/api';

  static Future<Map<String, String>> _authHeaders() async {
    final token = await AuthService.getAccessToken();
    final headers = {'Content-Type': 'application/json'};
    if (token != null && token.isNotEmpty) {
      headers['Authorization'] = 'Bearer $token';
    }
    return headers;
  }

  /// Fetches all prescriptions for a given patient ID.
  /// Throws an [Exception] if the request fails.
  static Future<List<Prescription>> getPrescriptionsByPatient(String patientId) async {
    final uri = Uri.parse('$_baseUrl/prescriptions?patientId=$patientId&pageSize=50&sortDir=desc');
    final headers = await _authHeaders();
    final response = await http.get(uri, headers: headers);

    if (response.statusCode == 200) {
      final Map<String, dynamic> body = jsonDecode(response.body) as Map<String, dynamic>;
      final List<dynamic> rawItems = body['items'] as List<dynamic>? ?? [];
      return rawItems.map((e) => Prescription.fromJson(e as Map<String, dynamic>)).toList();
    } else {
      throw Exception('Failed to load prescriptions. Status: ${response.statusCode}');
    }
  }

  /// Fetches a single prescription by its ID.
  static Future<Prescription> getPrescriptionById(String id) async {
    final uri = Uri.parse('$_baseUrl/prescriptions/$id');
    final headers = await _authHeaders();
    final response = await http.get(uri, headers: headers);

    if (response.statusCode == 200) {
      return Prescription.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
    } else {
      throw Exception('Prescription not found.');
    }
  }
}
