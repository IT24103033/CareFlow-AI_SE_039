import 'package:flutter/material.dart';
import '../services/ApiService.dart';
import '../models/PatientProfile.dart';
import 'MobileWardStatus.dart';

class PatientHomeScreen extends StatefulWidget {
  const PatientHomeScreen({super.key});

  @override
  State<PatientHomeScreen> createState() => _PatientHomeScreenState();
}

class _PatientHomeScreenState extends State<PatientHomeScreen> {
  int _selectedIndex = 0;

  // The two screens we can navigate between
  final List<Widget> _screens = [
    const ProfileView(), // We will extract the profile into this widget below
    const MobileWardStatus(),
  ];

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _screens[_selectedIndex],
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _selectedIndex,
        onTap: _onItemTapped,
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Profile'),
          BottomNavigationBarItem(icon: Icon(Icons.bed), label: 'Wards'),
        ],
      ),
    );
  }
}

// Extracted from Day 9 code for clean tabs
class ProfileView extends StatefulWidget {
  const ProfileView({super.key});

  @override
  State<ProfileView> createState() => _ProfileViewState();
}

class _ProfileViewState extends State<ProfileView> {
  final ApiService _apiService = ApiService();
  late Future<PatientProfile?> _patientProfileFuture;

  @override
  void initState() {
    super.initState();
    _patientProfileFuture = _apiService.fetchPatientProfile('Test User');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('My Medical Profile'), backgroundColor: Colors.blueAccent, foregroundColor: Colors.white),
      body: FutureBuilder<PatientProfile?>(
        future: _patientProfileFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) return const Center(child: CircularProgressIndicator());
          if (snapshot.hasError) return Center(child: Text('Error: ${snapshot.error}'));
          if (!snapshot.hasData || snapshot.data == null) return const Center(child: Text('No patient profile found.'));

          final patient = snapshot.data!;
          return Padding(
            padding: const EdgeInsets.all(20.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const CircleAvatar(radius: 40, backgroundColor: Colors.blueAccent, child: Icon(Icons.person, size: 50, color: Colors.white)),
                const SizedBox(height: 20),
                Text('Name: ${patient.fullName}', style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
                const Divider(),
                Text('Blood Group: ${patient.bloodGroup}', style: const TextStyle(fontSize: 18)),
                const SizedBox(height: 10),
                const Text('Medical History:', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                Text(patient.medicalHistorySummary, style: const TextStyle(fontSize: 16)),
              ],
            ),
          );
        },
      ),
    );
  }
}