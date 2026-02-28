# Flutter 菜谱管理 App - 整体架构指南

> 本指南基于后端API和设计文档编写，供学习参考使用。
> 依赖版本: go_router ^17.1.0 | provider ^6.1.5+1 | shared_preferences ^2.5.4 | dio ^5.9.1

---

## 一、项目整体架构

### 1.1 架构模式
采用 **Provider + Repository** 模式：

```
┌─────────────────────────────────────────────────────────┐
│                      UI Layer (Pages/Widgets)           │
├─────────────────────────────────────────────────────────┤
│              State Management (Providers)               │
├─────────────────────────────────────────────────────────┤
│              Service Layer (API/Services)               │
├─────────────────────────────────────────────────────────┤
│              Data Layer (Models/Storage)                │
└─────────────────────────────────────────────────────────┘
```

### 1.2 目录结构规划

```
lib/
├── main.dart                      # 应用入口
├── app.dart                       # App配置 (Provider注入、路由)
├── router.dart                    # 路由配置 (go_router)
│
├── models/                        # 数据模型 (DTO)
│   ├── user.dart
│   ├── recipe.dart
│   ├── ingredient.dart
│   ├── category.dart
│   ├── food_log.dart
│   └── api_response.dart          # 统一API响应封装
│
├── services/                      # 业务服务层
│   ├── api_client.dart            # Dio封装
│   ├── auth_service.dart          # 认证服务
│   ├── recipe_service.dart        # 菜谱服务
│   ├── ingredient_service.dart    # 原料服务
│   ├── category_service.dart      # 分类服务
│   ├── food_log_service.dart      # 饮食记录服务
│   └── user_service.dart          # 用户服务
│
├── providers/                     # 状态管理
│   ├── auth_provider.dart         # 认证状态
│   ├── recipe_provider.dart       # 菜谱状态
│   ├── ingredient_provider.dart   # 原料状态
│   ├── category_provider.dart     # 分类状态
│   ├── food_log_provider.dart     # 饮食记录状态
│   └── user_provider.dart         # 用户状态
│
├── pages/                         # 页面
│   ├── login_page.dart
│   ├── home_page.dart             # 主框架(底部导航)
│   ├── recipe/
│   │   ├── recipes_overview_page.dart
│   │   ├── recipe_detail_page.dart
│   │   └── recipe_edit_page.dart
│   ├── ingredient/
│   │   ├── ingredient_list_page.dart
│   │   └── ingredient_edit_page.dart
│   ├── foodlog/
│   │   ├── today_foodlog_page.dart
│   │   └── foodlog_calendar_page.dart
│   └── profile/
│       ├── profile_page.dart
│       └── favorites_page.dart
│
├── widgets/                       # 公共组件
│   ├── common/
│   │   ├── app_button.dart
│   │   ├── app_text_field.dart
│   │   ├── loading_widget.dart
│   │   └── error_widget.dart
│   ├── recipe/
│   │   ├── recipe_card.dart
│   │   ├── category_selector.dart
│   │   └── difficulty_stars.dart
│   ├── ingredient/
│   │   ├── ingredient_chip.dart
│   │   └── ingredient_group.dart
│   └── foodlog/
│       ├── meal_section.dart
│       └── calendar_cell.dart
│
└── utils/                         # 工具类
    ├── constants.dart             # 常量
    ├── extensions.dart            # 扩展方法
    └── helpers.dart               # 辅助函数
```

---

## 二、基础层代码

### 2.1 统一API响应模型 (models/api_response.dart)

```dart
/// API统一响应格式，对应后端 OperateResult<T>
class ApiResponse<T> {
  final int code;
  final String? message;
  final T? data;
  final bool success;

  ApiResponse({
    required this.code,
    this.message,
    this.data,
    required this.success,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic)? fromJsonT,
  ) {
    return ApiResponse(
      code: json['code'] ?? 500,
      message: json['message'],
      data: json['data'] != null && fromJsonT != null 
          ? fromJsonT(json['data']) 
          : null,
      success: json['code'] == 200,
    );
  }

  bool get isSuccess => code == 200;
}

/// 分页数据模型
class PagedList<T> {
  final List<T> list;
  final int total;
  final int page;
  final int pageSize;

  PagedList({
    required this.list,
    required this.total,
    required this.page,
    required this.pageSize,
  });

  factory PagedList.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic) fromJsonT,
  ) {
    return PagedList(
      list: (json['list'] as List).map((e) => fromJsonT(e)).toList(),
      total: json['total'] ?? 0,
      page: json['page'] ?? 1,
      pageSize: json['pageSize'] ?? 20,
    );
  }

  bool get hasMore => list.length < total;
}
```

### 2.2 用户模型 (models/user.dart)

```dart
class User {
  final int id;
  final String userName;
  final String role;  // 'admin' | 'user'
  final String? avatarUrl;

  User({
    required this.id,
    required this.userName,
    required this.role,
    this.avatarUrl,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'] ?? 0,
      userName: json['userName'] ?? '',
      role: json['role'] ?? 'user',
      avatarUrl: json['avatarUrl'],
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'userName': userName,
    'role': role,
    'avatarUrl': avatarUrl,
  };

  bool get isAdmin => role == 'admin';
}

/// 登录响应
class LoginResponse {
  final String token;
  final String userName;
  final String role;

  LoginResponse({
    required this.token,
    required this.userName,
    required this.role,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      token: json['token'] ?? '',
      userName: json['userName'] ?? '',
      role: json['role'] ?? 'user',
    );
  }
}

/// 用户资料
class UserProfile {
  final int id;
  final String userName;
  final String? avatarUrl;
  final String role;

  UserProfile({
    required this.id,
    required this.userName,
    this.avatarUrl,
    required this.role,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'] ?? 0,
      userName: json['userName'] ?? '',
      avatarUrl: json['avatarUrl'],
      role: json['role'] ?? 'user',
    );
  }
}
```

### 2.3 分类模型 (models/category.dart)

```dart
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

/// 分类类型定义（前端硬编码或从API获取）
class CategoryTypes {
  static const String soup = '汤';
  static const String stirFry = '炒菜';
  static const String coldDish = '凉菜';
  static const String dessert = '甜品';
  static const String staple = '主食';
  static const String drink = '饮品';
  
  static const List<String> all = [soup, stirFry, coldDish, dessert, staple, drink];
}
```

### 2.4 菜谱模型 (models/recipe.dart)

