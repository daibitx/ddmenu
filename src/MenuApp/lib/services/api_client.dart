import 'dart:async';
import 'dart:ui';

import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio/dio.dart';
import 'package:dio_cookie_manager/dio_cookie_manager.dart';
import 'package:menuapp/models/app_config.dart';
import 'package:menuapp/utils/app_constant.dart';
import 'package:path_provider/path_provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ApiClient {
  static final ApiClient _instance = ApiClient._internal();

  factory ApiClient() => _instance;

  late Dio _dio;
  late Dio _refreshDio;
  late CookieJar cookieJar;
  String? _accessToken;

  bool _isRefreshing = false;
  final List<Function(String)> _pendingCallbacks = [];

  VoidCallback? onAuthFailed;

  ApiClient._internal() {
    _dio = Dio(
      BaseOptions(
        baseUrl: AppConfig.baseUrl ?? "",
        connectTimeout: const Duration(seconds: 30),
        receiveTimeout: const Duration(seconds: 30),
      ),
    );

    _refreshDio = Dio(BaseOptions(baseUrl: AppConfig.baseUrl ?? ""));

    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) {
          if (_accessToken != null) {
            options.headers['Authorization'] = 'Bearer $_accessToken';
          }
          return handler.next(options);
        },
        onError: (error, handler) async {
          if (error.response?.statusCode == 401) {
            final newToken = await _handleRefresh();

            if (newToken != null) {
              error.requestOptions.headers['Authorization'] =
                  'Bearer $newToken';
              final response = await _dio.fetch(error.requestOptions);
              return handler.resolve(response);
            } else {
              onAuthFailed?.call();
              return handler.reject(error);
            }
          }
          return handler.next(error);
        },
      ),
    );
  }

  // 处理刷新
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

  // 实际执行刷新请求
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

  Future<void> init() async {
    final appDocDir = await getApplicationDocumentsDirectory();
    cookieJar = PersistCookieJar(
      storage: FileStorage("${appDocDir.path}/.cookies/"),
    );

    // 只加载 AccessToken，RefreshToken 存储在 Cookie 中
    final prefs = await SharedPreferences.getInstance();
    _accessToken = prefs.getString(AppConstant.accessToken);
    _dio.interceptors.add(CookieManager(cookieJar));
    _refreshDio.interceptors.add(CookieManager(cookieJar));
  }

  // 对外方法：登录后设置
  Future<void> setTokens(String access, String refresh) async {
    _accessToken = access;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(AppConstant.accessToken, access);
  }

  // 对外方法：清除（登出）
  Future<void> clearTokens() async {
    _accessToken = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(AppConstant.accessToken);
    
    // 清除 Cookie
    await cookieJar.deleteAll();
  }

  Future<void> _saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(AppConstant.accessToken, token);
  }

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
