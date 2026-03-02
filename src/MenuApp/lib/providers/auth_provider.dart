import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/user.dart';
import '../services/api_client.dart';
import '../services/auth_service.dart';

class AuthProvider extends ChangeNotifier {
  final AuthService _authService = AuthService();
  final ApiClient _apiClient = ApiClient();
  
  bool _isLoading = false;
  String? _error;
  User? _currentUser;
  bool _isAuthenticated = false;

  // Getters
  bool get isLoading => _isLoading;
  String? get error => _error;
  User? get currentUser => _currentUser;
  bool get isAuthenticated => _isAuthenticated;
  bool get isAdmin => _currentUser?.isAdmin ?? false;

  /// 初始化
  Future<void> initialize() async {
    _isLoading = true;
    notifyListeners();

    try {
      await _apiClient.init();
      
      _apiClient.onAuthFailed = () {
        logout();
      };

      final prefs = await SharedPreferences.getInstance();
      final accessToken = prefs.getString('AccessToken');
      final userName = prefs.getString('UserName');
      final role = prefs.getString('Role');
      final userId = prefs.getInt('UserId');

      if (accessToken != null && userName != null && role != null) {
        _currentUser = User(
          id: userId ?? 0,
          userName: userName,
          role: role,
        );
        _isAuthenticated = true;
      }
    } catch (e) {
      _error = '初始化失败: $e';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// 登录
  Future<bool> login(String userName, String password) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _authService.login(userName, password);
      
      if (response.isSuccess && response.data != null) {
        final loginData = response.data!;
        
        _currentUser = User(
          id: loginData.id,
          userName: loginData.userName,
          role: loginData.role,
        );
        _isAuthenticated = true;

        final prefs = await SharedPreferences.getInstance();
        await prefs.setString('UserName', loginData.userName);
        await prefs.setString('Role', loginData.role);

        await _apiClient.setTokens(loginData.token, '');

        _isLoading = false;
        notifyListeners();
        return true;
      } else {
        _error = response.message ?? '登录失败';
        _isLoading = false;
        notifyListeners();
        return false;
      }
    } catch (e) {
      _error = '登录出错: $e';
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<void> logout() async {
    _isLoading = true;
    notifyListeners();

    try {
      await _apiClient.clearTokens();
      
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove('UserName');
      await prefs.remove('Role');
      await prefs.remove('UserId');
      
      _currentUser = null;
      _isAuthenticated = false;
      _error = null;
    } catch (e) {
      _error = '登出出错: $e';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  void updateUserInfo(User user) {
    _currentUser = user;
    notifyListeners();
  }

  void clearError() {
    _error = null;
    notifyListeners();
  }
}