```dart
/// 菜谱列表项
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

/// 菜谱详情
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

/// 收藏请求
class FavoriteRequest {
  final bool isFavorite;

  FavoriteRequest({required this.isFavorite});

  Map<String, dynamic> toJson() => {'isFavorite': isFavorite};
}

/// 菜谱查询参数
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
```

### 2.5 原料模型 (models/ingredient.dart)

```dart
/// 原料
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

/// 分组后的原料
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

/// 原料类型
class IngredientTypes {
  static const String seasoning = '调味料';
  static const String meat = '肉类';
  static const String vegetable = '蔬菜';
  static const String grain = '五谷';
  static const String other = '其他';

  static const List<String> all = [seasoning, meat, vegetable, grain, other];
  
  static String getIcon(String type) {
    switch (type) {
      case seasoning: return '🧂';
      case meat: return '🥩';
      case vegetable: return '🥬';
      case grain: return '🌾';
      default: return '📦';
    }
  }
}

/// 创建/更新原料请求
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

/// 原料查询参数
class IngredientQueryParams {
  final String? type;
  final String? keyword;

  IngredientQueryParams({this.type, this.keyword});

  Map<String, dynamic> toQueryParams() => {
    if (type != null) 'type': type,
    if (keyword != null) 'keyword': keyword,
  };
}
```

### 2.6 饮食记录模型 (models/food_log.dart)

```dart
/// 饮食记录项
class FoodLogItem {
  final int id;
  final String time;        // HH:mm 格式
  final String mealType;    // 早餐/午餐/晚餐/加餐
  final String name;
  final int? recipeId;      // 关联的菜谱ID
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

/// 按餐段分组的饮食记录
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

/// 某天的饮食记录
class FoodLogDay {
  final String date;        // YYYY-MM-DD
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

/// 日历数据
class FoodLogCalendar {
  final Map<String, int> data;  // { "2024-01-15": 3, ... }

  FoodLogCalendar({required this.data});

  factory FoodLogCalendar.fromJson(Map<String, dynamic> json) {
    return FoodLogCalendar(
      data: Map<String, int>.from(json['data'] ?? {}),
    );
  }
}

/// 创建/更新饮食记录请求
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

/// 餐段类型
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

  /// 默认时间段建议
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
```

---

## 三、服务层代码

### 3.1 API客户端封装 (services/api_client.dart)

```dart
import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ApiClient {
  static final ApiClient _instance = ApiClient._internal();
  factory ApiClient() => _instance;
  
  late Dio _dio;
  String? _token;
  
  // API基础地址，根据实际情况修改
  static const String baseUrl = 'http://10.0.2.2:5000';  // Android模拟器
  // static const String baseUrl = 'http://localhost:5000';  // iOS模拟器
  // static const String baseUrl = 'https://your-api.com';  // 生产环境

  ApiClient._internal() {
    _dio = Dio(BaseOptions(
      baseUrl: baseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 10),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    ));

    // 添加拦截器
    _dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        // 自动添加Token
        if (_token != null) {
          options.headers['Authorization'] = 'Bearer $_token';
        }
        print('REQUEST: ${options.method} ${options.path}');
        return handler.next(options);
      },
      onResponse: (response, handler) {
        print('RESPONSE: ${response.statusCode} ${response.data}');
        return handler.next(response);
      },
      onError: (error, handler) {
        print('ERROR: ${error.response?.statusCode} ${error.message}');
        // Token过期处理
        if (error.response?.statusCode == 401) {
          // 可以在这里触发重新登录或刷新Token
        }
        return handler.next(error);
      },
    ));
  }

  /// 设置Token
  void setToken(String token) {
    _token = token;
  }

  /// 清除Token
  void clearToken() {
    _token = null;
  }

  /// 从本地存储加载Token
  Future<void> loadToken() async {
    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString('token');
  }

  /// GET请求
  Future<Response> get(String path, {Map<String, dynamic>? queryParameters}) async {
    return await _dio.get(path, queryParameters: queryParameters);
  }

  /// POST请求
  Future<Response> post(String path, {dynamic data}) async {
    return await _dio.post(path, data: data);
  }

  /// PUT请求
  Future<Response> put(String path, {dynamic data}) async {
    return await _dio.put(path, data: data);
  }

  /// DELETE请求
  Future<Response> delete(String path) async {
    return await _dio.delete(path);
  }

  /// 上传文件
  Future<Response> upload(String path, String filePath, String fileName) async {
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(filePath, filename: fileName),
    });
    return await _dio.post(path, data: formData);
  }
}
```

### 3.2 认证服务 (services/auth_service.dart)

```dart
import '../models/api_response.dart';
import '../models/user.dart';
import 'api_client.dart';

class AuthService {
  final ApiClient _client = ApiClient();

  /// 登录
  /// POST /api/auth/login
  Future<ApiResponse<LoginResponse>> login(String userName, String password) async {
    try {
      final response = await _client.post('/api/auth/login', data: {
        'userName': userName,
        'password': password,
      });
      
      return ApiResponse.fromJson(
        response.data,
        (json) => LoginResponse.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(
        code: 500,
        message: '网络错误: $e',
        success: false,
      );
    }
  }

  /// 刷新Token
  /// POST /api/auth/refresh
  Future<ApiResponse<LoginResponse>> refreshToken() async {
    try {
      final response = await _client.post('/api/auth/refresh');
      return ApiResponse.fromJson(
        response.data,
        (json) => LoginResponse.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(
        code: 500,
        message: '刷新Token失败: $e',
        success: false,
      );
    }
  }
}
```

### 3.3 菜谱服务 (services/recipe_service.dart)

