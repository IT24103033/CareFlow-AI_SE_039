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
  final String? schedulingOutcome;
  final Map<String, dynamic>? appointmentDetails;
  final String? safetyVerdict;
  final String? safetySummary;
  final String? notificationOutcome;
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
    this.schedulingOutcome,
    this.appointmentDetails,
    this.safetyVerdict,
    this.safetySummary,
    this.notificationOutcome,
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
      schedulingOutcome: json['schedulingOutcome'] as String?,
      appointmentDetails: json['appointmentDetails'] as Map<String, dynamic>?,
      safetyVerdict:  json['safetyVerdict']  as String?,
      safetySummary:  json['safetySummary']  as String?,
      notificationOutcome: json['notificationOutcome'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }
}
