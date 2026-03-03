import 'dart:developer';

import 'package:flutter/material.dart';
import 'package:menuapp/models/app_config.dart';
import 'package:provider/provider.dart';
import 'app.dart';
import 'providers/auth_provider.dart';

Future<void> main() async {
  //
  WidgetsFlutterBinding.ensureInitialized();
  
  await AppConfig.init();

  final authProvider = AuthProvider();
  await authProvider.initialize();

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider<AuthProvider>.value(value: authProvider),
        // 其他Provider按需添加
        // ChangeNotifierProvider(create: (_) => CategoryProvider()),
        // ChangeNotifierProvider(create: (_) => RecipeProvider()),
        // ChangeNotifierProvider(create: (_) => IngredientProvider()),
        // ChangeNotifierProvider(create: (_) => FoodLogProvider()),
      ],
      child: const MyApp(),
    ),
  );
}
