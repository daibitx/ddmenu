# Flutter 菜谱管理 App - 完整架构设计文档

> 本文档是菜谱管理 App 的完整前端架构设计，供开发团队参考实现。
> 
> 版本: 1.0.0 | 更新日期: 2026-03-03
> 
> 依赖版本: go_router ^17.1.0 | provider ^6.1.5+1 | dio ^5.9.1 | shared_preferences ^2.5.4

---

## 目录

1. [项目概述](#一项目概述)
2. [架构设计](#二架构设计)
3. [项目结构](#三项目结构)
4. [数据层 (Models)](#四数据层-models)
5. [服务层 (Services)](#五服务层-services)
6. [状态管理层 (Providers)](#六状态管理层-providers)
7. [路由配置](#七路由配置)
8. [页面实现 (Pages)](#八页面实现-pages)
9. [组件库 (Widgets)](#九组件库-widgets)
10. [工具类](#十工具类)
11. [开发规范](#十一开发规范)
12. [开发计划](#十二开发计划)

---

## 一、项目概述

### 1.1 项目简介

菜谱管理 App 是一款面向个人及家庭的菜谱管理应用，主要功能包括：

- **菜谱管理**: 浏览、创建、编辑菜谱，支持分类筛选和收藏
- **原料库**: 管理烹饪原料，按类型分组展示
- **饮食记录**: 记录每日饮食，支持日历视图
- **权限控制**: 区分管理员和普通用户权限

### 1.2 技术栈

| 层级 | 技术 | 版本 | 用途 |
|------|------|------|------|
| 框架 | Flutter | 3.11.0+ | 跨平台 UI 框架 |
| 语言 | Dart | 3.0+ | 编程语言 |
| 路由 | go_router | ^17.1.0 | 声明式路由管理 |
| 状态管理 | provider | ^6.1.5+1 | 状态管理方案 |
| 网络请求 | dio | ^5.9.1 | HTTP 客户端 |
| 本地存储 | shared_preferences | ^2.5.4 | 键值对存储 |
| Cookie 管理 | dio_cookie_manager | ^3.3.0 | Cookie 自动管理 |

### 1.3 架构模式

采用 **Provider + Repository** 分层架构模式：

```
┌─────────────────────────────────────────────────────────┐
│                      UI Layer (Pages/Widgets)           │
│                      职责: 页面展示、用户交互              │
├─────────────────────────────────────────────────────────┤
│              State Management (Providers)               │
│              职责: 状态管理、业务逻辑协调                  │
├─────────────────────────────────────────────────────────┤
│              Service Layer (API/Services)               │
│              职责: API 调用、数据转换                      │
├─────────────────────────────────────────────────────────┤
│              Data Layer (Models/Storage)                │
│              职责: 数据模型定义、本地存储                  │
└─────────────────────────────────────────────────────────┘
```

---

## 二、架构设计

### 2.1 架构设计原则

1. **单一职责**: 每个类只负责一个明确的功能
2. **依赖倒置**: 上层依赖抽象，不依赖具体实现
3. **数据流向**: 单向数据流 UI → Provider → Service → API
4. **错误处理**: 分层处理，Service 层捕获异常，Provider 层处理业务错误，UI 层展示错误信息

### 2.2 数据流向

```
用户操作 → UI 层 → Provider → Service → API Client → 后端 API
                                              ↓
UI 更新 ← Provider ← Service ← API Client ← 响应数据
```

### 2.3 认证流程

```
┌─────────┐    POST /api/auth/login    ┌─────────┐
│         │ ─────────────────────────→ │         │
│  Login  │                            │  Backend│
│  Page   │ ←───────────────────────── │         │
│         │   { token, userName, role }│         │
└────┬────┘                            └─────────┘
     │
     │ 1. 保存 AccessToken 到 SP
     │ 2. RefreshToken 保存到 Cookie
     │ 3. 更新 AuthProvider 状态
     ▼
┌─────────┐
│  Home   │
│  Page   │
└─────────┘
```

### 2.4 Token 刷新机制

```
┌─────────┐     请求 API      ┌─────────┐
│         │ ───────────────→ │         │
│  Client │                  │  Server │
│         │ ←──── 401 ────── │         │
└────┬────┘                  └─────────┘
     │
     │ 自动触发刷新
     ▼
┌─────────┐   POST /api/auth/refresh   ┌─────────┐
│         │ ─────────────────────────→ │         │
│  Client │   (Cookie 自动带 Refresh)   │  Server │
│         │ ←──────── 新 Token ─────── │         │
└────┬────┘                            └─────────┘
     │
     │ 重试原请求
     ▼
┌─────────┐
│  成功   │
└─────────┘
```

---

## 三、项目结构

### 3.1 完整目录结构

```
lib/
├── main.dart                          # 应用入口
├── app.dart                           # App 配置 (主题、Provider 注入)
├── router.dart                        # 路由配置 (go_router)
│
├── models/                            # 数据模型层
│   ├── api_response.dart              # 统一 API 响应封装
│   ├── user.dart                      # 用户模型
│   ├── category.dart                  # 分类模型
│   ├── recipe.dart                    # 菜谱模型
│   ├── ingredient.dart                # 原料模型
│   ├── food_log.dart                  # 饮食记录模型
│   └── app_config.dart                # 应用配置模型
│
├── services/                          # 服务层 (API 调用)
│   ├── api_client.dart                # Dio 封装、Token 管理
│   ├── auth_service.dart              # 认证相关 API
│   ├── recipe_service.dart            # 菜谱相关 API
│   ├── ingredient_service.dart        # 原料相关 API
│   ├── category_service.dart          # 分类相关 API
│   ├── food_log_service.dart          # 饮食记录 API
│   └── user_service.dart              # 用户相关 API
│
├── providers/                         # 状态管理层
│   ├── auth_provider.dart             # 认证状态
│   ├── recipe_provider.dart           # 菜谱状态
│   ├── ingredient_provider.dart       # 原料状态
│   ├── category_provider.dart         # 分类状态
│   ├── food_log_provider.dart         # 饮食记录状态
│   └── user_provider.dart             # 用户状态
│
├── pages/                             # 页面层
│   ├── login_page.dart                # 登录页面
│   ├── home_page.dart                 # 主框架 (底部导航)
│   │
│   ├── recipe/                        # 菜谱模块
│   │   ├── recipes_overview_page.dart # 菜谱总览
│   │   ├── recipe_detail_page.dart    # 菜谱详情
│   │   └── recipe_edit_page.dart      # 创建/编辑菜谱
│   │
│   ├── ingredient/                    # 原料模块
│   │   ├── ingredient_list_page.dart  # 原料库列表
│   │   └── ingredient_edit_page.dart  # 原料编辑
│   │
│   ├── foodlog/                       # 饮食记录模块
│   │   ├── today_foodlog_page.dart    # 今日饮食
│   │   └── foodlog_calendar_page.dart # 饮食日历
│   │
│   └── profile/                       # 个人中心模块
│       ├── profile_page.dart          # 个人中心
│       └── favorites_page.dart        # 我的收藏
│
├── widgets/                           # 公共组件层
│   ├── common/                        # 通用组件
│   │   ├── app_button.dart            # 按钮组件
│   │   ├── app_text_field.dart        # 输入框组件
│   │   ├── loading_widget.dart        # 加载组件
│   │   └── error_widget.dart          # 错误组件
│   │
│   ├── recipe/                        # 菜谱相关组件
│   │   ├── recipe_card.dart           # 菜谱卡片
│   │   ├── category_selector.dart     # 分类选择器
│   │   ├── difficulty_stars.dart      # 难度星级
│   │   └── ingredient_input.dart      # 原料输入
│   │
│   ├── ingredient/                    # 原料相关组件
│   │   ├── ingredient_chip.dart       # 原料标签
│   │   └── ingredient_group.dart      # 原料分组
│   │
│   └── foodlog/                       # 饮食记录组件
│       ├── meal_section.dart          # 餐段分组
│       ├── calendar_cell.dart         # 日历单元格
│       └── food_log_dialog.dart       # 添加记录弹窗
│
└── utils/                             # 工具类
    ├── constants.dart                 # 常量定义
    ├── extensions.dart                # 扩展方法
    └── helpers.dart                   # 辅助函数
```

---

## 四、数据层 (Models)

### 4.1 统一 API 响应模型

**文件**: `lib/models/api_response.dart`

```dart
/// API 统一响应格式，对应后端 OperateResult<T>
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

### 4.2 用户模型

**文件**: `lib/models/user.dart`

```dart
/// 用户基础信息
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
  final int id;

  LoginResponse({
    required this.token,
    required this.userName,
    required this.role,
    required this.id,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      token: json['token'] ?? '',
      userName: json['userName'] ?? '',
      role: json['role'] ?? 'user',
      id: json['id'] ?? 0,
    );
  }
}

/// 用户资料
class UserProfile {
  final int id;
  final String userName;
  final String? avatarUrl;
  final String role;
  final DateTime createdAt;

  UserProfile({
    required this.id,
    required this.userName,
    this.avatarUrl,
    required this.role,
    required this.createdAt,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'] ?? 0,
      userName: json['userName'] ?? '',
      avatarUrl: json['avatarUrl'],
      role: json['role'] ?? 'user',
      createdAt: DateTime.parse(json['createdAt'] ?? DateTime.now().toIso8601String()),
    );
  }
}
```

### 4.3 分类模型

**文件**: `lib/models/category.dart`

```dart
/// 菜谱分类
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

/// 分类类型定义（前端硬编码或从 API 获取）
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

### 4.4 菜谱模型

**文件**: `lib/models/recipe.dart`

```dart
import 'category.dart';

/// 菜谱列表项（精简信息）
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

/// 菜谱详情（完整信息）
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

/// 菜谱原料关联
class RecipeIngredient {
  final int? id;            // null 表示自定义原料
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

### 4.5 原料模型

**文件**: `lib/models/ingredient.dart`

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

### 4.6 饮食记录模型

**文件**: `lib/models/food_log.dart`

```dart
/// 饮食记录项
class FoodLogItem {
  final int id;
  final String time;        // HH:mm 格式
  final String mealType;    // 早餐/午餐/晚餐/加餐
  final String name;
  final int? recipeId;      // 关联的菜谱 ID
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

### 4.7 应用配置模型

**文件**: `lib/models/app_config.dart`

```dart
import 'dart:convert';
import 'package:flutter/services.dart';

/// 应用配置
class AppConfig {
  static late String _baseUrl;
  static late String _apiVersion;
  
  static String get baseUrl => _baseUrl;
  static String get apiVersion => _apiVersion;
  
  /// 初始化配置
  static Future<void> init() async {
    try {
      final jsonString = await rootBundle.loadString('assets/app_config.json');
      final json = jsonDecode(jsonString);
      _baseUrl = json['baseUrl'] ?? 'http://10.0.2.2:5000';
      _apiVersion = json['apiVersion'] ?? 'v1';
    } catch (e) {
      // 使用默认配置
      _baseUrl = 'http://10.0.2.2:5000';
      _apiVersion = 'v1';
    }
  }
}
```

---

## 五、服务层 (Services)

### 5.1 API 客户端封装

**文件**: `lib/services/api_client.dart`

```dart
import 'dart:async';
import 'dart:ui';
import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio/dio.dart';
import 'package:dio_cookie_manager/dio_cookie_manager.dart';
import 'package:path_provider/path_provider.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/app_config.dart';

class ApiClient {
  static final ApiClient _instance = ApiClient._internal();

  factory ApiClient() => _instance;

  late Dio _dio;
  late Dio _refreshDio;
  late CookieJar cookieJar;
  String? _accessToken;

  bool _isRefreshing = false;
  final List<Function(String)> _pendingCallbacks = [];

  /// Token 刷新失败回调，由 AuthProvider 设置
  VoidCallback? onAuthFailed;

  ApiClient._internal() {
    _dio = Dio(
      BaseOptions(
        baseUrl: AppConfig.baseUrl,
        connectTimeout: const Duration(seconds: 30),
        receiveTimeout: const Duration(seconds: 30),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );

    // 专门用于刷新 Token 的 Dio 实例（避免拦截器递归）
    _refreshDio = Dio(BaseOptions(baseUrl: AppConfig.baseUrl));

    // 添加拦截器处理 Token 和刷新逻辑
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) {
          // 自动添加 AccessToken 到请求头
          if (_accessToken != null) {
            options.headers['Authorization'] = 'Bearer $_accessToken';
          }
          return handler.next(options);
        },
        onError: (error, handler) async {
          // 遇到 401，尝试刷新 Token
          if (error.response?.statusCode == 401) {
            final newToken = await _handleRefresh();

            if (newToken != null) {
              // 刷新成功，重试原请求
              error.requestOptions.headers['Authorization'] = 'Bearer $newToken';
              final response = await _dio.fetch(error.requestOptions);
              return handler.resolve(response);
            } else {
              // 刷新失败，触发回调并拒绝请求
              onAuthFailed?.call();
              return handler.reject(error);
            }
          }
          return handler.next(error);
        },
      ),
    );
  }

  /// 处理刷新（带锁，防止并发刷新多个请求）
  Future<String?> _handleRefresh() async {
    if (_isRefreshing) {
      final completer = Completer<String?>();
      _pendingCallbacks.add((token) => completer.complete(token));
      return completer.future;
    }

    _isRefreshing = true;

    try {
      final newToken = await _doRefresh();

      // 通知所有排队的请求
      for (var callback in _pendingCallbacks) {
        callback(newToken ?? '');
      }
      _pendingCallbacks.clear();

      return newToken;
    } finally {
      _isRefreshing = false;
    }
  }

  /// 实际执行刷新请求
  Future<String?> _doRefresh() async {
    try {
      final res = await _refreshDio.post('/api/auth/refresh');

      final token = res.data['data']['token'] as String?;
      if (token != null) {
        _accessToken = token;
        await _saveToken(token);
      }
      return token;
    } catch (e) {
      return null;
    }
  }

  /// 初始化：加载 Cookie 和 Token
  Future<void> init() async {
    final appDocDir = await getApplicationDocumentsDirectory();
    cookieJar = PersistCookieJar(
      storage: FileStorage("${appDocDir.path}/.cookies/"),
    );

    // 只加载 AccessToken，RefreshToken 存储在 Cookie 中
    final prefs = await SharedPreferences.getInstance();
    _accessToken = prefs.getString('AccessToken');

    _dio.interceptors.add(CookieManager(cookieJar));
    _refreshDio.interceptors.add(CookieManager(cookieJar));
  }

  /// 登录成功后设置 Token
  Future<void> setTokens(String access, String refresh) async {
    _accessToken = access;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('AccessToken', access);
  }

  /// 清除 Token（登出时调用）
  Future<void> clearTokens() async {
    _accessToken = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('AccessToken');
    
    // 清除 Cookie（包含 RefreshToken）
    await cookieJar.deleteAll();
  }

  /// 保存新的 AccessToken（刷新后）
  Future<void> _saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('AccessToken', token);
  }

  // ============ HTTP 请求方法 ============

  Future<Response> get(String path, {Map<String, dynamic>? queryParameters}) async {
    return await _dio.get(path, queryParameters: queryParameters);
  }

  Future<Response> post(String path, {dynamic data}) async {
    return await _dio.post(path, data: data);
  }

  Future<Response> put(String path, {dynamic data}) async {
    return await _dio.put(path, data: data);
  }

  Future<Response> delete(String path) async {
    return await _dio.delete(path);
  }

  Future<Response> upload(String path, String filePath, String fileName) async {
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(filePath, filename: fileName),
    });
    return await _dio.post(path, data: formData);
  }
}
```

### 5.2 认证服务

**文件**: `lib/services/auth_service.dart`

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

  /// 刷新 Token
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
        message: '刷新 Token 失败: $e',
        success: false,
      );
    }
  }
}
```

### 5.3 菜谱服务

**文件**: `lib/services/recipe_service.dart`

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

### 5.4 原料服务

**文件**: `lib/services/ingredient_service.dart`

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

### 5.5 分类服务

**文件**: `lib/services/category_service.dart`

```dart
import '../models/api_response.dart';
import '../models/category.dart';
import 'api_client.dart';

class CategoryService {
  final ApiClient _client = ApiClient();

  /// 获取所有分类
  /// GET /api/categories
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

### 5.6 饮食记录服务

**文件**: `lib/services/food_log_service.dart`

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

### 5.7 用户服务

**文件**: `lib/services/user_service.dart`

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

## 六、状态管理层 (Providers)

### 6.1 认证状态管理

**文件**: `lib/providers/auth_provider.dart`

```dart
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/user.dart';
import '../services/api_client.dart';
import '../services/auth_service.dart';

class AuthProvider extends ChangeNotifier {
  final AuthService _authService = AuthService();
  final ApiClient _apiClient = ApiClient();
  
  bool _isLoading = false;
  String? _error;
  User? _currentUser;
  bool _isAuthenticated = false;

  // Getters
  bool get isLoading => _isLoading;
  String? get error => _error;
  User? get currentUser => _currentUser;
  bool get isAuthenticated => _isAuthenticated;
  bool get isAdmin => _currentUser?.isAdmin ?? false;

  /// 初始化：从本地存储加载登录状态
  Future<void> initialize() async {
    _isLoading = true;
    notifyListeners();

    try {
      await _apiClient.init();
      
      _apiClient.onAuthFailed = () {
        logout();
      };

      final prefs = await SharedPreferences.getInstance();
      final accessToken = prefs.getString('AccessToken');
      final userName = prefs.getString('UserName');
      final role = prefs.getString('Role');
      final userId = prefs.getInt('UserId');

      if (accessToken != null && userName != null && role != null) {
        _currentUser = User(
          id: userId ?? 0,
          userName: userName,
          role: role,
        );
        _isAuthenticated = true;
      }
    } catch (e) {
      _error = '初始化失败: $e';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 登录
  Future<bool> login(String userName, String password) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _authService.login(userName, password);
      
      if (response.isSuccess && response.data != null) {
        final loginData = response.data!;
        
        _currentUser = User(
          id: loginData.id,
          userName: loginData.userName,
          role: loginData.role,
        );
        _isAuthenticated = true;

        final prefs = await SharedPreferences.getInstance();
        await prefs.setString('UserName', loginData.userName);
        await prefs.setString('Role', loginData.role);

        await _apiClient.setTokens(loginData.token, '');

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
    _isLoading = true;
    notifyListeners();

    try {
      await _apiClient.clearTokens();
      
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove('UserName');
      await prefs.remove('Role');
      await prefs.remove('UserId');
      
      _currentUser = null;
      _isAuthenticated = false;
      _error = null;
    } catch (e) {
      _error = '登出出错: $e';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 更新当前用户信息
  void updateUserInfo(User user) {
    _currentUser = user;
    notifyListeners();
  }

  /// 清除错误信息
  void clearError() {
    _error = null;
    notifyListeners();
  }
}
```

### 6.2 菜谱状态管理

**文件**: `lib/providers/recipe_provider.dart`

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
        // 更新本地列表数据
        final index = _recipes.indexWhere((r) => r.id == recipeId);
        if (index != -1) {
          final oldRecipe = _recipes[index];
          _recipes[index] = RecipeListItem(
            id: oldRecipe.id,
            name: oldRecipe.name,
            category: oldRecipe.category,
            image: oldRecipe.image,
            cookTime: oldRecipe.cookTime,
            difficulty: oldRecipe.difficulty,
            isFavorite: isFavorite,
            createdAt: oldRecipe.createdAt,
          );
        }
        // 更新详情数据
        if (_currentRecipe != null && _currentRecipe!.id == recipeId) {
          _currentRecipe = RecipeDetail(
            id: _currentRecipe!.id,
            name: _currentRecipe!.name,
            category: _currentRecipe!.category,
            image: _currentRecipe!.image,
            cookTime: _currentRecipe!.cookTime,
            difficulty: _currentRecipe!.difficulty,
            isFavorite: isFavorite,
            ingredients: _currentRecipe!.ingredients,
            steps: _currentRecipe!.steps,
            createdAt: _currentRecipe!.createdAt,
            updatedAt: _currentRecipe!.updatedAt,
          );
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

  void clearCurrentRecipe() {
    _currentRecipe = null;
    notifyListeners();
  }
}
```

### 6.3 分类状态管理

**文件**: `lib/providers/category_provider.dart`

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

  /// 根据 ID 获取分类
  Category? getCategoryById(int id) {
    try {
      return _categories.firstWhere((c) => c.id == id);
    } catch (e) {
      return null;
    }
  }

  /// 创建分类
  Future<bool> createCategory(String name, {String? icon, int sortOrder = 0}) async {
    try {
      final response = await _service.createCategory({
        'name': name,
        'icon': icon,
        'sortOrder': sortOrder,
      });
      if (response.isSuccess) {
        await loadCategories();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  /// 更新分类
  Future<bool> updateCategory(int id, String name, {String? icon, int? sortOrder}) async {
    try {
      final response = await _service.updateCategory(id, {
        'name': name,
        if (icon != null) 'icon': icon,
        if (sortOrder != null) 'sortOrder': sortOrder,
      });
      if (response.isSuccess) {
        await loadCategories();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  /// 删除分类
  Future<bool> deleteCategory(int id) async {
    try {
      final response = await _service.deleteCategory(id);
      if (response.isSuccess) {
        _categories.removeWhere((c) => c.id == id);
        notifyListeners();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }
}
```

### 6.4 原料状态管理

**文件**: `lib/providers/ingredient_provider.dart`

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

  /// 根据 ID 获取原料
  Ingredient? getIngredientById(int id) {
    try {
      return _allIngredients.firstWhere((i) => i.id == id);
    } catch (e) {
      return null;
    }
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

### 6.5 饮食记录状态管理

**文件**: `lib/providers/food_log_provider.dart`

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
      debugPrint('加载日历失败: $e');
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

### 6.6 用户状态管理

**文件**: `lib/providers/user_provider.dart`

```dart
import 'package:flutter/material.dart';
import '../models/user.dart';
import '../services/user_service.dart';

class UserProvider extends ChangeNotifier {
  final UserService _service = UserService();

  UserProfile? _profile;
  bool _isLoading = false;
  String? _error;

  UserProfile? get profile => _profile;
  bool get isLoading => _isLoading;
  String? get error => _error;

  /// 加载用户资料
  Future<void> loadProfile() async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _service.getProfile();
      if (response.isSuccess) {
        _profile = response.data;
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

  /// 更新用户资料
  Future<bool> updateProfile({String? userName, String? avatarUrl}) async {
    try {
      final response = await _service.updateProfile({
        if (userName != null) 'userName': userName,
        if (avatarUrl != null) 'avatarUrl': avatarUrl,
      });
      if (response.isSuccess) {
        _profile = response.data;
        notifyListeners();
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  /// 修改密码
  Future<bool> changePassword(String currentPassword, String newPassword) async {
    try {
      final response = await _service.changePassword(currentPassword, newPassword);
      return response.isSuccess;
    } catch (e) {
      return false;
    }
  }

  /// 上传头像
  Future<String?> uploadAvatar(String filePath, String fileName) async {
    try {
      final response = await _service.uploadAvatar(filePath, fileName);
      if (response.isSuccess) {
        return response.data;
      }
      return null;
    } catch (e) {
      return null;
    }
  }
}
```


---

## 七、路由配置

### 7.1 路由定义

**文件**: `lib/router.dart`

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

/// 路由路径常量
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

/// 创建路由器
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

      // 主页（带底部导航的 ShellRoute）
      ShellRoute(
        builder: (context, state, child) => HomePage(child: child),
        routes: [
          // 菜谱总览（默认页）
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

## 八、页面实现 (Pages)

### 8.1 登录页面

**文件**: `lib/pages/login_page.dart`

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

### 8.2 主框架页面

**文件**: `lib/pages/home_page.dart`

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
    
    // 使用 go_router 导航
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

### 8.3 菜谱总览页面

**文件**: `lib/pages/recipe/recipes_overview_page.dart`

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

### 8.4 菜谱详情页面

**文件**: `lib/pages/recipe/recipe_detail_page.dart`

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../models/recipe.dart';
import '../../providers/auth_provider.dart';
import '../../providers/recipe_provider.dart';

class RecipeDetailPage extends StatefulWidget {
  final int recipeId;
  
  const RecipeDetailPage({super.key, required this.recipeId});

  @override
  State<RecipeDetailPage> createState() => _RecipeDetailPageState();
}

class _RecipeDetailPageState extends State<RecipeDetailPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<RecipeProvider>().loadRecipeDetail(widget.recipeId);
    });
  }

  @override
  void dispose() {
    // 清理当前菜谱数据
    context.read<RecipeProvider>().clearCurrentRecipe();
    super.dispose();
  }

  Future<void> _deleteRecipe() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('确认删除'),
        content: const Text('确定要删除这道菜吗？'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('取消'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('删除', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      final provider = context.read<RecipeProvider>();
      final success = await provider.deleteRecipe(widget.recipeId);
      if (success && mounted) {
        context.pop();
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('删除成功')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();
    final recipeProvider = context.watch<RecipeProvider>();
    final recipe = recipeProvider.currentRecipe;

    return Scaffold(
      body: CustomScrollView(
        slivers: [
          // 可折叠 AppBar
          SliverAppBar(
            expandedHeight: 250,
            pinned: true,
            flexibleSpace: FlexibleSpaceBar(
              title: Text(recipe?.name ?? '菜谱详情'),
              background: recipe?.image != null
                ? Image.network(
                    recipe!.image!,
                    fit: BoxFit.cover,
                  )
                : Container(
                    color: Colors.grey[300],
                    child: const Icon(Icons.image, size: 80, color: Colors.grey),
                  ),
            ),
            actions: [
              if (authProvider.isAdmin && recipe != null) ...[
                IconButton(
                  icon: const Icon(Icons.edit),
                  onPressed: () => context.push('/recipes/${widget.recipeId}/edit'),
                ),
                IconButton(
                  icon: const Icon(Icons.delete),
                  onPressed: _deleteRecipe,
                ),
              ],
            ],
          ),

          // 内容区域
          SliverToBoxAdapter(
            child: recipeProvider.isLoadingDetail
              ? const Center(child: Padding(
                  padding: EdgeInsets.all(32),
                  child: CircularProgressIndicator(),
                ))
              : recipe == null
                ? const Center(child: Padding(
                    padding: EdgeInsets.all(32),
                    child: Text('加载失败'),
                  ))
                : _buildContent(recipe),
          ),
        ],
      ),
      floatingActionButton: recipe != null
        ? FloatingActionButton.extended(
            onPressed: () {
              // TODO: 添加到今日饮食
            },
            icon: const Icon(Icons.add),
            label: const Text('加入今日饮食'),
          )
        : null,
    );
  }

  Widget _buildContent(RecipeDetail recipe) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 基本信息
          Row(
            children: [
              Chip(label: Text(recipe.category.name)),
              const SizedBox(width: 8),
              Icon(Icons.timer, size: 16, color: Colors.grey[600]),
              Text('${recipe.cookTime}分钟'),
              const SizedBox(width: 16),
              _buildDifficultyStars(recipe.difficulty),
            ],
          ),
          const SizedBox(height: 8),
          
          // 收藏按钮
          IconButton(
            icon: Icon(
              recipe.isFavorite ? Icons.favorite : Icons.favorite_border,
              color: recipe.isFavorite ? Colors.red : null,
            ),
            onPressed: () {
              context.read<RecipeProvider>().toggleFavorite(
                recipe.id, 
                !recipe.isFavorite,
              );
            },
          ),
          
          const Divider(height: 32),
          
          // 原料清单
          Text(
            '原料清单',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 12),
          ...recipe.ingredients.map((i) => Padding(
            padding: const EdgeInsets.symmetric(vertical: 4),
            child: Row(
              children: [
                const Icon(Icons.circle, size: 6),
                const SizedBox(width: 8),
                Text(i.name),
                const Spacer(),
                Text(
                  i.amount,
                  style: TextStyle(color: Colors.grey[600]),
                ),
                if (i.fromLibrary)
                  const Padding(
                    padding: EdgeInsets.only(left: 4),
                    child: Icon(Icons.check_circle, size: 14, color: Colors.green),
                  ),
              ],
            ),
          )),
          
          const Divider(height: 32),
          
          // 制作步骤
          Text(
            '制作步骤',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 12),
          ...recipe.steps.asMap().entries.map((entry) {
            final index = entry.key;
            final step = entry.value;
            return Padding(
              padding: const EdgeInsets.symmetric(vertical: 8),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  CircleAvatar(
                    radius: 12,
                    child: Text('${index + 1}'),
                  ),
                  const SizedBox(width: 12),
                  Expanded(child: Text(step.description)),
                ],
              ),
            );
          }),
          
          const SizedBox(height: 80), // 为 FAB 留出空间
        ],
      ),
    );
  }

  Widget _buildDifficultyStars(int difficulty) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(5, (index) {
        return Icon(
          index < difficulty ? Icons.star : Icons.star_border,
          size: 16,
          color: Colors.orange,
        );
      }),
    );
  }
}
```

### 8.5 菜谱编辑页面

**文件**: `lib/pages/recipe/recipe_edit_page.dart`

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../models/category.dart';
import '../../models/recipe.dart';
import '../../providers/category_provider.dart';
import '../../providers/recipe_provider.dart';

class RecipeEditPage extends StatefulWidget {
  final int? recipeId;
  
  const RecipeEditPage({super.key, this.recipeId});

  @override
  State<RecipeEditPage> createState() => _RecipeEditPageState();
}

class _RecipeEditPageState extends State<RecipeEditPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _cookTimeController = TextEditingController();
  
  int? _selectedCategoryId;
  int _difficulty = 3;
  final List<RecipeIngredient> _ingredients = [];
  final List<RecipeStep> _steps = [];

  bool get isEdit => widget.recipeId != null;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<CategoryProvider>().loadCategories();
      if (isEdit) {
        _loadRecipeData();
      }
    });
  }

  void _loadRecipeData() {
    final recipe = context.read<RecipeProvider>().currentRecipe;
    if (recipe != null) {
      _nameController.text = recipe.name;
      _cookTimeController.text = recipe.cookTime.toString();
      _selectedCategoryId = recipe.category.id;
      _difficulty = recipe.difficulty;
      _ingredients.addAll(recipe.ingredients);
      _steps.addAll(recipe.steps);
      setState(() {});
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _cookTimeController.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedCategoryId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('请选择分类')),
      );
      return;
    }
    if (_ingredients.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('请添加至少一个原料')),
      );
      return;
    }
    if (_steps.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('请添加至少一个步骤')),
      );
      return;
    }

    final request = CreateRecipeRequest(
      name: _nameController.text.trim(),
      categoryId: _selectedCategoryId!,
      cookTime: int.tryParse(_cookTimeController.text) ?? 0,
      difficulty: _difficulty,
      ingredients: _ingredients,
      steps: _steps.map((s) => s.description).toList(),
    );

    final provider = context.read<RecipeProvider>();
    final success = isEdit
      ? await provider.updateRecipe(widget.recipeId!, request)
      : await provider.createRecipe(request);

    if (success && mounted) {
      context.pop();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(isEdit ? '更新成功' : '创建成功')),
      );
    } else if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(provider.error ?? '操作失败')),
      );
    }
  }

  void _addIngredient() {
    // TODO: 显示原料选择对话框
  }

  void _addStep() {
    showDialog(
      context: context,
      builder: (context) {
        final controller = TextEditingController();
        return AlertDialog(
          title: const Text('添加步骤'),
          content: TextField(
            controller: controller,
            decoration: const InputDecoration(
              hintText: '请输入步骤描述',
            ),
            maxLines: 3,
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () {
                if (controller.text.isNotEmpty) {
                  setState(() {
                    _steps.add(RecipeStep(
                      stepNumber: _steps.length + 1,
                      description: controller.text,
                    ));
                  });
                  Navigator.pop(context);
                }
              },
              child: const Text('确定'),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final categoryProvider = context.watch<CategoryProvider>();
    final recipeProvider = context.watch<RecipeProvider>();

    return Scaffold(
      appBar: AppBar(
        title: Text(isEdit ? '编辑菜谱' : '新建菜谱'),
        actions: [
          TextButton(
            onPressed: recipeProvider.isLoading ? null : _save,
            child: recipeProvider.isLoading
              ? const SizedBox(
                  width: 16,
                  height: 16,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Text('保存'),
          ),
        ],
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            // 图片上传占位
            Container(
              height: 150,
              decoration: BoxDecoration(
                color: Colors.grey[200],
                borderRadius: BorderRadius.circular(12),
              ),
              child: const Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.camera_alt, size: 48, color: Colors.grey),
                    SizedBox(height: 8),
                    Text('点击上传图片', style: TextStyle(color: Colors.grey)),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),

            // 菜名
            TextFormField(
              controller: _nameController,
              decoration: const InputDecoration(
                labelText: '菜名 *',
                hintText: '请输入菜名',
              ),
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return '请输入菜名';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),

            // 分类选择
            DropdownButtonFormField<int>(
              value: _selectedCategoryId,
              decoration: const InputDecoration(
                labelText: '分类 *',
              ),
              items: categoryProvider.categories.map((c) {
                return DropdownMenuItem(
                  value: c.id,
                  child: Text(c.name),
                );
              }).toList(),
              onChanged: (value) => setState(() => _selectedCategoryId = value),
              validator: (value) {
                if (value == null) {
                  return '请选择分类';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),

            // 预计用时
            TextFormField(
              controller: _cookTimeController,
              decoration: const InputDecoration(
                labelText: '预计用时（分钟）',
                hintText: '请输入预计用时',
              ),
              keyboardType: TextInputType.number,
            ),
            const SizedBox(height: 16),

            // 难度选择
            Text('难度', style: Theme.of(context).textTheme.titleSmall),
            const SizedBox(height: 8),
            Row(
              children: List.generate(5, (index) {
                return IconButton(
                  icon: Icon(
                    index < _difficulty ? Icons.star : Icons.star_border,
                    color: Colors.orange,
                  ),
                  onPressed: () => setState(() => _difficulty = index + 1),
                );
              }),
            ),
            const SizedBox(height: 16),

            // 原料清单
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('原料清单', style: Theme.of(context).textTheme.titleMedium),
                TextButton.icon(
                  onPressed: _addIngredient,
                  icon: const Icon(Icons.add),
                  label: const Text('添加'),
                ),
              ],
            ),
            ..._ingredients.asMap().entries.map((entry) {
              final index = entry.key;
              final ingredient = entry.value;
              return ListTile(
                leading: Text('${index + 1}.'),
                title: Text(ingredient.name),
                subtitle: Text(ingredient.amount),
                trailing: IconButton(
                  icon: const Icon(Icons.delete, color: Colors.red),
                  onPressed: () {
                    setState(() => _ingredients.removeAt(index));
                  },
                ),
              );
            }),
            const SizedBox(height: 16),

            // 制作步骤
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('制作步骤', style: Theme.of(context).textTheme.titleMedium),
                TextButton.icon(
                  onPressed: _addStep,
                  icon: const Icon(Icons.add),
                  label: const Text('添加'),
                ),
              ],
            ),
            ..._steps.asMap().entries.map((entry) {
              final index = entry.key;
              final step = entry.value;
              return ListTile(
                leading: CircleAvatar(
                  radius: 12,
                  child: Text('${index + 1}'),
                ),
                title: Text(step.description),
                trailing: IconButton(
                  icon: const Icon(Icons.delete, color: Colors.red),
                  onPressed: () {
                    setState(() {
                      _steps.removeAt(index);
                      // 重新编号
                      for (var i = 0; i < _steps.length; i++) {
                        _steps[i] = RecipeStep(
                          stepNumber: i + 1,
                          description: _steps[i].description,
                        );
                      }
                    });
                  },
                ),
              );
            }),
            
            if (isEdit) ...[
              const SizedBox(height: 32),
              ElevatedButton.icon(
                onPressed: () {
                  // TODO: 删除菜谱
                },
                icon: const Icon(Icons.delete, color: Colors.red),
                label: const Text('删除菜谱', style: TextStyle(color: Colors.red)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.red[50],
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
```

### 8.6 原料库列表页面

**文件**: `lib/pages/ingredient/ingredient_list_page.dart`

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../models/ingredient.dart';
import '../../providers/auth_provider.dart';
import '../../providers/ingredient_provider.dart';

class IngredientListPage extends StatefulWidget {
  const IngredientListPage({super.key});

  @override
  State<IngredientListPage> createState() => _IngredientListPageState();
}

class _IngredientListPageState extends State<IngredientListPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<IngredientProvider>().loadIngredients();
    });
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();
    final ingredientProvider = context.watch<IngredientProvider>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('原料库'),
        actions: [
          if (authProvider.isAdmin)
            IconButton(
              icon: const Icon(Icons.add),
              onPressed: () => context.push('/ingredients/create'),
            ),
        ],
      ),
      body: Column(
        children: [
          // 搜索框
          Padding(
            padding: const EdgeInsets.all(16),
            child: TextField(
              decoration: const InputDecoration(
                hintText: '搜索原料...',
                prefixIcon: Icon(Icons.search),
              ),
              onChanged: (value) => ingredientProvider.search(value),
            ),
          ),
          
          // 类型筛选
          SizedBox(
            height: 50,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 16),
              itemCount: IngredientTypes.all.length + 1,
              itemBuilder: (context, index) {
                if (index == 0) {
                  return _buildFilterChip(null, '全部');
                }
                final type = IngredientTypes.all[index - 1];
                return _buildFilterChip(type, type);
              },
            ),
          ),
          
          // 原料列表
          Expanded(
            child: ingredientProvider.isLoading
              ? const Center(child: CircularProgressIndicator())
              : ingredientProvider.error != null
                ? Center(child: Text('加载失败: ${ingredientProvider.error}'))
                : ListView.builder(
                    itemCount: ingredientProvider.groupedIngredients.length,
                    itemBuilder: (context, index) {
                      final group = ingredientProvider.groupedIngredients[index];
                      return _buildIngredientGroup(group);
                    },
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildFilterChip(String? type, String label) {
    final provider = context.read<IngredientProvider>();
    final isSelected = provider.selectedType == type;
    
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: ChoiceChip(
        label: Text(label),
        selected: isSelected,
        onSelected: (_) => provider.setTypeFilter(type),
      ),
    );
  }

  Widget _buildIngredientGroup(IngredientGroup group) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // 分组标题
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
          child: Row(
            children: [
              Text(
                IngredientTypes.getIcon(group.type),
                style: const TextStyle(fontSize: 20),
              ),
              const SizedBox(width: 8),
              Text(
                '${group.type} (${group.items.length})',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ),
        // 原料列表
        ...group.items.map((ingredient) => ListTile(
          title: Text(ingredient.name),
          subtitle: ingredient.description != null
            ? Text(
                ingredient.description!,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              )
            : null,
          trailing: const Icon(Icons.chevron_right),
          onTap: () => context.push('/ingredients/${ingredient.id}'),
        )),
        const Divider(),
      ],
    );
  }
}
```

### 8.7 原料编辑页面

**文件**: `lib/pages/ingredient/ingredient_edit_page.dart`

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../models/ingredient.dart';
import '../../providers/ingredient_provider.dart';

class IngredientEditPage extends StatefulWidget {
  final int? ingredientId;
  
  const IngredientEditPage({super.key, this.ingredientId});

  @override
  State<IngredientEditPage> createState() => _IngredientEditPageState();
}

class _IngredientEditPageState extends State<IngredientEditPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descriptionController = TextEditingController();
  
  String _selectedType = IngredientTypes.other;

  bool get isEdit => widget.ingredientId != null;

  @override
  void initState() {
    super.initState();
    if (isEdit) {
      _loadIngredientData();
    }
  }

  void _loadIngredientData() {
    final ingredient = context.read<IngredientProvider>()
      .getIngredientById(widget.ingredientId!);
    if (ingredient != null) {
      _nameController.text = ingredient.name;
      _descriptionController.text = ingredient.description ?? '';
      _selectedType = ingredient.type;
      setState(() {});
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;

    final request = CreateIngredientRequest(
      name: _nameController.text.trim(),
      type: _selectedType,
      description: _descriptionController.text.isEmpty
        ? null
        : _descriptionController.text.trim(),
    );

    final provider = context.read<IngredientProvider>();
    final success = isEdit
      ? await provider.updateIngredient(widget.ingredientId!, request)
      : await provider.createIngredient(request);

    if (success && mounted) {
      context.pop();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(isEdit ? '更新成功' : '创建成功')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(isEdit ? '编辑原料' : '新建原料'),
        actions: [
          TextButton(
            onPressed: _save,
            child: const Text('保存'),
          ),
        ],
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            // 原料名称
            TextFormField(
              controller: _nameController,
              decoration: const InputDecoration(
                labelText: '原料名称 *',
                hintText: '请输入原料名称',
              ),
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return '请输入原料名称';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),

            // 类型选择
            DropdownButtonFormField<String>(
              value: _selectedType,
              decoration: const InputDecoration(
                labelText: '类型 *',
              ),
              items: IngredientTypes.all.map((type) {
                return DropdownMenuItem(
                  value: type,
                  child: Row(
                    children: [
                      Text(IngredientTypes.getIcon(type)),
                      const SizedBox(width: 8),
                      Text(type),
                    ],
                  ),
                );
              }).toList(),
              onChanged: (value) {
                if (value != null) {
                  setState(() => _selectedType = value);
                }
              },
            ),
            const SizedBox(height: 16),

            // 描述
            TextFormField(
              controller: _descriptionController,
              decoration: const InputDecoration(
                labelText: '描述',
                hintText: '请输入原料描述（可选）',
              ),
              maxLines: 3,
            ),
            
            if (isEdit) ...[
              const SizedBox(height: 32),
              ElevatedButton.icon(
                onPressed: () async {
                  final confirmed = await showDialog<bool>(
                    context: context,
                    builder: (context) => AlertDialog(
                      title: const Text('确认删除'),
                      content: const Text('确定要删除这个原料吗？'),
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.pop(context, false),
                          child: const Text('取消'),
                        ),
                        TextButton(
                          onPressed: () => Navigator.pop(context, true),
                          child: const Text('删除', style: TextStyle(color: Colors.red)),
                        ),
                      ],
                    ),
                  );

                  if (confirmed == true) {
                    final provider = context.read<IngredientProvider>();
                    final success = await provider.deleteIngredient(widget.ingredientId!);
                    if (success && mounted) {
                      context.pop();
                    }
                  }
                },
                icon: const Icon(Icons.delete, color: Colors.red),
                label: const Text('删除原料', style: TextStyle(color: Colors.red)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.red[50],
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
```

