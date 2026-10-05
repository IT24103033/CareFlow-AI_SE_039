import 'package:flutter/material.dart';
import '../services/api_service.dart';

import '../services/auth_service.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class MobileWardStatus extends StatefulWidget {
  const MobileWardStatus({super.key});

  @override
  State<MobileWardStatus> createState() => _MobileWardStatusState();
}

class _MobileWardStatusState extends State<MobileWardStatus> {
  Map<String, dynamic>? _myAdmission;
  @override
  void initState() {
    super.initState();
    _loadMyAdmission();
  }

  Future<void> _loadMyAdmission() async {
    try {
      final token = await AuthService.getAccessToken();
      if (token != null && token.isNotEmpty) {
        final response = await http.get(
          Uri.parse('${ApiService.baseUrl}/PatientProfiles/me'),
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer $token',
          }
        );
        if (response.statusCode == 200) {
          final data = json.decode(response.body);
          final admissions = data['admissions'] as List<dynamic>? ?? [];
          final active = admissions.where((a) => a['dischargedAt'] == null).toList();
          if (active.isNotEmpty && mounted) {
            setState(() {
              _myAdmission = active.first['ward'];
            });
          }
        }
      }
    } catch (e) {
      debugPrint("Error loading admission: $e");
    }
  }



  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF0FAFA),
      appBar: AppBar(
        title: const Text('Hospital Wards',
            style: TextStyle(fontWeight: FontWeight.w600, fontSize: 18)),
        backgroundColor: const Color(0xFF0AB39C),
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: _myAdmission == null
          ? const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.hotel_class_outlined, size: 64, color: Colors.black26),
                  SizedBox(height: 16),
                  Text('You are not currently admitted.',
                      style: TextStyle(fontSize: 16, color: Colors.black54)),
                ],
              ),
            )
          : Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const Text('Your Current Admission', 
                    style: TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: Colors.black54)),
                  const SizedBox(height: 16),
                  Container(
                    padding: const EdgeInsets.all(24),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16),
                      boxShadow: [
                        BoxShadow(
                          color: Colors.black.withValues(alpha: 0.05),
                          blurRadius: 10,
                          offset: const Offset(0, 4),
                        )
                      ],
                    ),
                    child: Column(
                      children: [
                        Container(
                          padding: const EdgeInsets.all(16),
                          decoration: BoxDecoration(
                            color: const Color(0xFF0AB39C).withValues(alpha: 0.1),
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(Icons.bed, size: 48, color: Color(0xFF0AB39C)),
                        ),
                        const SizedBox(height: 24),
                        Text(
                          'Ward ${_myAdmission!['wardNumber']}',
                          style: const TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: Color(0xFF212529)),
                        ),
                        const SizedBox(height: 8),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
                          decoration: BoxDecoration(
                            color: const Color(0xFF0AB39C).withValues(alpha: 0.1),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Text(
                            '${_myAdmission!['wardType']} Ward',
                            style: const TextStyle(fontWeight: FontWeight.bold, color: Color(0xFF0AB39C)),
                          ),
                        ),
                        const SizedBox(height: 32),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Icon(Icons.verified_user, color: Colors.green),
                            const SizedBox(width: 8),
                            const Text('Status: Active', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w500)),
                          ],
                        )
                      ],
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}
