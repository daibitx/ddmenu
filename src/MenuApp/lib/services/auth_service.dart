import 'package:menuapp/services/api_client.dart';

import '../models/api_response.dart';
import '../models/user.dart';

class AuthService {
  final ApiClient _client = ApiClient();

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