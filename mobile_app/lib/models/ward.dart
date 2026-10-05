class Ward {
  final String id;
  final String wardNumber;
  final String wardType;
  final int capacity;
  final int occupiedBeds;

  Ward({
    required this.id,
    required this.wardNumber,
    required this.wardType,
    required this.capacity,
    required this.occupiedBeds,
  });

  factory Ward.fromJson(Map<String, dynamic> json) {
    return Ward(
      id: json['id'] ?? '',
      wardNumber: json['wardNumber'] ?? '',
      wardType: json['wardType'] ?? '',
      capacity: json['capacity'] ?? 0,
      occupiedBeds: json['occupiedBeds'] ?? 0,
    );
  }
}