import 'package:flutter/material.dart';
import 'package:menuapp/models/app_config.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await AppConfig.init();

}