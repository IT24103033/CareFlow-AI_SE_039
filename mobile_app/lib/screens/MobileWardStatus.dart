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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Hospital Wards'),
        backgroundColor: Colors.blueAccent,
        foregroundColor: Colors.white,
      ),
      body: FutureBuilder<List<Ward>>(
        future: _wardsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          } else if (snapshot.hasError) {
            return Center(child: Text('Error: ${snapshot.error}'));
          } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(child: Text('No wards available.'));
          }

          return ListView.builder(
            padding: const EdgeInsets.all(10),
            itemCount: snapshot.data!.length,
            itemBuilder: (context, index) {
              final ward = snapshot.data![index];
              final isFull = ward.occupiedBeds >= ward.capacity;

              return Card(
                elevation: 3,
                margin: const EdgeInsets.symmetric(vertical: 8),
                child: ListTile(
                  leading: Icon(
                    Icons.local_hospital,
                    color: isFull ? Colors.red : Colors.green,
                    size: 40,
                  ),
                  title: Text('Ward ${ward.wardNumber} (${ward.wardType})'),
                  subtitle: Text('${ward.capacity - ward.occupiedBeds} beds available'),
                  trailing: isFull 
                      ? const Text('FULL', style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold))
                      : const Text('OPEN', style: TextStyle(color: Colors.green, fontWeight: FontWeight.bold)),
                ),
              );
            },
          );
        },
      ),
    );
  }
}