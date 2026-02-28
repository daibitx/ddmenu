class Category {
  final int id;
  final String name;
  final String? icon;
  final int sortOrder;

  Category({
    required this.id,
    required this.name,
    this.icon,
    required this.sortOrder,
  });

  factory Category.fromJson(Map<String, dynamic> json) {
    return Category(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      icon: json['icon'],
      sortOrder: json['sortOrder'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'name': name,
    'icon': icon,
    'sortOrder': sortOrder,
  };
}