```dart
import '../models/api_response.dart';
import '../models/recipe.dart';
import 'api_client.dart';

class RecipeService {
  final ApiClient _client = ApiClient();

  /// 获取菜谱列表
  /// GET /api/recipes
  Future<ApiResponse<PagedList<RecipeListItem>>> getRecipes(RecipeQueryParams params) async {
    try {
      final response = await _client.get(
        '/api/recipes',
        queryParameters: params.toQueryParams(),
      );

      return ApiResponse.fromJson(
        response.data,
        (json) => PagedList.fromJson(json, (item) => RecipeListItem.fromJson(item)),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 获取菜谱详情
  /// GET /api/recipes/{id}
  Future<ApiResponse<RecipeDetail>> getRecipeDetail(int id) async {
    try {
      final response = await _client.get('/api/recipes/$id');
      return ApiResponse.fromJson(
        response.data,
        (json) => RecipeDetail.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 创建菜谱
  /// POST /api/recipes
  Future<ApiResponse<RecipeDetail>> createRecipe(CreateRecipeRequest request) async {
    try {
      final response = await _client.post('/api/recipes', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
        (json) => RecipeDetail.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 更新菜谱
  /// PUT /api/recipes/{id}
  Future<ApiResponse<RecipeDetail>> updateRecipe(int id, CreateRecipeRequest request) async {
    try {
      final response = await _client.put('/api/recipes/$id', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
        (json) => RecipeDetail.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 删除菜谱
  /// DELETE /api/recipes/{id}
  Future<ApiResponse<bool>> deleteRecipe(int id) async {
    try {
      final response = await _client.delete('/api/recipes/$id');
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 切换收藏状态
  /// POST /api/recipes/{id}/favorite
  Future<ApiResponse<bool>> toggleFavorite(int id, bool isFavorite) async {
    try {
      final response = await _client.post(
        '/api/recipes/$id/favorite',
        data: {'isFavorite': isFavorite},
      );
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}
```

### 3.4 原料服务 (services/ingredient_service.dart)

```dart
import '../models/api_response.dart';
import '../models/ingredient.dart';
import 'api_client.dart';

class IngredientService {
  final ApiClient _client = ApiClient();

  /// 获取原料列表（分组）
  /// GET /api/ingredients
  Future<ApiResponse<List<IngredientGroup>>> getIngredients(IngredientQueryParams params) async {
    try {
      final response = await _client.get(
        '/api/ingredients',
        queryParameters: params.toQueryParams(),
      );

      return ApiResponse.fromJson(
        response.data,
        (json) => (json as List).map((e) => IngredientGroup.fromJson(e)).toList(),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 获取原料详情
  /// GET /api/ingredients/{id}
  Future<ApiResponse<Ingredient>> getIngredientDetail(int id) async {
    try {
      final response = await _client.get('/api/ingredients/$id');
      return ApiResponse.fromJson(
        response.data,
        (json) => Ingredient.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 创建原料
  /// POST /api/ingredients
  Future<ApiResponse<Ingredient>> createIngredient(CreateIngredientRequest request) async {
    try {
      final response = await _client.post('/api/ingredients', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
        (json) => Ingredient.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 更新原料
  /// PUT /api/ingredients/{id}
  Future<ApiResponse<Ingredient>> updateIngredient(int id, CreateIngredientRequest request) async {
    try {
      final response = await _client.put('/api/ingredients/$id', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
        (json) => Ingredient.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 删除原料
  /// DELETE /api/ingredients/{id}
  Future<ApiResponse<bool>> deleteIngredient(int id) async {
    try {
      final response = await _client.delete('/api/ingredients/$id');
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}
```

### 3.5 分类服务 (services/category_service.dart)

```dart
import '../models/api_response.dart';
import '../models/category.dart';
import 'api_client.dart';

class CategoryService {
  final ApiClient _client = ApiClient();

  /// 获取所有分类
  /// GET /api/categories (AllowAnonymous)
  Future<ApiResponse<List<Category>>> getCategories() async {
    try {
      final response = await _client.get('/api/categories');
      return ApiResponse.fromJson(
        response.data,
        (json) => (json as List).map((e) => Category.fromJson(e)).toList(),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 获取分类详情
  /// GET /api/categories/{id}
  Future<ApiResponse<Category>> getCategoryDetail(int id) async {
    try {
      final response = await _client.get('/api/categories/$id');
      return ApiResponse.fromJson(
        response.data,
        (json) => Category.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 创建分类
  /// POST /api/categories
  Future<ApiResponse<Category>> createCategory(Map<String, dynamic> data) async {
    try {
      final response = await _client.post('/api/categories', data: data);
      return ApiResponse.fromJson(
        response.data,
        (json) => Category.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 更新分类
  /// PUT /api/categories/{id}
  Future<ApiResponse<Category>> updateCategory(int id, Map<String, dynamic> data) async {
    try {
      final response = await _client.put('/api/categories/$id', data: data);
      return ApiResponse.fromJson(
        response.data,
        (json) => Category.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 删除分类
  /// DELETE /api/categories/{id}
  Future<ApiResponse<bool>> deleteCategory(int id) async {
    try {
      final response = await _client.delete('/api/categories/$id');
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}
```

### 3.6 饮食记录服务 (services/food_log_service.dart)

```dart
import '../models/api_response.dart';
import '../models/food_log.dart';
import 'api_client.dart';

class FoodLogService {
  final ApiClient _client = ApiClient();

  /// 获取某天的饮食记录
  /// GET /api/foodlog?date=YYYY-MM-DD
  Future<ApiResponse<FoodLogDay>> getFoodLogByDate(String date) async {
    try {
      final response = await _client.get(
        '/api/foodlog',
        queryParameters: {'date': date},
      );
      return ApiResponse.fromJson(
        response.data,
        (json) => FoodLogDay.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 获取日历数据
  /// GET /api/foodlog/calendar?year=2024&month=1
  Future<ApiResponse<FoodLogCalendar>> getCalendar(int year, int month) async {
    try {
      final response = await _client.get(
        '/api/foodlog/calendar',
        queryParameters: {'year': year, 'month': month},
      );
      return ApiResponse.fromJson(
        response.data,
        (json) => FoodLogCalendar.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 创建饮食记录
  /// POST /api/foodlog
  Future<ApiResponse<FoodLogItem>> createFoodLog(CreateFoodLogRequest request) async {
    try {
      final response = await _client.post('/api/foodlog', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
        (json) => FoodLogItem.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 更新饮食记录
  /// PUT /api/foodlog/{id}
  Future<ApiResponse<FoodLogItem>> updateFoodLog(int id, CreateFoodLogRequest request) async {
    try {
      final response = await _client.put('/api/foodlog/$id', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
        (json) => FoodLogItem.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 删除饮食记录
  /// DELETE /api/foodlog/{id}
  Future<ApiResponse<bool>> deleteFoodLog(int id) async {
    try {
      final response = await _client.delete('/api/foodlog/$id');
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}
```

### 3.7 用户服务 (services/user_service.dart)

