class PatientProfile {
  final String id;
  final String fullName;
  final String bloodGroup;
  final String medicalHistorySummary;

  PatientProfile({
    required this.id,
    required this.fullName,
    required this.bloodGroup,
    required this.medicalHistorySummary,
  });

  // This factory method converts the JSON from your C# API into a Dart object
  factory PatientProfile.fromJson(Map<String, dynamic> json) {
    return PatientProfile(
      id: json['id'] ?? '',
      fullName: json['fullName'] ?? '',
      bloodGroup: json['bloodGroup'] ?? '',
      medicalHistorySummary: json['medicalHistorySummary'] ?? '',
    );
  }
}