class Ingredient {
  final int id;
  final String name;
  final String type;        // 调味料/肉类/蔬菜/五谷/其他
  final String? description;

  Ingredient({
    required this.id,
    required this.name,
    required this.type,
    this.description,
  });

  factory Ingredient.fromJson(Map<String, dynamic> json) {
    return Ingredient(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      type: json['type'] ?? '其他',
      description: json['description'],
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'name': name,
    'type': type,
    'description': description,
  };
}

class IngredientGroup {
  final String type;
  final List<Ingredient> items;

  IngredientGroup({
    required this.type,
    required this.items,
  });

  factory IngredientGroup.fromJson(Map<String, dynamic> json) {
    return IngredientGroup(
      type: json['type'] ?? '',
      items: (json['items'] as List? ?? [])
          .map((e) => Ingredient.fromJson(e))
          .toList(),
    );
  }
}


class CreateIngredientRequest {
  final String name;
  final String type;
  final String? description;

  CreateIngredientRequest({
    required this.name,
    required this.type,
    this.description,
  });

  Map<String, dynamic> toJson() => {
    'name': name,
    'type': type,
    'description': description,
  };
}

class IngredientQueryParams {
  final String? type;
  final String? keyword;

  IngredientQueryParams({this.type, this.keyword});

  Map<String, dynamic> toQueryParams() => {
    if (type != null) 'type': type,
    if (keyword != null) 'keyword': keyword,
  };
}