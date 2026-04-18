import 'dart:convert';
import 'dart:typed_data';
import 'package:http/http.dart' as http;

class ApiException implements Exception {
  final int statusCode;
  final String message;
  const ApiException(this.statusCode, this.message);

  @override
  String toString() => 'ApiException($statusCode): $message';
}

class ApiService {
  final http.Client _client;

  ApiService({http.Client? client}) : _client = client ?? http.Client();

  Map<String, String> _headers({String? token}) => {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      };

  Future<dynamic> get(String url, {String? token}) async {
    final response = await _client.get(
      Uri.parse(url),
      headers: _headers(token: token),
    );
    return _handle(response);
  }

  Future<Uint8List> getBytes(String url, {String? token}) async {
    final response = await _client.get(
      Uri.parse(url),
      headers: _headers(token: token),
    );
    if (response.statusCode >= 200 && response.statusCode < 300) {
      return response.bodyBytes;
    }
    _handle(response);
    return Uint8List(0);
  }

  Future<dynamic> post(String url, dynamic body, {String? token}) async {
    final response = await _client.post(
      Uri.parse(url),
      headers: _headers(token: token),
      body: json.encode(body),
    );
    return _handle(response);
  }

  Future<dynamic> put(String url, dynamic body, {String? token}) async {
    final response = await _client.put(
      Uri.parse(url),
      headers: _headers(token: token),
      body: json.encode(body),
    );
    return _handle(response);
  }

  Future<dynamic> delete(String url, {String? token}) async {
    final response = await _client.delete(
      Uri.parse(url),
      headers: _headers(token: token),
    );
    return _handle(response);
  }

  Future<dynamic> postMultipart(
    String url, {
    required Map<String, String> fields,
    Map<String, String>? files,
    String? token,
  }) async {
    final request = http.MultipartRequest('POST', Uri.parse(url))
      ..headers.addAll({
        'Accept': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      })
      ..fields.addAll(fields);

    if (files != null) {
      for (final entry in files.entries) {
        request.files.add(await http.MultipartFile.fromPath(entry.key, entry.value));
      }
    }

    final streamed = await request.send();
    final response = await http.Response.fromStream(streamed);
    return _handle(response);
  }

  Future<dynamic> putMultipart(
    String url, {
    required Map<String, String> fields,
    Map<String, String>? files,
    String? token,
  }) async {
    final request = http.MultipartRequest('PUT', Uri.parse(url))
      ..headers.addAll({
        'Accept': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      })
      ..fields.addAll(fields);

    if (files != null) {
      for (final entry in files.entries) {
        request.files.add(await http.MultipartFile.fromPath(entry.key, entry.value));
      }
    }

    final streamed = await request.send();
    final response = await http.Response.fromStream(streamed);
    return _handle(response);
  }

  dynamic _handle(http.Response response) {
    final body = utf8.decode(response.bodyBytes);
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (body.isEmpty) return null;
      return json.decode(body);
    }
    String message = 'Erro ${response.statusCode}';
    try {
      final decoded = json.decode(body);
      if (decoded is Map) {
        message = decoded['erro'] ?? decoded['title'] ?? message;
      }
    } catch (_) {}
    throw ApiException(response.statusCode, message);
  }
}
