import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/PatientProfile.dart';
import '../models/Ward.dart';

class ApiService {
  // Use 10.0.2.2 for Android Emulator, localhost for Chrome web target
  static const String baseUrl = 'http://10.0.2.2:5241/api';

  /// Login: searches for a patient by full name as their identifier
  Future<PatientProfile?> loginPatient(String fullName) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/PatientProfiles/search?name=${Uri.encodeComponent(fullName)}'),
      );
      if (response.statusCode == 200) {
        List<dynamic> data = json.decode(response.body);
        if (data.isNotEmpty) {
          return PatientProfile.fromJson(data[0]);
        }
      }
      return null;
    } catch (e) {
      throw Exception('Failed to connect to the API: $e');
    }
  }

  Future<PatientProfile?> fetchPatientProfile(String name) async {
    return loginPatient(name);
  }

  Future<List<Ward>> fetchWards() async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/Wards'));
      if (response.statusCode == 200) {
        List<dynamic> data = json.decode(response.body);
        return data.map((json) => Ward.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      throw Exception('Failed to load wards: $e');
    }
  }
}