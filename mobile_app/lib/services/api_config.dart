/// Shared backend address for all mobile API services.
class ApiConfig {
  static const String origin = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'https://careflow-ai-se-039.onrender.com',
  );
  static const String apiBaseUrl = '$origin/api';
}