```dart
import '../models/api_response.dart';
import '../models/user.dart';
import 'api_client.dart';

class UserService {
  final ApiClient _client = ApiClient();

  /// 获取用户资料
  /// GET /api/user/profile
  Future<ApiResponse<UserProfile>> getProfile() async {
    try {
      final response = await _client.get('/api/user/profile');
      return ApiResponse.fromJson(
        response.data,
        (json) => UserProfile.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 更新用户资料
  /// PUT /api/user/profile
  Future<ApiResponse<UserProfile>> updateProfile(Map<String, dynamic> data) async {
    try {
      final response = await _client.put('/api/user/profile', data: data);
      return ApiResponse.fromJson(
        response.data,
        (json) => UserProfile.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 修改密码
  /// PUT /api/user/password
  Future<ApiResponse<bool>> changePassword(String currentPassword, String newPassword) async {
    try {
      final response = await _client.put('/api/user/password', data: {
        'currentPassword': currentPassword,
        'newPassword': newPassword,
      });
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  /// 上传头像
  /// POST /api/user/avatar
  Future<ApiResponse<String>> uploadAvatar(String filePath, String fileName) async {
    try {
      final response = await _client.upload('/api/user/avatar', filePath, fileName);
      return ApiResponse.fromJson(response.data, (json) => json as String);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}
```

---

## 四、状态管理 (Providers)

### 4.1 认证状态管理 (providers/auth_provider.dart)

```dart
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/user.dart';
import '../services/api_client.dart';
import '../services/auth_service.dart';

class AuthProvider extends ChangeNotifier {
  final AuthService _authService = AuthService();
  
  bool _isLoading = false;
  String? _error;
  User? _currentUser;
  String? _token;

  // Getters
  bool get isLoading => _isLoading;
  String? get error => _error;
  User? get currentUser => _currentUser;
  String? get token => _token;
  bool get isAuthenticated => _token != null;
  bool get isAdmin => _currentUser?.isAdmin ?? false;

  /// 初始化：从本地存储加载登录状态
  Future<void> initialize() async {
    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString('token');
    
    if (_token != null) {
      ApiClient().setToken(_token!);
      // 可以在这里调用获取用户信息的API
      final userId = prefs.getInt('userId') ?? 0;
      final userName = prefs.getString('userName') ?? '';
      final role = prefs.getString('role') ?? 'user';
      _currentUser = User(id: userId, userName: userName, role: role);
    }
    
    notifyListeners();
  }

  /// 登录
  Future<bool> login(String userName, String password) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _authService.login(userName, password);
      
      if (response.isSuccess && response.data != null) {
        _token = response.data!.token;
        _currentUser = User(
          id: 0,  // 可以从Token解析或后续API获取
          userName: response.data!.userName,
          role: response.data!.role,
        );

        // 保存到本地
        final prefs = await SharedPreferences.getInstance();
        await prefs.setString('token', _token!);
        await prefs.setString('userName', response.data!.userName);
        await prefs.setString('role', response.data!.role);

        // 设置API客户端Token
        ApiClient().setToken(_token!);

        _isLoading = false;
        notifyListeners();
        return true;
      } else {
        _error = response.message ?? '登录失败';
        _isLoading = false;
        notifyListeners();
        return false;
      }
    } catch (e) {
      _error = '登录出错: $e';
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  /// 退出登录
  Future<void> logout() async {
    _token = null;
    _currentUser = null;
    _error = null;

    // 清除本地存储
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('token');
    await prefs.remove('userId');
    await prefs.remove('userName');
    await prefs.remove('role');

    // 清除API客户端Token
    ApiClient().clearToken();

    notifyListeners();
  }

  /// 清除错误信息
  void clearError() {
    _error = null;
    notifyListeners();
  }
}
```

### 4.2 菜谱状态管理 (providers/recipe_provider.dart)

```dart
import 'package:flutter/material.dart';
import '../models/recipe.dart';
import '../services/recipe_service.dart';

class RecipeProvider extends ChangeNotifier {
  final RecipeService _service = RecipeService();

  // 列表状态
  List<RecipeListItem> _recipes = [];
  bool _isLoading = false;
  String? _error;
  int _currentPage = 1;
  bool _hasMore = true;

  // 详情状态
  RecipeDetail? _currentRecipe;
  bool _isLoadingDetail = false;

  // 分类筛选
  int? _selectedCategoryId;

  // Getters
  List<RecipeListItem> get recipes => _recipes;
  bool get isLoading => _isLoading;
  String? get error => _error;
  RecipeDetail? get currentRecipe => _currentRecipe;
  bool get isLoadingDetail => _isLoadingDetail;
  int? get selectedCategoryId => _selectedCategoryId;
  bool get hasMore => _hasMore;

  /// 加载菜谱列表
  Future<void> loadRecipes({bool refresh = false, String? keyword}) async {
    if (refresh) {
      _currentPage = 1;
      _recipes = [];
      _hasMore = true;
    }

    if (_isLoading || (!_hasMore && !refresh)) return;

    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final params = RecipeQueryParams(
        categoryId: _selectedCategoryId,
        keyword: keyword,
        page: _currentPage,
        pageSize: 20,
      );

      final response = await _service.getRecipes(params);

      if (response.isSuccess && response.data != null) {
        _recipes.addAll(response.data!.list);
        _hasMore = response.data!.hasMore;
        _currentPage++;
      } else {
        _error = response.message;
      }
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 加载菜谱详情
  Future<void> loadRecipeDetail(int id) async {
    _isLoadingDetail = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _service.getRecipeDetail(id);
      if (response.isSuccess) {
        _currentRecipe = response.data;
      } else {
        _error = response.message;
      }
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoadingDetail = false;
      notifyListeners();
    }
  }

  /// 设置分类筛选
  void setCategoryFilter(int? categoryId) {
    _selectedCategoryId = categoryId;
    loadRecipes(refresh: true);
  }

  /// 切换收藏状态
  Future<bool> toggleFavorite(int recipeId, bool isFavorite) async {
    try {
      final response = await _service.toggleFavorite(recipeId, isFavorite);
      if (response.isSuccess) {
        // 更新本地数据
        final index = _recipes.indexWhere((r) => r.id == recipeId);
        if (index != -1) {
          // 需要重新获取列表或使用可变对象
        }
        if (_currentRecipe != null && _currentRecipe!.id == recipeId) {
          // 更新详情
        }
        notifyListeners();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  /// 创建菜谱
  Future<bool> createRecipe(CreateRecipeRequest request) async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _service.createRecipe(request);
      if (response.isSuccess) {
        await loadRecipes(refresh: true);
        return true;
      }
      _error = response.message;
      return false;
    } catch (e) {
      _error = e.toString();
      return false;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 更新菜谱
  Future<bool> updateRecipe(int id, CreateRecipeRequest request) async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _service.updateRecipe(id, request);
      if (response.isSuccess) {
        _currentRecipe = response.data;
        await loadRecipes(refresh: true);
        return true;
      }
      _error = response.message;
      return false;
    } catch (e) {
      _error = e.toString();
      return false;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 删除菜谱
  Future<bool> deleteRecipe(int id) async {
    try {
      final response = await _service.deleteRecipe(id);
      if (response.isSuccess) {
        _recipes.removeWhere((r) => r.id == id);
        notifyListeners();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  void clearError() {
    _error = null;
    notifyListeners();
  }
}
```

