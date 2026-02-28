import '../models/api_response.dart';
import '../models/recipe.dart';
import 'api_client.dart';

class RecipeService {
  final ApiClient _client = ApiClient();
  Future<ApiResponse<PagedList<RecipeListItem>>> getRecipes(RecipeQueryParams params) async {
    try {
      final response = await _client.get(
        '/api/recipes',
        queryParameters: params.toQueryParams(),
      );

      return ApiResponse.fromJson(
        response.data,
            (json) => PagedList.fromJson(json, (item) => RecipeListItem.fromJson(item)),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<RecipeDetail>> getRecipeDetail(int id) async {
    try {
      final response = await _client.get('/api/recipes/$id');
      return ApiResponse.fromJson(
        response.data,
            (json) => RecipeDetail.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<RecipeDetail>> createRecipe(CreateRecipeRequest request) async {
    try {
      final response = await _client.post('/api/recipes', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
            (json) => RecipeDetail.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<RecipeDetail>> updateRecipe(int id, CreateRecipeRequest request) async {
    try {
      final response = await _client.put('/api/recipes/$id', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
            (json) => RecipeDetail.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<bool>> deleteRecipe(int id) async {
    try {
      final response = await _client.delete('/api/recipes/$id');
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<bool>> toggleFavorite(int id, bool isFavorite) async {
    try {
      final response = await _client.post(
        '/api/recipes/$id/favorite',
        data: {'isFavorite': isFavorite},
      );
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}