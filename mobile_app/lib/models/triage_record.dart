// Component B – Triage Record model
// Mirrors the TriageResponseDto returned by the ASP.NET Core backend.

class TriageRecord {
  final String  id;
  final String  patientId;
  final String  symptoms;
  final String  severityLevel;
  final String  triageStatus;
  final String? doctorNotes;
  final String? aiPlan;
  final String? aiAgentStatus;
  final String? approvalStatus;
  final String? analysisMethod;
  final DateTime createdAt;
  final DateTime updatedAt;

  const TriageRecord({
    required this.id,
    required this.patientId,
    required this.symptoms,
    required this.severityLevel,
    required this.triageStatus,
    this.doctorNotes,
    this.aiPlan,
    this.aiAgentStatus,
    this.approvalStatus,
    this.analysisMethod,
    required this.createdAt,
    required this.updatedAt,
  });

  factory TriageRecord.fromJson(Map<String, dynamic> json) {
    return TriageRecord(
      id:             json['id']             as String,
      patientId:      json['patientId']      as String,
      symptoms:       json['symptoms']       as String,
      severityLevel:  json['severityLevel']  as String? ?? '',
      triageStatus:   json['triageStatus']   as String? ?? 'Pending',
      doctorNotes:    json['doctorNotes']    as String?,
      aiPlan:         json['aiPlan']         as String?,
      aiAgentStatus:  json['aiAgentStatus']  as String?,
      approvalStatus: json['approvalStatus'] as String?,
      analysisMethod: json['analysisMethod'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }
}