### 4.3 分类状态管理 (providers/category_provider.dart)

```dart
import 'package:flutter/material.dart';
import '../models/category.dart';
import '../services/category_service.dart';

class CategoryProvider extends ChangeNotifier {
  final CategoryService _service = CategoryService();

  List<Category> _categories = [];
  bool _isLoading = false;
  String? _error;

  List<Category> get categories => _categories;
  bool get isLoading => _isLoading;
  String? get error => _error;

  /// 加载分类列表
  Future<void> loadCategories() async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _service.getCategories();
      if (response.isSuccess && response.data != null) {
        _categories = response.data!;
      } else {
        _error = response.message;
      }
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 根据ID获取分类
  Category? getCategoryById(int id) {
    try {
      return _categories.firstWhere((c) => c.id == id);
    } catch (e) {
      return null;
    }
  }
}
```

### 4.4 原料状态管理 (providers/ingredient_provider.dart)

```dart
import 'package:flutter/material.dart';
import '../models/ingredient.dart';
import '../services/ingredient_service.dart';

class IngredientProvider extends ChangeNotifier {
  final IngredientService _service = IngredientService();

  List<IngredientGroup> _groupedIngredients = [];
  List<Ingredient> _allIngredients = [];
  bool _isLoading = false;
  String? _error;
  String? _selectedType;
  String? _searchKeyword;

  List<IngredientGroup> get groupedIngredients => _groupedIngredients;
  List<Ingredient> get allIngredients => _allIngredients;
  bool get isLoading => _isLoading;
  String? get error => _error;
  String? get selectedType => _selectedType;

  /// 加载原料列表
  Future<void> loadIngredients() async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final params = IngredientQueryParams(
        type: _selectedType,
        keyword: _searchKeyword,
      );

      final response = await _service.getIngredients(params);
      if (response.isSuccess && response.data != null) {
        _groupedIngredients = response.data!;
        // 展平所有原料
        _allIngredients = _groupedIngredients
            .expand((g) => g.items)
            .toList();
      } else {
        _error = response.message;
      }
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 设置类型筛选
  void setTypeFilter(String? type) {
    _selectedType = type;
    loadIngredients();
  }

  /// 搜索原料
  void search(String keyword) {
    _searchKeyword = keyword.isEmpty ? null : keyword;
    loadIngredients();
  }

  /// 创建原料
  Future<bool> createIngredient(CreateIngredientRequest request) async {
    try {
      final response = await _service.createIngredient(request);
      if (response.isSuccess) {
        await loadIngredients();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  /// 更新原料
  Future<bool> updateIngredient(int id, CreateIngredientRequest request) async {
    try {
      final response = await _service.updateIngredient(id, request);
      if (response.isSuccess) {
        await loadIngredients();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  /// 删除原料
  Future<bool> deleteIngredient(int id) async {
    try {
      final response = await _service.deleteIngredient(id);
      if (response.isSuccess) {
        await loadIngredients();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }
}
```

### 4.5 饮食记录状态管理 (providers/food_log_provider.dart)

```dart
import 'package:flutter/material.dart';
import '../models/food_log.dart';
import '../services/food_log_service.dart';

class FoodLogProvider extends ChangeNotifier {
  final FoodLogService _service = FoodLogService();

  FoodLogDay? _todayLog;
  FoodLogCalendar? _calendar;
  bool _isLoading = false;
  String? _error;
  DateTime _selectedDate = DateTime.now();

  FoodLogDay? get todayLog => _todayLog;
  FoodLogCalendar? get calendar => _calendar;
  bool get isLoading => _isLoading;
  String? get error => _error;
  DateTime get selectedDate => _selectedDate;
  String get selectedDateStr => _formatDate(_selectedDate);

  String _formatDate(DateTime date) {
    return '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
  }

  /// 加载今日饮食记录
  Future<void> loadTodayLog() async {
    await loadFoodLogByDate(_selectedDate);
  }

  /// 加载指定日期的饮食记录
  Future<void> loadFoodLogByDate(DateTime date) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _service.getFoodLogByDate(_formatDate(date));
      if (response.isSuccess) {
        _todayLog = response.data;
      } else {
        _error = response.message;
      }
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 加载日历数据
  Future<void> loadCalendar(int year, int month) async {
    try {
      final response = await _service.getCalendar(year, month);
      if (response.isSuccess) {
        _calendar = response.data;
        notifyListeners();
      }
    } catch (e) {
      print('加载日历失败: $e');
    }
  }

  /// 选择日期
  void selectDate(DateTime date) {
    _selectedDate = date;
    loadFoodLogByDate(date);
  }

  /// 添加饮食记录
  Future<bool> addFoodLog(CreateFoodLogRequest request) async {
    try {
      final response = await _service.createFoodLog(request);
      if (response.isSuccess) {
        await loadTodayLog();
        // 刷新日历
        await loadCalendar(_selectedDate.year, _selectedDate.month);
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  /// 更新饮食记录
  Future<bool> updateFoodLog(int id, CreateFoodLogRequest request) async {
    try {
      final response = await _service.updateFoodLog(id, request);
      if (response.isSuccess) {
        await loadTodayLog();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  /// 删除饮食记录
  Future<bool> deleteFoodLog(int id) async {
    try {
      final response = await _service.deleteFoodLog(id);
      if (response.isSuccess) {
        await loadTodayLog();
        // 刷新日历
        await loadCalendar(_selectedDate.year, _selectedDate.month);
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }
}
```

---

## 五、路由配置

### 5.1 路由定义 (router.dart)

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'providers/auth_provider.dart';

