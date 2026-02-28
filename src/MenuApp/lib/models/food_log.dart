
class FoodLogItem {
  final int id;
  final String time;
  final String mealType;
  final String name;
  final int? recipeId;
  final DateTime createdAt;

  FoodLogItem({
    required this.id,
    required this.time,
    required this.mealType,
    required this.name,
    this.recipeId,
    required this.createdAt,
  });

  factory FoodLogItem.fromJson(Map<String, dynamic> json) {
    return FoodLogItem(
      id: json['id'] ?? 0,
      time: json['time'] ?? '',
      mealType: json['mealType'] ?? '',
      name: json['name'] ?? '',
      recipeId: json['recipeId'],
      createdAt: DateTime.parse(json['createdAt'] ?? DateTime.now().toIso8601String()),
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'time': time,
    'mealType': mealType,
    'name': name,
    'recipeId': recipeId,
  };
}

class FoodLogGroup {
  final String mealType;
  final List<FoodLogItem> items;

  FoodLogGroup({
    required this.mealType,
    required this.items,
  });

  factory FoodLogGroup.fromJson(Map<String, dynamic> json) {
    return FoodLogGroup(
      mealType: json['mealType'] ?? '',
      items: (json['items'] as List? ?? [])
          .map((e) => FoodLogItem.fromJson(e))
          .toList(),
    );
  }
}

class FoodLogDay {
  final String date;
  final List<FoodLogGroup> items;

  FoodLogDay({
    required this.date,
    required this.items,
  });

  factory FoodLogDay.fromJson(Map<String, dynamic> json) {
    return FoodLogDay(
      date: json['date'] ?? '',
      items: (json['items'] as List? ?? [])
          .map((e) => FoodLogGroup.fromJson(e))
          .toList(),
    );
  }
}

class FoodLogCalendar {
  final Map<String, int> data;

  FoodLogCalendar({required this.data});

  factory FoodLogCalendar.fromJson(Map<String, dynamic> json) {
    return FoodLogCalendar(
      data: Map<String, int>.from(json['data'] ?? {}),
    );
  }
}

class CreateFoodLogRequest {
  final String date;        // YYYY-MM-DD
  final String time;        // HH:mm
  final String mealType;
  final String name;
  final int? recipeId;

  CreateFoodLogRequest({
    required this.date,
    required this.time,
    required this.mealType,
    required this.name,
    this.recipeId,
  });

  Map<String, dynamic> toJson() => {
    'date': date,
    'time': time,
    'mealType': mealType,
    'name': name,
    'recipeId': recipeId,
  };
}

class MealTypes {
  static const String breakfast = '早餐';
  static const String lunch = '午餐';
  static const String dinner = '晚餐';
  static const String snack = '加餐';

  static const List<String> all = [breakfast, lunch, dinner, snack];

  static String getIcon(String type) {
    switch (type) {
      case breakfast: return '🌅';
      case lunch: return '🌞';
      case dinner: return '🌙';
      case snack: return '🍰';
      default: return '🍽️';
    }
  }

  static String getDefaultTime(String type) {
    switch (type) {
      case breakfast: return '08:00';
      case lunch: return '12:00';
      case dinner: return '18:00';
      case snack: return '15:00';
      default: return '12:00';
    }
  }
}