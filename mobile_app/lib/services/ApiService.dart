import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/PatientProfile.dart';
import '../models/Ward.dart';

class ApiService {
  // Since we are testing in Chrome, localhost works perfectly. 
  // (If you ever test on an Android Emulator, change this to 10.0.2.2)
  static const String baseUrl = 'http://localhost:5241/api';

  Future<PatientProfile?> fetchPatientProfile(String name) async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/PatientProfiles/search?name=$name'));
      
      if (response.statusCode == 200) {
        List<dynamic> data = json.decode(response.body);
        if (data.isNotEmpty) {
          return PatientProfile.fromJson(data[0]); // Return the first match
        }
      }
      return null;
    } catch (e) {
      throw Exception('Failed to connect to the API: $e');
    }
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