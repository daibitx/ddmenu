import '../models/api_response.dart';
import '../models/user.dart';
import 'api_client.dart';

class UserService {
  final ApiClient _client = ApiClient();

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

  Future<ApiResponse<String>> uploadAvatar(String filePath, String fileName) async {
    try {
      final response = await _client.upload('/api/user/avatar', filePath, fileName);
      return ApiResponse.fromJson(response.data, (json) => json as String);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}