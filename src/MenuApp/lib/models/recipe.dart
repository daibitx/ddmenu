import 'category.dart';

class RecipeListItem {
  final int id;
  final String name;
  final Category category;
  final String? image;
  final int cookTime;       // 分钟
  final int difficulty;     // 1-5
  final bool isFavorite;
  final DateTime createdAt;

  RecipeListItem({
    required this.id,
    required this.name,
    required this.category,
    this.image,
    required this.cookTime,
    required this.difficulty,
    required this.isFavorite,
    required this.createdAt,
  });

  factory RecipeListItem.fromJson(Map<String, dynamic> json) {
    return RecipeListItem(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      category: Category.fromJson(json['category'] ?? {}),
      image: json['image'],
      cookTime: json['cookTime'] ?? 0,
      difficulty: json['difficulty'] ?? 1,
      isFavorite: json['isFavorite'] ?? false,
      createdAt: DateTime.parse(json['createdAt'] ?? DateTime.now().toIso8601String()),
    );
  }
}
class RecipeDetail {
  final int id;
  final String name;
  final Category category;
  final String? image;
  final int cookTime;
  final int difficulty;
  final bool isFavorite;
  final List<RecipeIngredient> ingredients;
  final List<RecipeStep> steps;
  final DateTime createdAt;
  final DateTime updatedAt;

  RecipeDetail({
    required this.id,
    required this.name,
    required this.category,
    this.image,
    required this.cookTime,
    required this.difficulty,
    required this.isFavorite,
    required this.ingredients,
    required this.steps,
    required this.createdAt,
    required this.updatedAt,
  });

  factory RecipeDetail.fromJson(Map<String, dynamic> json) {
    return RecipeDetail(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      category: Category.fromJson(json['category'] ?? {}),
      image: json['image'],
      cookTime: json['cookTime'] ?? 0,
      difficulty: json['difficulty'] ?? 1,
      isFavorite: json['isFavorite'] ?? false,
      ingredients: (json['ingredients'] as List? ?? [])
          .map((e) => RecipeIngredient.fromJson(e))
          .toList(),
      steps: (json['steps'] as List? ?? [])
          .map((e) => RecipeStep.fromJson(e))
          .toList(),
      createdAt: DateTime.parse(json['createdAt'] ?? DateTime.now().toIso8601String()),
      updatedAt: DateTime.parse(json['updatedAt'] ?? DateTime.now().toIso8601String()),
    );
  }
}

/// 菜谱原料
class RecipeIngredient {
  final int? id;            // null表示自定义原料
  final String name;
  final bool fromLibrary;   // 是否来自原料库
  final String amount;      // 用量

  RecipeIngredient({
    this.id,
    required this.name,
    required this.fromLibrary,
    required this.amount,
  });

  factory RecipeIngredient.fromJson(Map<String, dynamic> json) {
    return RecipeIngredient(
      id: json['id'],
      name: json['name'] ?? '',
      fromLibrary: json['fromLibrary'] ?? false,
      amount: json['amount'] ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
    'ingredientId': id,
    'name': name,
    'amount': amount,
  };
}

/// 制作步骤
class RecipeStep {
  final int stepNumber;
  final String description;

  RecipeStep({
    required this.stepNumber,
    required this.description,
  });

  factory RecipeStep.fromJson(Map<String, dynamic> json) {
    return RecipeStep(
      stepNumber: json['stepNumber'] ?? 1,
      description: json['description'] ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
    'stepNumber': stepNumber,
    'description': description,
  };
}

/// 创建/更新菜谱请求
class CreateRecipeRequest {
  final String name;
  final int categoryId;
  final String? image;
  final int cookTime;
  final int difficulty;
  final List<RecipeIngredient> ingredients;
  final List<String> steps;

  CreateRecipeRequest({
    required this.name,
    required this.categoryId,
    this.image,
    this.cookTime = 0,
    this.difficulty = 1,
    required this.ingredients,
    required this.steps,
  });

  Map<String, dynamic> toJson() => {
    'name': name,
    'categoryId': categoryId,
    'image': image,
    'cookTime': cookTime,
    'difficulty': difficulty,
    'ingredients': ingredients.map((e) => e.toJson()).toList(),
    'steps': steps,
  };
}
class RecipeQueryParams {
  final int? categoryId;
  final String? keyword;
  final int page;
  final int pageSize;

  RecipeQueryParams({
    this.categoryId,
    this.keyword,
    this.page = 1,
    this.pageSize = 20,
  });

  Map<String, dynamic> toQueryParams() => {
    if (categoryId != null) 'categoryId': categoryId.toString(),
    if (keyword != null && keyword!.isNotEmpty) 'keyword': keyword,
    'page': page.toString(),
    'pageSize': pageSize.toString(),
  };
}
