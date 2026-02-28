import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio/dio.dart';
import 'package:dio_cookie_manager/dio_cookie_manager.dart';
import 'package:flutter/material.dart';
import 'package:menuapp/models/app_config.dart';
import 'package:path_provider/path_provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

typedef OnAuthFailed = void Function();

class ApiClient {
  static final ApiClient _instance = ApiClient._internal();
  factory ApiClient() => _instance;

  late Dio _dio;
  String? _token;
  PersistCookieJar? _cookieJar;
  OnAuthFailed? _onAuthFailed;

  bool _isRefreshing = false;
  final List<ErrorInterceptorHandler> _pendingHandlers = [];
  final List<RequestOptions> _pendingRequests = [];

  ApiClient._internal() {
    _dio = Dio(
      BaseOptions(
        baseUrl: AppConfig.baseUrl ?? "",
        connectTimeout: const Duration(seconds: 25),
        receiveTimeout: const Duration(seconds: 25),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );

    _initCookieManager();

    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          if (_token != null && _token!.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $_token';
          }
          return handler.next(options);
        },
        onError: (error, handler) async {
          if (error.response?.statusCode == 401) {
            return await _handle401(error, handler);
          }
          return handler.next(error);
        },
      ),
    );
  }

  void setOnAuthFailed(OnAuthFailed callback) => _onAuthFailed = callback;

  Future<void> _handle401(
      DioException error,
      ErrorInterceptorHandler handler,
      ) async {
    final requestOptions = error.requestOptions;

    if (_isRefreshing) {
      _pendingHandlers.add(handler);
      _pendingRequests.add(requestOptions);
      return;
    }

    _isRefreshing = true;

    try {
      final newToken = await _refreshToken();

      if (newToken == null || newToken.isEmpty) {
        await _doAuthFailed(error, handler);
        return;
      }

      await setToken(newToken);
      _isRefreshing = false;

      final response = await _retryRequest(requestOptions);
      handler.resolve(response);

      await _processPendingRequests(success: true);

    } catch (e) {
      await _doAuthFailed(error, handler, exception: e);
    }
  }

  Future<void> _doAuthFailed(
      DioException originalError,
      ErrorInterceptorHandler currentHandler, {
        Object? exception,
      }) async {
    _isRefreshing = false;
    _token = null;

    await clearToken();

    _onAuthFailed?.call();

    currentHandler.reject(
      exception != null
          ? DioException(
        requestOptions: originalError.requestOptions,
        error: exception,
        type: DioExceptionType.unknown,
      )
          : originalError,
    );

    await _processPendingRequests(success: false, originalError: originalError);
  }

  Future<void> _processPendingRequests({
    required bool success,
    DioException? originalError,
  }) async {
    if (_pendingHandlers.isEmpty) return;

    for (var i = 0; i < _pendingHandlers.length; i++) {
      final handler = _pendingHandlers[i];
      final request = _pendingRequests[i];

      if (success) {
        try {
          final response = await _retryRequest(request);
          handler.resolve(response);
        } catch (e) {
          handler.reject(
            DioException(
              requestOptions: request,
              error: e,
              type: DioExceptionType.unknown,
            ),
          );
        }
      } else {
        handler.reject(
          originalError ??
              DioException(
                requestOptions: request,
                error: 'Authentication failed',
                type: DioExceptionType.cancel,
              ),
        );
      }
    }

    _pendingHandlers.clear();
    _pendingRequests.clear();
  }

  Future<Response<dynamic>> _retryRequest(RequestOptions requestOptions) async {
    final options = Options(
      method: requestOptions.method,
      headers: {
        ...requestOptions.headers,
        'Authorization': 'Bearer $_token',
      },
    );

    return _dio.request<dynamic>(
      requestOptions.path,
      data: requestOptions.data,
      queryParameters: requestOptions.queryParameters,
      options: options,
    );
  }

  Future<String?> _refreshToken() async {
    try {
      final response = await _dio.post("/api/auth/refresh");
      return response.data?["data"]?["token"] as String?;
    } catch (_) {
      return null;
    }
  }


  Future<void> _initCookieManager() async {
    final dir = await getApplicationDocumentsDirectory();
    _cookieJar = PersistCookieJar(storage: FileStorage("${dir.path}/cookies"));
    _dio.interceptors.add(CookieManager(_cookieJar!));
  }

  Future<void> setToken(String? token) async {
    _token = token;
    final prefs = await SharedPreferences.getInstance();
    if (token != null && token.isNotEmpty) {
      await prefs.setString('token', token);
    } else {
      await prefs.remove('token');
    }
  }

  Future<void> clearToken() async {
    _token = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('token');
    await _cookieJar?.deleteAll();
  }

  Future<void> loadToken() async {
    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString('token');
  }


  Future<Response<T>> get<T>(
      String path, {
        Map<String, dynamic>? queryParameters,
        Options? options,
        CancelToken? cancelToken,
        ProgressCallback? onReceiveProgress,
      }) => _dio.get<T>(
    path,
    queryParameters: queryParameters,
    options: options,
    cancelToken: cancelToken,
    onReceiveProgress: onReceiveProgress,
  );

  Future<Response<T>> post<T>(
      String path, {
        dynamic data,
        Map<String, dynamic>? queryParameters,
        Options? options,
        CancelToken? cancelToken,
        ProgressCallback? onSendProgress,
        ProgressCallback? onReceiveProgress,
      }) => _dio.post<T>(
    path,
    data: data,
    queryParameters: queryParameters,
    options: options,
    cancelToken: cancelToken,
    onSendProgress: onSendProgress,
    onReceiveProgress: onReceiveProgress,
  );

  Future<Response<T>> put<T>(
      String path, {
        dynamic data,
        Map<String, dynamic>? queryParameters,
        Options? options,
        CancelToken? cancelToken,
        ProgressCallback? onSendProgress,
        ProgressCallback? onReceiveProgress,
      }) => _dio.put<T>(
    path,
    data: data,
    queryParameters: queryParameters,
    options: options,
    cancelToken: cancelToken,
    onSendProgress: onSendProgress,
    onReceiveProgress: onReceiveProgress,
  );

  Future<Response<T>> delete<T>(
      String path, {
        dynamic data,
        Map<String, dynamic>? queryParameters,
        Options? options,
        CancelToken? cancelToken,
      }) => _dio.delete<T>(
    path,
    data: data,
    queryParameters: queryParameters,
    options: options,
    cancelToken: cancelToken,
  );

  Future<Response> upload(String path, String filePath, String fileName) async {
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(filePath, filename: fileName),
    });
    return await _dio.post(path, data: formData);
  }
}