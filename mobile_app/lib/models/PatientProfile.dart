class PatientProfile {
  final String id;
  final String fullName;
  final String bloodGroup;
  final String medicalHistorySummary;
  final String dateOfBirth;
  final String? email;
  final String? phone;

  PatientProfile({
    required this.id,
    required this.fullName,
    required this.bloodGroup,
    required this.medicalHistorySummary,
    this.dateOfBirth = '',
    this.email,
    this.phone,
  });

  factory PatientProfile.fromJson(Map<String, dynamic> json) {
    return PatientProfile(
      id: json['id']?.toString() ?? '',
      fullName: json['fullName'] ?? '',
      bloodGroup: json['bloodGroup'] ?? '',
      medicalHistorySummary: json['medicalHistorySummary'] ?? '',
      dateOfBirth: (json['dateOfBirth'] ?? '').toString().split('T').first,
      email: json['email']?.toString(),
      phone: json['phone']?.toString(),
    );
  }
}
