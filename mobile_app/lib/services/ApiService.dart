import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../models/PatientProfile.dart';
import '../models/Ward.dart';
import 'auth_service.dart';

class ApiService {
  static String get baseUrl {
    if (kIsWeb) {
      return 'http://localhost:5241/api';
    }
    if (defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5241/api';
    }
    return 'http://localhost:5241/api';
  }

  static Future<Map<String, String>> _authHeaders() async {
    final token = await AuthService.getAccessToken();
    return {
      'Content-Type': 'application/json',
      if (token != null && token.isNotEmpty) 'Authorization': 'Bearer $token',
    };
  }

  /// Login: searches for a patient by full name as their identifier
  Future<PatientProfile?> loginPatient(String fullName) async {
    try {
      final response = await http.get(
        Uri.parse(
          '$baseUrl/PatientProfiles/search?name=${Uri.encodeComponent(fullName)}',
        ),
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

  /// Loads the signed-in patient's record, including medical history.
  Future<PatientProfile> fetchMyMedicalProfile() async {
    final patientId = await AuthService.getPatientId();
    if (patientId == null || patientId.isEmpty) {
      throw Exception('Could not find your patient ID. Please log in again.');
    }

    final response = await http.get(
      Uri.parse('$baseUrl/PatientProfiles/$patientId'),
      headers: await _authHeaders(),
    );

    if (response.statusCode == 200) {
      return PatientProfile.fromJson(
        json.decode(response.body) as Map<String, dynamic>,
      );
    }

    if (response.statusCode == 401 || response.statusCode == 403) {
      throw Exception('Session expired. Please log in again.');
    }

    if (response.statusCode == 404) {
      throw Exception('No medical profile was found for this account.');
    }

    throw Exception('Could not load medical history (${response.statusCode}).');
  }

  Future<List<Ward>> fetchWards() async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/Wards'),
        headers: await _authHeaders(),
      );

      if (response.statusCode == 200) {
        List<dynamic> data = json.decode(response.body);

        return data.map((json) => Ward.fromJson(json)).toList();
      }

      return [];
    } catch (e) {
      throw Exception('Failed to load wards: $e');
    }
  }

  // ---------------------------------------------------------
  // COMPONENT C - APPOINTMENTS & RESOURCE SCHEDULING
  // ---------------------------------------------------------

  /// Search doctors and their available schedules.
  Future<List<dynamic>> searchDoctorAvailability({
    String? specialization,
    String? date,
  }) async {
    try {
      final queryParameters = <String, String>{};

      if (specialization != null && specialization.isNotEmpty) {
        queryParameters['specialization'] = specialization;
      }

      if (date != null && date.isNotEmpty) {
        queryParameters['date'] = date;
      }

      final uri = Uri.parse(
        '$baseUrl/DoctorAvailability/search',
      ).replace(
        queryParameters: queryParameters,
      );

      final response = await http.get(uri, headers: await _authHeaders());

      if (response.statusCode == 200) {
        return json.decode(response.body) as List<dynamic>;
      }

      throw Exception(
        'Failed to search doctor availability '
        '(Status: ${response.statusCode})',
      );
    } catch (e) {
      throw Exception(
        'Failed to search doctor availability: $e',
      );
    }
  }

  /// Get available slots for a specific doctor on a specific date.
  Future<List<dynamic>> fetchAvailableSlots({
    required String doctorId,
    required String date,
    int slotDurationMinutes = 30,
  }) async {
    try {
      final uri = Uri.parse(
        '$baseUrl/DoctorAvailability/slots',
      ).replace(
        queryParameters: {
          'doctorId': doctorId,
          'date': date,
          'slotDurationMinutes':
              slotDurationMinutes.toString(),
        },
      );

      final response = await http.get(uri, headers: await _authHeaders());

      if (response.statusCode == 200) {
        return json.decode(response.body) as List<dynamic>;
      }

      throw Exception(
        'Failed to load available slots '
        '(Status: ${response.statusCode})',
      );
    } catch (e) {
      throw Exception(
        'Failed to load available slots: $e',
      );
    }
  }

  /// Book a slot for a triage record.
  Future<Map<String, dynamic>> bookTriageSlot(String triageId, Map<String, dynamic> slotData) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/Triage/$triageId/book-slot'),
        headers: await _authHeaders(),
        body: json.encode(slotData),
      );

      if (response.statusCode == 200) {
        return json.decode(response.body) as Map<String, dynamic>;
      }

      throw Exception(
        'Failed to book slot '
        '(Status: ${response.statusCode})',
      );
    } catch (e) {
      throw Exception(
        'Failed to book slot: $e',
      );
    }
  }

  /// Create a tentative appointment.
  Future<bool> createTentativeAppointment({
    required String doctorId,
    required String patientId,
    required String appointmentDate,
    required String startTime,
    required String endTime,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/Appointments/tentative'),
        headers: await _authHeaders(),
        body: json.encode({
          'doctorId': doctorId,
          'patientId': patientId,
          'appointmentDate': appointmentDate,
          'startTime': startTime,
          'endTime': endTime,
        }),
      );

      if (response.statusCode == 200 ||
          response.statusCode == 201) {
        return true;
      }

      throw Exception(
        'Failed to create appointment '
        '(Status: ${response.statusCode})',
      );
    } catch (e) {
      throw Exception(
        'Failed to create appointment: $e',
      );
    }
  }

  /// Get all appointments for a patient.
  Future<List<dynamic>> getPatientAppointments(String patientId) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/Appointments/patient/$patientId'),
        headers: await _authHeaders(),
      );

      if (response.statusCode == 200) {
        return json.decode(response.body) as List<dynamic>;
      }

      throw Exception(
        'Failed to load appointments '
        '(Status: ${response.statusCode})',
      );
    } catch (e) {
      throw Exception(
        'Failed to load appointments: $e',
      );
    }
  }
}