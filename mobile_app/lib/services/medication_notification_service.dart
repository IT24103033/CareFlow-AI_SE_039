// Component D — Amodhya: Medication readiness push notifications
// Uses flutter_local_notifications to schedule and show local alerts
// when a prescription moves to "Issued" (medication ready for pickup).

import 'package:flutter_local_notifications/flutter_local_notifications.dart';

class MedicationNotificationService {
  MedicationNotificationService._();
  static final MedicationNotificationService instance =
      MedicationNotificationService._();

  final FlutterLocalNotificationsPlugin _plugin =
      FlutterLocalNotificationsPlugin();

  bool _initialized = false;

  /// Call once from main() before runApp.
  Future<void> init() async {
    if (_initialized) return;

    const androidSettings =
        AndroidInitializationSettings('@mipmap/ic_launcher');
    const iosSettings = DarwinInitializationSettings(
      requestAlertPermission: true,
      requestBadgePermission: true,
      requestSoundPermission: true,
    );
    const settings = InitializationSettings(
      android: androidSettings,
      iOS: iosSettings,
    );

    await _plugin.initialize(settings);
    _initialized = true;
  }

  /// Show an immediate local notification that medication is ready for pickup.
  ///
  /// [prescriptionId] is used as the notification ID so each prescription
  /// gets its own dismissible notification.
  Future<void> notifyMedicationReady({
    required String prescriptionId,
    required String medicineSummary,
  }) async {
    await init();

    // Derive a stable int ID from the first 8 hex chars of the UUID
    final notifId =
        int.parse(prescriptionId.replaceAll('-', '').substring(0, 8), radix: 16)
            .abs();

    const androidDetails = AndroidNotificationDetails(
      'medication_ready',                     // channel id
      'Medication Readiness',                 // channel name
      channelDescription:
          'Alerts when your prescription is ready for pharmacy pickup',
      importance: Importance.high,
      priority: Priority.high,
      icon: '@mipmap/ic_launcher',
      color: Color(0xFF6C63FF),
    );

    const iosDetails = DarwinNotificationDetails(
      presentAlert: true,
      presentBadge: true,
      presentSound: true,
    );

    const details = NotificationDetails(
      android: androidDetails,
      iOS: iosDetails,
    );

    await _plugin.show(
      notifId,
      '💊 Medication Ready for Pickup',
      medicineSummary,
      details,
      payload: prescriptionId,
    );
  }

  /// Cancel the notification for a specific prescription (e.g. after dispensed).
  Future<void> cancelForPrescription(String prescriptionId) async {
    final notifId =
        int.parse(prescriptionId.replaceAll('-', '').substring(0, 8), radix: 16)
            .abs();
    await _plugin.cancel(notifId);
  }
}
