// Component D: Pharmacy Inventory & E-Prescriptions — Flutter model
// Mirrors the API response shape from GET /api/prescriptions

class PrescriptionItem {
  final String id;
  final String medicineId;
  final String medicineName;
  final String medicineCategory;
  final int quantity;
  final String dosage;
  final int durationDays;
  final int stockAvailable;

  const PrescriptionItem({
    required this.id,
    required this.medicineId,
    required this.medicineName,
    required this.medicineCategory,
    required this.quantity,
    required this.dosage,
    required this.durationDays,
    required this.stockAvailable,
  });

  factory PrescriptionItem.fromJson(Map<String, dynamic> json) {
    return PrescriptionItem(
      id:               json['id']               as String? ?? '',
      medicineId:       json['medicineId']       as String? ?? '',
      medicineName:     json['medicineName']     as String? ?? 'Unknown Medicine',
      medicineCategory: json['medicineCategory'] as String? ?? '',
      quantity:         json['quantity']         as int? ?? 0,
      dosage:           json['dosage']           as String? ?? '',
      durationDays:     json['durationDays']     as int? ?? 0,
      stockAvailable:   json['stockAvailable']   as int? ?? 0,
    );
  }
}

class Prescription {
  final String id;
  final String patientName;
  final String patientId;
  final String triageRecordId;
  final String triageSeverity;
  final String status;
  final String? aiSafetyStatus;
  final String? aiSafetyCheckResult;
  final String? notes;
  final bool notificationSent;
  final String? notificationChannel;
  final String? notifiedAt;
  final String createdAt;
  final List<PrescriptionItem> items;

  const Prescription({
    required this.id,
    required this.patientName,
    required this.patientId,
    required this.triageRecordId,
    required this.triageSeverity,
    required this.status,
    this.aiSafetyStatus,
    this.aiSafetyCheckResult,
    this.notes,
    required this.notificationSent,
    this.notificationChannel,
    this.notifiedAt,
    required this.createdAt,
    required this.items,
  });

  factory Prescription.fromJson(Map<String, dynamic> json) {
    final rawItems = json['items'] as List<dynamic>? ?? [];
    return Prescription(
      id:                    json['id']                    as String? ?? '',
      patientName:           json['patientName']           as String? ?? 'Unknown Patient',
      patientId:             json['patientId']             as String? ?? '',
      triageRecordId:        json['triageRecordId']        as String? ?? '',
      triageSeverity:        json['triageSeverity']        as String? ?? '',
      status:                json['status']                as String? ?? 'Draft',
      aiSafetyStatus:        json['aiSafetyStatus']        as String?,
      aiSafetyCheckResult:   json['aiSafetyCheckResult']   as String?,
      notes:                 json['notes']                 as String?,
      notificationSent:      json['notificationSent']      as bool? ?? false,
      notificationChannel:   json['notificationChannel']   as String?,
      notifiedAt:            json['notifiedAt']            as String?,
      createdAt:             json['createdAt']             as String? ?? '',
      items:                 rawItems.map((e) => PrescriptionItem.fromJson(e as Map<String, dynamic>)).toList(),
    );
  }
}
