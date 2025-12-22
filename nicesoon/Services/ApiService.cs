using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using nicesoon.Models;  

namespace nicesoon.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        public readonly LocalStorageService _localStorage;
        private const string DeepSeekApiUrl = "https://api.deepseek.com/v1/chat/completions";

        private string _apiKey = "sk-25b37a97fb674de19fb9f07404c00b6c";

        public ApiService(LocalStorageService localStorage)
        {
            _httpClient = new HttpClient();
            _localStorage = localStorage;

            // Загружаем сохраненный API ключ
            LoadApiKeyAsync().Wait();
        }

        // 1. Аутентификация пользователя (локальная, упрощенная)
        public async Task<User> LoginAsync(string email, string password)
        {
            try
            {
                // Простая локальная проверка
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return null;

                await Task.Delay(300); // Небольшая задержка для реалистичности

                var user = new User
                {
                    Id = 1,
                    Email = email,
                    Username = email.Split('@')[0],
                    Token = "local_token",
                    CreatedAt = DateTime.Now
                };

                // Сохраняем пользователя локально
                await _localStorage.SaveUserLocallyAsync(user);

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка входа: {ex.Message}");
                return null;
            }
        }

        // 2. Регистрация (локальная)
        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            try
            {
                // Простая проверка
                if (string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                    return null;

                await Task.Delay(300);

                var user = new User
                {
                    Id = new Random().Next(1000, 9999),
                    Email = email,
                    Username = username,
                    Token = "local_token",
                    CreatedAt = DateTime.Now
                };

                await _localStorage.SaveUserLocallyAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка регистрации: {ex.Message}");
                return null;
            }
        }

        // 3. ГЛАВНОЕ: Отправка сообщения DeepSeek API
        public async Task<string> SendToDeepSeekAsync(string message, string context = "")
        {
            try
            {
                // Проверка ключа
                if (string.IsNullOrEmpty(_apiKey) || _apiKey == "sk-ваш_настоящий_ключ_здесь")
                {
                    // Если забыли вставить ключ, возвращаем тестовый ответ
                    await Task.Delay(800);
                    return @"🦩 **Найсон** (тестовый режим): 

Я вижу, что вы описываете сон. В тестовом режиме я не могу подключиться к AI.

Для полноценной работы:
1. Замените 'sk-ваш_настоящий_ключ_здесь' в ApiService.cs на ваш реальный ключ
2. Ключ можно получить на platform.deepseek.com

А пока пример анализа:
• Тревожные сны часто связаны с дневными переживаниями
• Запись сна уже уменьшает его воздействие
• Попробуйте вести дневник снов регулярно

Как вы себя чувствуете после этого сна?";
                }

                // Подготовка системного промпта
                var systemPrompt = @"Ты - Найсон, AI-ассистент для анализа кошмаров. Твой тон: поддерживающий, эмпатичный. Формат ответа:
1. Короткое сочувствие
2. Анализ 2-3 ключевых символов
3. Один вопрос для рефлексии
4. Поддерживающее завершение

Не давай медицинских советов.";

                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = message }
                };

                if (!string.IsNullOrEmpty(context))
                {
                    messages.Insert(1, new { role = "assistant", content = $"Контекст: {context}" });
                }

                var request = new
                {
                    model = "deepseek-chat",
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 800
                };

                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.PostAsync(DeepSeekApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(jsonResponse);
                    var aiResponse = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return aiResponse;
                }
                else
                {
                    return $"❌ Ошибка API ({response.StatusCode}). Проверьте ключ и баланс.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return $"⚠️ Сетевая ошибка: {ex.Message}";
            }
        }

        // 4. Сохранить запись сна
        public async Task<NightmareRecord> SaveRecordAsync(NightmareRecord record)
        {
            try
            {
                record.Id = new Random().Next(1000, 9999);
                record.CreatedAt = DateTime.Now;

                var records = await _localStorage.LoadRecordsLocallyAsync();
                records.Add(record);

                await _localStorage.SaveRecordsLocallyAsync(records);
                return record;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                return null;
            }
        }

        // 5. Получить все записи
        public async Task<List<NightmareRecord>> GetRecordsAsync()
        {
            var records = await _localStorage.LoadRecordsLocallyAsync();

            // Если нет записей, создаем примеры для демонстрации
            if (!records.Any())
            {
                records = new List<NightmareRecord>
                {
                    new NightmareRecord
                    {
                        Id = 1,
                        Title = "Бег в темноте",
                        Content = "Мне снилось, что я бегу по темному коридору, а за мной кто-то гонится. Я не мог разглядеть кто это, но чувствовал сильный страх.",
                        RecordDate = DateTime.Now.AddDays(-2),
                        CreatedAt = DateTime.Now.AddDays(-2),
                        Emotions = new List<string> { "страх", "тревога" }
                    },
                    new NightmareRecord
                    {
                        Id = 2,
                        Title = "Падение с высоты",
                        Content = "Я стоял на краю высотного здания и вдруг начал падать. Проснулся в холодном поту.",
                        RecordDate = DateTime.Now.AddDays(-1),
                        CreatedAt = DateTime.Now.AddDays(-1),
                        Emotions = new List<string> { "паника", "беспомощность" }
                    }
                };

                await _localStorage.SaveRecordsLocallyAsync(records);
            }

            return records;
        }

        // 6. Удалить запись
        public async Task<bool> DeleteRecordAsync(int recordId)
        {
            try
            {
                var records = await _localStorage.LoadRecordsLocallyAsync();
                var record = records.FirstOrDefault(r => r.Id == recordId);

                if (record != null)
                {
                    records.Remove(record);
                    await _localStorage.SaveRecordsLocallyAsync(records);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления: {ex.Message}");
                return false;
            }
        }

        // 7. Анализировать запись
        public async Task<string> AnalyzeRecordAsync(NightmareRecord record)
        {
            var prompt = $"Проанализируй этот сон: {record.Content}\n\n" +
                        $"Эмоции: {(record.Emotions != null ? string.Join(", ", record.Emotions) : "не указаны")}";

            return await SendToDeepSeekAsync(prompt);
        }

        // 8. Получить пользователя
        public async Task<User> GetCurrentUserAsync()
        {
            var user = await _localStorage.LoadUserLocallyAsync();

            // Если нет пользователя, создаем демо
            if (user == null)
            {
                user = new User
                {
                    Id = 1,
                    Email = "demo@example.com",
                    Username = "Демо-пользователь",
                    Token = "demo_token",
                    CreatedAt = DateTime.Now
                };
            }

            return user;
        }
    }
}

