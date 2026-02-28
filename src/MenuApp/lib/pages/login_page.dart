import 'package:flutter/material.dart';

import '../services/auth_service.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _userNameController = TextEditingController();
  final _passwordController = TextEditingController();

  bool _obscurePassword = true;
  bool _isLoading = false;

  String? _errorMessage;

  Future<void> _handleLogin() async{
    final userName = _userNameController.text.trim();
    final password = _passwordController.text.trim();

    if(userName.isEmpty || password.isEmpty){
      setState(() {
        _errorMessage = "用户名和密码不能为空";
      });
      return ;
    }
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });
    try{
      final authService = AuthService();
    }
    catch(e){
      _errorMessage = '服务器错误，请稍后重试';
    }
  }

  @override
  Widget build(BuildContext context) {
    return const Placeholder();
  }
}