// 页面导入
import 'pages/login_page.dart';
import 'pages/home_page.dart';
import 'pages/recipe/recipes_overview_page.dart';
import 'pages/recipe/recipe_detail_page.dart';
import 'pages/recipe/recipe_edit_page.dart';
import 'pages/ingredient/ingredient_list_page.dart';
import 'pages/ingredient/ingredient_edit_page.dart';
import 'pages/foodlog/today_foodlog_page.dart';
import 'pages/foodlog/foodlog_calendar_page.dart';
import 'pages/profile/profile_page.dart';
import 'pages/profile/favorites_page.dart';

// 路由路径常量
class AppRoutes {
  static const String login = '/login';
  static const String home = '/';
  static const String recipes = '/recipes';
  static const String recipeDetail = '/recipes/:id';
  static const String recipeCreate = '/recipes/create';
  static const String recipeEdit = '/recipes/:id/edit';
  static const String ingredients = '/ingredients';
  static const String ingredientDetail = '/ingredients/:id';
  static const String ingredientCreate = '/ingredients/create';
  static const String foodLog = '/foodlog';
  static const String foodLogCalendar = '/foodlog/calendar';
  static const String profile = '/profile';
  static const String favorites = '/favorites';
}

// 创建路由器
GoRouter createRouter(BuildContext context) {
  return GoRouter(
    initialLocation: AppRoutes.login,
    // 刷新监听器：当登录状态变化时重新评估路由
    refreshListenable: Provider.of<AuthProvider>(context, listen: false),
    
    // 路由重定向逻辑
    redirect: (BuildContext context, GoRouterState state) {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      final isAuthenticated = authProvider.isAuthenticated;
      final isLoginPage = state.matchedLocation == AppRoutes.login;

      // 未登录且不在登录页，重定向到登录页
      if (!isAuthenticated && !isLoginPage) {
        return AppRoutes.login;
      }

      // 已登录且在登录页，重定向到首页
      if (isAuthenticated && isLoginPage) {
        return AppRoutes.home;
      }

      return null;
    },

    routes: [
      // 登录页
      GoRoute(
        path: AppRoutes.login,
        builder: (context, state) => const LoginPage(),
      ),

      // 主页（带底部导航）
      ShellRoute(
        builder: (context, state, child) => HomePage(child: child),
        routes: [
          // 菜谱总览
          GoRoute(
            path: AppRoutes.home,
            builder: (context, state) => const RecipesOverviewPage(),
          ),
          // 原料库
          GoRoute(
            path: AppRoutes.ingredients,
            builder: (context, state) => const IngredientListPage(),
          ),
          // 今日饮食
          GoRoute(
            path: AppRoutes.foodLog,
            builder: (context, state) => const TodayFoodLogPage(),
          ),
          // 个人中心
          GoRoute(
            path: AppRoutes.profile,
            builder: (context, state) => const ProfilePage(),
          ),
        ],
      ),

      // 独立页面（不在底部导航中）
      
      // 菜谱详情
      GoRoute(
        path: AppRoutes.recipeDetail,
        builder: (context, state) {
          final id = int.parse(state.pathParameters['id']!);
          return RecipeDetailPage(recipeId: id);
        },
      ),

      // 创建菜谱
      GoRoute(
        path: AppRoutes.recipeCreate,
        builder: (context, state) => const RecipeEditPage(),
      ),

      // 编辑菜谱
      GoRoute(
        path: AppRoutes.recipeEdit,
        builder: (context, state) {
          final id = int.parse(state.pathParameters['id']!);
          return RecipeEditPage(recipeId: id);
        },
      ),

      // 原料编辑
      GoRoute(
        path: AppRoutes.ingredientCreate,
        builder: (context, state) => const IngredientEditPage(),
      ),

      GoRoute(
        path: AppRoutes.ingredientDetail,
        builder: (context, state) {
          final id = int.parse(state.pathParameters['id']!);
          return IngredientEditPage(ingredientId: id);
        },
      ),

      // 饮食日历
      GoRoute(
        path: AppRoutes.foodLogCalendar,
        builder: (context, state) => const FoodLogCalendarPage(),
      ),

      // 我的收藏
      GoRoute(
        path: AppRoutes.favorites,
        builder: (context, state) => const FavoritesPage(),
      ),
    ],

    // 错误页面
    errorBuilder: (context, state) => Scaffold(
      body: Center(
        child: Text('页面未找到: ${state.error}'),
      ),
    ),
  );
}
```

---

## 六、应用入口

### 6.1 主程序 (main.dart)

```dart
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'app.dart';
import 'providers/auth_provider.dart';
import 'providers/category_provider.dart';
import 'providers/recipe_provider.dart';
import 'providers/ingredient_provider.dart';
import 'providers/food_log_provider.dart';

void main() async {
  // 确保Flutter绑定初始化
  WidgetsFlutterBinding.ensureInitialized();

  // 创建AuthProvider并初始化
  final authProvider = AuthProvider();
  await authProvider.initialize();

  runApp(
    MultiProvider(
      providers: [
        // 全局单例Provider
        ChangeNotifierProvider<AuthProvider>.value(value: authProvider),
        
        // 分类Provider（全局需要）
        ChangeNotifierProvider(create: (_) => CategoryProvider()),
        
        // 其他Provider根据需要设置
        ChangeNotifierProvider(create: (_) => RecipeProvider()),
        ChangeNotifierProvider(create: (_) => IngredientProvider()),
        ChangeNotifierProvider(create: (_) => FoodLogProvider()),
      ],
      child: const MyApp(),
    ),
  );
}
```

### 6.2 应用配置 (app.dart)

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'router.dart';

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    // 创建路由器
    final router = createRouter(context);

    return MaterialApp.router(
      title: '菜谱管理',
      debugShowCheckedModeBanner: false,
      
      // 主题配置
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: Colors.orange,
          brightness: Brightness.light,
        ),
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          elevation: 0,
        ),
        cardTheme: CardTheme(
          elevation: 2,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
        inputDecorationTheme: InputDecorationTheme(
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(8),
          ),
          contentPadding: const EdgeInsets.symmetric(
            horizontal: 16,
            vertical: 12,
          ),
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            padding: const EdgeInsets.symmetric(vertical: 12),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
          ),
        ),
      ),

      // 暗黑主题
      darkTheme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: Colors.orange,
          brightness: Brightness.dark,
        ),
      ),

      // 路由器配置
      routerConfig: router,
    );
  }
}
```

---

## 七、页面代码示例

### 7.1 登录页面 (pages/login_page.dart)

