import 'dart:io';
import '../models/triage_record.dart';
import 'auth_service.dart';
import 'triage_service.dart';

class TriageHistory {
  final String? patientName;
  final List<TriageRecord> records;
  const TriageHistory({required this.patientName, required this.records});
}

/// Screen-facing boundary: production uses the existing patient-session
/// adapter; tests supply controlled responses without real patient data or AI calls.
abstract class TriageRepository {
  const TriageRepository();
  Future<Map<String, dynamic>> submit({
    required String symptoms,
    File? imageFile,
  });
  Future<Map<String, dynamic>> revise({
    required String triageId,
    required String updatedSymptoms,
    required DateTime expectedUpdatedAt,
  });
  Future<TriageHistory> loadHistory();
}

class ApiTriageRepository extends TriageRepository {
  const ApiTriageRepository();

  @override
  Future<Map<String, dynamic>> submit({
    required String symptoms,
    File? imageFile,
  }) async {
    final patientId = await AuthService.getPatientId();
    if (patientId == null) throw StateError('Not signed in.');
    return TriageService.submitTriage(
      patientId: patientId,
      symptoms: symptoms,
      imageFile: imageFile,
    );
  }

  @override
  Future<Map<String, dynamic>> revise({
    required String triageId,
    required String updatedSymptoms,
    required DateTime expectedUpdatedAt,
  }) async {
    final patientId = await AuthService.getPatientId();
    if (patientId == null) throw StateError('Not signed in.');
    return TriageService.reviseTriage(
      triageId: triageId,
      updatedSymptoms: updatedSymptoms,
      expectedUpdatedAt: expectedUpdatedAt,
    );
  }

  @override
  Future<TriageHistory> loadHistory() async {
    final patientId = await AuthService.getPatientId();
    if (patientId == null) throw StateError('Not signed in.');
    final name = await AuthService.getPatientName();
    final records = await TriageService.getMyTriageRecords(patientId);
    return TriageHistory(
      patientName: name,
      records: records.map(TriageRecord.fromJson).toList(),
    );
  }
}
