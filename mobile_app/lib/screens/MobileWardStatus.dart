import 'package:flutter/material.dart';
import '../services/ApiService.dart';
import '../models/Ward.dart';

class MobileWardStatus extends StatefulWidget {
  const MobileWardStatus({super.key});

  @override
  State<MobileWardStatus> createState() => _MobileWardStatusState();
}

class _MobileWardStatusState extends State<MobileWardStatus> {
  final ApiService _apiService = ApiService();
  late Future<List<Ward>> _wardsFuture;

  @override
  void initState() {
    super.initState();
    _wardsFuture = _apiService.fetchWards();
  }

  Color _wardColor(Ward ward) {
    final pct = ward.capacity > 0 ? ward.occupiedBeds / ward.capacity : 1.0;
    if (pct >= 1.0) return Colors.red.shade400;
    if (pct >= 0.8) return Colors.orange.shade400;
    return const Color(0xFF0AB39C);
  }

  IconData _wardIcon(String type) {
    switch (type.toLowerCase()) {
      case 'icu': return Icons.monitor_heart_rounded;
      case 'maternity': return Icons.pregnant_woman_rounded;
      case 'pediatrics': return Icons.child_care_rounded;
      default: return Icons.bed_rounded;
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
      body: FutureBuilder<List<Ward>>(
        future: _wardsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(
                child: CircularProgressIndicator(color: Color(0xFF0AB39C)));
          } else if (snapshot.hasError) {
            return Center(
                child: Text('Error: ${snapshot.error}',
                    style: const TextStyle(color: Colors.red)));
          } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(child: Text('No wards available.'));
          }

          final wards = snapshot.data!;
          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: wards.length,
            itemBuilder: (context, index) {
              final ward = wards[index];
              final color = _wardColor(ward);
              final pct = ward.capacity > 0
                  ? ward.occupiedBeds / ward.capacity
                  : 1.0;
              final available = ward.capacity - ward.occupiedBeds;
              final isFull = available <= 0;

              return Container(
                margin: const EdgeInsets.only(bottom: 14),
                padding: const EdgeInsets.all(18),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(14),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.05),
                      blurRadius: 8,
                      offset: const Offset(0, 2),
                    )
                  ],
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Header row
                    Row(
                      children: [
                        Container(
                          width: 44,
                          height: 44,
                          decoration: BoxDecoration(
                            color: color.withOpacity(0.12),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: Icon(_wardIcon(ward.wardType), color: color, size: 24),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text('${ward.wardType} Ward',
                                  style: const TextStyle(
                                      fontWeight: FontWeight.w700,
                                      fontSize: 16,
                                      color: Color(0xFF212529))),
                              Text('Ward ${ward.wardNumber}',
                                  style: const TextStyle(
                                      fontSize: 12, color: Colors.black45)),
                            ],
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 10, vertical: 5),
                          decoration: BoxDecoration(
                            color: color.withOpacity(0.12),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Text(
                            isFull ? 'FULL' : 'OPEN',
                            style: TextStyle(
                                color: color,
                                fontWeight: FontWeight.bold,
                                fontSize: 12),
                          ),
                        ),
                      ],
                    ),

                    const SizedBox(height: 16),

                    // Stats row
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        _StatBadge('Total', '${ward.capacity}', Icons.bed_rounded, const Color(0xFF6C757D)),
                        _StatBadge('Occupied', '${ward.occupiedBeds}', Icons.person_rounded, Colors.orange.shade400),
                        _StatBadge('Available', '$available', Icons.check_circle_rounded, color),
                      ],
                    ),

                    const SizedBox(height: 14),

                    // Progress bar
                    ClipRRect(
                      borderRadius: BorderRadius.circular(6),
                      child: LinearProgressIndicator(
                        value: pct.clamp(0.0, 1.0),
                        minHeight: 8,
                        backgroundColor: const Color(0xFFE9ECEF),
                        valueColor: AlwaysStoppedAnimation<Color>(color),
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      '${(pct * 100).round()}% occupied',
                      style:
                          const TextStyle(fontSize: 11, color: Colors.black45),
                    ),
                  ],
                ),
              );
            },
          );
        },
      ),
    );
  }
}

class _StatBadge extends StatelessWidget {
  final String label;
  final String value;
  final IconData icon;
  final Color color;

  const _StatBadge(this.label, this.value, this.icon, this.color);

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Icon(icon, color: color, size: 20),
        const SizedBox(height: 4),
        Text(value,
            style: TextStyle(
                fontWeight: FontWeight.bold, fontSize: 16, color: color)),
        Text(label,
            style: const TextStyle(fontSize: 11, color: Colors.black45)),
      ],
    );
  }
}