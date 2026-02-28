class ApiResponse<T> {
  final int code;
  final String? message;
  final T? data;
  final bool success;

  ApiResponse({
    required this.code,
    this.message,
    this.data,
    required this.success,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic)? fromJsonT,
  ) {
    return ApiResponse(
      code: json['code'] ?? 500,
      success: json['success'],
      message: json['message'],
      data: json['data'] != null && fromJsonT != null
          ? fromJsonT(json['data'])
          : null,
    );
  }

  bool get isSuccess => code == 200;
}

class PagedList<T> {
  final List<T> list;
  final int total;
  final int page;
  final int pageSize;

  PagedList({
    required this.list,
    required this.total,
    required this.page,
    required this.pageSize,
  });

  factory PagedList.fromJson(
      Map<String, dynamic> json,
      T Function(dynamic) fromJsonT,
      ) {
    return PagedList(
      list: (json['list'] as List).map((e) => fromJsonT(e)).toList(),
      total: json['total'] ?? 0,
      page: json['page'] ?? 1,
      pageSize: json['pageSize'] ?? 20,
    );
  }

  bool get hasMore => list.length < total;
}