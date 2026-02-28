import 'dart:convert';

import 'package:flutter/services.dart';

class AppConfig {
  static String? baseUrl;

  static Future<void> init() async {
    try {
      final jsonStr = await rootBundle.loadString('assets/app_config.json');
      final data = json.decode(jsonStr);
      baseUrl = data['baseUrl'];
    } catch (e) {
      rethrow;
    }
  }
}
