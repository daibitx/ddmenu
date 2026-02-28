import '../models/api_response.dart';
import '../models/food_log.dart';
import 'api_client.dart';

class FoodLogService {
  final ApiClient _client = ApiClient();

  Future<ApiResponse<FoodLogDay>> getFoodLogByDate(String date) async {
    try {
      final response = await _client.get(
        '/api/foodlog',
        queryParameters: {'date': date},
      );
      return ApiResponse.fromJson(
        response.data,
            (json) => FoodLogDay.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<FoodLogCalendar>> getCalendar(int year, int month) async {
    try {
      final response = await _client.get(
        '/api/foodlog/calendar',
        queryParameters: {'year': year, 'month': month},
      );
      return ApiResponse.fromJson(
        response.data,
            (json) => FoodLogCalendar.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<FoodLogItem>> createFoodLog(CreateFoodLogRequest request) async {
    try {
      final response = await _client.post('/api/foodlog', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
            (json) => FoodLogItem.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<FoodLogItem>> updateFoodLog(int id, CreateFoodLogRequest request) async {
    try {
      final response = await _client.put('/api/foodlog/$id', data: request.toJson());
      return ApiResponse.fromJson(
        response.data,
            (json) => FoodLogItem.fromJson(json),
      );
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }

  Future<ApiResponse<bool>> deleteFoodLog(int id) async {
    try {
      final response = await _client.delete('/api/foodlog/$id');
      return ApiResponse.fromJson(response.data, (json) => json as bool);
    } catch (e) {
      return ApiResponse(code: 500, message: e.toString(), success: false);
    }
  }
}