```dart
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _formKey = GlobalKey<FormState>();
  final _userNameController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _userNameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;

    final authProvider = context.read<AuthProvider>();
    final success = await authProvider.login(
      _userNameController.text.trim(),
      _passwordController.text,
    );

    if (!success && mounted) {
      // 显示错误提示
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(authProvider.error ?? '登录失败'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();

    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // Logo/图标
                  Icon(
                    Icons.restaurant_menu,
                    size: 80,
                    color: Theme.of(context).colorScheme.primary,
                  ),
                  const SizedBox(height: 16),
                  
                  // 标题
                  Text(
                    '菜谱管理系统',
                    style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 40),

                  // 用户名输入
                  TextFormField(
                    controller: _userNameController,
                    decoration: const InputDecoration(
                      labelText: '用户名',
                      prefixIcon: Icon(Icons.person_outline),
                      hintText: '请输入用户名',
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return '请输入用户名';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),

                  // 密码输入
                  TextFormField(
                    controller: _passwordController,
                    obscureText: _obscurePassword,
                    decoration: InputDecoration(
                      labelText: '密码',
                      prefixIcon: const Icon(Icons.lock_outline),
                      hintText: '请输入密码',
                      suffixIcon: IconButton(
                        icon: Icon(
                          _obscurePassword 
                            ? Icons.visibility_off 
                            : Icons.visibility,
                        ),
                        onPressed: () {
                          setState(() {
                            _obscurePassword = !_obscurePassword;
                          });
                        },
                      ),
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return '请输入密码';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 24),

                  // 登录按钮
                  SizedBox(
                    width: double.infinity,
                    height: 48,
                    child: ElevatedButton(
                      onPressed: authProvider.isLoading ? null : _login,
                      child: authProvider.isLoading
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('登录', style: TextStyle(fontSize: 16)),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
```

### 7.2 主框架页面 (pages/home_page.dart)

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class HomePage extends StatefulWidget {
  final Widget child;
  
  const HomePage({super.key, required this.child});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  int _currentIndex = 0;

  // 导航项配置
  final List<_NavItem> _navItems = const [
    _NavItem(
      label: '菜谱',
      icon: Icons.restaurant_menu,
      route: '/',
    ),
    _NavItem(
      label: '原料',
      icon: Icons.local_dining,
      route: '/ingredients',
    ),
    _NavItem(
      label: '记录',
      icon: Icons.edit_note,
      route: '/foodlog',
    ),
    _NavItem(
      label: '我的',
      icon: Icons.person_outline,
      route: '/profile',
    ),
  ];

  void _onTap(int index) {
    if (index == _currentIndex) return;
    
    setState(() {
      _currentIndex = index;
    });
    
    // 使用go_router导航
    context.go(_navItems[index].route);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: widget.child,
      bottomNavigationBar: NavigationBar(
        selectedIndex: _currentIndex,
        onDestinationSelected: _onTap,
        destinations: _navItems.map((item) => NavigationDestination(
          icon: Icon(item.icon),
          label: item.label,
        )).toList(),
      ),
    );
  }
}

class _NavItem {
  final String label;
  final IconData icon;
  final String route;

  const _NavItem({
    required this.label,
    required this.icon,
    required this.route,
  });
}
```

### 7.3 菜谱总览页面框架 (pages/recipe/recipes_overview_page.dart)

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../providers/auth_provider.dart';
import '../../providers/category_provider.dart';
import '../../providers/recipe_provider.dart';
import '../../widgets/recipe/recipe_card.dart';
import '../../widgets/recipe/category_selector.dart';

class RecipesOverviewPage extends StatefulWidget {
  const RecipesOverviewPage({super.key});

  @override
  State<RecipesOverviewPage> createState() => _RecipesOverviewPageState();
}

class _RecipesOverviewPageState extends State<RecipesOverviewPage> {
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    // 初始化加载数据
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<CategoryProvider>().loadCategories();
      context.read<RecipeProvider>().loadRecipes(refresh: true);
    });
    
    // 监听滚动到底部加载更多
    _scrollController.addListener(_onScroll);
  }

  void _onScroll() {
    if (_scrollController.position.pixels >= 
        _scrollController.position.maxScrollExtent - 200) {
      context.read<RecipeProvider>().loadRecipes();
    }
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();
    final categoryProvider = context.watch<CategoryProvider>();
    final recipeProvider = context.watch<RecipeProvider>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('菜谱总览'),
        actions: [
          // 仅管理员显示添加按钮
          if (authProvider.isAdmin)
            IconButton(
              icon: const Icon(Icons.add),
              onPressed: () => context.push('/recipes/create'),
            ),
        ],
      ),
      body: Column(
        children: [
          // 分类选择器
          CategorySelector(
            categories: categoryProvider.categories,
            selectedId: recipeProvider.selectedCategoryId,
            onSelect: (id) => recipeProvider.setCategoryFilter(id),
          ),
          
          // 菜谱列表
          Expanded(
            child: RefreshIndicator(
              onRefresh: () => recipeProvider.loadRecipes(refresh: true),
              child: _buildRecipeList(recipeProvider),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildRecipeList(RecipeProvider provider) {
    if (provider.isLoading && provider.recipes.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }

    if (provider.error != null && provider.recipes.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text('加载失败: ${provider.error}'),
            ElevatedButton(
              onPressed: () => provider.loadRecipes(refresh: true),
              child: const Text('重试'),
            ),
          ],
        ),
      );
    }

    if (provider.recipes.isEmpty) {
      return const Center(child: Text('暂无菜谱'));
    }

    return ListView.builder(
      controller: _scrollController,
      padding: const EdgeInsets.all(16),
      itemCount: provider.recipes.length + (provider.hasMore ? 1 : 0),
      itemBuilder: (context, index) {
        if (index >= provider.recipes.length) {
          return const Center(
            child: Padding(
              padding: EdgeInsets.all(16),
              child: CircularProgressIndicator(),
            ),
          );
        }

        final recipe = provider.recipes[index];
        return RecipeCard(
          recipe: recipe,
          onTap: () => context.push('/recipes/${recipe.id}'),
          onFavoriteTap: () => provider.toggleFavorite(
            recipe.id, 
            !recipe.isFavorite,
          ),
        );
      },
    );
  }
}
```

---

## 八、工具类和常量

### 8.1 常量定义 (utils/constants.dart)

```dart
class AppConstants {
  // 存储键
  static const String keyToken = 'token';
  static const String keyUserId = 'userId';
  static const String keyUserName = 'userName';
  static const String keyRole = 'role';

  // 分页
  static const int defaultPageSize = 20;

  // 图片占位
  static const String placeholderImage = 'assets/images/placeholder.png';
}

class ApiConstants {
  // 错误码
  static const int success = 200;
  static const int badRequest = 400;
  static const int unauthorized = 401;
  static const int forbidden = 403;
  static const int notFound = 404;
  static const int serverError = 500;
}
```

