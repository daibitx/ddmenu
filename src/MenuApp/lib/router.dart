import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:menuapp/pages/login_page.dart';
import 'package:menuapp/providers/auth_provider.dart';
import 'package:provider/provider.dart';

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
  var router = GoRouter(
    initialLocation: AppRoutes.login,
    redirect: (BuildContext context, GoRouterState state) {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      final isAuthenticated = authProvider.isAuthenticated;
      final isLoginPage = state.matchedLocation == AppRoutes.login;
      if (!isAuthenticated && !isLoginPage) {
        return AppRoutes.login;
      } else if (isAuthenticated && isLoginPage) {
        return AppRoutes.home;
      } else {
        return null;
      }
    },
    routes: [
      GoRoute(
        path: AppRoutes.login,
        builder: (context, state) => const LoginPage(),
      ),
    ],
  );
  return router;
}
