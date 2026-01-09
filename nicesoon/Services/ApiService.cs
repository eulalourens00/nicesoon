using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using nicesoon.Models;


namespace nicesoon.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openRouterApiKey;

        private const string OpenRouterApiUrl = "https://openrouter.ai/api/v1/chat/completions";

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);

            _openRouterApiKey = Secrets.OpenRouterApiKey;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                DebugLog("ТЕСТ ПОДКЛЮЧЕНИЯ : Отправка запроса...");
                // для проверки
                var testRequest = new
                {
                    model = "tngtech/deepseek-r1t2-chimera:free",
                    messages = new[] { new { role = "user", content = "Привет" } },
                    max_tokens = 10
                };

                var jsonRequest = JsonSerializer.Serialize(testRequest);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openRouterApiKey}");
                }
                //на статистику
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://nicesoon.app");
                _httpClient.DefaultRequestHeaders.Add("X-Title", "NiceSoon");

                var response = await _httpClient.PostAsync(OpenRouterApiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                DebugLog($" Тест подключения. Статус: {response.StatusCode}. Ответ: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                DebugLog($" Тест подключения. ОШИБКА: {ex.Message}");
                return false;
            }
        }

        public async Task<string> SendToAIAsync(string userMessage)
        {
            try
            {
                DebugLog($" Отправка сообщения: '{userMessage.Substring(0, Math.Min(userMessage.Length, 50))}...'");

                var systemPrompt = @"Ты - Найсон, AI-ассистент для анализа кошмаров. Отвечай подробно на русском.";
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                };

                var requestData = new
                {
                    model = "tngtech/deepseek-r1t2-chimera:free",
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 1500,
                    stream = false
                };

                var jsonRequest = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(OpenRouterApiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                DebugLog($" Ответ API. Статус: {response.StatusCode}. Тело: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    var aiResponse = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    DebugLog($" Успешный ответ получен.");
                    return aiResponse?.Trim() ?? "ИИ не вернул текст.";
                }
                else
                {
                    // ошибки. я скоро разобью монитор
                    string errorMessage = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => " Неверный API ключ",
                        System.Net.HttpStatusCode.PaymentRequired => " Недостаточно монеток на счету",
                        System.Net.HttpStatusCode.TooManyRequests => " Слишком много запросов",
                        System.Net.HttpStatusCode.NotFound => " Модель не найдена или не поддерживает параметры",
                        _ => $" Ошибка API ({response.StatusCode})"
                    };
                    DebugLog($" {errorMessage}");
                    return $"{errorMessage}\n(Ответ сервера: {responseBody})";
                }
            }
            catch (Exception ex)
            {
                DebugLog($" Исключение в SendToAIAsync: {ex.Message}");
                return $" Сетевая ошибка: {ex.Message}";
            }
        }

        // тест 
        public async Task<string> SendToAITestAsync(string userMessage)
        {
            await Task.Delay(1200);
            var responses = new[] { "**Найсон** (тест): Я понимаю...", "**Найсон** (тест): Интересный сон..." };
            return responses[new Random().Next(responses.Length)];
        }

        private void DebugLog(string message)
        {
            Console.WriteLine($"[ApiService] {DateTime.Now:HH:mm:ss} - {message}");
            System.Diagnostics.Debug.WriteLine($"[ApiService] {message}");
        }
    }
}