### 8.2 扩展方法 (utils/extensions.dart)

```dart
import 'package:flutter/material.dart';

// String扩展
extension StringExtension on String {
  /// 截断文本
  String truncate(int maxLength, {String suffix = '...'}) {
    if (length <= maxLength) return this;
    return '${substring(0, maxLength)}$suffix';
  }
}

// DateTime扩展
extension DateTimeExtension on DateTime {
  /// 格式化为日期字符串
  String toDateString() {
    return '$year-${month.toString().padLeft(2, '0')}-${day.toString().padLeft(2, '0')}';
  }

  /// 格式化为时间字符串
  String toTimeString() {
    return '${hour.toString().padLeft(2, '0')}:${minute.toString().padLeft(2, '0')}';
  }

  /// 获取星期中文
  String get weekdayCN {
    const weekdays = ['', '周一', '周二', '周三', '周四', '周五', '周六', '周日'];
    return weekdays[weekday];
  }
}

// BuildContext扩展
extension BuildContextExtension on BuildContext {
  /// 显示SnackBar
  void showSnackBar(String message, {bool isError = false}) {
    ScaffoldMessenger.of(this).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError 
            ? Theme.of(this).colorScheme.error 
            : Theme.of(this).colorScheme.primary,
      ),
    );
  }
}
```

---

## 九、组件示例

### 9.1 菜谱卡片 (widgets/recipe/recipe_card.dart)

```dart
import 'package:flutter/material.dart';
import '../../models/recipe.dart';

class RecipeCard extends StatelessWidget {
  final RecipeListItem recipe;
  final VoidCallback onTap;
  final VoidCallback onFavoriteTap;

  const RecipeCard({
    super.key,
    required this.recipe,
    required this.onTap,
    required this.onFavoriteTap,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Row(
            children: [
              // 图片
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: recipe.image != null
                    ? Image.network(
                        recipe.image!,
                        width: 80,
                        height: 80,
                        fit: BoxFit.cover,
                        errorBuilder: (_, __, ___) => _buildPlaceholder(),
                      )
                    : _buildPlaceholder(),
              ),
              const SizedBox(width: 12),
              
              // 信息
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      recipe.name,
                      style: const TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      recipe.category.name,
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.grey[600],
                      ),
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Icon(Icons.timer, size: 14, color: Colors.grey[600]),
                        const SizedBox(width: 4),
                        Text(
                          '${recipe.cookTime}分钟',
                          style: TextStyle(fontSize: 12, color: Colors.grey[600]),
                        ),
                        const SizedBox(width: 16),
                        _buildDifficultyStars(recipe.difficulty),
                      ],
                    ),
                  ],
                ),
              ),
              
              // 收藏按钮
              IconButton(
                icon: Icon(
                  recipe.isFavorite ? Icons.favorite : Icons.favorite_border,
                  color: recipe.isFavorite ? Colors.red : Colors.grey,
                ),
                onPressed: onFavoriteTap,
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildPlaceholder() {
    return Container(
      width: 80,
      height: 80,
      color: Colors.grey[300],
      child: const Icon(Icons.image, color: Colors.grey),
    );
  }

  Widget _buildDifficultyStars(int difficulty) {
    return Row(
      children: List.generate(5, (index) {
        return Icon(
          index < difficulty ? Icons.star : Icons.star_border,
          size: 14,
          color: Colors.orange,
        );
      }),
    );
  }
}
```

### 9.2 分类选择器 (widgets/recipe/category_selector.dart)

```dart
import 'package:flutter/material.dart';
import '../../models/category.dart';

class CategorySelector extends StatelessWidget {
  final List<Category> categories;
  final int? selectedId;
  final Function(int?) onSelect;

  const CategorySelector({
    super.key,
    required this.categories,
    this.selectedId,
    required this.onSelect,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 50,
      padding: const EdgeInsets.symmetric(horizontal: 8),
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: categories.length + 1, // +1 for "全部"
        itemBuilder: (context, index) {
          if (index == 0) {
            return _buildCategoryChip(null, '全部');
          }
          final category = categories[index - 1];
          return _buildCategoryChip(category.id, category.name);
        },
      ),
    );
  }

  Widget _buildCategoryChip(int? id, String name) {
    final isSelected = selectedId == id;
    
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 8),
      child: ChoiceChip(
        label: Text(name),
        selected: isSelected,
        onSelected: (_) => onSelect(id),
        backgroundColor: Colors.grey[200],
        selectedColor: Colors.orange,
        labelStyle: TextStyle(
          color: isSelected ? Colors.white : Colors.black87,
          fontSize: 13,
        ),
        padding: const EdgeInsets.symmetric(horizontal: 8),
      ),
    );
  }
}
```

---

## 十、开发顺序建议

### 第一阶段：基础框架
1. 创建项目结构和文件夹
2. 配置依赖 (pubspec.yaml)
3. 实现API响应模型和基础模型
4. 实现API客户端封装
5. 配置路由
6. 实现主程序和App入口

### 第二阶段：认证模块
1. 实现AuthService
2. 实现AuthProvider
3. 实现LoginPage
4. 测试登录流程

### 第三阶段：核心功能
1. 菜谱列表和详情
2. 原料库
3. 分类管理
4. 饮食记录

### 第四阶段：完善功能
1. 创建/编辑功能
2. 图片上传
3. 搜索和筛选
4. 收藏功能

### 第五阶段：优化和测试
1. 错误处理
2. 加载状态
3. 空状态
4. 性能优化

---

## 十一、注意事项

### 11.1 API地址配置
Android模拟器使用 `10.0.2.2` 访问本机，iOS模拟器使用 `localhost`，真机测试需要使用实际IP地址。

### 11.2 Token处理
Token会在API客户端拦截器中自动添加到请求头，登录成功后需要调用 `ApiClient().setToken(token)`。

### 11.3 图片处理
目前设计中使用的是Base64或URL，实际项目中可以考虑使用 `image_picker` 插件实现拍照和相册选择。

### 11.4 状态管理
复杂状态使用Provider管理，简单状态可以使用StatefulWidget的setState。

### 11.5 错误处理
建议在Service层统一处理API错误，Provider层处理业务逻辑错误，UI层显示错误信息。
