import '../models/api_response.dart';
import '../models/ingredient.dart';
import 'api_client.dart';

class IngredientService {
  final ApiClient _client = ApiClient();

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

  Future<ApiResponse<bool>> deleteIngredient(int id) async {
    try {
      final response = await _client.delete('/api/ingredients/$id');
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}