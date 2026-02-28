import '../models/api_response.dart';
import '../models/category.dart';
import 'api_client.dart';

class CategoryService {
  final ApiClient _client = ApiClient();

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

  Future<ApiResponse<bool>> deleteCategory(int id) async {
    try {
      final response = await _client.delete('/api/categories/$id');
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}