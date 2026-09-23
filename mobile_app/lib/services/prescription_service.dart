// Component D: Pharmacy Inventory & E-Prescriptions — Flutter HTTP service
// Communicates with the ASP.NET Core API to fetch patient prescriptions.

import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/prescription_model.dart';

class PrescriptionService {
  // Change this base URL to match your running API.
  // For Android emulator use: http://10.0.2.2:5241/api
  // For real device / web / Windows use: http://localhost:5241/api
  static const String _baseUrl = 'http://localhost:5241/api';

  /// Fetches all prescriptions for a given patient ID.
  /// Throws an [Exception] if the request fails.
  static Future<List<Prescription>> getPrescriptionsByPatient(String patientId) async {
    final uri = Uri.parse('$_baseUrl/prescriptions?patientId=$patientId&pageSize=50&sortDir=desc');

    final response = await http.get(uri, headers: {'Content-Type': 'application/json'});

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
    final uri      = Uri.parse('$_baseUrl/prescriptions/$id');
    final response = await http.get(uri);

    if (response.statusCode == 200) {
      return Prescription.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
    } else {
      throw Exception('Prescription not found.');
    }
  }
}
