class User {
  final int id;
  final String userName;
  final String role;  // 'admin' | 'user'
  final String? avatarUrl;

  User({
    required this.id,
    required this.userName,
    required this.role,
    this.avatarUrl,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'] ?? 0,
      userName: json['userName'] ?? '',
      role: json['role'] ?? 'user',
      avatarUrl: json['avatarUrl'],
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'userName': userName,
    'role': role,
    'avatarUrl': avatarUrl,
  };

  bool get isAdmin => role == 'admin';
}
class LoginResponse {
  final String token;
  final String userName;
  final String role;

  LoginResponse({
    required this.token,
    required this.userName,
    required this.role,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      token: json['token'] ?? '',
      userName: json['userName'] ?? '',
      role: json['role'] ?? 'user',
    );
  }
}
class UserProfile {
  final int id;
  final String userName;
  final String? avatarUrl;
  final String role;

  UserProfile({
    required this.id,
    required this.userName,
    this.avatarUrl,
    required this.role,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'] ?? 0,
      userName: json['userName'] ?? '',
      avatarUrl: json['avatarUrl'],
      role: json['role'] ?? 'user',
    );
  }
}