### 8.8 今日饮食记录页面

**文件**: `lib/pages/foodlog/today_foodlog_page.dart`

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../models/food_log.dart';
import '../../providers/food_log_provider.dart';
import '../../utils/extensions.dart';

class TodayFoodLogPage extends StatefulWidget {
  const TodayFoodLogPage({super.key});

  @override
  State<TodayFoodLogPage> createState() => _TodayFoodLogPageState();
}

class _TodayFoodLogPageState extends State<TodayFoodLogPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<FoodLogProvider>().loadTodayLog();
    });
  }

  @override
  Widget build(BuildContext context) {
    final provider = context.watch<FoodLogProvider>();
    final selectedDate = provider.selectedDate;

    return Scaffold(
      appBar: AppBar(
        title: const Text('今日饮食'),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () => _showAddFoodLogDialog(context),
          ),
        ],
      ),
      body: Column(
        children: [
          // 日期显示
          Container(
            padding: const EdgeInsets.all(16),
            child: Column(
              children: [
                Text(
                  '${selectedDate.year}年${selectedDate.month}月${selectedDate.day}日',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                Text(
                  selectedDate.weekdayCN,
                  style: TextStyle(color: Colors.grey[600]),
                ),
              ],
            ),
          ),
          
          // 饮食记录列表
          Expanded(
            child: provider.isLoading
              ? const Center(child: CircularProgressIndicator())
              : provider.todayLog?.items.isEmpty ?? true
                ? const Center(child: Text('今天还没有记录哦'))
                : ListView.builder(
                    itemCount: provider.todayLog!.items.length,
                    itemBuilder: (context, index) {
                      final group = provider.todayLog!.items[index];
                      return _buildMealGroup(group);
                    },
                  ),
          ),
          
          // 查看日历按钮
          Padding(
            padding: const EdgeInsets.all(16),
            child: OutlinedButton.icon(
              onPressed: () => context.push('/foodlog/calendar'),
              icon: const Icon(Icons.calendar_month),
              label: const Text('查看日历'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMealGroup(FoodLogGroup group) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
          child: Row(
            children: [
              Text(
                MealTypes.getIcon(group.mealType),
                style: const TextStyle(fontSize: 20),
              ),
              const SizedBox(width: 8),
              Text(
                group.mealType,
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ),
        ...group.items.map((item) => ListTile(
          leading: Text(item.time),
          title: Text(item.name),
          trailing: IconButton(
            icon: const Icon(Icons.delete_outline, color: Colors.red),
            onPressed: () => _deleteFoodLog(item.id),
          ),
        )),
        const Divider(),
      ],
    );
  }

  void _showAddFoodLogDialog(BuildContext context) {
    // TODO: 实现添加饮食记录弹窗
  }

  Future<void> _deleteFoodLog(int id) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('确认删除'),
        content: const Text('确定要删除这条记录吗？'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('取消'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('删除', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      await context.read<FoodLogProvider>().deleteFoodLog(id);
    }
  }
}
```

### 8.9 饮食日历页面

**文件**: `lib/pages/foodlog/foodlog_calendar_page.dart`

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../providers/food_log_provider.dart';

class FoodLogCalendarPage extends StatefulWidget {
  const FoodLogCalendarPage({super.key});

  @override
  State<FoodLogCalendarPage> createState() => _FoodLogCalendarPageState();
}

class _FoodLogCalendarPageState extends State<FoodLogCalendarPage> {
  DateTime _currentMonth = DateTime.now();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadCalendarData();
    });
  }

  void _loadCalendarData() {
    context.read<FoodLogProvider>().loadCalendar(
      _currentMonth.year,
      _currentMonth.month,
    );
  }

  @override
  Widget build(BuildContext context) {
    final provider = context.watch<FoodLogProvider>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('饮食日历'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.pop(),
        ),
      ),
      body: Column(
        children: [
          // 月份选择器
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              IconButton(
                icon: const Icon(Icons.chevron_left),
                onPressed: () {
                  setState(() {
                    _currentMonth = DateTime(
                      _currentMonth.year,
                      _currentMonth.month - 1,
                    );
                  });
                  _loadCalendarData();
                },
              ),
              Text(
                '${_currentMonth.year}年${_currentMonth.month}月',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              IconButton(
                icon: const Icon(Icons.chevron_right),
                onPressed: () {
                  setState(() {
                    _currentMonth = DateTime(
                      _currentMonth.year,
                      _currentMonth.month + 1,
                    );
                  });
                  _loadCalendarData();
                },
              ),
            ],
          ),
          
          // 星期标题
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceAround,
            children: const [
              Text('日'),
              Text('一'),
              Text('二'),
              Text('三'),
              Text('四'),
              Text('五'),
              Text('六'),
            ],
          ),
          
          // 日历网格
          Expanded(
            child: GridView.builder(
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 7,
              ),
              itemCount: _getDaysInMonth(_currentMonth),
              itemBuilder: (context, index) {
                final day = index + 1;
                final date = DateTime(_currentMonth.year, _currentMonth.month, day);
                final dateStr = '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
                final count = provider.calendar?.data[dateStr] ?? 0;
                final isSelected = provider.selectedDateStr == dateStr;

                return InkWell(
                  onTap: () {
                    provider.selectDate(date);
                    context.pop();
                  },
                  child: Container(
                    decoration: BoxDecoration(
                      color: isSelected ? Theme.of(context).colorScheme.primary : null,
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(
                          '$day',
                          style: TextStyle(
                            color: isSelected ? Colors.white : null,
                          ),
                        ),
                        if (count > 0)
                          Container(
                            width: 6,
                            height: 6,
                            decoration: BoxDecoration(
                              color: isSelected ? Colors.white : Colors.orange,
                              shape: BoxShape.circle,
                            ),
                          ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  int _getDaysInMonth(DateTime date) {
    return DateTime(date.year, date.month + 1, 0).day;
  }
}
```

### 8.10 个人中心页面

**文件**: `lib/pages/profile/profile_page.dart`

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../providers/auth_provider.dart';

class ProfilePage extends StatelessWidget {
  const ProfilePage({super.key});

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();
    final user = authProvider.currentUser;

    return Scaffold(
      appBar: AppBar(
        title: const Text('我的'),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // 用户信息卡片
          Center(
            child: Column(
              children: [
                // 头像
                CircleAvatar(
                  radius: 40,
                  backgroundColor: Colors.grey[300],
                  child: user?.avatarUrl != null
                    ? Image.network(user!.avatarUrl!)
                    : const Icon(Icons.person, size: 40, color: Colors.grey),
                ),
                const SizedBox(height: 12),
                // 用户名
                Text(
                  user?.userName ?? '未登录',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: 4),
                // 角色标签
                Chip(
                  label: Text(
                    user?.isAdmin ?? false ? '管理员' : '普通用户',
                    style: const TextStyle(fontSize: 12),
                  ),
                  backgroundColor: user?.isAdmin ?? false
                    ? Colors.orange[100]
                    : Colors.blue[100],
                ),
              ],
            ),
          ),
          
          const SizedBox(height: 32),
          
          // 功能列表
          _buildMenuItem(
            icon: Icons.star,
            iconColor: Colors.orange,
            title: '我的收藏',
            onTap: () => context.push('/favorites'),
          ),
          _buildMenuItem(
            icon: Icons.settings,
            iconColor: Colors.grey,
            title: '设置',
            onTap: () {
              // TODO: 设置页面
            },
          ),
          _buildMenuItem(
            icon: Icons.help_outline,
            iconColor: Colors.blue,
            title: '帮助与反馈',
            onTap: () {
              // TODO: 帮助页面
            },
          ),
          
          const SizedBox(height: 32),
          
          // 退出登录
          ElevatedButton.icon(
            onPressed: () async {
              final confirmed = await showDialog<bool>(
                context: context,
                builder: (context) => AlertDialog(
                  title: const Text('确认退出'),
                  content: const Text('确定要退出登录吗？'),
                  actions: [
                    TextButton(
                      onPressed: () => Navigator.pop(context, false),
                      child: const Text('取消'),
                    ),
                    TextButton(
                      onPressed: () => Navigator.pop(context, true),
                      child: const Text('退出', style: TextStyle(color: Colors.red)),
                    ),
                  ],
                ),
              );

              if (confirmed == true) {
                await authProvider.logout();
              }
            },
            icon: const Icon(Icons.logout, color: Colors.red),
            label: const Text('退出登录', style: TextStyle(color: Colors.red)),
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.red[50],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMenuItem({
    required IconData icon,
    required Color iconColor,
    required String title,
    required VoidCallback onTap,
  }) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        leading: Icon(icon, color: iconColor),
        title: Text(title),
        trailing: const Icon(Icons.chevron_right),
        onTap: onTap,
      ),
    );
  }
}
```

### 8.11 我的收藏页面

**文件**: `lib/pages/profile/favorites_page.dart`

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../providers/recipe_provider.dart';
import '../../widgets/recipe/recipe_card.dart';

class FavoritesPage extends StatefulWidget {
  const FavoritesPage({super.key});

  @override
  State<FavoritesPage> createState() => _FavoritesPageState();
}

class _FavoritesPageState extends State<FavoritesPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      // 加载收藏的菜谱
      context.read<RecipeProvider>().loadRecipes(refresh: true);
    });
  }

  @override
  Widget build(BuildContext context) {
    final recipeProvider = context.watch<RecipeProvider>();
    // 过滤出收藏的菜谱
    final favorites = recipeProvider.recipes.where((r) => r.isFavorite).toList();

    return Scaffold(
      appBar: AppBar(
        title: const Text('我的收藏'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.pop(),
        ),
      ),
      body: recipeProvider.isLoading && favorites.isEmpty
        ? const Center(child: CircularProgressIndicator())
        : favorites.isEmpty
          ? const Center(child: Text('暂无收藏的菜谱'))
          : ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: favorites.length,
              itemBuilder: (context, index) {
                final recipe = favorites[index];
                return RecipeCard(
                  recipe: recipe,
                  onTap: () => context.push('/recipes/${recipe.id}'),
                  onFavoriteTap: () => recipeProvider.toggleFavorite(
                    recipe.id,
                    !recipe.isFavorite,
                  ),
                );
              },
            ),
    );
  }
}
```

---

## 九、组件库 (Widgets)

### 9.1 菜谱卡片

**文件**: `lib/widgets/recipe/recipe_card.dart`

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

### 9.2 分类选择器

**文件**: `lib/widgets/recipe/category_selector.dart`

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

### 9.3 难度星级组件

**文件**: `lib/widgets/recipe/difficulty_stars.dart`

```dart
import 'package:flutter/material.dart';

class DifficultyStars extends StatelessWidget {
  final int difficulty;
  final int maxStars;
  final double size;
  final ValueChanged<int>? onChanged;

  const DifficultyStars({
    super.key,
    required this.difficulty,
    this.maxStars = 5,
    this.size = 24,
    this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(maxStars, (index) {
        final isSelected = index < difficulty;
        return GestureDetector(
          onTap: onChanged != null ? () => onChanged!(index + 1) : null,
          child: Icon(
            isSelected ? Icons.star : Icons.star_border,
            size: size,
            color: Colors.orange,
          ),
        );
      }),
    );
  }
}
```

---

## 十、工具类

### 10.1 常量定义

**文件**: `lib/utils/constants.dart`

```dart
/// 应用常量
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

/// API 常量
class ApiConstants {
  // 错误码
  static const int success = 200;
  static const int badRequest = 400;
  static const int unauthorized = 401;
  static const int forbidden = 403;
  static const int notFound = 404;
  static const int serverError = 500;
}

/// 路由常量
class RouteConstants {
  static const String login = '/login';
  static const String home = '/';
  static const String recipes = '/recipes';
  static const String ingredients = '/ingredients';
  static const String foodLog = '/foodlog';
  static const String profile = '/profile';
}
```

### 10.2 扩展方法

**文件**: `lib/utils/extensions.dart`

```dart
import 'package:flutter/material.dart';

/// String 扩展
extension StringExtension on String {
  /// 截断文本
  String truncate(int maxLength, {String suffix = '...'}) {
    if (length <= maxLength) return this;
    return '${substring(0, maxLength)}$suffix';
  }
}

/// DateTime 扩展
extension DateTimeExtension on DateTime {
  /// 格式化为日期字符串 YYYY-MM-DD
  String toDateString() {
    return '$year-${month.toString().padLeft(2, '0')}-${day.toString().padLeft(2, '0')}';
  }

  /// 格式化为时间字符串 HH:mm
  String toTimeString() {
    return '${hour.toString().padLeft(2, '0')}:${minute.toString().padLeft(2, '0')}';
  }

  /// 获取星期中文
  String get weekdayCN {
    const weekdays = ['', '周一', '周二', '周三', '周四', '周五', '周六', '周日'];
    return weekdays[weekday];
  }
}

/// BuildContext 扩展
extension BuildContextExtension on BuildContext {
  /// 显示 SnackBar
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

  /// 获取主题颜色
  ColorScheme get colors => Theme.of(this).colorScheme;
  
  /// 获取文本主题
  TextTheme get textTheme => Theme.of(this).textTheme;
}
```

### 10.3 辅助函数

**文件**: `lib/utils/helpers.dart`

```dart
import 'package:flutter/material.dart';

/// 显示确认对话框
Future<bool> showConfirmDialog(
  BuildContext context, {
  required String title,
  required String content,
  String confirmText = '确定',
  String cancelText = '取消',
  Color? confirmColor,
}) async {
  final result = await showDialog<bool>(
    context: context,
    builder: (context) => AlertDialog(
      title: Text(title),
      content: Text(content),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context, false),
          child: Text(cancelText),
        ),
        TextButton(
          onPressed: () => Navigator.pop(context, true),
          child: Text(
            confirmText,
            style: TextStyle(color: confirmColor ?? Colors.red),
          ),
        ),
      ],
    ),
  );
  return result ?? false;
}

/// 显示加载对话框
void showLoadingDialog(BuildContext context, {String message = '加载中...'}) {
  showDialog(
    context: context,
    barrierDismissible: false,
    builder: (context) => AlertDialog(
      content: Row(
        children: [
          const CircularProgressIndicator(),
          const SizedBox(width: 16),
          Text(message),
        ],
      ),
    ),
  );
}

/// 隐藏加载对话框
void hideLoadingDialog(BuildContext context) {
  Navigator.of(context).pop();
}

/// 格式化持续时间
String formatDuration(Duration duration) {
  String twoDigits(int n) => n.toString().padLeft(2, '0');
  final hours = twoDigits(duration.inHours);
  final minutes = twoDigits(duration.inMinutes.remainder(60));
  return '$hours:$minutes';
}
```

---

## 十一、开发规范

### 11.1 命名规范

| 类型 | 命名规则 | 示例 |
|------|----------|------|
| 文件 | 小写下划线 | `recipe_detail_page.dart` |
| 类 | 大驼峰 | `RecipeDetailPage` |
| 变量 | 小驼峰 | `recipeList` |
| 常量 | 大写下划线 | `DEFAULT_PAGE_SIZE` |
| 私有成员 | 下划线前缀 | `_isLoading` |

### 11.2 代码组织原则

1. **单一职责**: 每个类/函数只做一件事
2. **依赖注入**: 通过构造函数注入依赖，便于测试
3. **状态管理**: 
   - 全局状态使用 Provider
   - 局部状态使用 StatefulWidget
4. **错误处理**: 
   - Service 层捕获异常
   - Provider 层处理业务错误
   - UI 层展示错误信息

### 11.3 Widget 构建规范

```dart
// 推荐：提取为独立方法
Widget _buildHeader() {
  return Container(...);
}

// 推荐：提取为独立组件
class RecipeCard extends StatelessWidget {...}

// 不推荐：嵌套过深
Widget build(BuildContext context) {
  return Container(
    child: Container(
      child: Container(...), // 嵌套过深
    ),
  );
}
```

### 11.4 状态管理规范

```dart
// Provider 中状态修改后必须调用 notifyListeners()
void updateData() {
  _data = newData;
  notifyListeners(); // 不要忘记！
}

// UI 中根据状态显示不同内容
Widget build(BuildContext context) {
  final provider = context.watch<RecipeProvider>();
  
  if (provider.isLoading) {
    return const LoadingWidget();
  }
  
  if (provider.error != null) {
    return ErrorWidget(message: provider.error!);
  }
  
  return DataWidget(data: provider.data);
}
```

---

## 十二、开发计划

### 第一阶段：基础框架（第 1 周）

- [ ] 创建项目结构和文件夹
- [ ] 配置依赖 (pubspec.yaml)
- [ ] 实现 API 响应模型和基础模型
- [ ] 实现 API 客户端封装
- [ ] 配置路由
- [ ] 实现主程序和 App 入口

### 第二阶段：认证模块（第 2 周）

- [ ] 实现 AuthService
- [ ] 实现 AuthProvider
- [ ] 实现 LoginPage
- [ ] 测试登录流程

### 第三阶段：核心功能（第 3-4 周）

- [ ] 菜谱列表和详情
- [ ] 原料库
- [ ] 分类管理
- [ ] 饮食记录

### 第四阶段：完善功能（第 5 周）

- [ ] 创建/编辑功能
- [ ] 图片上传
- [ ] 搜索和筛选
- [ ] 收藏功能

### 第五阶段：优化和测试（第 6 周）

- [ ] 错误处理
- [ ] 加载状态
- [ ] 空状态
- [ ] 性能优化

---

## 附录

### A. pubspec.yaml 完整配置

```yaml
name: menuapp
description: "我的菜谱"

publish_to: 'none'

version: 1.0.0+1

environment:
  sdk: ^3.11.0

dependencies:
  flutter:
    sdk: flutter

  # 路由
  go_router: ^17.1.0
  
  # 状态管理
  provider: ^6.1.5+1
  
  # 网络请求
  dio: ^5.9.1
  dio_cookie_manager: ^3.3.0
  cookie_jar: ^4.0.9
  
  # 本地存储
  shared_preferences: ^2.5.4
  path_provider: ^2.1.5

  cupertino_icons: ^1.0.8

dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^6.0.0

flutter:
  assets:
    - assets/app_config.json
    - assets/images/
  uses-material-design: true
```

### B. API 端点汇总

| 端点 | 方法 | 描述 |
|------|------|------|
| `/api/auth/login` | POST | 用户登录 |
| `/api/auth/refresh` | POST | 刷新 Token |
| `/api/recipes` | GET | 获取菜谱列表 |
| `/api/recipes` | POST | 创建菜谱 |
| `/api/recipes/{id}` | GET | 获取菜谱详情 |
| `/api/recipes/{id}` | PUT | 更新菜谱 |
| `/api/recipes/{id}` | DELETE | 删除菜谱 |
| `/api/recipes/{id}/favorite` | POST | 切换收藏 |
| `/api/categories` | GET | 获取分类列表 |
| `/api/ingredients` | GET | 获取原料列表 |
| `/api/foodlog` | GET | 获取饮食记录 |
| `/api/foodlog` | POST | 创建饮食记录 |
| `/api/foodlog/calendar` | GET | 获取日历数据 |
| `/api/user/profile` | GET | 获取用户资料 |

---

**文档版本**: 1.0.0
**最后更新**: 2026-03-03
**作者**: Flutter 开发团队
